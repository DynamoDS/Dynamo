using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Engine.CodeGeneration;
using Dynamo.Graph.Nodes.CustomNodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// ScopedNodeModel will put its children in its scope so that they won't 
    /// get compiled in global scope.
    /// </summary>
    public class ScopedNodeModel: NodeModel
    {
        [JsonConstructor]
        protected ScopedNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        protected ScopedNodeModel() { }

        /// <summary>
        /// If all nodes that the node outputs to are in scopes list. I.e.,
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

            foreach (var index in Enumerable.Range(0, node.OutPorts.Count))
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
        /// Specify if the corresponding inport has scope or not. 
        /// </summary>
        /// <param name="portIndex"></param>
        /// <returns></returns>
        protected virtual bool IsScopedInport(int portIndex)
        {
            return true;
        }

        /// <summary>
        /// Returns all nodes that in its input ports' scope. A node is in its 
        /// scope if that node is one of its upstream nodes. 
        /// </summary>
        /// <param name="portIndex">Inport index</param>
        /// <param name="checkEscape">
        /// If need to exclude nodes that one of their downstream nodes are not 
        /// in the scope
        /// </param>
        /// <param name="isInclusive">
        /// If a upstream node is ScopedNodeModel, need to include all upstream 
        /// nodes of that node.
        /// </param>
        /// <param name="forceToGetNodeForInport"></param>
        /// <returns></returns>
        public IEnumerable<NodeModel> GetInScopeNodesForInport(
            int portIndex, 
            bool checkEscape = true, 
            bool isInclusive = true, 
            bool forceToGetNodeForInport = false)
        {
            // The related test cases are in DynmoTest.ScopedNodeTest.
            var scopedNodes = new HashSet<NodeModel>();

            Tuple<int, NodeModel> inputTuple = null;
            if ((!forceToGetNodeForInport && !IsScopedInport(portIndex)) || 
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
                if (!isInclusive && currentNode is ScopedNodeModel)
                {
                    continue;
                }

                foreach (int index in Enumerable.Range(0, currentNode.InPorts.Count))
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
        /// Returns all nodes that are in the scope of this node. 
        /// nodes are not in the scope.
        /// </summary>
        /// <param name="checkEscape">
        /// Specifies if need to exclude nodes that one of their downstream
        /// nodes are not in the scope
        /// </param>
        /// <param name="isInclusive">
        /// If one of its upstream node is ScopedNodeModel, if need to include 
        /// all upstream nodes of that node.
        /// </param>
        /// <returns></returns>
        internal IEnumerable<NodeModel> GetInScopeNodes(bool checkEscape = true, bool isInclusive = true)
        {
            var inScopedNodes = new List<NodeModel>();

            foreach (int index in Enumerable.Range(0, InPorts.Count))
            {
                if (!IsScopedInport(index))
                {
                    continue;
                }

                var inScopedNodesForInport = GetInScopeNodesForInport(index, checkEscape, isInclusive);
                inScopedNodes.AddRange(inScopedNodesForInport);
            }

            return inScopedNodes;
        }

        /// <summary>
        /// Iterate over nodes and remove all nodes that are in the scope of
        /// some scoped node. So all returned nodes are in global scope.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        internal static IEnumerable<NodeModel> GetNodesInTopScope(IEnumerable<NodeModel> nodes)
        {
            HashSet<NodeModel> topScopedNodes = new HashSet<NodeModel>(nodes);
            foreach (var node in nodes)
            {
                var scopedNode = node as ScopedNodeModel;
                if (scopedNode == null)
                {
                    continue;
                }

                var nodesInItsScope = scopedNode.GetInScopeNodes(false);
                topScopedNodes.ExceptWith(nodesInItsScope);
            }

            return topScopedNodes;
        }

        /// <summary>
        /// Similar to NodeModel.BuildOutputAst(). When compiled to AST, for
        /// ScopedNodeModel this method will be called when all requirements are
        /// satisfied. The derived class needs to implement this method to 
        /// compile its children into some scopes.
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <param name="verboseLogging"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public virtual IEnumerable<AssociativeNode> BuildOutputAstInScope(List<AssociativeNode> inputAstNodes, bool verboseLogging, AstBuilder builder)
        {
            throw new NotImplementedException("BuildOutputAstInScope");
        }

        /// <summary>
        /// Similar to NodeModel.BuildAst(). When compiled to AST, for 
        /// ScopedNodeModel this method will be called when all requirements
        /// are satisfied. 
        /// </summary>
        /// <param name="inputAstNodes"></param>
        /// <param name="verboseLogging"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        internal virtual IEnumerable<AssociativeNode> BuildAstInScope(List<AssociativeNode> inputAstNodes, bool verboseLogging, AstBuilder builder)
        {
            OnBuilt();
            var result = BuildOutputAstInScope(inputAstNodes, verboseLogging, builder);
            return result;
        }
    }
}
