using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;

namespace Dynamo.Models
{
    /// <summary>
    /// ScopedNodeModel will be put all its children inside its scope so that 
    /// they won't get compiled in global scope.
    /// </summary>
    public class ScopedNodeModel: NodeModel
    {
        private bool IsNodeInScope(NodeModel node, IEnumerable<NodeModel> scopes)
        {
            if (node is Symbol)
            {
                return false;
            }

            foreach (var index in Enumerable.Range(0, node.OutPortData.Count))
            {
                HashSet<Tuple<int, NodeModel>> outputTuples = null;
                if (!TryGetOutput(index, out outputTuples))
                {
                    continue;
                }

                if (!outputTuples.All(t => scopes.Contains(t.Item2)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// If the input port is the scoped port or not. 
        /// </summary>
        /// <param name="portIndex"></param>
        /// <returns></returns>
        protected virtual bool IsScopedInPort(int portIndex)
        {
            return true;
        }

        /// <summary>
        /// Get all nodes that in its input ports's scope. A node is in its 
        /// scope if it is one of its upstream nodes and all of that node's 
        /// downstream nodes are in this node's scope.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodeModel> GetInScopeNodesForInport(int portIndex)
        {
            // The related test cases are in test fixture ScopedNodeTest.

            var scopedNodes = new HashSet<NodeModel>();

            Tuple<int, NodeModel> inputTuple = null;
            if (!IsScopedInPort(portIndex) || 
                !this.TryGetInput(portIndex, out inputTuple))
            {
                return scopedNodes;
            }

            scopedNodes.Add(this);
            var inputNode = inputTuple.Item2;
            var workingList = new Queue<NodeModel>();
            if (IsNodeInScope(inputNode, scopedNodes))
            {
                workingList.Enqueue(inputNode);
                scopedNodes.Add(inputNode);
            }

            while (workingList.Any())
            {
                var currentNode = workingList.Dequeue();
                foreach (int index in Enumerable.Range(0, currentNode.InPortData.Count))
                {
                    if (currentNode.TryGetInput(index, out inputTuple))
                    {
                        inputNode = inputTuple.Item2;
                        if (IsNodeInScope(inputNode, scopedNodes))
                        {
                            workingList.Enqueue(inputNode);
                            scopedNodes.Add(inputNode);
                        }
                    }
                }
            }

            return scopedNodes.Skip(1);
        }

        /// <summary>
        /// Return all nodes that are in the scope of this node.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodeModel> GetInScopeNodes()
        {
            var inScopedNodes = new List<NodeModel>();

            foreach (int index in Enumerable.Range(0, InPortData.Count))
            {
                if (!IsScopedInPort(index))
                {
                    continue;
                }

                var inScopedNodesForInport = GetInScopeNodesForInport(index);
                inScopedNodes.AddRange(inScopedNodesForInport);
            }

            return inScopedNodes;
        }
    }
}
