using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Library;
using Dynamo.Logging;
using Dynamo.Engine.Profiling;

using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;
using Type = ProtoCore.Type;

namespace Dynamo.Engine.CodeGeneration
{
    /// <summary>
    /// AstBuilder is a factory class to create different kinds of AST nodes.
    /// </summary>
    public class AstBuilder : LogSourceBase
    {
        private readonly IAstNodeContainer nodeContainer;

        internal ProfilingSession ProfilingSession { get; set; }

        /// <summary>
        /// Construct a AstBuilder with AST node contiainer.
        /// </summary>
        /// <param name="nodeContainer"></param>
        internal AstBuilder(IAstNodeContainer nodeContainer)
        {
            this.nodeContainer = nodeContainer;
        }

        // Reverse post-order to sort nodes
        private static void MarkNode(NodeModel node, Dictionary<NodeModel, MarkFlag> nodeFlags, Stack<NodeModel> sortedList)
        {
            MarkFlag flag;
            if (!nodeFlags.TryGetValue(node, out flag))
            {
                flag = MarkFlag.NoMark;
                nodeFlags[node] = flag;
            }

            if (MarkFlag.TempMark == flag)
                return;

            if (MarkFlag.NoMark == flag)
            {
                nodeFlags[node] = MarkFlag.TempMark;

                IEnumerable<NodeModel> outputs =
                    node.OutputNodes.Values.SelectMany(set => set.Select(t => t.Item2)).Distinct();
                foreach (NodeModel output in outputs)
                    MarkNode(output, nodeFlags, sortedList);

                nodeFlags[node] = MarkFlag.Marked;
                sortedList.Push(node);
            }
        }

        /// <summary>
        ///     Sort nodes in topological order.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        internal static IEnumerable<NodeModel> TopologicalSort(IEnumerable<NodeModel> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            var sortedNodes = new Stack<NodeModel>();
            IList<NodeModel> nodeModels = nodes as IList<NodeModel> ?? nodes.ToList();

            Dictionary<NodeModel, MarkFlag> nodeFlags = nodeModels.ToDictionary(node => node, _ => MarkFlag.NoMark);

            foreach (NodeModel candidate in GetUnvisitedNodes(nodeFlags))
                MarkNode(candidate, nodeFlags, sortedNodes);

            return sortedNodes.Where(nodeModels.Contains);
        }

        /// <summary>
        /// Starts from the input node as root, do breadth-first and post-order
        /// traversal of the graph (inputs nodes as children nodes). Breadth-first
        /// traversal ensures all inputs nodes are visited in their input order 
        /// and post-order traversal ensures all upstream nodes are visited 
        /// firstly. 
        /// </summary>
        /// <param name="node">Root node</param>
        /// <param name="nodeFlags">Dictionary to record if a node has been visited or not</param>
        /// <param name="sortedNodes">Record all visited nodes</param>
        private static void BfsTraverse(
            NodeModel node, 
            Dictionary<NodeModel, MarkFlag> nodeFlags, 
            Queue<NodeModel> sortedNodes)
        {
            MarkFlag flag;
            if (!nodeFlags.TryGetValue(node, out flag))
            {
                flag = MarkFlag.NoMark;
                nodeFlags[node] = flag;
            }

            if (flag != MarkFlag.NoMark)
                return;

            nodeFlags[node] = MarkFlag.TempMark;

            for (int i = 0; i < node.InPorts.Count; ++i)
            {
                Tuple<int, NodeModel> t;
                if (!node.TryGetInput(i, out t))
                    continue;

                BfsTraverse(t.Item2, nodeFlags, sortedNodes);
            }

            sortedNodes.Enqueue(node);
            nodeFlags[node] = MarkFlag.Marked;
        }

        /// <summary>
        /// Topological sort *the whole graph*. If the input nodes are part of a
        /// graph, it does not promise to generate a good topological order. For 
        /// example, for the following graph:
        /// 
        ///         +---+
        ///         | A | -----+
        ///         +---+      +----> +---+
        ///                           | C |
        ///                    +----> +---+
        ///         +---+      |
        ///         | B | -----+
        ///         +---+
        /// 
        /// Their ideal topological order is A -> B -> C. If the input is only {A, B}, 
        /// it may return {B, A} which is not OK in node to code.
        ///
        /// Note it is much slower than TopologicalSort().
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        internal static IEnumerable<NodeModel> TopologicalSortForGraph(IEnumerable<NodeModel> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            var nodeFlags = nodes.ToDictionary(node => node, _ => MarkFlag.NoMark);
            var sortedNodes = new Queue<NodeModel>();

            // Returns roots of these nodes
            var roots = nodes.Where(n => !n.OutputNodes.Any());
            foreach (NodeModel candidate in roots)
                BfsTraverse(candidate, nodeFlags, sortedNodes);

            foreach (NodeModel candidate in GetUnvisitedNodes(nodeFlags))
                BfsTraverse(candidate, nodeFlags, sortedNodes);

            return sortedNodes;
        }

