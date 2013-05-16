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
using System.Windows;
using System.Windows.Input;
using System.Xml;

using Dynamo.Connectors;
using Dynamo.Utilities;
using DynamoPython;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using IronPython.Hosting;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    internal static class Converters
    {
        internal static Value convertPyFunction(Func<IList<dynamic>, dynamic> pyf)
        {
            return Value.NewFunction(
                FSharpFunc<FSharpList<Value>, Value>.FromConverter(
                    args =>
                        convertToValue(
                            pyf(args.Select(ex => convertFromValue(ex)).ToList()))));
        }

        internal static Value convertToValue(dynamic data)
        {
            if (data is Value)
                return data;
            else if (data is string)
                return Value.NewString(data);
            else if (data is double)
                return Value.NewNumber(data);
            else if (data is IEnumerable<dynamic>)
            {
                FSharpList<Value> result = FSharpList<Value>.Empty;

                data.reverse();

                foreach (var x in data)
                {
                    result = FSharpList<Value>.Cons(convertToValue(x), result);
                }

                return Value.NewList(result);
            }
            //else if (data is PythonFunction)
            //{
            //   return FuncContainer.MakeFunction(
            //      new FScheme.ExternFunc(
            //         args =>
            //            convertToValue(
            //               data(args.Select(ex => convertFromValue(ex)))
            //            )
            //      )
            //   );
            //}
            //else if (data is Func<dynamic, dynamic>)
            //{
            //   return Value.NewCurrent(FuncContainer.MakeContinuation(
            //      new Continuation(
            //         exp =>
            //            convertToValue(
            //               data(convertFromValue(exp))
            //            )
            //      )
            //   ));
            //}
            else
                return Value.NewContainer(data);
        }

        internal static dynamic convertFromValue(Value exp)
        {
            if (exp.IsList)
                return ((Value.List)exp).Item.Select(x => convertFromValue(x)).ToList();
            else if (exp.IsNumber)
                return ((Value.Number)exp).Item;
            else if (exp.IsString)
                return ((Value.String)exp).Item;
            else if (exp.IsContainer)
                return ((Value.Container)exp).Item;
            //else if (exp.IsFunction)
            //{
            //   return new Func<IList<dynamic>, dynamic>(
            //      args =>
            //         ((Value.Function)exp).Item
            //            .Invoke(ExecutionEnvironment.IDENT)
            //            .Invoke(Utils.convertSequence(args.Select(
            //               x => (Value)Converters.convertToValue(x)
            //            )))
            //   );
            //}
            //else if (exp.IsSpecial)
            //{
            //   return new Func<IList<dynamic>, dynamic>(
            //      args =>
            //         ((Value.Special)exp).Item
            //            .Invoke(ExecutionEnvironment.IDENT)
            //            .Invoke(
            //}
            //else if (exp.IsCurrent)
            //{
            //   return new Func<dynamic, dynamic>(
            //      ex => 
            //         Converters.convertFromValue(
            //            ((Value.Current)exp).Item.Invoke(Converters.convertToValue(ex))
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

        public Value Evaluate(IEnumerable<Binding> bindings)
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
            catch(Exception e)
            {
                dynSettings.Controller.DynamoViewModel.Log("Unable to execute python script:");
                dynSettings.Controller.DynamoViewModel.Log(e.Message);
                dynSettings.Controller.DynamoViewModel.Log(e.StackTrace);

                return Value.NewNumber(0);
            }

            Value result = Value.NewNumber(1);

            if (scope.ContainsVariable("OUT"))
            {
                dynamic output = scope.GetVariable("OUT");

                result = Converters.convertToValue(output);
            }

            return result;
        }
    }

    public struct Binding
    {
        public string Symbol;
        public dynamic Value;

        public Binding(string sym, dynamic val)
        {
            this.Symbol = sym;
            this.Value = val;
        }
    }

    public static class PythonBindings
    {
        static PythonBindings()
        {
            Bindings = new HashSet<Binding>();
            Bindings.Add(new Binding("__dynamo__", dynSettings.Controller));
        }

        public static HashSet<Binding> Bindings { get; private set; }
    }

    public static class PythonEngine
    {
        public delegate Value EvaluationDelegate(bool dirty, string script, IEnumerable<Binding> bindings);

        public static EvaluationDelegate Evaluator;
        
        private static DynPythonEngine engine = new DynPythonEngine();

        static PythonEngine()
        {
            Evaluator = delegate(bool dirty, string script, IEnumerable<Binding> bindings)
            {
                if (dirty)
                {
                    engine.ProcessCode(script);
                    dirty = false;
                }

                return engine.Evaluate(PythonBindings.Bindings.Concat(bindings));
            };
        }
    }

    [NodeName("Python Script")]
    [NodeCategory(BuiltinNodeCategories.SCRIPTING_PYTHON)]
    [NodeDescription("Runs an embedded IronPython script")]
    public class dynPython : dynNodeWithOneOutput
    {
        private bool dirty = true;
        private Dictionary<string, dynamic> stateDict = new Dictionary<string, dynamic>();

        private string script = "# Write your script here.";

        public dynPython()
        {
            InPortData.Add(new PortData("IN", "Input", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Result of the python script", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
        {
            //topControl.Height = 200;
            //topControl.Width = 300;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem();
            editWindowItem.Header = "Edit...";
            editWindowItem.IsCheckable = false;
            NodeUI.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += new RoutedEventHandler(editWindowItem_Click);
            NodeUI.UpdateLayout();
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
            //script.InnerText = this.tb.Text;
            script.InnerText = this.script;
            dynEl.AppendChild(script);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Script")
                    //this.tb.Text = subNode.InnerText;
                    script = subNode.InnerText;
            }
        }

        private List<Binding> makeBindings(IEnumerable<Value> args)
        {
            //Zip up our inputs
            var bindings = this.InPortData
               .Select(x => x.NickName)
               .Zip(args, (s, v) => new Binding(s, Converters.convertFromValue(v)))
               .Concat(PythonBindings.Bindings)
               .ToList();

            bindings.Add(new Binding("__persistant__", this.stateDict));

            return bindings;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return PythonEngine.Evaluator(dirty, script, makeBindings(args));
        }

        private dynScriptEditWindow editWindow;
        private bool initWindow = false;

        void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            if (!initWindow)
            {
                editWindow = new dynScriptEditWindow();
                // callbacks for autocompletion
                editWindow.editText.TextArea.TextEntering += textEditor_TextArea_TextEntering;
                editWindow.editText.TextArea.TextEntered += textEditor_TextArea_TextEntered;

                var pythonHighlighting = "ICSharpCode.PythonBinding.Resources.Python.xshd";
                var elem = GetType().Assembly.GetManifestResourceStream("DynamoPython.Resources." + pythonHighlighting);

                editWindow.editText.SyntaxHighlighting =
                HighlightingLoader.Load(new XmlTextReader(elem ),
                HighlightingManager.Instance);
            }

            //set the text of the edit window to begin
            editWindow.editText.Text = script;

            if (editWindow.ShowDialog() != true)
            {
                return;
            }

            //set the value from the text in the box
            script = editWindow.editText.Text;

            this.dirty = true;
        }

        CompletionWindow completionWindow;
        private PythonConsoleCompletionDataProvider completionProvider = new PythonConsoleCompletionDataProvider();

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                completionWindow = new CompletionWindow(editWindow.editText.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

                var completions = completionProvider.GenerateCompletionData(editWindow.editText.Text);

                foreach (var ele in completions)
                {
                    data.Add(ele);
                }

                completionWindow.Show();

                completionWindow.Closed += delegate
                {
                    completionWindow = null;
                };
            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

    }


    [NodeName("Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.SCRIPTING_PYTHON)]
    [NodeDescription("Runs a IronPython script from a string")]
    public class dynPythonString : dynNodeWithOneOutput
    {
        private DynPythonEngine engine = new DynPythonEngine();
        private Dictionary<string, dynamic> stateDict = new Dictionary<string, dynamic>();

        public dynPythonString()
        {
            InPortData.Add(new PortData("script", "Script to run", typeof(Value.String)));
            InPortData.Add(new PortData("IN", "Input", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Result of the python script", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private List<Binding> makeBindings(IEnumerable<Value> args)
        {
            //Zip up our inputs
            var bindings = this.InPortData
               .Select(x => x.NickName)
               .Zip(args, (s, v) => new Binding(s, Converters.convertFromValue(v)))
               .Concat(PythonBindings.Bindings)
               .ToList();

            bindings.Add(new Binding("__persistant__", this.stateDict));

            return bindings;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return PythonEngine.Evaluator(
                RequiresRecalc, 
                ((Value.String)args[0]).Item, 
                makeBindings(args.Skip(1)));
        }
    }
}
