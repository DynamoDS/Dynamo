using System;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using VMDataBridge;

namespace Dynamo.Engine.Profiling
{
    class NodeProfilingData : NotificationObject, IDisposable
    {
        NodeModel node = null;

        private DateTime? startTime = null;
        private DateTime? endTime = null;

        public NodeProfilingData(NodeModel node)
        {
            this.node = node;
            DataBridge.Instance.RegisterCallback(node.GUID.ToString(), RecordEvaluationState);
        }

        public void Reset()
        {
            startTime = null;
            endTime = null;
        }

        public void Dispose()
        {
            DataBridge.Instance.UnregisterCallback(node.GUID.ToString());
        }

        private void RecordEvaluationState(object data)
        {
            if (!startTime.HasValue)
            {
                startTime = DateTime.Now;
                return;
            }

            endTime = DateTime.Now;
        }

        public TimeSpan? ExecutionTime
        {
            get
            {
                if (!HasPerformanceData())
                    return null;

                return endTime - startTime;
            }
        }

        public bool HasPerformanceData()
        {
            return startTime.HasValue && endTime.HasValue;
        }
    }
}
