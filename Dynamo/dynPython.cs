//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;

using Dynamo;
using Dynamo.Nodes;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;
using Expression = Dynamo.FScheme.Expression;

using Microsoft.FSharp.Collections;

using IronPython;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Windows;
using System.Xml;

namespace Dynamo.Nodes
{
    internal static class Converters
    {
        internal static Expression convertPyFunction(Func<IList<dynamic>, dynamic> pyf)
        {
            return FuncContainer.MakeFunction(
               new FScheme.ExternFunc(
                  args =>
                     convertToExpression(
                        pyf(args.Select(convertFromExpression).ToList())
                     )
               )
            );
        }

        internal static Expression convertToExpression(dynamic data)
        {
            if (data is Expression)
                return data;
            else if (data is string)
                return Expression.NewString(data);
            else if (data is double)
                return Expression.NewNumber(data);
            else if (data is IEnumerable<dynamic>)
            {
                FSharpList<Expression> result = FSharpList<Expression>.Empty;

                data.reverse();

                foreach (var x in data)
                {
                    result = FSharpList<Expression>.Cons(convertToExpression(x), result);
                }

                return Expression.NewList(result);
            }
            //else if (data is PythonFunction)
            //{
            //   return FuncContainer.MakeFunction(
            //      new FScheme.ExternFunc(
            //         args =>
            //            convertToExpression(
            //               data(args.Select(convertFromExpression))
            //            )
            //      )
            //   );
            //}
            //else if (data is Func<dynamic, dynamic>)
            //{
            //   return Expression.NewCurrent(FuncContainer.MakeContinuation(
            //      new Continuation(
            //         exp =>
            //            convertToExpression(
            //               data(convertFromExpression(exp))
            //            )
            //      )
            //   ));
            //}
            else
                return Expression.NewContainer(data);
        }

        internal static dynamic convertFromExpression(Expression exp)
        {
            if (exp.IsList)
                return ((Expression.List)exp).Item.Select(convertFromExpression).ToList();
            else if (exp.IsNumber)
                return ((Expression.Number)exp).Item;
            else if (exp.IsString)
                return ((Expression.String)exp).Item;
            else if (exp.IsContainer)
                return ((Expression.Container)exp).Item;
            //else if (exp.IsFunction)
            //{
            //   return new Func<IList<dynamic>, dynamic>(
            //      args =>
            //         ((Expression.Function)exp).Item
            //            .Invoke(ExecutionEnvironment.IDENT)
            //            .Invoke(Utils.convertSequence(args.Select(
            //               x => (Expression)Converters.convertToExpression(x)
            //            )))
            //   );
            //}
            //else if (exp.IsSpecial)
            //{
            //   return new Func<IList<dynamic>, dynamic>(
            //      args =>
            //         ((Expression.Special)exp).Item
            //            .Invoke(ExecutionEnvironment.IDENT)
            //            .Invoke(
            //}
            //else if (exp.IsCurrent)
            //{
            //   return new Func<dynamic, dynamic>(
            //      ex => 
            //         Converters.convertFromExpression(
            //            ((Expression.Current)exp).Item.Invoke(Converters.convertToExpression(ex))
            //         )
            //   );
            //}
            else
                throw new Exception("Not allowed to pass Functions into a Python Script.");
        }
    }

    internal class DynPythonEngine
    {
        private ScriptEngine engine;
        private ScriptSource source;

        public DynPythonEngine()
        {
            this.engine = Python.CreateEngine();
        }

        public void ProcessCode(string code)
        {
            code = "import clr\nclr.AddReference('RevitAPI')\nclr.AddReference('RevitAPIUI')\nfrom Autodesk.Revit.DB import *\n" + code;
            this.source = engine.CreateScriptSourceFromString(code, SourceCodeKind.Statements);
        }

        public Expression Evaluate(IEnumerable<Binding> bindings)
        {
            var scope = this.engine.CreateScope();

            foreach (var bind in bindings)
            {
                scope.SetVariable(bind.Symbol, bind.Value);
            }

            try
            {
                this.source.Execute(scope);
            }
            catch (SyntaxErrorException ex)
            {
                throw new Exception(
                   ex.Message
                   + " at Line " + (ex.Line - 4)
                   + ", Column " + ex.Column
                );
            }

            Expression result = Expression.NewNumber(1);

            if (scope.ContainsVariable("OUT"))
            {
                dynamic output = scope.GetVariable("OUT");

                result = Converters.convertToExpression(output);
            }

            return result;
        }
    }

    internal struct Binding
    {
        public string Symbol;
        public dynamic Value;

        public Binding(string sym, dynamic val)
        {
            this.Symbol = sym;
            this.Value = val;
        }
    }

