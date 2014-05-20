using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLayout
{
    public class Graph
    {
        public List<Node> Nodes = new List<Node>();
        public List<Edge> Edges = new List<Edge>();

        public void AddNode(Guid guid, double width, double height)
        {
            var node = new Node(guid, width, height, this);
            Nodes.Add(node);
        }

        public void AddEdge(Guid guid, Guid start, Guid end)
        {
            var edge = new Edge(guid, start, end, this);
            Edges.Add(edge);
        }

        public Node FindNode(Guid guid)
        {
            foreach (Node node in Nodes)
                if (guid.Equals(node.Id))
                    return node;
            return null;
        }
    }

    public class Node
    {
        private Graph OwnerGraph;
        
        public double Width;
        public double Height;

        public int Rank;

        public Guid Id;
        
        public List<Edge> LeftEdges;
        public List<Edge> RightEdges;

        public Node(Guid guid, double width, double height, Graph ownerGraph)
        {
            Id = guid;
            Width = width;
            Height = height;
            OwnerGraph = ownerGraph;
        }
    }

    public class Edge
    {
        private Graph OwnerGraph;

        public Guid Id;

        public Guid StartId;
        public Guid EndId;

        private Node _startNode;
        public Node StartNode
        {
            get
            {
                if (_startNode == null)
                    _startNode = OwnerGraph.FindNode(StartId);
                return _startNode;
            }
        }

        public Node _endNode;
        public Node EndNode
        {
            get
            {
                if (_endNode == null)
                    _endNode = OwnerGraph.FindNode(StartId);
                return _endNode;
            }
        }

        public Edge(Guid guid, Guid start, Guid end, Graph ownerGraph)
        {
            Id = guid;
            StartId = start;
            EndId = end;
            OwnerGraph = ownerGraph;
        }
    }
}
