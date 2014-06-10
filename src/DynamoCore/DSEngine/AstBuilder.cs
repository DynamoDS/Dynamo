﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using Type = ProtoCore.Type;

#endregion

namespace Dynamo.DSEngine
{
    /// <summary>
    ///     Get notification when AstBuilder starts building node and
    ///     finishes building node.
    /// </summary>
    public interface IAstNodeContainer
    {
        void OnAstNodeBuilding(Guid nodeGuid);
        void OnAstNodeBuilt(Guid nodeGuid, IEnumerable<AssociativeNode> astNodes);
    }

    /// <summary>
    ///     AstBuilder is a factory class to create different kinds of AST nodes.
    /// </summary>
    public class AstBuilder
    {
        private readonly IAstNodeContainer nodeContainer;

        public AstBuilder(IAstNodeContainer nodeContainer)
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
                    node.Outputs.Values.SelectMany(set => set.Select(t => t.Item2)).Distinct();
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
        public static IEnumerable<NodeModel> TopologicalSort(IEnumerable<NodeModel> nodes)
        {
            var sortedNodes = new Stack<NodeModel>();
            IList<NodeModel> nodeModels = nodes as IList<NodeModel> ?? nodes.ToList();

            Dictionary<NodeModel, MarkFlag> nodeFlags = nodeModels.ToDictionary(node => node, _ => MarkFlag.NoMark);

            foreach (NodeModel candidate in SortCandidates(nodeFlags))
                MarkNode(candidate, nodeFlags, sortedNodes);

            return sortedNodes.Where(nodeModels.Contains);
        }

        private static IEnumerable<NodeModel> SortCandidates(Dictionary<NodeModel, MarkFlag> nodeFlags)
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

        private void _CompileToAstNodes(NodeModel node, List<AssociativeNode> resultList, bool isDeltaExecution)
        {

            var inputAstNodes = new List<AssociativeNode>();
            foreach (int index in Enumerable.Range(0, node.InPortData.Count))
            {
                Tuple<int, NodeModel> inputTuple;

                if (node.TryGetInput(index, out inputTuple))
                {
                    int outputIndexOfInput = inputTuple.Item1;
                    NodeModel inputModel = inputTuple.Item2;
                    AssociativeNode inputNode = inputModel.GetAstIdentifierForOutputIndex(outputIndexOfInput);

#if DEBUG
                    Validity.Assert(inputNode != null,
                        "Shouldn't have null nodes in the AST list");
#endif
                    inputAstNodes.Add(inputNode);
                }
                else
                {
                    PortData port = node.InPortData[index];
                    inputAstNodes.Add(
                        port.HasDefaultValue
                            ? AstFactory.BuildPrimitiveNodeFromObject(port.DefaultValue)
                            : new NullNode());
                }
            }

            //TODO: This should do something more than just log a generic message. --SJE
            if (node.State == ElementState.Error)
                dynSettings.DynamoLogger.Log("Error in Node. Not sent for building and compiling");

            if (isDeltaExecution)
                OnAstNodeBuilding(node.GUID);

#if DEBUG
            Validity.Assert(!inputAstNodes.Any((n) => n == null), 
                "Shouldn't have null nodes in the AST list");
#endif

            IEnumerable<AssociativeNode> astNodes = node.BuildAst(inputAstNodes);
            
            if (dynSettings.Controller.DebugSettings.VerboseLogging)
            {
                foreach (var n in astNodes)
                {
                    dynSettings.DynamoLogger.Log(n.ToString());
                }
            }

            if(null == astNodes)
                resultList.AddRange(new AssociativeNode[0]);
            else if (isDeltaExecution)
            {
                OnAstNodeBuilt(node.GUID, astNodes);
                resultList.AddRange(astNodes);
            }
            else //Inside custom node compilation.
            {
                bool notified = false;
                foreach (var item in astNodes)
                {
                    if (item is FunctionDefinitionNode)
                    {
                        if (!notified)
                            OnAstNodeBuilding(node.GUID);
                        notified = true;
                        //Register the function node in global scope with Graph Sync data,
                        //so that we don't have a function definition inside the function def
                        //of custom node.
                        OnAstNodeBuilt(node.GUID, new AssociativeNode[] { item });
                    }
                    else
                        resultList.Add(item);
                }
            }
        }

        /// <summary>
        ///     Compiling a collection of Dynamo nodes to AST nodes, no matter
        ///     whether Dynamo node has been compiled or not.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="isDeltaExecution"></param>
        public List<AssociativeNode> CompileToAstNodes(IEnumerable<NodeModel> nodes, bool isDeltaExecution)
        {
            // TODO: compile to AST nodes should be triggered after a node is 
            // modified.

            IEnumerable<NodeModel> sortedNodes = TopologicalSort(nodes);

            if (isDeltaExecution)
            {
                sortedNodes = sortedNodes.Where(n => n.RequiresRecalc || n.ForceReExecuteOfNode);
            }

            var result = new List<AssociativeNode>();

            foreach (NodeModel node in sortedNodes)
            {
                _CompileToAstNodes(node, result, isDeltaExecution);

                if (isDeltaExecution)
                    node.RequiresRecalc = false;
            }

            return result;
        }


