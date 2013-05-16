//Copyright 2013 Ian Keough

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
using System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Autodesk.Revit.DB;
using Dynamo.Connectors;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.Controls;

namespace Dynamo.Nodes
{
    public interface WatchHandler
    {
        bool AcceptsValue(object o);

        void ProcessNode(object value, WatchNode node);
    }

    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Visualize the output of node.")]
    [NodeSearchTags("print", "output", "display")]
    public class dynWatch: dynNodeWithOneOutput
    {
        public WatchTree watchTree;
        public WatchTreeBranch watchTreeBranch;

        private class WatchHandlers
        {
            public HashSet<WatchHandler> handlers
            {
                get;
                private set;
            }

            public WatchHandlers()
            {
                handlers = new HashSet<WatchHandler>();
            }

            public void ProcessNode(object value, WatchNode node)
            {
                foreach (var handler in handlers.Where(x => x.AcceptsValue(value)))
                {
                    handler.ProcessNode(value, node);
                }
            }
        }

        static WatchHandlers handlerManager = new WatchHandlers();

        public static void AddWatchHandler(WatchHandler h)
        {
            handlerManager.handlers.Add(h);
        }

        public static void RemoveWatchHandler(WatchHandler h)
        {
            handlerManager.handlers.Remove(h);
        }

        public dynWatch()
        {
            InPortData.Add(new PortData("", "Node to evaluate.", typeof(object)));
            OutPortData.Add(new PortData("", "Watch contents.", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            foreach (dynPortModel p in InPorts)
            {
                p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);
            }
        }

        public override void SetupCustomUIElements(dynNodeView NodeUI)
        {
            watchTree = new WatchTree();

            NodeUI.inputGrid.Children.Add(watchTree);

            watchTreeBranch = watchTree.FindResource("Tree") as WatchTreeBranch;
        }

        void p_PortDisconnected(object sender, EventArgs e)
        {
            watchTreeBranch.Clear();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string content = "";
            string prefix = "";

            int count = 0;

            DispatchOnUIThread(
                delegate
                {
                    watchTreeBranch.Clear();

                    foreach (Value e in args)
                    {
                        watchTreeBranch.Add(Process(e, ref content, prefix, count));
                        count++;
                    }
                }
            );

            //return the content that has been gathered
            return args[0]; //watch should be a 'pass through' node
        }

        public void ShowClickedElementInView()
        {

        }

        WatchNode Process(Value eIn, ref string content, string prefix, int count)
        {
            content += prefix + string.Format("[{0}]:", count.ToString());

            WatchNode node = null;
            
            if (eIn.IsContainer)
            {
                if ((eIn as Value.Container).Item != null)
                {
                    //TODO: make clickable hyperlinks to show the element in Revit
                    //http://stackoverflow.com/questions/7890159/programmatically-make-textblock-with-hyperlink-in-between-text

                    content += (eIn as Value.Container).Item.ToString();

                    node = new WatchNode((eIn as Value.Container).Item.ToString());

                    handlerManager.ProcessNode((eIn as Value.Container).Item, node);
                    
                    //node.Link = id;
                }
            }
            else if (eIn.IsFunction)
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

                foreach(Value eIn2 in (eIn as Value.List).Item)
                {
                    node.Children.Add(Process(eIn2, ref content, newPrefix, innerCount));
                    innerCount++;
                }
            }
            else if (eIn.IsNumber)
            {
                content += (eIn as Value.Number).Item.ToString() + "\n";
                node = new WatchNode((eIn as Value.Number).Item.ToString());
            }
            else if (eIn.IsString)
            {
                content += (eIn as Value.String).Item.ToString() + "\n";
                node = new WatchNode((eIn as Value.String).Item.ToString());
            }
            else if (eIn.IsSymbol)
            {
                content += (eIn as Value.Symbol).Item.ToString() + "\n";
                node = new WatchNode((eIn as Value.Symbol).Item.ToString());
            }

            return node;
        }
    }

}
