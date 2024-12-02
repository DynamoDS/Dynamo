using Autodesk.DesignScript.Geometry;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Dynamo.Search.SearchElements
{
    [DataContract]
    internal class MLNodeAutoCompletionRequest
    {
        internal MLNodeAutoCompletionRequest(string dynamoVersion, int numberOfResults)
        {
            DynamoVersion = dynamoVersion;
            NumberOfResults = numberOfResults;
            Node = new NodeItem();
            Port = new PortItem();
            Context = new ContextItem();
            Packages = new List<PackageItem>();
        }

        [DataMember(Name = "node")]
        internal NodeItem Node { get; set; }

        [DataMember(Name = "port")]
        internal PortItem Port { get; set; }

        [DataMember(Name = "host")]
        internal HostItem Host { get; set; }

        [DataMember(Name = "dynamoVersion")]
        internal string DynamoVersion { get; set; }

        [DataMember(Name = "numberOfResults")]
        internal int NumberOfResults { get; set; }

        [DataMember(Name = "packages")]
        internal IEnumerable<PackageItem> Packages { get; set; }

        [DataMember(Name = "context")]
        internal ContextItem Context { get; set; }
    }

    /// <summary>
    /// Data representing node properties for ML autocomplete.
    /// </summary>
    [DataContract]
    public class NodeItem
    {
        internal NodeItem()
        {
            Type = new NodeType();
        }

        internal NodeItem(string id)
        {
            Id = id;
            Type = new NodeType();
        }

        [DataMember(Name = "id")]
        internal string Id { get; set; }

        [DataMember(Name = "type")]
        internal NodeType Type { get; set; }

        [DataMember(Name = "lacing")]
        internal string Lacing { get; set; }
    }

    [DataContract]
    internal class NodeType
    {
        [DataMember(Name = "id")]
        internal string Id { get; set; }
    }

    [DataContract]
    internal class PortItem
    {
        internal PortItem()
        {
            //nothing
        }

        internal PortItem(string name, string direction)
        {
            Name = name;
            Direction = direction;
        }

        [DataMember(Name = "name")]
        internal string Name { get; set; }

        [DataMember(Name = "index")]
        internal int Index { get; set; }

        [DataMember(Name = "direction")]
        internal string Direction { get; set; }

        [DataMember(Name = "listAtLevel")]
        internal int ListAtLevel { get; set; }

        [DataMember(Name = "keepListStructure")]
        internal string KeepListStructure { get; set; }
    }

    [DataContract]
    internal class HostItem
    {
        internal HostItem(string name, string version)
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
    internal class ContextItem
    {
        internal ContextItem()
        {
            Connections = new List<ConnectionItem>();
            Nodes = new List<NodeItem>();
        }

        [DataMember(Name = "nodes")]
        internal IEnumerable<NodeItem> Nodes { get; set; }

        [DataMember(Name = "connections")]
        internal IEnumerable<ConnectionItem> Connections { get; set; }
    }

    /// <summary>
    /// Data representing node connections for ML autocomplete.
    /// </summary>
    [DataContract]
    public class ConnectionItem
    {
        [DataMember(Name = "from")]
        internal ConnectorNodeItem StartNode { get; set; }

        [DataMember(Name = "to")]
        internal ConnectorNodeItem EndNode { get; set; }
    }

    [DataContract]
    internal class ConnectorNodeItem
    {
        internal ConnectorNodeItem()
        {
            // default constructor for deserialization.
        }

        internal ConnectorNodeItem(string nodeId, string portName)
        {
            NodeId = nodeId;
            PortName = portName;
        }

        internal ConnectorNodeItem(string nodeId, int portIndex)
        {
            NodeId = nodeId;
            PortIndex = portIndex;
        }

        [DataMember(Name = "nodeId")]
        internal string NodeId { get; set; }

        [DataMember(Name = "portName")]
        internal string PortName { get; set; }

        [DataMember(Name = "portIndex")]
        internal int PortIndex { get; set; }
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
    internal class MLNodeClusterAutoCompletionResponse
    {
        internal MLNodeClusterAutoCompletionResponse()
        {
            Results = new List<ClusterResultItem>();
        }

        [DataMember(Name = "version")]
        internal string Version { get; set; }

        [DataMember(Name = "numberOfResults")]
        internal int NumberOfResults { get; set; }

        [DataMember(Name = "results")]
        internal IEnumerable<ClusterResultItem> Results { get; set; }
    }

    [DataContract]
    internal class ResultItem
    {
        [DataMember(Name = "node")]
        internal NodeResponseItem Node { get; set; }

        [DataMember(Name = "port")]
        internal PortResponseItem Port { get; set; }

        [DataMember(Name = "score")]
        internal double Score { get; set; }
    }

    [DataContract]
    internal class ClusterResultItem
    {
        [DataMember(Name = "title")]
        internal string Title { get; set; }

        [DataMember(Name = "description")]
        internal string Description { get; set; }

        [DataMember(Name = "probability")]
        internal string Probability { get; set; }

        [DataMember(Name = "entryNodeIndex")]
        internal int EntryNodeIndex { get; set; }

        [DataMember(Name = "entryNodeInPort")]
        internal int EntryNodeInPort { get; set; }

        [DataMember(Name = "topology")]
        internal TopologyItem Topology { get; set; }
    }

    [DataContract]
    internal class TopologyItem
    {
        internal TopologyItem()
        {
            Nodes = new List<NodeItem>();
            Connections = new List<ConnectionItem>();
        }

        [DataMember(Name = "nodes")]
        internal IEnumerable<NodeItem> Nodes { get; set; }

        [DataMember(Name = "connections")]
        internal IEnumerable<ConnectionItem> Connections { get; set; }
    }

    [DataContract]
    internal class PortResponseItem
    {
        [DataMember(Name = "name")]
        internal string Name { get; set; }

        [DataMember(Name = "index")]
        internal int Index { get; set; }
    }

    [DataContract]
    internal class NodeResponseItem
    {
        internal NodeResponseItem()
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
