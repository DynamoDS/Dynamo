using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace GraphToDSCompiler
{
    public class AST
    {
        /// <summary>
        /// Dictionary to hold nodes indexed to their corresponding guids.
        /// </summary>
        /// 

        // Jun make this private again
        public Dictionary<uint, Node> nodeMap = new Dictionary<uint, Node>();

        /// <summary>
        /// A list to hold all the nodes present in the graph.
        /// </summary>
        /// 
        // Jun make this private again
        public List<Node> nodeList = new List<Node>();

        /// <summary>
        /// Returns a list of all the nodes present in the graph.
        /// </summary>
        /// <returns>List<Node></returns>
        public List<Node> GetNodes()
        {
            return nodeList;
        }

        /*public HashSet<uint> GetNames()
        {
            HashSet<uint> nameSet = new HashSet<uint>(nodeMap.Keys);
            return nameSet;
        }
        */
        /// <summary>
        /// Adds a new node to the nodemap given its name and guid if it is not present in the graph.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Node AddNode(string name, uint guid)
        {
            Node newNode = null;

            if (nodeMap.ContainsKey(guid))
            {
                newNode = (Node)nodeMap[guid];
            }
            else
            {
                newNode = new Node(name, guid);
                nodeMap.Add(guid, newNode);
                nodeList.Add(newNode);
            }
            return newNode;
        }

        /// <summary>
        /// Adds the node passed as parameter to the nodemap if it is not present in the graph.
        /// Returns the added newNode.
        /// </summary>
        /// <param name="nodus"></param>
        /// <returns></returns>
        public Node AddNode(Node nodus)
        {
            Node newNode = null;

            if (nodeMap.ContainsKey(nodus.Guid))
            {
                newNode = (Node)nodeMap[nodus.Guid];
            }
            else
            {
                newNode = nodus;
                nodeMap.Add(nodus.Guid, newNode);
                if (newNode is ImportNode) nodeList.Insert(0, newNode);
                else
                    nodeList.Add(newNode);
            }
            return newNode;
        }

        /// <summary>
        /// Inserts a node at the required position
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="nodus"></param>
        /// <returns></returns>
        public Node InsertNode(int pos, Node nodus)
        {
            Node newNode = nodus;
            nodeList.Insert(pos, newNode);
            return newNode;
        }

        /// <summary>
        /// Add edge from the "from" node to the "to" node 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="guidFrom"></param>
        /// <param name="guidTo"></param>
        /// <param name="inputIndex"></param>
        public void AddEdge(string from, string to, uint guidFrom, uint guidTo, int inputIndex, int fromIndex)
        {
            Node n1 = AddNode(from, guidFrom);
            Node n2 = AddNode(to, guidTo);
            AddEdge(n1, n2, inputIndex, fromIndex);
        }

        public void AddEdge(Node from, Node to, int inputIndex, int fromIndex)
        {
            from.AddChild(to, inputIndex, fromIndex);
            to.AddParent(from);
            List<Node> cycle = CheckForCycle.CreatesCycle(to);
            if (cycle != null)
            {
                // remove edge which introduced cycle
                //    RemoveEdge( from, to );
                String msg = "Edge between '" + from.Name + "' and '" + to.Name + "' introduces a cycle in the graph";
                GraphCompilationStatus.HandleError(new HasCycleException(msg, cycle)) ;
            }
        }

        /// <summary>
        /// Removes an edge from the "from" node to the "to" node.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="guidFrom"></param>
        /// <param name="guidTo"></param>
        public void RemoveEdge(string from, string to, uint guidFrom, uint guidTo)
        {
            Node n1 = AddNode(from, guidFrom);
            Node n2 = AddNode(to, guidTo);
            RemoveEdge(n1, n2);
        }

        public void RemoveEdge(Node from, Node to)
        {
            from.RemoveChild(to);
            to.RemoveParent(from);
        }

        /// <summary>
        /// Returns the node whose guid is passed as parameter
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Node GetNode(uint guid)
        {
            Node retNode = null;
            if (nodeMap.ContainsKey(guid))
            {
                retNode = (Node)nodeMap[guid];
            }
            return retNode;
        }

        /// <summary>
        /// Remove all edges from or to the node passed as parameter.
        /// </summary>
        /// <param name="rem"></param>
        /// <returns></returns>
        public bool RemoveAllEdges(Node rem)
        {
            if (nodeMap.ContainsKey(rem.Guid))
            {
                IEnumerable iter = rem.GetChildren();
                foreach (Node child in iter)
                {
                    rem.RemoveChild(child); 
                    child.RemoveParent(rem);
                }
                iter = rem.GetParents();
                foreach (Node dad in iter)
                {
                    rem.RemoveChild(dad); 
                    dad.RemoveChild(rem);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the node passed as parameter from the graph(represented using NodeMap)
        /// </summary>
        /// <param name="rem"></param>
        /// <returns></returns>
        public bool RemoveNode(Node rem)
        {
            if (nodeMap.ContainsKey(rem.Guid))
            {
                IEnumerable iter = rem.GetChildren();
                foreach (Node child in iter)
                {
                    child.RemoveParent(rem);
                }
                iter = rem.GetParents();
                foreach (Node dad in iter)
                {
                    dad.RemoveChild(rem);
                }

                nodeMap.Remove(rem.Guid); 
                nodeList.Remove(rem); 
                return true;
            }
            return false;
        }

        public bool ReplaceNode(Node replacement,Node toBeReplaced)
        {
            if (nodeMap.ContainsKey(replacement.Guid))
            {
                
                foreach (Node dad in toBeReplaced.GetParents())
                {
                    //rem.RemoveEdgeTo(child); 
                    dad.ReplaceChild(toBeReplaced,replacement);
                }
                nodeList.Remove(toBeReplaced);
                nodeMap[replacement.Guid]=replacement;
                nodeList.Add(replacement);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Returns if one node is an immediate child of the other
        /// </summary>
        /// <param name="fromguid"></param>
        /// <param name="toguid"></param>
        /// <returns></returns>
        public Boolean HasEdge(uint fromguid, uint toguid,int index)
        {
            Node n1 = GetNode(fromguid);
            Node n2 = GetNode(toguid);
            Dictionary<int,Node> d = n1.GetChildrenWithIndices();
            Node n;
            bool b = false;
            if (d.TryGetValue(index, out n)) if(n.Guid==n2.Guid)b=true;
            return b;
        }

        /// <summary>
        /// Returns a list of the names of the children of the node whose guid is passed
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<string> GetChildNames(uint guid)
        {
            Node node = GetNode(guid);
            return node.GetChildNames();
        }

        /// <summary>
        /// Returns a list of the names of the parents of the node whose guid is passed
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<string> GetParentNames(uint guid)
        {
            Node node = GetNode(guid);
            return node.GetParentNames();
        }

        /// <summary>
        /// Is the node connected to other Nodes?
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Boolean IsConnected(uint guid)
        {
            Node node = GetNode(guid);
            return node.IsConnected;
        }

        public override string ToString()
        {
            string ret = "";
            List<Node> toPrint = TopSort.sort(this);
            IEnumerable iter = toPrint;
            foreach (Node node in iter)
            {
                if (node is BinExprNode)
                {
                    BinExprNode b = (BinExprNode)node;
                    if (b.left != null && b.right != null)
                        ret += b.left.Name + " " + b.Name + " " + b.right.Name;
                    else ret += b.Name;
                }
                else ret += " " + node.ToString();
            }
            return ret;
        }

        public string ToScript()
        {
            string ret = "";
            List<Node> toPrint = TopSort.sort(this);
            IEnumerable iter = toPrint;
            foreach (Node node in iter)
                if (node != null)
                    ret += "\n" + node.ToScript() + ";";
            return ret;
        }
    }
}
