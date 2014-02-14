using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ProtoCore.AssociativeGraph;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoFFI
{
    public class DSPropertyChangedEventArgs : EventArgs
    {
        public DSPropertyChangedEventArgs(String dsVariable, String propertyName, object value)
        {
            this.Variable = dsVariable;
            this.PropertyName = PropertyName;
            this.Value = value;
        }

        public String Variable { get; set; }
        public String PropertyName { get; set; }
        public Object Value { get; set; }
    }

    public class FFIPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public FFIPropertyChangedEventArgs(string propertyName, GraphNode graphNode):
            base(propertyName)
        {
            hostGraphNode = graphNode;
        }

        public GraphNode hostGraphNode { get; set; }
    }

    public delegate void DSPropertyChangedHandler(DSPropertyChangedEventArgs arg);
    public delegate void FFIPropertyChangedHandler(FFIPropertyChangedEventArgs arg);

    public class FFIPropertyChangedMonitor
    {
        public FFIPropertyChangedMonitor(ProtoCore.Core core)
        {
            mHostCore = core;
        }

        #region Implement property changed event from FFI to DS

        /// <summary>
        /// Start monitoring ffiObject's property changed event. 
        /// </summary>
        /// <param name="ffiObject"></param>
        public void AddFFIObject(object ffiObject)
        {
            Validity.Assert(ffiObject != null);
            if (ffiObject == null)
            {
                return;
            }

            INotifyPropertyChanged ipcObject = ffiObject as INotifyPropertyChanged;
            if (ipcObject != null)
            {
                GraphNode executingGraphNode = mHostCore.GetExecutingGraphNode();
                if (executingGraphNode != null)
                {
                    mFFIObjectHostGraphNode.Add(ipcObject, executingGraphNode);
                    ipcObject.PropertyChanged += this.FFIObjectPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Stop monitoring ffiObject's property changed event. 
        /// </summary>
        /// <param name="ffiObject"></param>
        public void RemoveFFIObject(Object ffiObject)
        {
            Validity.Assert(ffiObject != null);
            if (ffiObject == null)
            {
                return;
            }

            INotifyPropertyChanged ipcObject = ffiObject as INotifyPropertyChanged;
            if (ipcObject != null)
            {
                GraphNode graphNode = null;
                if (mFFIObjectHostGraphNode.TryGetValue(ipcObject, out graphNode))
                {
                    graphNode.propertyChanged = false;
                }
                mFFIObjectHostGraphNode.Remove(ipcObject);
                ipcObject.PropertyChanged -= this.FFIObjectPropertyChanged;
            }
        }

        public event FFIPropertyChangedHandler FFIPropertyChangedEventHandler;

        /// <summary>
        /// Property changed event from a ffi object.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void FFIObjectPropertyChanged(Object sender, PropertyChangedEventArgs args)
        {
            string propertyName = args.PropertyName;
            if (String.IsNullOrEmpty(propertyName))
            {
                return;
            }

            // Ignore all property changed event when the update engine is
            // re-executing graph nodes because of previous property changed
            // event
            if (mHostCore.IsEvalutingPropertyChanged())
            {
                return;
            }

            GraphNode graphNode = null;
            if (mFFIObjectHostGraphNode.TryGetValue(sender, out graphNode))
            {
                graphNode.propertyChanged = true;
                if (FFIPropertyChangedEventHandler != null)
                {
                    FFIPropertyChangedEventArgs newargs = new FFIPropertyChangedEventArgs(propertyName, graphNode);
                    FFIPropertyChangedEventHandler(newargs);
                }
            }
        }

        private Dictionary<Object, GraphNode> mFFIObjectHostGraphNode = new Dictionary<object, GraphNode>();
        private ProtoCore.Core mHostCore;
        #endregion

        #region Implement property changed event from DS to FFI

        public void DSObjectPropertyChanged(ProtoCore.DSASM.Executive exe, int thisptr, String propertyName, object value)
        {
            /*
            Validity.Assert(thisptr != Constants.kInvalidIndex);

            // It is a naive implementation, need to optimize it later on. 
            // At runtime we don't have mapping between a DS pointer and a 
            // symbol name, so we have to go through registered symbols to 
            // find out if some symbol's value is equal to thisptr. 
            //
            // The other reason to do this is there is possible that two vars
            // reference to the same object, so if the property of one of them
            // is modified, the other one should be notified. 
            foreach (var varPropertyChangedHandler in mSubscribers)
            {
                string symbolName = varPropertyChangedHandler.Key;
                SymbolNode symbolNode = exe.GetGlobalSymbolNode(symbolName);
                if (symbolNode == null)
                {
                    continue;
                }

                StackValue dsValue = exe.Core.Rmem.GetAtRelative(symbolNode);
                if (dsValue.optype != AddressType.Pointer && dsValue.opdata != thisptr)
                {
                    continue;
                }

                var propertyChangedHandler = varPropertyChangedHandler.Value;
                DSPropertyChangedHandler handler = null;
                if (propertyChangedHandler.TryGetValue(propertyName, out handler))
                {
                    if (handler != null)
                    {
                        DSPropertyChangedEventArgs args = new DSPropertyChangedEventArgs(symbolName, propertyName, value);
                        handler(args);
                    }
                }
            }
            */
        }

        public void RegisterDSPropertyChangedHandler(String variable, String property, DSPropertyChangedHandler handler)
        {
            /*
            Validity.Assert(!String.IsNullOrEmpty(variable));
            Validity.Assert(!String.IsNullOrEmpty(property));

            Dictionary<String, DSPropertyChangedHandler> propertyChangedHandler = null;
            if (!mSubscribers.TryGetValue(variable, out propertyChangedHandler))
            {
                propertyChangedHandler = new Dictionary<string, DSPropertyChangedHandler>();
                mSubscribers[variable] = propertyChangedHandler; 
            }

            DSPropertyChangedHandler existingHandler = null;
            if (!propertyChangedHandler.TryGetValue(property, out existingHandler))
            {
                existingHandler = handler;
                propertyChangedHandler[property] = existingHandler;
            }
            else
            {
                existingHandler += handler;
            }
            */
        }

        private Dictionary<String, Dictionary<String, DSPropertyChangedHandler>> mSubscribers = new Dictionary<String,Dictionary<String, DSPropertyChangedHandler>>();
        #endregion
    }
}