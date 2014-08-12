using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
{
    /// <summary>
    ///     SyncDataManager is to manage the state of a Dynamo node and the
    ///     corresponding AST nodes of that Dynamo node. It is responsible for
    ///     generating GraphSyncData that will be consumed by LiveRunner.
    /// </summary>
    internal class SyncDataManager
    {
        private readonly LinkedListOfList<Guid, AssociativeNode> nodes = new LinkedListOfList<Guid, AssociativeNode>();

        private readonly Dictionary<Guid, State> states = new Dictionary<Guid, State>();

        /// <summary>
        ///     Return graph sync data that will be executed by LiveRunner.
        /// </summary>
        /// <returns></returns>
        public GraphSyncData GetSyncData()
        {
            List<Subtree> added = GetSubtrees(State.Added);
            List<Subtree> modified = GetSubtrees(State.Modified);
            List<Subtree> deleted = GetSubtrees(State.Deleted);
            return new GraphSyncData(deleted, added, modified);
        }

        /// <summary>
        ///     Reset states of all nodes to State.NoChange. It should be called
        ///     before each running.
        /// </summary>
        public void ResetStates()
        {
            // Remove all thoses deleted nodes, so if a node is deleted and undo,
            // its state is "Added" instead of "RequestSync".
            var deletedKeys = states.Keys.Where(k => states[k] == State.Deleted).ToList();
            foreach (var key in deletedKeys)
            {
                states.Remove(key);
            }
     
            states.Keys.ToList().ForEach(key => states[key] = State.Clean);
        }

        /// <summary>
        ///     Notify SyncDataManager that is going to add AST nodes.
        /// </summary>
        /// <param name="guid"></param>
        public void MarkForAdding(Guid guid)
        {
            if (states.ContainsKey(guid))
                states[guid] = State.Modified;
            else
                states[guid] = State.Added;
            nodes.Removes(guid);
        }

        /// <summary>
        ///     Add an AST node to the existing AST node list.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="node"></param>
        public void AddNode(Guid guid, AssociativeNode node)
        {
            nodes.AddItem(guid, node);
        }

        /// <summary>
        ///     Delete all AST nodes for this Dynamo node.
        /// </summary>
        /// <param name="guid"></param>
        public void DeleteNodes(Guid guid)
        {
            states[guid] = State.Deleted;
            nodes.Removes(guid);
        }

        private List<Subtree> GetSubtrees(State state)
        {
            List<Guid> guids = states.Where(x => x.Value == state).Select(x => x.Key).ToList();

            return guids.Select(guid => new Subtree(nodes.GetItems(guid), guid)).ToList();
        }

        internal enum State
        {
            Clean,
            Added,
            Modified,
            Deleted
        }
    }
}