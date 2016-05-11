using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;

namespace Dynamo.Diagnostics
{
    /// <summary>
    /// Stores a node's performance statistics
    /// </summary>
    public class NodeStatistics
    {
        public string Name { get; set; }

        public string NickName { get; set; }

        [XmlArray]
        public List<PerformanceData> Performance { get; set; }

        public NodeStatistics()
        {
            Performance = new List<PerformanceData>();
        }
    }

    public interface IQueryNodePerformance
    {
        IEnumerable<PerformanceData> GetNodePerformance(NodeModel node);
    }

    /// <summary>
    /// Stores performance statistics of all nodes.
    /// </summary>
    public class PerformanceStatistics : IQueryNodePerformance
    {
        private Dictionary<string, NodeStatistics> statistics = new Dictionary<string, NodeStatistics>();

        public void AddPerformanceData(NodeModel node, PerformanceData data)
        {
            var name = GetUniqueNodeName(node);
            NodeStatistics stats;
            if (!statistics.TryGetValue(name, out stats))
            {
                stats = new NodeStatistics() { Name = name, NickName = node.NickName };
            }
            
            stats.Performance.Add(data);
            statistics[name] = stats;
        }

        public IEnumerable<PerformanceData> GetNodePerformance(NodeModel node)
        {
            var name = GetUniqueNodeName(node);
            NodeStatistics stats;
            if (statistics.TryGetValue(name, out stats))
                return stats.Performance;

            return Enumerable.Empty<PerformanceData>();
        }

        public string GetUniqueNodeName(NodeModel node)
        {
            if (node is DSFunction)
                return node.CreationName;

            var codeblock = node as CodeBlockNodeModel;
            if (codeblock != null)
                return "CodeBlock_" + codeblock.Code.GetHashCode();

            var customNode = node as Function;
            if (customNode != null)
                return customNode.Definition.DisplayName;

            var name = node.Name;
            if (!string.IsNullOrEmpty(name))
                return name;

            return node.GetType().FullName;
        }

        [XmlArray]
        public List<NodeStatistics> Data 
        { 
            get
            {
                return statistics.Values.ToList();
            }
            set
            {
                if (value != null)
                {
                    statistics = value.ToDictionary(x => x.Name);
                }
            }
        }

        public static PerformanceStatistics Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var serializer = new XmlSerializer(typeof(PerformanceStatistics), new[] { typeof(NodeStatistics), typeof(PerformanceData) });
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return serializer.Deserialize(fs) as PerformanceStatistics;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed to load PerformanceStatistics from file {0}", filePath));
                Console.Write(ex.Message);
            }

            return null;
        }

        public void Save(string filePath)
        {
            var serializer = new XmlSerializer(typeof(PerformanceStatistics), new[] { typeof(NodeStatistics), typeof(PerformanceData) });
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(fs, this);
            }
        }
    }
}
