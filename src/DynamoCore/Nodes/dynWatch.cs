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

        void ProcessNode(object value, WatchNode node);
    }

    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Visualize the output of node. ")]
    [NodeSearchTags("print", "output", "display")]
    [IsDesignScriptCompatible]
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
                node = new WatchNode(nullString);
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

                    Root.Children.Add(ProcessAstNode(AstIdentifierForPreview, "", 0));

                    //rehook the binding
                    OnRequestBindingRehook(EventArgs.Empty);
                }
            );
        }

        #endregion

        #region Watch Node creation for AST node

        WatchNode ProcessAstNode(AssociativeNode ast, string prefix, int count, bool isListMember = false)
        {
            //content += prefix + string.Format("[{0}]:", count.ToString());
            string varName = GraphToDSCompiler.GraphUtilities.ASTListToCode(
                new List<ProtoCore.AST.AssociativeAST.AssociativeNode> { ast });

            var mirror = dynSettings.Controller.EngineController.GetMirror(varName);
            if(null == mirror)
                return new WatchNode(nullString);

            var mirrorData = mirror.GetData();
            return ProcessMirrorData(mirrorData, prefix, count, isListMember);
        }

        WatchNode ProcessMirrorData(MirrorData data, string prefix, int count, bool isListMember)
        {
            if (null == data || data.IsNull)
                return new WatchNode(nullString);
            if (data.IsCollection)
            {
                string newPrefix = prefix + "\t";

                var list = data.GetElements();

                WatchNode node = new WatchNode(list.Count == 0 ? "Empty List" : "List", isListMember, count);

                foreach (var e in list.Select((x, i) => new { Element = x, Index = i }))
                {
                    node.Children.Add(ProcessMirrorData(e.Element, newPrefix, e.Index, true));
                }
                return node;
            }

            var classMirror = data.Class;
            if (null != classMirror)
            {
                WatchNode node = new WatchNode(classMirror.ClassName, isListMember, count);

                handlerManager.ProcessNode(data.Data, node);
                return node;
            }

            string previewData = data.Data.ToString();
            return new WatchNode(previewData, isListMember, count);
        }

        #endregion
#endif
    }

}
