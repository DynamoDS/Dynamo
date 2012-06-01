using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;

namespace Dynamo.Elements
{
   [RequiresTransaction(false)]
   [IsInteractive(false)]
   public class dynFunction : dynBuiltinMacro
   {
      public dynFunction(IEnumerable<string> inputs, string output, string symbol)
         : base(symbol)
      {
         this.SetInputs(inputs);

         OutPortData = new PortData(output, "function output", typeof(object));

         this.NickName = symbol;

         this.MouseDoubleClick += delegate
         {
            dynElementSettings.SharedInstance.Bench.DisplayFunction(symbol);
         };

         base.RegisterInputsAndOutputs();
      }

      public dynFunction()
         : base(null)
      {
         this.MouseDoubleClick += delegate
         {
            dynElementSettings.SharedInstance.Bench.DisplayFunction(this.Symbol);
         };
      }

      public override bool IsDirty
      {
         get
         {
            if (_taggedSymbols.Contains(this.Symbol))
               return base.IsDirty;
            _taggedSymbols.Add(this.Symbol);

            var ws = this.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
            bool dirtyInternals = ws.Elements
               //.Where(e => !(e is dynFunction && ((dynFunction)e).Symbol.Equals(this.Symbol)))
               .Any(e => e.IsDirty);
            return dirtyInternals || base.IsDirty;
         }
         set
         {
            //TODO: Implement tagging algorithm for mutual recursion
            base.IsDirty = value;
            if (!value)
            {
               bool start = _startTag;
               _startTag = true;

               if (_taggedSymbols.Contains(this.Symbol))
                  return;
               _taggedSymbols.Add(this.Symbol);

               var ws = this.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
               foreach (var e in ws.Elements)
                  e.IsDirty = false;

               if (!start)
               {
                  _startTag = false;
                  _taggedSymbols.Clear();
               }
            }
         }
      }

      public void SetInputs(IEnumerable<string> inputs)
      {
         int i = 0;
         foreach (string input in inputs)
         {
            PortData data = new PortData(input, "Input #" + (i + 1), typeof(object));

            if (this.InPortData.Count > i)
            {
               InPortData[i] = data;
            }
            else
            {
               InPortData.Add(data);
            }

            i++;
         }

         if (i < InPortData.Count)
         {
            InPortData.RemoveRange(i, InPortData.Count - i);
         }
      }

      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
         //Debug.WriteLine(pd.Object.GetType().ToString());
         XmlElement outEl = xmlDoc.CreateElement("Symbol");
         outEl.SetAttribute("value", this.Symbol);
         dynEl.AppendChild(outEl);

         outEl = xmlDoc.CreateElement("Output");
         outEl.SetAttribute("value", OutPortData.NickName);
         dynEl.AppendChild(outEl);

         outEl = xmlDoc.CreateElement("Inputs");
         foreach (var input in InPortData.Select(x => x.NickName))
         {
            var inputEl = xmlDoc.CreateElement("Input");
            inputEl.SetAttribute("value", input);
            outEl.AppendChild(inputEl);
         }
         dynEl.AppendChild(outEl);
      }

      public override void LoadElement(XmlNode elNode)
      {
         foreach (XmlNode subNode in elNode.ChildNodes)
         {
            if (subNode.Name.Equals("Symbol"))
            {
               this.Symbol = subNode.Attributes[0].Value;
            }
            else if (subNode.Name.Equals("Output"))
            {
               var data = new PortData(subNode.Attributes[0].Value, "function output", typeof(object));

               OutPortData = data;
            }
            else if (subNode.Name.Equals("Inputs"))
            {
               int i = 0;
               foreach (XmlNode inputNode in subNode.ChildNodes)
               {
                  var data = new PortData(inputNode.Attributes[0].Value, "Input #" + (i + 1), typeof(object));

                  if (InPortData.Count > i)
                  {
                     InPortData[i] = data;
                  }
                  else
                  {
                     InPortData.Add(data);
                  }

                  i++;
               }
            }
         }

         base.RegisterInputsAndOutputs();
      }

      //protected internal override INode Build()
      //{
      //   if (this.SaveResult && !this.IsDirty)
      //      return new ExpressionNode(this.oldValue);
      //   else
      //      return base.Build();
      //}

      //protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      //{
      //   return new FunctionNode(this.Symbol, portNames);
      //}

      public override void Destroy()
      {
         var ws = dynElementSettings.SharedInstance.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
         foreach (var el in ws.Elements)
         {
            if (!(el is dynFunction) || !((dynFunction)el).Symbol.Equals(this.Symbol))
               el.Destroy();
         }
      }
   }

   [ElementName("Variable")]
   [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
   [ElementDescription("A function variable")]
   [RequiresTransaction(false)]
   [IsInteractive(false)]
   public class dynSymbol : dynElement
   {
      TextBox tb;

      public dynSymbol()
      {
         OutPortData = new PortData("", "Symbol", typeof(object));

         //add a text box to the input grid of the control
         tb = new TextBox();
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         inputGrid.Children.Add(tb);
         System.Windows.Controls.Grid.SetColumn(tb, 0);
         System.Windows.Controls.Grid.SetRow(tb, 0);
         tb.Text = "";
         //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
         //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

         //turn off the border
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);

         base.RegisterInputsAndOutputs();
      }

      public override bool IsDirty
      {
         get
         {
            return false;
         }
         set { }
      }

      public string Symbol
      {
         get { return this.tb.Text; }
         set { this.tb.Text = value; }
      }

      protected internal override INode Build()
      {
         return new SymbolNode(
            (string)this.Dispatcher.Invoke(new Func<string>(
               () => this.Symbol
            ))
         );
      }

      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
         //Debug.WriteLine(pd.Object.GetType().ToString());
         XmlElement outEl = xmlDoc.CreateElement("Symbol");
         outEl.SetAttribute("value", this.Symbol);
         dynEl.AppendChild(outEl);
      }

      public override void LoadElement(XmlNode elNode)
      {
         foreach (XmlNode subNode in elNode.ChildNodes)
         {
            if (subNode.Name == "Symbol")
            {
               this.Symbol = subNode.Attributes[0].Value;
            }
         }
      }
   }

   #region Disabled Anonymous Function Node
   //[RequiresTransaction(false)]
   //[IsInteractive(false)]
   //public class dynAnonFunction : dynElement
   //{
   //   private INode entryPoint;

   //   public dynAnonFunction(IEnumerable<string> inputs, string output, INode entryPoint)
   //   {
   //      int i = 1;
   //      foreach (string input in inputs)
   //      {
   //         InPortData.Add(new PortData(null, input, "Input #" + i++, typeof(object)));
   //      }

   //      OutPortData = new PortData(null, output, "function output", typeof(object));

   //      this.entryPoint = entryPoint;

   //      base.RegisterInputsAndOutputs();
   //   }

   //   protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
   //   {
   //      return new AnonymousFunctionNode(portNames, this.entryPoint);
   //   }
   //}
   #endregion
}