        /// <summary>
        ///     Compiles a collection of Dynamo nodes into a function definition for a custom node.
        /// </summary>
        /// <param name="def"></param>
        /// <param name="funcBody"></param>
        /// <param name="outputs"></param>
        /// <param name="parameters"></param>
        public void CompileCustomNodeDefinition(
            CustomNodeDefinition def, IEnumerable<NodeModel> funcBody, List<AssociativeNode> outputs,
            IEnumerable<string> parameters)
        {
            OnAstNodeBuilding(def.FunctionId);

            var functionBody = new CodeBlockNode();
            functionBody.Body.AddRange(CompileToAstNodes(funcBody, false));

            if (outputs.Count > 1)
            {
                /* rtn_array = {};
                 * rtn_array[key0] = out0;
                 * rtn_array[key1] = out1;
                 * ...
                 * return = rtn_array;
                 */

                // return array, holds all outputs
                string rtnName = "__temp_rtn_" + def.FunctionId.ToString().Replace("-", String.Empty); 
                functionBody.Body.Add(
                    AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(rtnName),
                        AstFactory.BuildExprList(new List<string>())));

                // indexers for each output
                IEnumerable<AssociativeNode> indexers = def.ReturnKeys != null
                    ? def.ReturnKeys.Select(AstFactory.BuildStringNode) as IEnumerable<AssociativeNode>
                    : Enumerable.Range(0, outputs.Count).Select(AstFactory.BuildIntNode);

                functionBody.Body.AddRange(
                    outputs.Zip(
                        indexers,
                        (outputId, indexer) => // for each outputId and return key
                            // pack the output into the return array
                            AstFactory.BuildAssignment(AstFactory.BuildIdentifier(rtnName, indexer), outputId)));

                // finally, return the return array
                functionBody.Body.Add(AstFactory.BuildReturnStatement(AstFactory.BuildIdentifier(rtnName)));
            }
            else
            {
                // For single output, directly return that identifier or null.
                AssociativeNode returnValue = outputs.Count == 1 ? outputs[0] : new NullNode();
                functionBody.Body.Add(AstFactory.BuildReturnStatement(returnValue));
            }

            Type allTypes = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar);

            //Create a new function definition
            var functionDef = new FunctionDefinitionNode
            {
                Name = def.FunctionName.Replace("-", string.Empty),
                Signature =
                    new ArgumentSignatureNode
                    {
                        Arguments =
                            parameters.Select(param => AstFactory.BuildParamNode(param, allTypes)).ToList()
                    },
                FunctionBody = functionBody,
                ReturnType = allTypes
            };

            OnAstNodeBuilt(def.FunctionId, new[] { functionDef });
        }

        /// <summary>
        ///     Notify IAstNodeContainer that starts building AST nodes.
        /// </summary>
        /// <param name="nodeGuid"></param>
        private void OnAstNodeBuilding(Guid nodeGuid)
        {
            if (nodeContainer != null)
                nodeContainer.OnAstNodeBuilding(nodeGuid);
        }

        /// <summary>
        ///     Notify IAstNodeContainer that AST nodes have been built.
        /// </summary>
        /// <param name="nodeGuid"></param>
        /// <param name="astNodes"></param>
        private void OnAstNodeBuilt(Guid nodeGuid, IEnumerable<AssociativeNode> astNodes)
        {
            if (nodeContainer != null)
                nodeContainer.OnAstNodeBuilt(nodeGuid, astNodes);
        }

        public class ASTBuildingEventArgs : EventArgs
        {
            public ASTBuildingEventArgs(NodeModel node)
            {
                Node = node;
            }

            public NodeModel Node { get; private set; }
        }

        public class ASTBuiltEventArgs : EventArgs
        {
            public ASTBuiltEventArgs(NodeModel node, IEnumerable<AssociativeNode> astNodes)
            {
                Node = node;
                AstNodes = astNodes;
            }

            public NodeModel Node { get; private set; }
            public IEnumerable<AssociativeNode> AstNodes { get; private set; }
        }

        private enum MarkFlag
        {
            NoMark,
            TempMark,
            Marked
        }

        internal class StringConstants
        {
            public const string ParamPrefix = @"p_";
            public const string FunctionPrefix = @"func_";
            public const string VarPrefix = @"var_";
            public const string ShortVarPrefix = @"t_";
            public const string CustomNodeReturnVariable = @"%arr";
        }
    }
}
