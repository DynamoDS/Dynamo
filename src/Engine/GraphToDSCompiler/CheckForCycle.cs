using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace GraphToDSCompiler
{
    class CheckForCycle
    {
        private static int notVisited = 0;
        private static int visiting = 1;
        private static int visited = 2;

        /// <summary>
        /// Checks if the node to which an edge was added introduces a cycle
        /// Returns a list of nodes in the detected cycle
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<Node> CreatesCycle(Node node)
        {
            Dictionary<Node, int> nodeStateMap = new Dictionary<Node, int>();
            return CreatesCycle( node, nodeStateMap );
        }

        public static List<Node> CreatesCycle(Node node, Dictionary<Node, int> nodeStateMap)
        {
            List<Node> cycleStack = new List<Node>();
            Boolean hasCycle = DFSVisit( node, nodeStateMap,cycleStack);
            if ( hasCycle )
            {
                // we have a situation like: [c, a, b, c, f, g, h].
                // Node which introduced  the cycle is at the first position in the list
                // We have to find second occurence of this node and use its position in the list
                // for getting the sublist of vertex labels of cycle paricipants
                // So in our case we are seraching for [c, a, b, c]
                //string name = cycleStack[0].Name;
                int pos = cycleStack.IndexOf(cycleStack[0],0);
                List<Node> cycle = new List<Node>(pos + 1);
                for(int i=0;i<pos+1;i++)
                    cycle.Add(cycleStack[i]);
                cycle.Reverse();
                return cycle;
            }
            return null;
        }

        /// <summary>
        /// Checks if a node has been visited or not
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeStateMap"></param>
        /// <returns></returns>
        private static Boolean IsNotVisited(Node node, Dictionary<Node, int> nodeStateMap)
        {
            if (!nodeStateMap.ContainsKey(node))
            {
                return true;
            }
            int state = nodeStateMap[node];
            return notVisited == state;
        }

        /// <summary>
        /// Checks if a node is being visited presently
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeStateMap"></param>
        /// <returns></returns>
        private static Boolean IsVisiting( Node node, Dictionary<Node, int> nodeStateMap)
        {
            int state = nodeStateMap[node];
            return visiting == state;
        }

        /// <summary>
        /// Returns false if a depth-first search of Graph yields no back edges.
        /// If back edges found then return true and update cycleStack.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeStateMap"></param>
        /// <param name="cycleStack"></param>
        /// <returns></returns>
        public static Boolean DFSVisit(Node node, Dictionary<Node, int> nodeStateMap, List<Node> cycleStack)
        {
            cycleStack.Add(node);
            nodeStateMap.Add(node, visiting);
            List<Node> nodes = node.GetChildren();
            IEnumerable iter = nodes;
            foreach (Node nodeI in iter)
            {
                if (IsNotVisited(nodeI, nodeStateMap))
                {
                    if (DFSVisit(nodeI, nodeStateMap, cycleStack)) return true;
                }
                else if (IsVisiting(nodeI, nodeStateMap))
                {
                    cycleStack.Insert(0,node);
                    return true;
                }
            }
            nodeStateMap[node] = visited;
            cycleStack.RemoveAt(0);
            return false;
        }
    }
}
