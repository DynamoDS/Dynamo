﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using ProtoCore;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Exceptions;
using ProtoCore.Lang;
using ProtoCore.Mirror;
using ProtoCore.Utils;
using ProtoFFI;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// A linked list of list (each node in linked list is a list), and node 
    /// can be accessed through a key. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LinkedListOfList<Key, T> : IEnumerable<List<T>>
    {
        private Dictionary<Key, LinkedListNode<List<T>>> map;
        private LinkedList<List<T>> list;

        public LinkedListOfList()
        {
            map = new Dictionary<Key, LinkedListNode<List<T>>>();
            list = new LinkedList<List<T>>();
        }

        public void AddItem(Key key, T item)
        {
            LinkedListNode<List<T>> listNode;
            if (!map.TryGetValue(key, out listNode))
            {
                listNode = new LinkedListNode<List<T>>(new List<T>());
                list.AddLast(listNode);
                map[key] = listNode;
            }
            listNode.Value.Add(item);
        }

        public bool Contains(Key key)
        {
            return map.ContainsKey(key);
        }

        public void Clears(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (map.TryGetValue(key, out listNode))
            {
                listNode.Value.Clear();
            }
        }

        public void Removes(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (map.TryGetValue(key, out listNode))
            {
                map.Remove(key);
                list.Remove(listNode);
            }
        }

        public List<T> GetItems(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (!map.TryGetValue(key, out listNode) || listNode.Value == null)
            {
                return null;
            }

            List<T> ret = new List<T>(listNode.Value);
            return ret;
        }

        public List<Key> GetKeys()
        {
            return new List<Key>(map.Keys);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<List<T>> IEnumerable<List<T>>.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

    /// <summary>
    /// SyncDataManager is to manage the state of a Dynamo node and the 
    /// corresponding AST nodes of that Dynamo node. It is responsible for 
    /// generating GraphSyncData that will be consumed by LiveRunner.
    /// </summary>
    internal class SyncDataManager
    {
        internal enum State
        {
            NoChange,
            Added,
            Modified,
            Deleted
        }

        public SyncDataManager()
        {
        }

        /// <summary>
        /// Return graph sync data that will be executed by LiveRunner.
        /// </summary>
        /// <returns></returns>
        public GraphSyncData GetSyncData()
        {
            var added = GetSubtrees(State.Added);
            var modified = GetSubtrees(State.Modified);
            var deleted = GetSubtrees(State.Deleted);
            return new GraphSyncData(deleted, added, modified);
        }

        /// <summary>
        /// Reset states of all nodes to State.NoChange. It should be called
        /// before each running. 
        /// </summary>
        public void ResetStates()
        {
            List<Guid> guids = new List<Guid>(states.Keys);
            foreach (var guid in guids)
            {
                states[guid] = State.NoChange;
            }
        }

        /// <summary>
        /// Notify SyncDataManager that is going to add AST nodes.
        /// </summary>
        /// <param name="guid"></param>
        public void MarkForAdding(Guid guid)
        {
            if (states.ContainsKey(guid))
            {
                states[guid] = State.Modified;
            }
            else
            {
                states[guid] = State.Added;
            }
            nodes.Removes(guid);
        }

        /// <summary>
        /// Add an AST node to the existing AST node list.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="node"></param>
        public void AddNode(Guid guid, AssociativeNode node)
        {
            nodes.AddItem(guid, node);
        }

        /// <summary>
        /// Delete all AST nodes for this Dynamo node.
        /// </summary>
        /// <param name="guid"></param>
        public void DeleteNodes(Guid guid)
        {
            states[guid] = State.Deleted;
            nodes.Removes(guid);
        }

        private List<Subtree> GetSubtrees(State state)
        {
            List<Guid> guids = states.Where(x => x.Value == state)
                                     .Select(x => x.Key)
                                     .ToList();

            List<Subtree> subtrees = new List<Subtree>();
            foreach (var guid in guids)
            {
                Subtree tree = new Subtree(nodes.GetItems(guid), guid);
                subtrees.Add(tree);
            }

            return subtrees;
        }

        private LinkedListOfList<Guid, AssociativeNode> nodes = new LinkedListOfList<Guid,AssociativeNode>();
        private Dictionary<Guid, State> states = new Dictionary<Guid,State>();
    }

    /// <summary>
    /// Generate ast nodes
    /// </summary>
    public interface IAstBuilder
    {
        void Build(NodeModel node, List<AssociativeNode> inputs);
        void Build(Dynamo.Nodes.DSFunction node, List<AssociativeNode> inputs);
        void Build(Dynamo.Nodes.Double node, List<AssociativeNode> inputs);
        void Build(Dynamo.Nodes.Bool node, List<AssociativeNode> inputs);
        void Build(Dynamo.Nodes.String node, List<AssociativeNode> inputs);
        void Build(Dynamo.Nodes.DoubleInput node, List<AssociativeNode> inputs);
        void Build(Dynamo.Nodes.CodeBlockNodeModel node, List<AssociativeNode> inputs);
    }

    /// <summary>
    /// AstBuilder is a factory class to create different kinds of AST nodes.
    /// </summary>
    public class AstBuilder : IAstBuilder
    {
        internal class StringConstants
        {
            public const string ParamPrefix = @"p_";
            public const string FunctionPrefix = @"func_";
            public const string VarPrefix = @"var_";
        }

        public class ASTBuildingEventArgs : EventArgs
        {
            public ASTBuildingEventArgs(NodeModel node)
            {
                this.Node = node;
            }

            public NodeModel Node { get; private set; }
        }

        public class ASTBuiltEventArgs: EventArgs
        {
            public ASTBuiltEventArgs(NodeModel node, List<AssociativeNode> astNodes)
            {
                this.Node = node;
                this.AstNodes = astNodes;
            }

            public NodeModel Node { get; private set;}
            public List<AssociativeNode> AstNodes { get; private set;}
        }

        public event EventHandler<ASTBuildingEventArgs> AstBuilding;
        public event EventHandler<ASTBuiltEventArgs> AstBuilt;
        
        public AstBuilder()
        {
        }

        #region IAstBuilder interface
        /// <summary>
        /// Build default AST node for Dynamo node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inputs"></param>
        public void Build(NodeModel node, List<AssociativeNode> inputs)
        {
            ASTBuildingEventArgs buildingEventArgs = new ASTBuildingEventArgs(node);
            OnAstBuilding(buildingEventArgs);

            var rhs = AstFactory.BuildNullNode();
            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);

            ASTBuiltEventArgs builtEventArgs = new ASTBuiltEventArgs(node, new List<AssociativeNode> { assignment });
            OnAstBuilt(builtEventArgs);
        }

        /// <summary>
        /// Build AST node for DSFunction node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inputs"></param>
        public void Build(DSFunction node, List<AssociativeNode> inputs)
        {
            ASTBuildingEventArgs buildingEventArgs = new ASTBuildingEventArgs(node);
            OnAstBuilding(buildingEventArgs);

            string function = node.Definition.Name;
            var functionCall = AstFactory.BuildFunctionCall(function, inputs);

            if (node.IsStaticMember() || node.IsConstructor())
            {
                IdentifierNode classNode = new IdentifierNode(node.Definition.ClassName);
                functionCall = CoreUtils.GenerateCallDotNode(classNode, 
                    functionCall as FunctionCallNode, 
                    EngineController.Instance.LiveRunnerCore);
            }
            else if (node.IsInstanceMember())
            {
                AssociativeNode thisNode = new NullNode(); 
                if (inputs.Count >= 1)
                {
                    thisNode = inputs[0];
                    inputs.RemoveAt(0);  // remove this pointer
                }
                functionCall = AstFactory.BuildFunctionCall(function, inputs);
                functionCall= CoreUtils.GenerateCallDotNode(thisNode, 
                    functionCall as FunctionCallNode, 
                    EngineController.Instance.LiveRunnerCore);
            }

            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, functionCall);

            ASTBuiltEventArgs builtEventArgs = new ASTBuiltEventArgs(node, new List<AssociativeNode> { assignment });
            OnAstBuilt(builtEventArgs);
        }

        /// <summary>
        /// Build AST node for Double node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inputs"></param>
        public void Build(Dynamo.Nodes.Double node, List<AssociativeNode> inputs)
        {
            ASTBuildingEventArgs buildingEventArgs = new ASTBuildingEventArgs(node);
            OnAstBuilding(buildingEventArgs);

            var rhs = AstFactory.BuildDoubleNode(node.Value);
            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);

            ASTBuiltEventArgs builtEventArgs = new ASTBuiltEventArgs(node, new List<AssociativeNode> { assignment });
            OnAstBuilt(builtEventArgs);
        }

        /// <summary>
        /// Build AST node for DoubleInput node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inputs"></param>
        public void Build(Dynamo.Nodes.DoubleInput node, List<AssociativeNode> inputs)
        {
            ASTBuildingEventArgs buildingEventArgs = new ASTBuildingEventArgs(node);
            OnAstBuilding(buildingEventArgs);

            AssociativeNode rhs = null;
            if (inputs.Count == 1)
            {
                rhs = inputs[0];
            }
            else
            {
                rhs = AstFactory.BuildExprList(inputs);
            }

            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);

            ASTBuiltEventArgs builtEventArgs = new ASTBuiltEventArgs(node, new List<AssociativeNode> { assignment });
            OnAstBuilt(builtEventArgs);
        }

        /// <summary>
        /// Build AST node for bool node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inputs"></param>
        public void Build(Dynamo.Nodes.Bool node, List<AssociativeNode> inputs)
        {
            ASTBuildingEventArgs buildingEventArgs = new ASTBuildingEventArgs(node);
            OnAstBuilding(buildingEventArgs);

            var rhs =  AstFactory.BuildBooleanNode(node.Value);
            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);

            ASTBuiltEventArgs builtEventArgs = new ASTBuiltEventArgs(node, new List<AssociativeNode> { assignment });
            OnAstBuilt(builtEventArgs);
        }

        /// <summary>
        /// Build AST node for String node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inputs"></param>
        public void Build(Dynamo.Nodes.String node, List<AssociativeNode> inputs)
        {
            ASTBuildingEventArgs buildingEventArgs = new ASTBuildingEventArgs(node);
            OnAstBuilding(buildingEventArgs);

            var rhs = AstFactory.BuildStringNode(node.Value);
            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);

            ASTBuiltEventArgs builtEventArgs = new ASTBuiltEventArgs(node, new List<AssociativeNode> { assignment });
            OnAstBuilt(builtEventArgs);
        }

        /// <summary>
        /// Build AST node for code block node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inputs"></param>
        public void Build(Dynamo.Nodes.CodeBlockNodeModel node, List<AssociativeNode> inputs)
        {
            if (node.State == ElementState.ERROR)
            {
                DynamoLogger.Instance.Log("Error in Code Block Node. Not sent for building and compiling");
            }

            ASTBuildingEventArgs buildingEventArgs = new ASTBuildingEventArgs(node);
            OnAstBuilding(buildingEventArgs);

            List<string> unboundIdentifiers = new List<string>();
            List<ProtoCore.AST.Node> resultNodes = new List<Node>();
            List<ProtoCore.BuildData.ErrorEntry> errors;
            List<ProtoCore.BuildData.WarningEntry> warnings;
            GraphToDSCompiler.GraphUtilities.Parse(node.Code, out resultNodes, out errors, out  warnings, unboundIdentifiers);
            
            List<AssociativeNode> astNodes = resultNodes.ConvertAll(n => n as AssociativeNode);
            ASTBuiltEventArgs builtEventArgs = new ASTBuiltEventArgs(node, astNodes);
            OnAstBuilt(builtEventArgs);

        }
        #endregion

        /// <summary>
        /// Create a function defintion for a partially applied function call. 
        /// E.g.
        /// 
        ///     foo(?, x, y, ?, z);
        ///     
        ///  ->
        ///
        ///     def foo_guid(p_1, p_2) = foo(p_1, x, y, p_2, z);
        ///     
        /// </summary>
        /// <param name="func"></param>
        /// </param>
        /// <returns></returns>
        private FunctionDefinitionNode BuildPartialFunction(FunctionCallNode func)
        {
            List<VarDeclNode> partialArgs = new List<VarDeclNode>();
            int paramPostfix = 0;

            for (int i = 0; i < func.FormalArguments.Count; ++i)
            {
                if (func.FormalArguments[i] == null)
                {
                    VarDeclNode param = AstFactory.BuildParamNode(AstBuilder.StringConstants.ParamPrefix + paramPostfix);
                    partialArgs.Add(param);

                    func.FormalArguments[i] = param.NameNode;
                    paramPostfix++;
                }
            }

            // It is not a partial function call. 
            if (paramPostfix == 0)
            {
                return null;
            }

            CodeBlockNode funcBody = new CodeBlockNode();
            {
                var lhs = AstFactory.BuildIdentifier(ProtoCore.DSDefinitions.Keyword.Return);
                var rhs = AstFactory.BuildFunctionCall(func.Function.Name, func.FormalArguments);
                var returnStmt = AstFactory.BuildAssignment(lhs, rhs);
                funcBody.Body.Add(returnStmt);
            }

            FunctionDefinitionNode partialFunc = new FunctionDefinitionNode();
            partialFunc.IsExternLib = false;
            partialFunc.IsDNI = false;
            partialFunc.ExternLibName = null;
            partialFunc.Name = StringConstants.FunctionPrefix + Guid.NewGuid().ToString().Replace("-", string.Empty);
            partialFunc.Singnature = new ArgumentSignatureNode();
            partialFunc.Singnature.Arguments = partialArgs;
            partialFunc.FunctionBody = funcBody;

            return partialFunc;
        }

        /// <summary>
        /// Dispatch AstBuilding event to event handlers.
        /// </summary>
        /// <param name="e"></param>
        private void OnAstBuilding(ASTBuildingEventArgs e)
        {
            if (AstBuilding != null)
            {
                AstBuilding(this, e);
            }
        }

        /// <summary>
        /// Dispatch AstBuilt event to event handlers.
        /// </summary>
        /// <param name="e"></param>
        private void OnAstBuilt(ASTBuiltEventArgs e)
        {
            if (AstBuilt != null)
            {
                AstBuilt(this, e);
            }
        }
    }
}
