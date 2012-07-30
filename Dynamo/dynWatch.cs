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
using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using TextBox = System.Windows.Controls.TextBox;
using Dynamo.Controls;
using System.Windows.Documents;

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

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string content = "";
            string prefix = "";

            int count = 0;
            foreach (Expression e in args)
            {
                Process(e, ref content, prefix, count);
                count++;
            }

            watchBlock.Dispatcher.Invoke(new Action(
            delegate
            {
                watchBlock.Text = content;
            }
            ));

            return Expression.NewString(content);
        }

        void Process(Expression eIn, ref string content, string prefix, int count)
        {
            content += prefix + string.Format("[{0}]:", count.ToString());

            if (eIn.IsContainer)
            {
                //TODO: make clickable hyperlinks to show the element in Revit
                //http://stackoverflow.com/questions/7890159/programmatically-make-textblock-with-hyperlink-in-between-text

                string id = "";
                Element revitEl = (eIn as Expression.Container).Item as Autodesk.Revit.DB.Element;
                if (revitEl != null)
                {
                    id = revitEl.Id.ToString();
                }
                content += (eIn as Expression.Container).Item.ToString() + ":" + id + "\n";
            }
            else if (eIn.IsFunction)
            {
                content += (eIn as Expression.Function).Item.ToString() + "\n";
            }
            else if (eIn.IsList)
            {
                content += eIn.GetType().ToString() + "\n";

                string newPrefix = prefix + "\t";
                int innerCount = 0;

                foreach(Expression eIn2 in (eIn as Expression.List).Item)
                {
                    Process(eIn2, ref content, newPrefix, innerCount);
                    innerCount++;
                }
            }
            else if (eIn.IsNumber)
            {
                content += (eIn as Expression.Number).Item.ToString() + "\n";
            }
            else if (eIn.IsString)
            {
                content += (eIn as Expression.String).Item.ToString() + "\n";
            }
            else if (eIn.IsSymbol)
            {
                content += (eIn as Expression.Symbol).Item.ToString() + "\n";
            }
        }
    }
}
