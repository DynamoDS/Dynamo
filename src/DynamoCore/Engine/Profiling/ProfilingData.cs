using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;

namespace Dynamo.Engine.Profiling
{
    internal class ProfilingData : IProfilingExecutionTimeData
    {
        private DateTime? startTime = null;
        private DateTime? endTime = null;
        private Dictionary<Guid, NodeProfilingData> nodeProfilingData = new Dictionary<Guid, NodeProfilingData>();

        internal DateTime? StartTime
        {
            get
            {
                return startTime;
            }

            set
            {
                if (startTime == null)
                {
                    startTime = new DateTime();
                }

                startTime = value;
            }
        }

        internal DateTime? EndTime
        {
            get
            {
                return endTime;
            }

            set
            {
                if (endTime == null)
                {
                    endTime = new DateTime();
                }

                endTime = value;
            }
        }

        public TimeSpan? TotalExecutionTime
        {
            get
            {
                if (StartTime == null || EndTime == null)
                    return null;

                return EndTime - StartTime;
            }
        }

        internal Dictionary<Guid, NodeProfilingData> NodeProfilingData
        {
            get { return nodeProfilingData; }
        }

        public TimeSpan? NodeExecutionTime(NodeModel node)
        {
            NodeProfilingData nodeData = null;
            if (nodeProfilingData.TryGetValue(node.GUID, out nodeData))
            {
                return nodeData.ExecutionTime;
            }

            return null;
        }

        internal void RegisterNode(NodeModel node)
        {
            NodeProfilingData nodeData = null;
            if (nodeProfilingData.TryGetValue(node.GUID, out nodeData))
            {
                nodeData.Reset();
                return;
            }

            nodeProfilingData.Add(node.GUID, new NodeProfilingData(node));
        }

        internal void UnregisterNode(Guid guid)
        {
            NodeProfilingData data = null;
            if(nodeProfilingData.TryGetValue(guid, out data))
            {
                nodeProfilingData.Remove(guid);
                data.Dispose();
            }
          
        }

        internal void UnregisterDeletedNodes(IEnumerable<NodeModel> modelNodes)
        {
            var remainingNodes = new Dictionary<Guid, NodeProfilingData>();
            foreach (NodeModel node in modelNodes)
            {
                NodeProfilingData data = null;
                if (nodeProfilingData.TryGetValue(node.GUID, out data))
                {
                    remainingNodes.Add(node.GUID, data);
                    continue;
                }

                UnregisterNode(node.GUID);
            }

            nodeProfilingData = remainingNodes;
        }

        internal void Reset()
        {
            foreach(var node in nodeProfilingData.Values)
            {
                node.Reset();
            }
        }
    }
}
