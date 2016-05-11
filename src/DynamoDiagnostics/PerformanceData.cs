using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using VMDataBridge;

namespace Dynamo.Diagnostics
{
    public struct PerformanceData
    {
        public int ExecutionTime;
        public int InputSize;
        public int OutputSize;
    }

    class NodeData : NotificationObject, IDisposable
    {
        private TimeSpan? executionTime;
        private DateTime? executionStartTime;
        private StringWriter traceWriter;
        private TraceListener traceListener;
        private string traceData = string.Empty;
        public NodeData(NodeModel node)
        {
            NodeId = node.GUID.ToString();
            Node = node;
            //Ensure that callback is registered with DataBridge
            DataBridge.Instance.RegisterCallback(NodeId, RecordEvaluationState);
            Node.PropertyChanged += OnNodePropertyChanged;
        }

        void OnNodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Position")
                RaisePropertyChanged("Node");
        }

        public void Reset()
        {
            executionStartTime = null;
            executionTime = null;
            traceData = string.Empty;
        }

        public PerformanceData GetPerformanceData()
        {
            return new PerformanceData { ExecutionTime = ExecutionTime, InputSize = InputDataSize, OutputSize = OutputDataSize };
        }

        public string TraceData { get { return traceData; } }

        public void Dispose()
        {
            Node = null;
            DataBridge.Instance.UnregisterCallback(NodeId);
            UnRegisterTraceListener();
        }

        private void RecordEvaluationState(object data)
        {
            var size = Count(data);
            if (!executionStartTime.HasValue)
            {
                RegisterTraceListener();

                executionStartTime = DateTime.Now;
                executionTime = null;
                InputDataSize = size;
                RaisePropertyChanged("InputDataSize");
            }
            else
            {
                executionTime = DateTime.Now.Subtract(executionStartTime.Value);
                executionStartTime = null;
                OutputDataSize = size;
                
                UnRegisterTraceListener();

                RaisePropertyChanged("OutputDataSize");
            }
            RaisePropertyChanged("IsEvaluating");
            RaisePropertyChanged("ExecutionTime");
        }

        private void UnRegisterTraceListener()
        {
            if (traceListener == null)
                return;
        
            Trace.Flush();
            traceData = traceWriter.ToString();
            
            Trace.Listeners.Remove(traceListener);
            
            traceListener.Dispose();
            traceWriter.Dispose();
            
            traceListener = null;
            traceWriter = null;
        }

        private void RegisterTraceListener()
        {
            traceData = string.Empty;
            traceWriter = new StringWriter();
            traceListener = new TextWriterTraceListener(traceWriter);
            Trace.Listeners.Add(traceListener);
        }

        private int Count(object data)
        {
            var collection = data as IEnumerable;
            if (collection == null) return 1;

            var count = 0;
            foreach (var item in collection)
            {
                count += Count(item);
            }

            return count;
        }

        public bool IsEvaluating { get { return executionStartTime.HasValue; } }

        public int ExecutionTime
        {
            get
            {
                return executionTime.HasValue ? (int)executionTime.Value.TotalMilliseconds : -1;
            }
        }

        public string NodeId { get; private set; }

        public int InputDataSize { get; private set; }

        public int OutputDataSize { get; private set; }

        public bool HasPerformanceData()
        {
            return executionTime.HasValue;
        }

        public NodeModel Node { get; private set; }

        public IEnumerable<PerformanceData> Statistics { get { return DiagnosticsExtension.NodePerformance.GetNodePerformance(Node); } }
    }
}
