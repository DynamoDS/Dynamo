using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;

using Dynamo;
using Dynamo.Elements;
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

namespace Dynamo.Elements
{
   internal static class Converters
   {
      internal static Expression convertPyFunction(Func<IList<dynamic>, dynamic> pyf)
      {
         return FuncContainer.MakeFunction(
            new FScheme.ExternFunc(
               args =>
                  convertToExpression(
                     pyf(args.Select(ex => convertFromExpression(ex)).ToList())
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
         //               data(args.Select(ex => convertFromExpression(ex)))
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
            return ((Expression.List)exp).Item.Select(x => convertFromExpression(x)).ToList();
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

         this.source.Execute(scope);

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

   [ElementName("Python Script")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Runs an embedded IronPython script")]
   [RequiresTransaction(true)]
   public class dynPython : dynElement
   {
      private DynPythonEngine engine = new DynPythonEngine();
      private bool dirty = true;

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

         this.ContentGrid.Children.Add(tb);

         //System.Windows.Controls.Button addButton = new System.Windows.Controls.Button();
         //addButton.Content = "+";
         //addButton.Width = 20;
         //addButton.Height = 20;
         //addButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         //addButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

         //System.Windows.Controls.Button subButton = new System.Windows.Controls.Button();
         //subButton.Content = "-";
         //subButton.Width = 20;
         //subButton.Height = 20;
         //subButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         //subButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

         //inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
         //inputGrid.ColumnDefinitions.Add(new ColumnDefinition());

         //inputGrid.Children.Add(addButton);
         //Grid.SetColumn(addButton, 0);

         //inputGrid.Children.Add(subButton);
         //Grid.SetColumn(subButton, 1);

         //addButton.Click += new RoutedEventHandler(AddInput);
         //subButton.Click += new RoutedEventHandler(RemoveInput);

         InPortData.Add(new PortData("IN", "Input", typeof(object)));
         OutPortData = new PortData("OUT", "Result of the python script", typeof(object));

         base.RegisterInputsAndOutputs();

         topControl.Height = 200;
         topControl.Width = 300;

         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
      }


      //protected virtual int getNewInputIndex()
      //{
      //   return this.InPortData.Count;
      //}


      //protected virtual void RemoveInput(object sender, RoutedEventArgs args)
      //{
      //   var count = InPortData.Count;
      //   if (count > 0)
      //   {
      //      InPortData.RemoveAt(count - 1);
      //      base.ReregisterInputs();
      //   }
      //}

      //protected virtual void AddInput(object sender, RoutedEventArgs args)
      //{
      //   InPortData.Add(new PortData(null, this.getInputRootName() + this.getNewInputIndex(), "", typeof(object)));
      //   base.ReregisterInputs();
      //}

      //TODO: Make this smarter
      public override bool IsDirty
      {
         get
         {
            return true;
         }
         set { }
      }


      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
         //foreach (var inport in InPortData)
         //{
         //   XmlElement input = xmlDoc.CreateElement("Input");

         //   input.SetAttribute("name", inport.NickName);

         //   dynEl.AppendChild(input);
         //}
         XmlElement script = xmlDoc.CreateElement("Script");
         script.InnerText = this.tb.Text;
         dynEl.AppendChild(script);
      }

      public override void LoadElement(XmlNode elNode)
      {
         //int i = InPortData.Count;
         foreach (XmlNode subNode in elNode.ChildNodes)
         {
            if (subNode.Name == "Script")
               this.tb.Text = subNode.InnerText;

            //if (i > 0) 
            //{
            //   i--;
            //   continue;
            //}

            //if (subNode.Name == "Input")
            //{
            //   this.InPortData.Add(new PortData(null, subNode.Attributes["name"].Value, "", typeof(object)));
            //}
         }
      }

      //protected string getInputRootName()
      //{
      //   return "arg";
      //}

      private delegate void LogDelegate(string msg);
      private delegate void SaveElementDelegate(Autodesk.Revit.DB.Element e);

      private List<Binding> makeBindings(IEnumerable<Expression> args)
      {
         var bindings = this.InPortData
            .Select(x => x.NickName)
            .Zip(args, (s, v) => new Binding(s, Converters.convertFromExpression(v)))
            .ToList();

         bindings.Add(new Binding("__revit__", this.UIDocument.Application));
         bindings.Add(new Binding("DynLog", new LogDelegate(this.Bench.Log)));
         bindings.Add(new Binding(
            "DynFunction",
            new Func<Func<IEnumerable<dynamic>, dynamic>, Expression>(
               Converters.convertPyFunction
            )
         ));

         bindings.Add(new Binding(
            "DynTransaction",
            new Func<Autodesk.Revit.DB.SubTransaction>(
               delegate
               {
                  if (!dynElementSettings.SharedInstance.Bench.IsTransactionActive())
                  {
                     dynElementSettings.SharedInstance.Bench.InitTransaction();
                  }
                  return new Autodesk.Revit.DB.SubTransaction(this.UIDocument.Document);
               }
            )
         ));

         bindings.Add(new Binding("__dynamo__", dynElementSettings.SharedInstance.Bench));
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
            = dynElementSettings.SharedInstance.Bench.Transaction != null
            && dynElementSettings.SharedInstance.Bench.Transaction.GetStatus() == Autodesk.Revit.DB.TransactionStatus.Started;

         Expression result = null;

         if (dynElementSettings.SharedInstance.Bench.InIdleThread)
            result = engine.Evaluate(bindings);
         else
         {
            result = IdlePromise<Expression>.ExecuteOnIdle(
               () => engine.Evaluate(bindings)
            );
         }

         if (transactionRunning)
         {
            if (!dynElementSettings.SharedInstance.Bench.IsTransactionActive())
            {
               dynElementSettings.SharedInstance.Bench.InitTransaction();
            }
            else
            {
               var ts = dynElementSettings.SharedInstance.Bench.Transaction.GetStatus();
               if (ts != Autodesk.Revit.DB.TransactionStatus.Started)
               {
                  if (ts != Autodesk.Revit.DB.TransactionStatus.RolledBack)
                     dynElementSettings.SharedInstance.Bench.CancelTransaction();
                  dynElementSettings.SharedInstance.Bench.InitTransaction();
               }
            }
         }
         else if (dynElementSettings.SharedInstance.Bench.RunInDebug)
         {
            if (dynElementSettings.SharedInstance.Bench.IsTransactionActive())
               dynElementSettings.SharedInstance.Bench.EndTransaction();
         }

         return result;
      }
   }
}
