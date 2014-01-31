using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphToDSCompiler;
using ProtoCore.Mirror;

namespace ProtoScript.Runners
{
    public class NodeValueReadyEventArgs : EventArgs
    {
        public NodeValueReadyEventArgs(RuntimeMirror mirror, Guid nodeGuid)
            : this(mirror, nodeGuid, EventStatus.OK, null)
        {
        }

        public NodeValueReadyEventArgs(RuntimeMirror mirror, Guid nodeGuid, EventStatus resultStatus, String errorString)
        {
            this.RuntimeMirror = mirror;
            this.NodeGuid = nodeGuid;
            ResultStatus = resultStatus;
            ErrorString = errorString;
        }

        public Guid NodeGuid { get; private set; }
        public EventStatus ResultStatus { get; private set; }
        public String ErrorString { get; private set; }
        public ProtoCore.Mirror.RuntimeMirror RuntimeMirror { get; private set; }
    }

    public class NodeValueNotAvailableEventArgs : NodeValueReadyEventArgs
    {
        public NodeValueNotAvailableEventArgs(Guid nodeGuid) :
            base(null, nodeGuid, EventStatus.OK, "Not value available for this node")
        { }
    }

    public class GraphUpdateReadyEventArgs : EventArgs
    {
        public GraphSyncData SyncData { get; private set; }
        public EventStatus ResultStatus { get; private set; }
        public String ErrorString { get; private set; }

        public struct ErrorObject
        {
            public string Message;
            public Guid Id;
        }
        public List<ErrorObject> Errors { get; set; }
        public List<ErrorObject> Warnings { get; set; }

        public GraphUpdateReadyEventArgs(GraphSyncData syncData)
            : this(syncData, EventStatus.OK, null)
        {
        }

        public GraphUpdateReadyEventArgs(GraphSyncData syncData, EventStatus resultStatus, String errorString)
        {
            this.SyncData = syncData;
            this.ResultStatus = resultStatus;
            this.ErrorString = errorString;

            if (string.IsNullOrEmpty(this.ErrorString))
                this.ErrorString = "";

            Errors = new List<ErrorObject>();
            Warnings = new List<ErrorObject>();
        }
    }

    public class NodesToCodeCompletedEventArgs : EventArgs
    {
        public String output { get; private set; }
        public EventStatus ResultStatus { get; private set; }
        public String ErrorString { get; private set; }

        public NodesToCodeCompletedEventArgs(
            string outputData, EventStatus resultStatus, String errorString)
        {
            this.output = outputData;
            this.ResultStatus = resultStatus;
            this.ErrorString = errorString;
        }
    }


    public delegate void NodeValueReadyEventHandler(object sender, NodeValueReadyEventArgs e);
    public delegate void GraphUpdateReadyEventHandler(object sender, GraphUpdateReadyEventArgs e);
    public delegate void NodesToCodeCompletedEventHandler(object sender, NodesToCodeCompletedEventArgs e);

    namespace Obsolete
    {
        public class NodeValueReadyEventArgs : EventArgs
        {
            public NodeValueReadyEventArgs(ProtoCore.Mirror.RuntimeMirror mirror, uint nodeId)
                : this(mirror, nodeId, EventStatus.OK, null)
            {
            }

            public NodeValueReadyEventArgs(ProtoCore.Mirror.RuntimeMirror mirror, uint nodeId, EventStatus resultStatus, String errorString)
            {
                this.RuntimeMirror = mirror;
                this.NodeId = nodeId;
                ResultStatus = resultStatus;
                ErrorString = errorString;
            }

            public uint NodeId { get; private set; }
            public EventStatus ResultStatus { get; private set; }
            public String ErrorString { get; private set; }
            public ProtoCore.Mirror.RuntimeMirror RuntimeMirror { get; private set; }
        }

        public class NodeValueNotAvailableEventArgs : NodeValueReadyEventArgs
        {
            public NodeValueNotAvailableEventArgs(uint nodeId) :
                base(null, nodeId, EventStatus.OK, "Not value available for this node")
            { }
        }

        public class GraphUpdateReadyEventArgs : EventArgs
        {

            public SynchronizeData SyncData { get; private set; }
            public EventStatus ResultStatus { get; private set; }
            public String ErrorString { get; private set; }

            public struct ErrorObject
            {
                public string Message;

                /// <summary>
                /// SSN UID
                /// </summary>
                public uint Id;

                //public bool IsError;
            }
            public List<ErrorObject> Errors { get; set; }
            public List<ErrorObject> Warnings { get; set; }

            public GraphUpdateReadyEventArgs(SynchronizeData syncData)
                : this(syncData, EventStatus.OK, null)
            {
            }

            public GraphUpdateReadyEventArgs(SynchronizeData syncData, EventStatus resultStatus, String errorString)
            {
                this.SyncData = syncData;
                this.ResultStatus = resultStatus;
                this.ErrorString = errorString;

                if (string.IsNullOrEmpty(this.ErrorString))
                    this.ErrorString = "";

                Errors = new List<ErrorObject>();
                Warnings = new List<ErrorObject>();
            }
        }

        public class NodesToCodeCompletedEventArgs : EventArgs
        {
            public List<uint> InputNodeIds { get; private set; }
            public List<SnapshotNode> OutputNodes { get; private set; }
            public EventStatus ResultStatus { get; private set; }
            public String ErrorString { get; private set; }

            public NodesToCodeCompletedEventArgs(List<uint> inputNodeIds,
                List<SnapshotNode> outputNodes, EventStatus resultStatus, String errorString)
            {
                this.InputNodeIds = inputNodeIds;
                this.OutputNodes = outputNodes;
                this.ResultStatus = resultStatus;
                this.ErrorString = errorString;
            }
        }

        public delegate void NodeValueReadyEventHandler(object sender, NodeValueReadyEventArgs e);
        public delegate void GraphUpdateReadyEventHandler(object sender, GraphUpdateReadyEventArgs e);
        public delegate void NodesToCodeCompletedEventHandler(object sender, NodesToCodeCompletedEventArgs e);
    }
}