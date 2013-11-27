using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    public interface WatchHandler
    {
        bool AcceptsValue(object o);

        void ProcessNode(object value, WatchNode node);
    }

    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Visualize the output of node. ")]
    [NodeSearchTags("print", "output", "display")]
    public partial class Watch: NodeWithOneOutput
    {

        public WatchTree watchTree;
        //private WatchTreeBranch watchTreeBranch;

        private WatchNode _root;
        public WatchNode Root
        {
            get { return _root; }
            set 
            { 
                _root = value;
                RaisePropertyChanged("Root");
            }
        }

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

        public event EventHandler RequestBindingUnhook;
        protected virtual void OnRequestBindingUnhook(EventArgs e)
        {
            if (RequestBindingUnhook != null)
                RequestBindingUnhook(this, e);
        }

        public event EventHandler RequestBindingRehook;
        protected virtual void OnRequestBindingRehook(EventArgs e)
        {
            if (RequestBindingRehook != null)
                RequestBindingRehook(this, e);
        }

        public static void AddWatchHandler(WatchHandler h)
        {
            handlerManager.handlers.Add(h);
        }

        public static void RemoveWatchHandler(WatchHandler h)
        {
            handlerManager.handlers.Remove(h);
        }

        public Watch()
        {
            InPortData.Add(new PortData("", "Node to evaluate.", typeof(object)));
            OutPortData.Add(new PortData("", "Watch contents.", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            foreach (PortModel p in InPorts)
            {
                p.PortDisconnected += new PortConnectedHandler(p_PortDisconnected);
            }
        }

        void p_PortDisconnected(object sender, EventArgs e)
        {
            if(Root != null)
                Root.Children.Clear();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string prefix = "";

            int count = 0;

            DispatchOnUIThread(
                delegate
                {
                    //unhook the binding
                    OnRequestBindingUnhook(EventArgs.Empty);

                    Root.Children.Clear();

                    foreach (Value e in args)
                    {
                        Root.Children.Add(Process(e, prefix, count));
                        count++;
                    }

                    //rehook the binding
                    OnRequestBindingRehook(EventArgs.Empty);
                }
            );

            //return the content that has been gathered
            return args[0]; //watch should be a 'pass through' node
        }

        public void ShowClickedElementInView()
        {

        }

        WatchNode Process(Value eIn, string prefix, int count, bool isListMember = false)
        {
            //content += prefix + string.Format("[{0}]:", count.ToString());

            WatchNode node = null;
            
            if (eIn == null || eIn.IsDummy)
            {
                node = new WatchNode("null");
                return node;
            }

            if (eIn.IsContainer)
            {
                if ((eIn as Value.Container).Item != null)
                {
                    //content += (eIn as Value.Container).Item.ToString();

                    node = new WatchNode((eIn as Value.Container).Item.ToString(), isListMember, count);

                    handlerManager.ProcessNode((eIn as Value.Container).Item, node);
                    
                    //node.Link = id;
                }
            }
            else if (eIn.IsFunction)
            {
                //content += eIn.ToString() + "\n";
                node = new WatchNode("<function>", isListMember, count);
            }
            else if (eIn.IsList)
            {
                //content += "List\n";

                string newPrefix = prefix + "\t";

                var list = (eIn as Value.List).Item;

                node = new WatchNode(list.IsEmpty ? "Empty List" : "List", isListMember, count);

                foreach (var e in list.Select((x, i) => new { Element = x, Index = i }))
                {
                    node.Children.Add( Process(e.Element, newPrefix, e.Index, true) );
                }
            }
            else if (eIn.IsNumber)
            {
                //content += (eIn as Value.Number).Item.ToString() + "\n";
                node = new WatchNode((eIn as Value.Number).Item.ToString(), isListMember, count);
            }
            else if (eIn.IsString)
            {
                //content += (eIn as Value.String).Item.ToString() + "\n";
                node = new WatchNode((eIn as Value.String).Item.ToString(), isListMember, count);
            }
            else if (eIn.IsSymbol)
            {
                //content += (eIn as Value.Symbol).Item.ToString() + "\n";
                node = new WatchNode((eIn as Value.Symbol).Item.ToString(), isListMember, count);
            }

            // This is a fix for the following defect. "VirtualizingStackPanel" 
            // does not quite work well with "WatchNode" being 'null' value.
            // 
            //      https://github.com/ikeough/Dynamo/issues/832
            // 
            if (null == node)
                node = new WatchNode("null");

            return node;
        }
    }

}
