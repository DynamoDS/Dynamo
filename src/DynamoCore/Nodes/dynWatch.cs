using System;
using System.Globalization;
using System.Linq;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Visualize the output of node. ")]
    [NodeSearchTags("print", "output", "display")]
    public partial class Watch : NodeWithOneOutput
    {
        #region private members

        private WatchTree _watchTree;

        private WatchItem _root;

        #endregion

        #region public properties

        /// <summary>
        /// The root node of the watch's tree.
        /// </summary>
        public WatchItem Root
        {
            get { return _root; }
            set
            {
                _root = value;
                RaisePropertyChanged("Root");
            }
        }

        #endregion

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
        }

        /// <summary>
        /// Called during Evaluation, this method handles the 
        /// conversion of an FScheme.Value object into a watchnode. 
        /// This process uses the IWatchHandler registered on
        /// the controller to dynamically dispatch watch node 
        /// processing based on the unboxed Value's object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tag"></param>
        /// <param name="showRawData"></param>
        /// <returns></returns>
        public WatchItem Process(Value value, string tag, bool showRawData = true)
        {
            WatchItem node;

            if (value == null || value.IsDummy)
            {
                node = new WatchItem("null");
            }
            else if (value.IsFunction)
            {
                node = new WatchItem("<function>");
            }
            else if (value.IsList)
            {
                var list = ((Value.List) value).Item;
                node = new WatchItem(list.IsEmpty ? "Empty List" : string.Format("[{0}] List", tag));

                foreach (var e in list.Select((x, i) => new {Element = x, Index = i}))
                {
                    node.Children.Add(Process(e.Element, e.Index.ToString(CultureInfo.InvariantCulture), showRawData));
                }
            }
            else
            {
                node = dynSettings.Controller.WatchHandler.Process(value.ToDynamic(), tag, showRawData);
            }

            return node ?? (new WatchItem("null"));
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
                        Root.Children.Add(Process(e, count.ToString(CultureInfo.InvariantCulture), Root.ShowRawData));
                        count++;
                    }

                    //rehook the binding
                    OnRequestBindingRehook(EventArgs.Empty);
                }
                );

            //return the content that has been gathered
            return args[0]; //watch should be a 'pass through' node
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
    }
}
