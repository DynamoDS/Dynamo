using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Models;
using GraphToDSCompiler;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.DSDefinitions;
using ProtoScript.Runners;
using Type = ProtoCore.Type;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// A linked list of list (each node in linked list is a list), and node 
    /// can be accessed through a key. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    internal class LinkedListOfList<TKey, T> : IEnumerable<List<T>>
    {
        private readonly Dictionary<TKey, LinkedListNode<List<T>>> _map;
        private readonly LinkedList<List<T>> _list;

        public LinkedListOfList()
        {
            _map = new Dictionary<TKey, LinkedListNode<List<T>>>();
            _list = new LinkedList<List<T>>();
        }

        public void AddItem(TKey key, T item)
        {
            LinkedListNode<List<T>> listNode;
            if (!_map.TryGetValue(key, out listNode))
            {
                listNode = new LinkedListNode<List<T>>(new List<T>());
                _list.AddLast(listNode);
                _map[key] = listNode;
            }
            listNode.Value.Add(item);
        }

        public bool Contains(TKey key)
        {
            return _map.ContainsKey(key);
        }

        public void Clears(TKey key)
        {
            LinkedListNode<List<T>> listNode;
            if (_map.TryGetValue(key, out listNode))
            {
                listNode.Value.Clear();
            }
        }

        public void Removes(TKey key)
        {
            LinkedListNode<List<T>> listNode;
            if (_map.TryGetValue(key, out listNode))
            {
                _map.Remove(key);
                _list.Remove(listNode);
            }
        }

        public List<T> GetItems(TKey key)
        {
            LinkedListNode<List<T>> listNode;
            if (!_map.TryGetValue(key, out listNode) || listNode.Value == null)
            {
                return null;
            }

            var ret = new List<T>(listNode.Value);
            return ret;
        }

        public List<TKey> GetKeys()
        {
            return new List<TKey>(_map.Keys);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator<List<T>> IEnumerable<List<T>>.GetEnumerator()
        {
            return _list.GetEnumerator();
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
            _states.Keys.ToList().ForEach(key => _states[key] = State.NoChange);
        }

        /// <summary>
        /// Notify SyncDataManager that is going to add AST nodes.
        /// </summary>
        /// <param name="guid"></param>
        public void MarkForAdding(Guid guid)
        {
            if (_states.ContainsKey(guid))
            {
                _states[guid] = State.Modified;
            }
            else
            {
                _states[guid] = State.Added;
            }
            _nodes.Removes(guid);
        }

        /// <summary>
        /// Add an AST node to the existing AST node list.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="node"></param>
        public void AddNode(Guid guid, AssociativeNode node)
        {
            _nodes.AddItem(guid, node);
        }

        /// <summary>
        /// Delete all AST nodes for this Dynamo node.
        /// </summary>
        /// <param name="guid"></param>
        public void DeleteNodes(Guid guid)
        {
            _states[guid] = State.Deleted;
            _nodes.Removes(guid);
        }

        private List<Subtree> GetSubtrees(State state)
        {
            List<Guid> guids = _states.Where(x => x.Value == state)
                                     .Select(x => x.Key)
                                     .ToList();

            return guids.Select(guid => new Subtree(_nodes.GetItems(guid), guid)).ToList();
        }

        private readonly LinkedListOfList<Guid, AssociativeNode> _nodes 
            = new LinkedListOfList<Guid,AssociativeNode>();

        private readonly Dictionary<Guid, State> _states = new Dictionary<Guid,State>();
    }

    /// <summary>
    /// Get notification when AstBuilder starts building node and
    /// finishes building node.
    /// </summary>
    public interface IAstNodeContainer
    {
        void OnAstNodeBuilding(Guid nodeGuid);
        void OnAstNodeBuilt(Guid nodeGuid, IEnumerable<AssociativeNode> astNodes);
    }

    /// <summary>
    /// AstBuilder is a factory class to create different kinds of AST nodes.
    /// </summary>
    public class AstBuilder
    {
        internal class StringConstants
        {
            public const string ParamPrefix = @"p_";
            public const string FunctionPrefix = @"func_";
            public const string VarPrefix = @"var_";
            public const string ShortVarPrefix = @"t_";
            public const string CustomNodeReturnVariable = @"%arr";
        }

        public class ASTBuildingEventArgs : EventArgs
        {
            public ASTBuildingEventArgs(NodeModel node)
            {
                Node = node;
            }

            public NodeModel Node { get; private set; }
        }

        public class ASTBuiltEventArgs: EventArgs
        {
            public ASTBuiltEventArgs(NodeModel node, List<AssociativeNode> astNodes)
            {
                Node = node;
                AstNodes = astNodes;
            }

            public NodeModel Node { get; private set;}
            public List<AssociativeNode> AstNodes { get; private set;}
        }

        private readonly IAstNodeContainer _nodeContainer;

        public AstBuilder(IAstNodeContainer nodeContainer)
        {
            _nodeContainer = nodeContainer;
        }

        private enum MarkFlag
        {
            NoMark,
            TempMark,
            Marked
        }

        // Reverse post-order to sort nodes
        private void MarkNode(NodeModel node, Dictionary<NodeModel, MarkFlag> nodeFlags, Stack<NodeModel> sortedList)
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

                var outputs = node.Outputs.Values.SelectMany(set => set.Select(t => t.Item2)).Distinct();
                foreach (var output in outputs)
                    MarkNode(output, nodeFlags, sortedList);

                nodeFlags[node] = MarkFlag.Marked;
                sortedList.Push(node);
            }
        }

        /// <summary>
        /// Sort nodes in topological order.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public IEnumerable<NodeModel> TopologicalSort(IEnumerable<NodeModel> nodes)
        {
            var sortedNodes = new Stack<NodeModel>();
            var nodeFlags = nodes.ToDictionary(node => node, _ => MarkFlag.NoMark);
            
            foreach (var candidate in TSortCandidates(nodeFlags))
                MarkNode(candidate, nodeFlags, sortedNodes);

            return sortedNodes.Where(n => nodes.Contains(n));
        }

        private IEnumerable<NodeModel> TSortCandidates(Dictionary<NodeModel, MarkFlag> nodeFlags)
        {
            while (true)
            {
                var candidate = nodeFlags.FirstOrDefault(pair => pair.Value == MarkFlag.NoMark).Key;
                if (candidate != null)
                    yield return candidate;
                else
                    yield break;
            }
        }

        private void _CompileToAstNodes(NodeModel node, List<AssociativeNode> resultList, bool isDeltaExecution)
        {
            var inputAstNodes = new List<AssociativeNode>();
            foreach (var index in Enumerable.Range(0, node.InPortData.Count))
            {
                Tuple<int, NodeModel> inputTuple;

                AssociativeNode inputNode;
                if (!node.TryGetInput(index, out inputTuple))
                {
                    inputNode = new NullNode(); 
                    //TODO: partial functions
                }
                else
                {
                    int outputIndexOfInput = inputTuple.Item1;
                    NodeModel inputModel = inputTuple.Item2;
                    inputNode = inputModel.GetAstIdentifierForOutputIndex(outputIndexOfInput);
                }

                inputAstNodes.Add(inputNode);
            }

            //TODO: This should do something more than just log a generic message. --SJE
            if (node.State == ElementState.Error)
            {
                DynamoLogger.Instance.Log("Error in Node. Not sent for building and compiling");
            }

            if (isDeltaExecution)
            {
                OnAstNodeBuilding(node.GUID);
            }

            var astNodes = node.BuildAst(inputAstNodes);
            if (astNodes != null && isDeltaExecution)
            {
                OnAstNodeBuilt(node.GUID, astNodes);
            }

            resultList.AddRange(astNodes ?? new AssociativeNode[0]);
        }

        /// <summary>
        /// Compiling a collection of Dynamo nodes to AST nodes, no matter 
        /// whether Dynamo node has been compiled or not.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="isDeltaExecution"></param>
        public List<AssociativeNode> CompileToAstNodes(IEnumerable<NodeModel> nodes, bool isDeltaExecution)
        {
            // TODO: compile to AST nodes should be triggered after a node is 
            // modified.

            var sortedNodes = TopologicalSort(nodes);

            if (isDeltaExecution)
                sortedNodes = sortedNodes.Where(n => n.isDirty);

            var result = new List<AssociativeNode>();

            foreach (var node in sortedNodes)
            {
                _CompileToAstNodes(node, result, isDeltaExecution);

                if (isDeltaExecution)
                    node.isDirty = false;
            }

            return result;
        }


        /// <summary>
        /// Compiles a collection of Dynamo nodes into a function definition for a custom node.
        /// </summary>
        /// <param name="functionGuid"></param>
        /// <param name="funcBody"></param>
        /// <param name="outputs"></param>
        /// <param name="parameters"></param>
        public void CompileCustomNodeDefinition( 
                        Guid functionGuid, 
                        IEnumerable<NodeModel> funcBody, 
                        List<AssociativeNode> outputs,
                        IEnumerable<string> parameters)
        {
            OnAstNodeBuilding(functionGuid);

            var functionBody = new CodeBlockNode();
            functionBody.Body.AddRange(CompileToAstNodes(funcBody, false));

            AssociativeNode returnValue;
            if (outputs.Count > 1)
            {
                // Return an array for multiple outputs.
                returnValue = AstFactory.BuildExprList(outputs);
            }
            else
            {
                // For single output, directly return that identifier or null.
                returnValue = outputs.Count == 1 ? outputs[0] : new NullNode(); 
            }
            functionBody.Body.Add(AstFactory.BuildReturnStatement(returnValue));

            //Create a new function definition
            var functionDef = new FunctionDefinitionNode
            {
                Name = StringConstants.FunctionPrefix 
                    + functionGuid.ToString().Replace("-", string.Empty),

                Signature = new ArgumentSignatureNode 
                { 
                    Arguments = parameters.Select(p => AstFactory.BuildParamNode(p)).ToList()
                },

                FunctionBody = functionBody,

                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false)
            };

            OnAstNodeBuilt(functionGuid, new[] { functionDef });
        }

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
        /// <returns></returns>
        private FunctionDefinitionNode BuildPartialFunction(FunctionCallNode func)
        {
            var partialArgs = new List<VarDeclNode>();
            int paramPostfix = 0;

            for (int i = 0; i < func.FormalArguments.Count; ++i)
            {
                if (func.FormalArguments[i] == null)
                {
                    VarDeclNode param = AstFactory.BuildParamNode(StringConstants.ParamPrefix + paramPostfix);
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

            var funcBody = new CodeBlockNode();
            {
                var lhs = AstFactory.BuildIdentifier(Keyword.Return);
                var rhs = AstFactory.BuildFunctionCall(func.Function.Name, func.FormalArguments);
                var returnStmt = AstFactory.BuildAssignment(lhs, rhs);
                funcBody.Body.Add(returnStmt);
            }

            var partialFunc = new FunctionDefinitionNode
            {
                IsExternLib = false,
                IsDNI = false,
                ExternLibName = null,
                Name =
                    StringConstants.FunctionPrefix
                    + Guid.NewGuid().ToString().Replace("-", string.Empty),
                Signature = new ArgumentSignatureNode { Arguments = partialArgs },
                FunctionBody = funcBody
            };

            return partialFunc;
        }

        /// <summary>
        /// Notify IAstNodeContainer that starts building AST nodes. 
        /// </summary>
        /// <param name="nodeGuid"></param>
        private void OnAstNodeBuilding(Guid nodeGuid)
        {
            if (_nodeContainer != null)
            {
                _nodeContainer.OnAstNodeBuilding(nodeGuid);
            }
        }

        /// <summary>
        /// Notify IAstNodeContainer that AST nodes have been built.
        /// </summary>
        /// <param name="nodeGuid"></param>
        /// <param name="astNodes"></param>
        private void OnAstNodeBuilt(Guid nodeGuid, IEnumerable<AssociativeNode> astNodes)
        {
            if (_nodeContainer != null)
            {
                _nodeContainer.OnAstNodeBuilt(nodeGuid, astNodes);
            }
        }
    }
}
