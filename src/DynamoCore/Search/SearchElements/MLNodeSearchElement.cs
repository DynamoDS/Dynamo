using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dynamo.Search.SearchElements
{
    [DataContract]
    internal class MLNodeAutoCompletionRequest
    {
        internal MLNodeAutoCompletionRequest(string dynamoVersion, int numberOfResults)
        {
            DynamoVersion = dynamoVersion;
            NumberOfResults = numberOfResults;
            Node = new NodeRequest();
            Port = new PortRequest();
            Context = new ContextRequest();
            Packages = new List<PackageItem>();
        }

        [DataMember(Name = "node")]
        internal NodeRequest Node { get; set; }

        [DataMember(Name = "port")]
        internal PortRequest Port { get; set; }

        [DataMember(Name = "host")]
        internal HostRequest Host { get; set; }

        [DataMember(Name = "dynamoVersion")]
        internal string DynamoVersion { get; set; }

        [DataMember(Name = "numberOfResults")]
        internal int NumberOfResults { get; set; }

        [DataMember(Name = "packages")]
        internal IEnumerable<PackageItem> Packages { get; set; }

        [DataMember(Name = "context")]
        internal ContextRequest Context { get; set; }
    }

    [DataContract]
    internal class NodeRequest
    {
        internal NodeRequest()
        {
            Type = new NodeTypeRequest();
        }

        internal NodeRequest(string id)
        {
            Id = id;
            Type = new NodeTypeRequest();
        }

        [DataMember(Name = "id")]
        internal string Id { get; set; }

        [DataMember(Name = "type")]
        internal NodeTypeRequest Type { get; set; }

        [DataMember(Name = "lacing")]
        internal string Lacing { get; set; }
    }

    [DataContract]
    internal class NodeTypeRequest
    {
        [DataMember(Name = "id")]
        internal string Id { get; set; }
    }

    [DataContract]
    internal class PortRequest
    {
        internal PortRequest()
        {
            //nothing
        }

        internal PortRequest(string name, string direction)
        {
            Name = name;
            Direction = direction;
        }

        [DataMember(Name = "name")]
        internal string Name { get; set; }

        [DataMember(Name = "direction")]
        internal string Direction { get; set; }

        [DataMember(Name = "listAtLevel")]
        internal int ListAtLevel { get; set; }

        [DataMember(Name = "keepListStructure")]
        internal string KeepListStructure { get; set; }
    }

    [DataContract]
    internal class HostRequest
    {
        internal HostRequest(string name, string version)
        {
            Name = name;
            Version = version;
        }

        [DataMember(Name = "name")]
        internal string Name { get; set; }

        [DataMember(Name = "version")]
        internal string Version { get; set; }
    }

    [DataContract]
    internal class ContextRequest
    {
        internal ContextRequest()
        {
            Connections = new List<ConnectionsRequest>();
            Nodes = new List<NodeRequest>();
        }

        [DataMember(Name = "nodes")]
        internal IEnumerable<NodeRequest> Nodes { get; set; }

        [DataMember(Name = "connections")]
        internal IEnumerable<ConnectionsRequest> Connections { get; set; }
    }

    [DataContract]
    internal class ConnectionsRequest
    {
        [DataMember(Name = "from")]
        internal ConnectorNodeItem StartNode { get; set; }

        [DataMember(Name = "to")]
        internal ConnectorNodeItem EndNode { get; set; }
    }

    [DataContract]
    internal class ConnectorNodeItem
    {
        internal ConnectorNodeItem(string nodeId, string portName)
        {
            NodeId = nodeId;
            PortName = portName;
        }

        [DataMember(Name = "nodeId")]
        internal string NodeId { get; set; }

        [DataMember(Name = "portName")]
        internal string PortName { get; set; }
    }

    [DataContract]
    internal class PackageItem
    {
        internal PackageItem(string name, string version)
        {
            Name = name;
            Version = version;
        }

        [DataMember(Name = "name")]
        internal string Name { get; set; }

        [DataMember(Name = "version")]
        internal string Version { get; set; }
    }

    [DataContract]
    internal class MLNodeAutoCompletionResponse
    {
        internal MLNodeAutoCompletionResponse()
        {
            Results = new List<ResultItem>();
        }

        [DataMember(Name = "version")]
        internal string Version { get; set; }

        [DataMember(Name = "numberOfResults")]
        internal int NumberOfResults { get; set; }

        [DataMember(Name = "results")]
        internal IEnumerable<ResultItem> Results { get; set; }
    }

    [DataContract]
    internal class ResultItem
    {
        [DataMember(Name = "node")]
        internal NodeResponse Node { get; set; }

        [DataMember(Name = "port")]
        internal PortResponse Port { get; set; }

        [DataMember(Name = "score")]
        internal double Score { get; set; }
    }

    [DataContract]
    internal class PortResponse
    {
        [DataMember(Name = "name")]
        internal string Name { get; set; }

        [DataMember(Name = "index")]
        internal int Index { get; set; }
    }

    [DataContract]
    internal class NodeResponse
    {
        internal NodeResponse()
        {
            Type = new NodeTypeResponse();
        }

        [DataMember(Name = "type")]
        internal NodeTypeResponse Type { get; set; }
    }

    [DataContract]
    internal class NodeTypeResponse
    {
        [DataMember(Name = "nodeType")]
        internal string NodeType { get; set; }

        [DataMember(Name = "id")]
        internal string Id { get; set; }
    }

    internal class NodeModelTypeId
    {
        internal NodeModelTypeId(string fullName)
        {
            FullName = fullName;
        }

        internal NodeModelTypeId(string fullName, string assemblyName)
        {
            FullName = fullName;
            AssemblyName = assemblyName;
        }

        internal string FullName { get; set; }

        internal string AssemblyName { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}", FullName, AssemblyName);
        }
    }

    internal enum HostNames
    {
        Revit,
        AdvanceSteel,
        Civil3d,
        FormIt,
        Alias,
        RSA,
        None
    };

    // Node types for different nodemodel nodes.
    internal enum NodeModelNodeTypes
    {
        FunctionNode,
        ExtensionNode,
        NumberInputNode,
        StringInputNode,
        BooleanInputNode,
        DateTimeInputNode,
        FormulaNode
    };
}
