using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace GraphToDSCompiler
{
    /// <summary>
    /// NodeConnection contains connection information associated with each child or parent
    /// It is currently used for child connections to split codeblocks
    /// </summary>
    public class NodeConnection
    {
        public NodeConnection(int toIndex, int fromIndex)
        {
            to = toIndex;
            from = fromIndex;
        }
        public int to;
        public int from;
    }

    /// <summary>
    /// Represents a token in a statement
    /// </summary>
    public class Node
    {
        #region FIELDS
        private string name = null;
        private uint guid;

        /// <summary>
        /// List of children, index -> entry
        /// Not a list as during assembly it may not be a complete mapping
        /// </summary>
        public Dictionary<int, Node> children { get; private set; }
        public Dictionary<int, NodeConnection> childConnections { get; private set; }


        List<Node> parents = new List<Node>();
        #endregion

        #region PROPERTIES
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public uint Guid
        {
            get
            {
                return guid;
            }
            set
            {
                guid = value;
            }
        }
        #endregion

        #region CONSTRUCTORS

        public Node(string name, uint guid)
        {
            this.name = name;
            this.guid = guid;
            children = new Dictionary<int, Node>();
            childConnections = new Dictionary<int, NodeConnection>();
        }
        #endregion



        /// <summary>
        /// Connects the output of this node to the specified input of the node passed
        /// Also call 'AddEdgeFrom' on the other node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="pos"></param>
        public void AddChild(Node node, int toIndex, int fromIndex)
        {
            children.Add(toIndex, node);
            childConnections.Add(toIndex, new NodeConnection(toIndex, fromIndex));
        }

        /// <summary>
        /// Remove the edge connecting the output of this note to specified node
        /// Also call RemoveEdgeFrom on node
        /// </summary>
        /// <param name="node"></param>
        public void RemoveChild(Node node)
        {
            var item = children.First(kvp => kvp.Value == node);
            children.Remove(item.Key);
            childConnections.Remove(item.Key);
        }

        /// <summary>
        ///  Removes the child at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveChild(int index)
        {
            children.Remove(index);
            childConnections.Remove(index);
        }

        public void ReplaceChild(Node nodeToBeReplaced, Node replacement)
        {
            var item = children.First(kvp => kvp.Value == nodeToBeReplaced);
            children[item.Key]=replacement;
        }
        /// <summary>
        /// Add edge from the output of the specifed node to this node
        /// Also call AddEdgeTo on the specified Node
        /// </summary>
        /// <param name="node"></param>
        public void AddParent(Node node)
        {
            parents.Add(node);
        }

        /// <summary>
        /// Remove the edge connecting the input of this note to output of the specified node
        /// Also call RemoveEdgeTo on node
        /// </summary>
        public void RemoveParent(Node node)
        {
            parents.Remove(node);
        }

        /// <summary>
        /// Returns a sorted list of children according to their entries
        /// </summary>
        /// <returns></returns>
        public List<Node> GetChildren()
        {
            var sortedDict = (from entry in children orderby entry.Key ascending select entry)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            List<Node> childNodes = new List<Node>(sortedDict.Values);
            return childNodes;
        }
        public Dictionary<int, Node> GetChildrenWithIndices()
        {
            var sortedDict = (from entry in children orderby entry.Key ascending select entry)
                    .ToDictionary(pair => pair.Key, pair => pair.Value);
            return sortedDict;
        }
        /// <summary>
        /// Returns a list of the child node's names, sorted by their position
        /// </summary>
        /// <returns></returns>
        public List<string> GetChildNames()
        {
            List<string> returnValue = new List<string>(children.Count);

            List<Node> childNodes = GetChildren();
            foreach (Node node in childNodes)
            {
                returnValue.Add(node.Name);
            }
            return returnValue;
        }

        /// <summary>
        /// Returns a list of the partents of this node
        /// </summary>
        /// <returns></returns>
        public List<Node> GetParents()
        {
            return parents;
        }

        /// <summary>
        /// Returns a list of the parent's nodes names
        /// </summary>
        /// <returns></returns>
        public List<string> GetParentNames()
        {
            List<string> returnValue = new List<string>(parents.Count);
            foreach (Node node in GetParents())
            {
                returnValue.Add(node.Name);
            }
            return returnValue;
        }


        public bool IsLeaf
        {
            get { return children.Count == 0; }
        }

        public bool IsRoot
        {
            get { return parents.Count == 0; }
        }

        /// <summary>
        /// Is this node connected to another node?
        /// </summary>
        public bool IsConnected
        {
            get { return !IsIsland; }
        }

        /// <summary>
        /// Is this node not connected to any other node?
        /// </summary>
        public bool IsIsland
        {
            get { return IsRoot && IsLeaf; }
        }

        public override string ToString()
        {
            return "Node{" +
                   "name='" + name + "'" +
                   "}\n";
        }

        public virtual string ToScript()
        {
            return null;
        }


        public virtual string ToCode()
        {
            throw new NotImplementedException();
        }
    }
}