    [NodeName("Python Script")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Runs an embedded IronPython script")]
    public class dynPython : dynRevitNode
    {
        private DynPythonEngine engine = new DynPythonEngine();
        private bool dirty = true;
        private Dictionary<string, dynamic> stateDict = new Dictionary<string, dynamic>();

        TextBox tb;

        public dynPython()
        {
            tb = new TextBox()
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                TextWrapping = System.Windows.TextWrapping.NoWrap,
                Height = double.NaN,
                Width = double.NaN,
                AcceptsReturn = true,
                AcceptsTab = true,
                FontFamily = new System.Windows.Media.FontFamily("Courier New")
            };

            tb.TextChanged += delegate { this.dirty = true; };

            NodeUI.ContentGrid.Children.Add(tb);

            InPortData.Add(new PortData("IN", "Input", typeof(object)));

            NodeUI.RegisterInputsAndOutput();

            NodeUI.topControl.Height = 200;
            NodeUI.topControl.Width = 300;

            NodeUI.UpdateLayout();
        }

        private PortData outPortData = new PortData("OUT", "Result of the python script", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        //TODO: Make this smarter
        public override bool RequiresRecalc
        {
            get
            {
                return true;
            }
            set { }
        }


        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            XmlElement script = xmlDoc.CreateElement("Script");
            script.InnerText = this.tb.Text;
            dynEl.AppendChild(script);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Script")
                    this.tb.Text = subNode.InnerText;
            }
        }

        private delegate void LogDelegate(string msg);
        private delegate void SaveElementDelegate(Autodesk.Revit.DB.Element e);

        private List<Binding> makeBindings(IEnumerable<Expression> args)
        {
            //Zip up our inputs
            var bindings = this.InPortData
               .Select(x => x.NickName)
               .Zip(args, (s, v) => new Binding(s, Converters.convertFromExpression(v)))
               .ToList();

            bindings.Add(new Binding("DynLog", new LogDelegate(this.Bench.Log))); //Logging
            //bindings.Add(new Binding(
            //   "DynFunction",
            //   new Func<Func<IEnumerable<dynamic>, dynamic>, Expression>(
            //      Converters.convertPyFunction
            //   )
            //));
            bindings.Add(new Binding(
               "DynTransaction",
               new Func<Autodesk.Revit.DB.SubTransaction>(
                  delegate
                  {
                      if (!dynSettings.Instance.Bench.IsTransactionActive())
                      {
                          dynSettings.Instance.Bench.InitTransaction();
                      }
                      return new Autodesk.Revit.DB.SubTransaction(this.UIDocument.Document);
                  }
               )
            ));
            bindings.Add(new Binding("__revit__", this.UIDocument.Application));
            bindings.Add(new Binding("__dynamo__", dynSettings.Instance.Bench));
            bindings.Add(new Binding("__persistant__", this.stateDict));

            // use this to pass into the python script a list of previously created elements from dynamo 
            bindings.Add(new Binding("DynStoredElements", this.Elements));
            return bindings;
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            if (this.dirty)
            {
                this.engine.ProcessCode(
                   (string)this.tb.Dispatcher.Invoke(new Func<string>(
                      delegate { return this.tb.Text; }
                   ))
                );
                this.dirty = false;
            }

            var bindings = this.makeBindings(args);

            bool transactionRunning
               = dynSettings.Instance.Bench.Transaction != null
               && dynSettings.Instance.Bench.Transaction.GetStatus() == Autodesk.Revit.DB.TransactionStatus.Started;

            Expression result = null;

            if (dynSettings.Instance.Bench.InIdleThread)
                result = engine.Evaluate(bindings);
            else
            {
                result = IdlePromise<Expression>.ExecuteOnIdle(
                   () => engine.Evaluate(bindings)
                );
            }

            if (transactionRunning)
            {
                if (!dynSettings.Instance.Bench.IsTransactionActive())
                {
                    dynSettings.Instance.Bench.InitTransaction();
                }
                else
                {
                    var ts = dynSettings.Instance.Bench.Transaction.GetStatus();
                    if (ts != Autodesk.Revit.DB.TransactionStatus.Started)
                    {
                        if (ts != Autodesk.Revit.DB.TransactionStatus.RolledBack)
                            dynSettings.Instance.Bench.CancelTransaction();
                        dynSettings.Instance.Bench.InitTransaction();
                    }
                }
            }
            else if (dynSettings.Instance.Bench.RunInDebug)
            {
                if (dynSettings.Instance.Bench.IsTransactionActive())
                    dynSettings.Instance.Bench.EndTransaction();
            }

            return result;
        }
    }
}
