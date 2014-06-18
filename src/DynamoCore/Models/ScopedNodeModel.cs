using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Models
{
    /// <summary>
    /// ScopedNodeModel will be put all its children inside its scope so that 
    /// they won't get compiled in global scope.
    /// </summary>
    public class ScopedNodeModel: NodeModel
    {
        /// <summary>
        /// If all nodes that the node outputs to are in scopes.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private bool IsNodeInScope(NodeModel node, IEnumerable<NodeModel> scopes)
        {
            if (node is Symbol)
            {
                return false;
            }

            foreach (var index in Enumerable.Range(0, node.OutPortData.Count))
            {
                HashSet<Tuple<int, NodeModel>> outputTuples = null;
                if (!node.TryGetOutput(index, out outputTuples))
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
        /// Specify if upstream nodes that connected to specified inport should
        /// be compiled in the scope or not. 
        /// </summary>
        /// <param name="portIndex"></param>
        /// <returns></returns>
        protected virtual bool IsScopedInport(int portIndex)
        {
            return true;
        }

        /// <summary>
        /// Get all nodes that in its input ports's scope. A node is in its 
        /// scope if it is one of its upstream nodes and all of that node's 
        /// downstream nodes are in this node's scope.
        /// </summary>
        /// <param name="portIndex">Inport index</param>
        /// <param name="checkEscape"></param>
        /// <returns></returns>
        public IEnumerable<NodeModel> GetInScopeNodesForInport(int portIndex, bool checkEscape = true)
        {
            // The related test cases are in DynmoTest.ScopedNodeTest.
            var scopedNodes = new HashSet<NodeModel>();

            Tuple<int, NodeModel> inputTuple = null;
            if (!IsScopedInport(portIndex) || 
                !this.TryGetInput(portIndex, out inputTuple))
            {
                return scopedNodes;
            }

            scopedNodes.Add(this);
            var inputNode = inputTuple.Item2;
            var workingList = new Queue<NodeModel>();
            if (!checkEscape || (checkEscape && IsNodeInScope(inputNode, scopedNodes)))
            {
                workingList.Enqueue(inputNode);
                scopedNodes.Add(inputNode);
            }

            // Collect all upstream nodes in BFS order
            while (workingList.Any())
            {
                var currentNode = workingList.Dequeue();
                foreach (int index in Enumerable.Range(0, currentNode.InPortData.Count))
                {
                    if (currentNode.TryGetInput(index, out inputTuple))
                    {
                        inputNode = inputTuple.Item2;
                        if (!checkEscape || (checkEscape && IsNodeInScope(inputNode, scopedNodes)))
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
        public IEnumerable<NodeModel> GetInScopeNodes(bool checkEscape = true)
        {
            var inScopedNodes = new List<NodeModel>();

            foreach (int index in Enumerable.Range(0, InPortData.Count))
            {
                if (!IsScopedInport(index))
                {
                    continue;
                }

                var inScopedNodesForInport = GetInScopeNodesForInport(index, checkEscape);
                inScopedNodes.AddRange(inScopedNodesForInport);
            }

            return inScopedNodes;
        }

        public virtual IEnumerable<AssociativeNode> BuildOutputAstInScope(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException("BuildOutputAstInScope");
        }

        internal virtual IEnumerable<AssociativeNode> BuildAstInScope(List<AssociativeNode> inputAstNodes)
        {
            OnBuilt();
            var result = BuildOutputAstInScope(inputAstNodes);
            return result;
        }
    }
}
