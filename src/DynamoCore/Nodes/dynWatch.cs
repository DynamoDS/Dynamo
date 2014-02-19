using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using ProtoCore.AST.AssociativeAST;
using System.ComponentModel;
using Dynamo.Utilities;
using ProtoCore.Mirror;

namespace Dynamo.Nodes
{
    public interface WatchHandler
    {
        bool AcceptsValue(object o);
        void ProcessNode(object value, WatchNode node, bool showRawData);
    }

    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Visualize the output of node. ")]
    [NodeSearchTags("print", "output", "display")]
    [IsDesignScriptCompatible]
    public partial class Watch: NodeWithOneOutput
    {

        public WatchTree watchTree;

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

            public void ProcessNode(object value, WatchNode node, bool showRawData)
            {
                foreach (var handler in handlers)   //.Where(x => x.AcceptsValue(value)))
                {
                    handler.ProcessNode(value, node, showRawData);
                }
            }
        }

        static WatchHandlers handlerManager = new WatchHandlers();
        static readonly string nullString = "null";

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
                p.PortDisconnected += p_PortDisconnected;
            }
#if USE_DSENGINE
            this.PropertyChanged += new PropertyChangedEventHandler(NodeValueUpdated);
#endif
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
                        Root.Children.Add(Process(e, prefix, count, Root.ShowRawData));
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

        WatchNode Process(Value eIn, string prefix, int count, bool isListMember = false, bool showRawData = true)
        {
            WatchNode node = null;
            
            if (eIn == null || eIn.IsDummy)
            {
                node = new WatchNode(nullString);
                return node;
            }

            if (eIn.IsContainer)
            {
                var value = (eIn as Value.Container).Item;
                if (value != null)
                {
                    node = new WatchNode(value.ToString(), isListMember, count);
                    handlerManager.ProcessNode(value, node, showRawData);
                }
            }
            else if (eIn.IsFunction)
            {
                node = new WatchNode("<function>", isListMember, count);
            }
            else if (eIn.IsList)
            {
                string newPrefix = prefix + "\t";

                var list = (eIn as Value.List).Item;

                node = new WatchNode(list.IsEmpty ? "Empty List" : "List", isListMember, count);

                foreach (var e in list.Select((x, i) => new { Element = x, Index = i }))
                {
                    node.Children.Add( Process(e.Element, newPrefix, e.Index, true, showRawData) );
                }
            }
            else if (eIn.IsNumber)
            {
                node = new WatchNode((eIn as Value.Number).Item.ToString(), isListMember, count);
            }
            else if (eIn.IsString)
            {
                node = new WatchNode((eIn as Value.String).Item, isListMember, count);
            }
            else if (eIn.IsSymbol)
            {
                node = new WatchNode((eIn as Value.Symbol).Item, isListMember, count);
            }

            // This is a fix for the following defect. "VirtualizingStackPanel" 
            // does not quite work well with "WatchNode" being 'null' value.
            // 
            //      https://github.com/ikeough/Dynamo/issues/832
            // 
            return node ?? (new WatchNode("null"));
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>
            {
                AstFactory.BuildAssignment(AstIdentifierForPreview, inputAstNodes[0])
            };

            return resultAst;
        }

#if USE_DSENGINE

        #region NodeValueUpdated event handler
        void NodeValueUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsUpdated")
                return;

            DispatchOnUIThread(
                delegate
                {
                    //unhook the binding
                    OnRequestBindingUnhook(EventArgs.Empty);

                    Root.Children.Clear();

                    Root.Children.Add(GetWatchNode());

                    //rehook the binding
                    OnRequestBindingRehook(EventArgs.Empty);
                }
            );
        }

        #endregion

        #region Watch Node creation for AST node

        /// <summary>
        /// This method returns a WatchNode for it's preview AST node.
        /// This method gets called on ui thread when "IsUpdated" property
        /// change is notified. This method is responsible for populating the 
        /// watch node with evaluated value of the input. Gets the MirrorData
        /// for the input/preview AST and then processes the mirror data to
        /// render the watch content properly.
        /// </summary>
        /// <returns>WatchNode</returns>
        internal WatchNode GetWatchNode()
        {
            //Get RuntimeMirror for input ast identifier.
            var mirror = dynSettings.Controller.EngineController.GetMirror(AstIdentifierForPreview.Name);
            if(null == mirror)
                return new WatchNode(nullString);

            //Get MirrorData from the RuntimeMirror
            var mirrorData = mirror.GetData();
            return ProcessMirrorData(mirrorData, "", 0, false);
        }


        /// <summary>
        /// Update the watch content from the given MirrorData and returns WatchNode.
        /// </summary>
        /// <param name="data">The Mirror data for which watch content is needed.</param>
        /// <param name="prefix">Prefix string used for formatting the content.</param>
        /// <param name="index">Index of input data if it is a part of a collection.</param>
        /// <param name="isListMember">Specifies if this data belongs to a collection.</param>
        /// <returns>WatchNode</returns>
        WatchNode ProcessMirrorData(MirrorData data, string prefix, int index, bool isListMember)
        {
            //Null data
            if (null == data || data.IsNull)
                return new WatchNode(nullString);

            //If the input data is collection, process each element recursively.
            if (data.IsCollection)
            {
                string newPrefix = prefix + "\t";

                var list = data.GetElements();

                WatchNode node = new WatchNode(list.Count == 0 ? "Empty List" : "List", isListMember, index);

                foreach (var e in list.Select((x, i) => new { Element = x, Index = i }))
                {
                    node.Children.Add(ProcessMirrorData(e.Element, newPrefix, e.Index, true));
                }
                return node;
            }

            //If the input data is an instance of a class, create a watch node
            //with the class name and let WatchHandler process the underlying CLR data
            var classMirror = data.Class;
            if (null != classMirror)
            {
                WatchNode node = new WatchNode(classMirror.ClassName, isListMember, index);

                handlerManager.ProcessNode(data.Data, node, true);
                return node;
            }

            //Finally for all else get the string representation of data as watch content.
            string previewData = data.Data.ToString();
            return new WatchNode(previewData, isListMember, index);
        }

        #endregion
#endif
    }

}
