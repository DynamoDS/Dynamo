using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
}