        private static IEnumerable<NodeModel> GetUnvisitedNodes(Dictionary<NodeModel, MarkFlag> nodeFlags)
        {
            while (true)
            {
                NodeModel candidate = nodeFlags.FirstOrDefault(pair => pair.Value == MarkFlag.NoMark).Key;
                if (candidate != null)
                    yield return candidate;
                else
                    yield break;
            }
        }

        private void CompileToAstNodes(
            NodeModel node, 
            List<AssociativeNode> resultList, 
            CompilationContext context, 
            bool verboseLogging)
        {

            var inputAstNodes = new List<AssociativeNode>();
            var inPortsCount = node.InPorts.Count;
            
            for (int index = 0; index < inPortsCount; index++)
            {
                Tuple<int, NodeModel> inputTuple;

                if (node.TryGetInput(index, out inputTuple))
                {
                    int outputIndexOfInput = inputTuple.Item1;
                    NodeModel inputModel = inputTuple.Item2;
                    AssociativeNode inputNode = inputModel.GetAstIdentifierForOutputIndex(outputIndexOfInput);

                    // If there are any null AST's (for e.g. if there's an error in the input node),
                    // graph update for the given node is skipped.
                    Validity.Assert(inputNode != null, "Shouldn't have null nodes in the AST list");

                    inputAstNodes.Add(inputNode);
                }
                else
                {
                    if (node.InPorts.Count > index)
                    {
                        var port = node.InPorts[index];
                        if (port.UsingDefaultValue && port.DefaultValue != null)
                        {
                            inputAstNodes.Add(port.DefaultValue);
                        }
                        else
                        {
                            inputAstNodes.Add(new NullNode());
                        }
                    }
                    else
                    {
                        Log("Node does not have InPortData at the requested index.");
                    }
                }
            }

            //TODO: This should do something more than just log a generic message. --SJE
            if (node.State == ElementState.Error)
                Log("Error in Node. Not sent for building and compiling");

            if (context == CompilationContext.DeltaExecution)
            {
                OnAstNodeBuilding(node.GUID);
                if (ProfilingSession != null)
                {
                    ProfilingSession.RegisterNode(node);
                    resultList.Add(ProfilingSession.CreatePreCompilationAstNode(node, inputAstNodes));
                }
            }
            else if (context == CompilationContext.PreviewGraph)
            {
                OnAstNodeBuilding(node.GUID);
            }

#if DEBUG
            Validity.Assert(inputAstNodes.All(n => n != null), 
                "Shouldn't have null nodes in the AST list");
#endif

            var scopedNode = node as ScopedNodeModel;
            IEnumerable<AssociativeNode> astNodes = 
                scopedNode != null
                    ? scopedNode.BuildAstInScope(inputAstNodes, verboseLogging, this)
                    : node.BuildAst(inputAstNodes, context);

            foreach (var astNode in astNodes)
            {
                if (astNode.Kind == AstKind.BinaryExpression)
                {
                    (astNode as BinaryExpressionNode).guid = node.GUID;
                }
            }

            if (context == CompilationContext.DeltaExecution)
            {
                resultList.AddRange(astNodes);
                if (ProfilingSession != null)
                {
                    resultList.Add(ProfilingSession.CreatePostCompilationAstNode(node, inputAstNodes));
                }

                OnAstNodeBuilt(node.GUID, resultList);
            }
            else if (context == CompilationContext.PreviewGraph)
            {
                resultList.AddRange(astNodes);

                OnAstNodeBuilt(node.GUID, resultList);
            }
            else if (context == CompilationContext.NodeToCode)
            {
                resultList.AddRange(astNodes);
            }
            else
            {
                // Inside custom node compilation
                bool notified = false;
                foreach (var item in astNodes)
                {
                    if (item is FunctionDefinitionNode)
                    {
                        if (!notified)
                        {
                            OnAstNodeBuilding(node.GUID);
                        }

                        notified = true;

                        // Register the function node in global scope with Graph Sync data,
                        // so that we don't have a function definition inside the function def
                        // of custom node.
                        OnAstNodeBuilt(node.GUID, new[] { item });
                    }
                    else
                    {
                        resultList.Add(item);
                    }
                }

                if (verboseLogging)
                {
                    foreach (var n in resultList)
                    {
                        Log(n.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Compile a collection of NodeModel to AST nodes in different contexts.
        /// If the context is ForNodeToCode, nodes should already be sorted in 
        /// topological order.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="context"></param>
        /// <param name="verboseLogging"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<NodeModel, IEnumerable<AssociativeNode>>> CompileToAstNodes(
            IEnumerable<NodeModel> nodes, 
            CompilationContext context, 
            bool verboseLogging)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            var topScopedNodes = ScopedNodeModel.GetNodesInTopScope(nodes);

            IEnumerable<NodeModel> sortedNodes = null;
            // Node should already be sorted!
            if (context == CompilationContext.NodeToCode)
                sortedNodes = topScopedNodes;
            else
                sortedNodes = TopologicalSort(topScopedNodes);

            if (context == CompilationContext.DeltaExecution)
            {
                sortedNodes = sortedNodes.Where(n => n.IsModified);
            }

            var result = new List<List<AssociativeNode>>();

            foreach (NodeModel node in sortedNodes)
            {
                var astNodes = new List<AssociativeNode>();
                CompileToAstNodes(node, astNodes, context, verboseLogging);
                result.Add(astNodes);
            }

            return result.Zip(
                sortedNodes, 
                (astNodes, node) => Tuple.Create(node, astNodes as IEnumerable<AssociativeNode>));
        }

        /// <summary>
        ///     Compiles a collection of Dynamo nodes into a function definition for a custom node.
        /// </summary>
        /// <param name="functionId"></param>
        /// <param name="returnKeys"></param>
        /// <param name="functionName"></param>
        /// <param name="funcBody"></param>
        /// <param name="outputNodes"></param>
        /// <param name="parameters"></param>
        /// <param name="verboseLogging"></param>
        internal void CompileCustomNodeDefinition(
            Guid functionId, IEnumerable<string> returnKeys, string functionName,
            IEnumerable<NodeModel> funcBody, IEnumerable<AssociativeNode> outputNodes,
            IEnumerable<TypedParameter> parameters, bool verboseLogging)
        {
            OnAstNodeBuilding(functionId);

            var functionBody = new CodeBlockNode();
            var asts = CompileToAstNodes(funcBody, CompilationContext.None, verboseLogging);
            functionBody.Body.AddRange(asts.SelectMany(t => t.Item2));

            var outputs = outputNodes.ToList();
            if (outputs.Count > 1)
            {
                /* rtn_dict = Dictionary.ByKeysValues({key0, ..., keyn}, {out0, ..., outn});
                 * return = rtn_dict;
                 */

                // return dictionary, holds all outputs
                string rtnName = "__temp_rtn_" + functionId.ToString().Replace("-", String.Empty);

                //// indexers for each output
                var indexers = returnKeys != null
                    ? returnKeys.Select(AstFactory.BuildStringNode) as IEnumerable<AssociativeNode>
                    : Enumerable.Range(0, outputs.Count).Select(AstFactory.BuildIntNode);

                // Create AST for Dictionary initialization
                var kvps = outputs.Zip(indexers, (outputId, indexer) =>
                    new KeyValuePair<AssociativeNode, AssociativeNode>(indexer, outputId));
                var dict = new DictionaryExpressionBuilder();
                foreach (var kvp in kvps)
                {
                    dict.AddKey(kvp.Key);
                    dict.AddValue(kvp.Value);
                }
                functionBody.Body.Add(AstFactory.BuildAssignment(AstFactory.BuildIdentifier(rtnName), dict.ToFunctionCall()));

                // finally, return the return array
                functionBody.Body.Add(AstFactory.BuildReturnStatement(AstFactory.BuildIdentifier(rtnName)));
            }
            else
            {
                // For single output, directly return that identifier or null.
                AssociativeNode returnValue = outputs.Count == 1 && outputs[0] != null ? outputs[0] : new NullNode();
                functionBody.Body.Add(AstFactory.BuildReturnStatement(returnValue));
            }

            Type allTypes = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var);

            //Create a new function definition
            var functionDef = new FunctionDefinitionNode
            {
                Name = functionName.Replace("-", string.Empty),
                Signature =
                    new ArgumentSignatureNode
                    {
                        Arguments =
                            parameters.Select(param => AstFactory.BuildParamNode(param.Name, param.Type)).ToList()
                    },
                FunctionBody = functionBody,
                ReturnType = allTypes
            };

            OnAstNodeBuilt(functionId, new[] { functionDef });
        }

        /// <summary>
        ///     Notify IAstNodeContainer that starts building AST nodes.
        /// </summary>
        /// <param name="nodeGuid"></param>
        private void OnAstNodeBuilding(Guid nodeGuid)
        {
            nodeContainer?.OnCompiling(nodeGuid);
        }

        /// <summary>
        ///     Notify IAstNodeContainer that AST nodes have been built.
        /// </summary>
        /// <param name="nodeGuid"></param>
        /// <param name="astNodes"></param>
        private void OnAstNodeBuilt(Guid nodeGuid, IEnumerable<AssociativeNode> astNodes)
        {
            nodeContainer?.OnCompiled(nodeGuid, astNodes);
        }

        private enum MarkFlag
        {
            NoMark,
            TempMark,
            Marked
        }

        internal class StringConstants
        {
            public const string FunctionPrefix = @"__func_";
            public const string VarPrefix = @"var_";
            public const string ShortVarPrefix = @"t";
        }
    }
}
