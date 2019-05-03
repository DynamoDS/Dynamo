using System;
using Dynamo.Graph.Nodes;
using VMDataBridge;

namespace Dynamo.Engine.Profiling
{
    class NodeProfilingData : IDisposable
    {
        NodeModel node = null;

        private DateTime? startTime = null;
        private DateTime? endTime = null;

        internal NodeProfilingData(NodeModel node)
        {
            this.node = node;
            DataBridge.Instance.RegisterCallback(node.GUID.ToString(), RecordEvaluationState);
        }

        internal void Reset()
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
                if (node.HasNodeExecutedEvent)
                {
                    node.OnNodeExecuted(NodeExecutedType.Start, data);
                }
                return;
            }

            if (node.HasNodeExecutedEvent)
            {
                node.OnNodeExecuted(NodeExecutedType.End, data);
            }
            endTime = DateTime.Now;
        }

        internal TimeSpan? ExecutionTime
        {
            get
            {
                if (!HasPerformanceData())
                    return null;

                return endTime - startTime;
            }
        }

        private bool HasPerformanceData()
        {
            return startTime.HasValue && endTime.HasValue;
        }
    }
}
