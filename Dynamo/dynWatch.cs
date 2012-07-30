using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using TextBox = System.Windows.Controls.TextBox;
using Dynamo.Controls;

namespace Dynamo.Elements
{
    [ElementName("Watch")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Visualize the output of node.")]
    [RequiresTransaction(false)]
    class dynWatch:dynElement
    {
        System.Windows.Controls.TextBlock watchBlock;

        public dynWatch()
        {
            InPortData.Add(new PortData("", "Node to evaluate.", typeof(object)));
            OutPortData = new PortData("", "Watch contents.", typeof(string));
            base.RegisterInputsAndOutputs();

            //take out the left and right margins
            //and make this so it's not so wide
            this.inputGrid.Margin = new Thickness(10, 5, 10, 5);
            this.topControl.Width = 300;
            this.topControl.Height = 300;

            //TODO:
            //add a text box to the grid
            ScrollViewer sv = new ScrollViewer();
            sv.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            sv.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

            watchBlock = new System.Windows.Controls.TextBlock();
            watchBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            watchBlock.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            sv.Content = watchBlock;

            this.inputGrid.Children.Add(sv);
 
        }

        private void generateContent(Expression eIn, System.Text.StringBuilder sb, string linePrefix="")
        {
           sb.Append(linePrefix);
           if (eIn.IsContainer)
           {
              sb.AppendLine((eIn as Expression.Container).Item.ToString());
           }
           //else if (eIn.IsFunction)
           //{
           //   sb.AppendLine((eIn as Expression.Function).Item.ToString());
           //}
           else if (eIn.IsList)
           {
              sb.AppendLine("List:");
              foreach (Expression subExp in (eIn as Expression.List).Item)
                 this.generateContent(subExp, sb, linePrefix + "    ");
           }
           else if (eIn.IsNumber)
           {
              sb.AppendLine((eIn as Expression.Number).Item.ToString());
           }
           else if (eIn.IsString)
           {
              sb.AppendLine((eIn as Expression.String).Item.ToString());
           }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (Expression e in args)
            {
               this.generateContent(e, sb);
            }
            string content = sb.ToString();

            watchBlock.Dispatcher.Invoke(new Action(
            delegate
            {
                watchBlock.Text = content;
            }
            ));

            return Expression.NewString(content);
        }
    }
}
