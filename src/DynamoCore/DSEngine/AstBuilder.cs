using System;
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
    /// 
    /// Internally it keeps a mapping between Dynamo node and the corresponding
    /// DesignScript AST nodes. 
    /// </summary>
    public class AstBuilder : IAstBuilder
    {
        internal class StringConstants
        {
            public const string kParamPrefix = @"p_";
            public const string kFunctionPrefix = @"func_";
            public const string kVarPrefix = @"var_";
        }

        internal enum NodeState
        {
            NoChange,
            Added,
            Modified,
            Deleted
        }

        public static AstBuilder Instance = new AstBuilder();
        private LinkedListOfList<Guid, AssociativeNode> astNodes;
        private Dictionary<Guid, NodeState> nodeStates;

        private AstBuilder()
        {
            astNodes = new LinkedListOfList<Guid, AssociativeNode>();
            nodeStates = new Dictionary<Guid, NodeState>();

            dynSettings.Controller.DynamoModel.NodeDeleted += this.OnNodeDeleted;
        }

        private void AddNode(Guid dynamoNodeId, AssociativeNode astNode)
        {
            astNodes.AddItem(dynamoNodeId, astNode);
        }

        private void RemoveAstNodes(Guid dynamoNodeId)
        {
            astNodes.Removes(dynamoNodeId); 
        }

        private void ClearAstNodes(Guid dynamoNodeId)
        {
            astNodes.Clears(dynamoNodeId);
        }

        private List<Subtree> GetSubtreesForState(NodeState state)
        {
            List<Subtree> subtrees = new List<Subtree>();
            List<Guid> addedGuids = nodeStates.Where(x => x.Value == state).Select(x => x.Key).ToList();
            foreach (var guid in addedGuids)
            {
                var nodes = astNodes.GetItems(guid);
                Subtree tree = new Subtree(nodes, guid);
                subtrees.Add(tree);
            }

            return subtrees;
        }

        public List<Guid> ToBeQueriedNodes
        {
            get
            {
                var nodes = nodeStates.Where(x => x.Value != NodeState.Deleted)
                                    .Select(x => x.Key)
                                    .ToList();
                return nodes;
            }
        }

        public GraphSyncData SyncData
        {
            get
            {
                var added = GetSubtreesForState(NodeState.Added);
                var modified = GetSubtreesForState(NodeState.Modified);
                var deleted = GetSubtreesForState(NodeState.Deleted);
                GraphSyncData syncData = new GraphSyncData(deleted, added, modified);
                return syncData;
            }
        }

        /// <summary>
        /// Dump code
        /// </summary>
        public string DumpCode()
        {
            List<AssociativeNode> allAstNodes = new List<AssociativeNode>();
            foreach (var item in astNodes)
            {
                allAstNodes.AddRange(item);
            }
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(allAstNodes);
            string code = codegen.GenerateCode();
            return code;
        }

        #region IAstBuilder interface
        public void Build(NodeModel node, List<AssociativeNode> inputs)
        {
            StartBuildingAstNodes(node);

            var rhs = AstFactory.BuildNullNode();
            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);
            AddNode(node.GUID, assignment);
        }

        public void Build(DSFunction node, List<AssociativeNode> inputs)
        {
            StartBuildingAstNodes(node);

            string function = node.Definition.Name;
            AssociativeNode functionCall = AstFactory.BuildFunctionCall(function, inputs);

            if (node.IsStaticMember() || node.IsConstructor())
            {
                IdentifierNode classNode = new IdentifierNode(node.Definition.ClassName);
                functionCall = CoreUtils.GenerateCallDotNode(classNode, functionCall as FunctionCallNode, LiveRunnerServices.Instance.Core);
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
                functionCall= CoreUtils.GenerateCallDotNode(thisNode, functionCall as FunctionCallNode, LiveRunnerServices.Instance.Core);
            }

            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, functionCall);
            AddNode(node.GUID, assignment);
        }

        public void Build(Dynamo.Nodes.Double node, List<AssociativeNode> inputs)
        {
            StartBuildingAstNodes(node);

            var rhs = AstFactory.BuildDoubleNode(node.Value);
            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);
            AddNode(node.GUID, assignment);
        }

        public void Build(Dynamo.Nodes.DoubleInput node, List<AssociativeNode> inputs)
        {
            StartBuildingAstNodes(node);

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
            AddNode(node.GUID, assignment);
        }

        public void Build(Dynamo.Nodes.Bool node, List<AssociativeNode> inputs)
        {
            StartBuildingAstNodes(node);

            var rhs =  AstFactory.BuildBooleanNode(node.Value);
            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);
            AddNode(node.GUID, assignment);
        }

        public void Build(Dynamo.Nodes.String node, List<AssociativeNode> inputs)
        {
            StartBuildingAstNodes(node);

            var rhs = AstFactory.BuildStringNode(node.Value);
            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);
            AddNode(node.GUID, assignment);
        }

        public void Build(Dynamo.Nodes.CodeBlockNodeModel node, List<AssociativeNode> inputs)
        {
            StartBuildingAstNodes(node);

            Dictionary<int, List<GraphToDSCompiler.VariableLine>> unboundIdentifiers;
            unboundIdentifiers = new Dictionary<int, List<GraphToDSCompiler.VariableLine>>();
            List<ProtoCore.AST.Node> resultNodes;
            GraphToDSCompiler.GraphUtilities.ParseCodeBlockNodeStatements(node.Code, unboundIdentifiers, out resultNodes);

            foreach (var astNode in resultNodes)
            {
                AddNode(node.GUID, (astNode as AssociativeNode));
            }
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
                    VarDeclNode param = AstFactory.BuildParamNode(AstBuilder.StringConstants.kParamPrefix + paramPostfix);
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
            partialFunc.Name = StringConstants.kFunctionPrefix + Guid.NewGuid().ToString().Replace("-", string.Empty);
            partialFunc.Singnature = new ArgumentSignatureNode();
            partialFunc.Singnature.Arguments = partialArgs;
            partialFunc.FunctionBody = funcBody;

            return partialFunc;
        }

        /*
        public void BuildEvaluation(NodeModel node, AssociativeNode rhs, bool isPartial = false)
        {
            // If it is a partially applied function, need to create a function 
            // definition and function pointer.
            if (isPartial && rhs is FunctionCallNode)
            {
                FunctionCallNode funcCall = rhs as FunctionCallNode;
                // create a function definition for it
                var newFunc = BuildPartialFunction(funcCall);
                AddNode(node.GUID, newFunc);

                // create a function pointer for this node
                rhs = AstFactory.BuildIdentifier(newFunc.Name);
            }

            var assignment = AstFactory.BuildAssignment(node.AstIdentifier, rhs);
            AddNode(node.GUID, assignment);
        }
        */

        public void BeginBuildingAst()
        {
            List<Guid> keys = new List<Guid>(nodeStates.Keys);

            foreach (var node in keys)
            {
                nodeStates[node] = NodeState.NoChange;
            }
        }

        public void FinishBuildingAst()
        {

        }

        private void StartBuildingAstNodes(NodeModel node)
        {
            RemoveAstNodes(node.GUID);

            if (nodeStates.ContainsKey(node.GUID))
            {
                nodeStates[node.GUID] = NodeState.Modified;
            }
            else
            {
                nodeStates[node.GUID] = NodeState.Added;
            }
        }

        public void OnNodeDeleted(NodeModel node)
        {
            RemoveAstNodes(node.GUID);
            nodeStates[node.GUID] = NodeState.Deleted;
        }
    }
}
