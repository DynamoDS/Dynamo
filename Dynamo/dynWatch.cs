//Copyright 2012 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

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
using System.ComponentModel;

namespace Dynamo.Elements
{
    [ElementName("Watch")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Visualize the output of node.")]
    [RequiresTransaction(false)]
    class dynWatch : dynElement
    {
        //System.Windows.Controls.TextBlock watchBlock;
        WatchTree wt;
        WatchTreeBranch wtb;

        public dynWatch()
        {
            InPortData.Add(new PortData("", "Node to evaluate.", typeof(object)));
            OutPortData = new PortData("", "Watch contents.", typeof(string));
            base.RegisterInputsAndOutputs();

            //take out the left and right margins
            //and make this so it's not so wide
            this.inputGrid.Margin = new Thickness(10, 5, 10, 5);
            this.topControl.Width = 500;
            this.topControl.Height = 300;

            wt = new WatchTree();
            this.inputGrid.Children.Add(wt);
            wtb = wt.FindResource("Tree") as WatchTreeBranch;

        }

        /*
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
         * */

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string content = "";
            string prefix = "";

            int count = 0;

            this.Dispatcher.Invoke(new Action(
                delegate
                {
                    wtb.Clear();

                    foreach (Expression e in args)
                    {
                        wtb.Add(Process(e, ref content, prefix, count));
                        count++;
                    }
                }
            ));

            //return the content that has been gathered
            return Expression.NewString(content);
        }

        WatchNode Process(Expression eIn, ref string content, string prefix, int count)
        {
            content += prefix + string.Format("[{0}]:", count.ToString());

            WatchNode node = null;
            
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

                node = new WatchNode((eIn as Expression.Container).Item.ToString());
                node.Link = id;

            }
            else if (eIn.IsFunction || eIn.IsSpecial)
            {
                content += eIn.ToString() + "\n";
                node = new WatchNode(eIn.ToString());

            }
            else if (eIn.IsList)
            {
                content += eIn.GetType().ToString() + "\n";

                string newPrefix = prefix + "\t";
                int innerCount = 0;

                node = new WatchNode(eIn.GetType().ToString());

                foreach(Expression eIn2 in (eIn as Expression.List).Item)
                {
                    node.Children.Add(Process(eIn2, ref content, newPrefix, innerCount));
                    innerCount++;
                }
            }
            else if (eIn.IsNumber)
            {
                content += (eIn as Expression.Number).Item.ToString() + "\n";
                node = new WatchNode((eIn as Expression.Number).Item.ToString());
            }
            else if (eIn.IsString)
            {
                content += (eIn as Expression.String).Item.ToString() + "\n";
                node = new WatchNode((eIn as Expression.String).Item.ToString());
            }
            else if (eIn.IsSymbol)
            {
                content += (eIn as Expression.Symbol).Item.ToString() + "\n";
                node = new WatchNode((eIn as Expression.Symbol).Item.ToString());
            }

            return node;
        }
    }

}
