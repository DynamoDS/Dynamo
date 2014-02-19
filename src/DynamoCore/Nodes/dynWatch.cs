using System;
using System.Globalization;
using System.Linq;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Units;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    public interface IWatchHandler
    {
        WatchNode Process(dynamic value, string tag, bool showRawData = true);
    }

    /// <summary>
    /// The default watch handler.
    /// </summary>
    public class DefaultWatchHandler : IWatchHandler
    {
        internal WatchNode ProcessThing(object value, string tag, bool showRawData = true)
        {
            var node = new WatchNode(value.ToString(), tag);
            return node;
        }

        internal WatchNode ProcessThing(SIUnit unit, string tag, bool showRawData = true)
        {
            if (showRawData)
                return new WatchNode(unit.Value.ToString(CultureInfo.InvariantCulture), tag);

            return new WatchNode(unit.ToString(), tag);
        }

        internal WatchNode ProcessThing(double value, string tag, bool showRawData = true)
        {
            return new WatchNode(value.ToString("0.000"), tag);
        }

        internal WatchNode ProcessThing(string value, string tag, bool showRawData = true)
        {
            return new WatchNode(value, tag);
        }

        public WatchNode Process(dynamic value, string tag, bool showRawData = true)
        {
            return ProcessThing(value, tag, showRawData);
        }
    }

    [NodeName("Watch")]
    [NodeCategory(BuiltinNodeCategories.CORE_VIEW)]
    [NodeDescription("Visualize the output of node. ")]
    [NodeSearchTags("print", "output", "display")]
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

        public WatchNode Process(Value value, string tag, bool showRawData = true)
        {
            WatchNode node;

            if (value == null || value.IsDummy)
            {
                node = new WatchNode("null");
            }
            else if (value.IsFunction)
            {
                node = new WatchNode("<function>");
            }
            else if (value.IsList)
            {
                var list = ((Value.List)value).Item;
                node = new WatchNode(list.IsEmpty ? "Empty List" : "List");

                foreach (var e in list.Select((x, i) => new { Element = x, Index = i }))
                {
                    node.Children.Add(Process(e.Element, "[" + e.Index + "]", showRawData));
                }
            }
            else
            {
                node = dynSettings.Controller.WatchHandler.Process(value.ToDynamic(), tag, showRawData);
            }

            return node ?? (new WatchNode("null"));
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

        public void ShowClickedElementInView()
        {

        }
 
    }

}
