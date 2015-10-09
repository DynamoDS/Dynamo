using System;
using System.Collections.Generic;
using System.ComponentModel;
using ProtoCore.AssociativeGraph;
using ProtoCore.Utils;

namespace ProtoFFI
{
    public class FFIPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public FFIPropertyChangedEventArgs(string propertyName, GraphNode graphNode):
            base(propertyName)
        {
            hostGraphNode = graphNode;
        }

        public GraphNode hostGraphNode { get; set; }
    }

    public delegate void FFIPropertyChangedHandler(FFIPropertyChangedEventArgs arg);

    public class FFIPropertyChangedMonitor
    {
        public FFIPropertyChangedMonitor(ProtoCore.RuntimeCore runtimeCore)
        {
            mHostCore = runtimeCore;
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
                GraphNode executingGraphNode = mHostCore.DSExecutable.ExecutingGraphnode;
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
        private ProtoCore.RuntimeCore mHostCore;
        #endregion
    }
}