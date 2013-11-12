using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Nodes;
using GraphToDSCompiler;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSDefinitions;
using ProtoCore.Utils;
using ProtoScript.Runners;
using Double = Dynamo.Nodes.Double;
using String = Dynamo.Nodes.String;

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
            foreach (var guid in _states.Keys)
                _states[guid] = State.NoChange;
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
        void OnAstNodeBuilding(NodeModel node);
        void OnAstNodeBuilt(NodeModel node, IEnumerable<AssociativeNode> astNodes);
    }

    /// <summary>
    /// AstBuilder is a factory class to create different kinds of AST nodes.
    /// </summary>
    public class AstBuilder
    {
        internal class StringConstants
        {
            public const string PARAM_PREFIX = @"p_";
            public const string FUNCTION_PREFIX = @"func_";
            public const string VAR_PREFIX = @"var_";
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
            var flag = nodeFlags[node];

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

            return sortedNodes;
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

        /// <summary>
        /// Compiling a collection of Dynamo nodes to AST nodes, no matter 
        /// whether Dynamo node has been compiled or not.
        /// </summary>
        /// <param name="nodes"></param>
        public void CompileToAstNodes(IEnumerable<NodeModel> nodes)
        {
            var sortedNodes = TopologicalSort(nodes);

            foreach (var node in sortedNodes)
            {
                var inputAstNodes = new List<AssociativeNode>();
                for (int index = 0; index < node.InPortData.Count; ++index)
                {
                    Tuple<int, NodeModel> inputTuple;

                    AssociativeNode inputNode;
                    if (!node.TryGetInput(index, out inputTuple))
                    {
                        inputNode = new NullNode();
                    }
                    else
                    {
                        int outputIndexOfInput = inputTuple.Item1;
                        NodeModel inputModel = inputTuple.Item2;
                        inputNode = inputModel.GetIndexedOutputNode(outputIndexOfInput);
                    }

                    inputAstNodes.Add(inputNode);
                }

                //TODO: This should do something more than just log a generic message. --SJE
                if (node.State == ElementState.ERROR)
                {
                    DynamoLogger.Instance.Log("Error in Node. Not sent for building and compiling");
                }

                OnAstNodeBuilding(node);

                var astNodes = node.BuildAst(inputAstNodes);

                if (astNodes != null)
                {
                    OnAstNodeBuilt(node, astNodes);
                }
            }
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
        /// </param>
        /// <returns></returns>
        private FunctionDefinitionNode BuildPartialFunction(FunctionCallNode func)
        {
            var partialArgs = new List<VarDeclNode>();
            int paramPostfix = 0;

            for (int i = 0; i < func.FormalArguments.Count; ++i)
            {
                if (func.FormalArguments[i] == null)
                {
                    VarDeclNode param = AstFactory.BuildParamNode(StringConstants.PARAM_PREFIX + paramPostfix);
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
                    StringConstants.FUNCTION_PREFIX
                    + Guid.NewGuid().ToString().Replace("-", string.Empty),
                Singnature = new ArgumentSignatureNode { Arguments = partialArgs },
                FunctionBody = funcBody
            };

            return partialFunc;
        }

        /// <summary>
        /// Notify IAstNodeContainer that starts building AST nodes. 
        /// </summary>
        /// <param name="node"></param>
        private void OnAstNodeBuilding(NodeModel node)
        {
            if (_nodeContainer != null)
            {
                _nodeContainer.OnAstNodeBuilding(node);
            }
        }

        /// <summary>
        /// Notify IAstNodeContainer that AST nodes have been built.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="astNodes"></param>
        private void OnAstNodeBuilt(NodeModel node, IEnumerable<AssociativeNode> astNodes)
        {
            if (_nodeContainer != null)
            {
                _nodeContainer.OnAstNodeBuilt(node, astNodes);
            }
        }
    }
}
