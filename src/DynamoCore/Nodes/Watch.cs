using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
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
        void ProcessNode(object value, WatchViewModel node, bool showRawData);
    }

    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Visualize the output of node. ")]
    [NodeSearchTags("print", "output", "display")]
    [IsDesignScriptCompatible]
    public partial class Watch : NodeWithOneOutput
    {
        #region private members

        private WatchTree _watchTree;

        private WatchViewModel _root;

        #endregion

        #region public properties

        /// <summary>
        /// The root node of the watch's tree.
        /// </summary>
        public WatchViewModel Root
        {
            get { return _root; }
            set
            {
                _root = value;
                RaisePropertyChanged("Root");
            }
        }

        #endregion

        private const string nullString = "null";

        #region events

        /// <summary>
        /// This event is handled by the UI and allows for 
        /// rapid regeneration of Watch content.
        /// </summary>
        public event EventHandler RequestBindingUnhook;
        
        /// <summary>
        /// After the Watch content has been regenerated, this 
        /// event is triggered to reestablish the bindings.
        /// </summary>
        public event EventHandler RequestBindingRehook;

        #endregion

        public Watch()
        {
            InPortData.Add(new PortData("", "Node to evaluate.", typeof (object)));
            OutPortData.Add(new PortData("", "Watch contents.", typeof (object)));

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

        /// <summary>
        /// Update the watch content from the given MirrorData and returns WatchNode.
        /// </summary>
        /// <param name="data">The Mirror data for which watch content is needed.</param>
        /// <param name="prefix">Prefix string used for formatting the content.</param>
        /// <param name="index">Index of input data if it is a part of a collection.</param>
        /// <param name="isListMember">Specifies if this data belongs to a collection.</param>
        /// <returns>WatchNode</returns>
        public WatchViewModel Process(MirrorData data, string path, bool showRawData = true)
        {
            WatchViewModel node = null;

            if (data == null || data.IsNull)
            {
                node = new WatchViewModel(nullString, path);
            }
            else if (data.IsCollection)
            {
                var list = data.GetElements();

                node = new WatchViewModel(list.Count == 0 ? "Empty List" : "List", path, true);

                foreach (var e in list.Select((x, i) => new { Element = x, Index = i }))
                {
                    node.Children.Add(Process(e.Element, path + ":" + e.Index, true));
                }
            }
            else
            {
                node = dynSettings.Controller.WatchHandler.Process(data as dynamic, path, showRawData);
            }

            return node ?? (new WatchViewModel("null", path));
        }

        /// <summary>
        /// Callback for port disconnection. Handles clearing the watch.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void p_PortDisconnected(object sender, EventArgs e)
        {
            if (Root != null)
                Root.Children.Clear();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //string prefix = "";

            //int count = 0;

            //DispatchOnUIThread(
            //    delegate
            //    {
            //        //unhook the binding
            //        OnRequestBindingUnhook(EventArgs.Empty);

            //        Root.Children.Clear();

            //        foreach (Value e in args)
            //        {
            //            Root.Children.Add(Process(e, count.ToString(CultureInfo.InvariantCulture), Root.ShowRawData));
            //            count++;
            //        }

            //        //rehook the binding
            //        OnRequestBindingRehook(EventArgs.Empty);
            //    }
            //    );

            ////return the content that has been gathered
            //return args[0]; //watch should be a 'pass through' node

            throw new NotImplementedException();
        }

        protected virtual void OnRequestBindingUnhook(EventArgs e)
        {
            if (RequestBindingUnhook != null)
                RequestBindingUnhook(this, e);
        }

        protected virtual void OnRequestBindingRehook(EventArgs e)
        {
            if (RequestBindingRehook != null)
                RequestBindingRehook(this, e);
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
        internal WatchViewModel GetWatchNode()
        {
            if (this.InPorts[0].Connectors.Count == 0)
            {
                return new WatchViewModel(nullString, AstIdentifierForPreview.Name);
            }
            else
            {
                var inputVar = this.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview.Name;

                //Get RuntimeMirror for input ast identifier.
                var mirror = dynSettings.Controller.EngineController.GetMirror(AstIdentifierForPreview.Name);
                if (null == mirror)
                    return new WatchViewModel(nullString, inputVar);

                //Get MirrorData from the RuntimeMirror
                var mirrorData = mirror.GetData();
                return Process(mirrorData, inputVar, false);
            }
        }

        public override void UpdateRenderPackage()
        {
            //do nothing
            //a watch should not draw its outputs
        }

        #endregion
#endif
    }
}
