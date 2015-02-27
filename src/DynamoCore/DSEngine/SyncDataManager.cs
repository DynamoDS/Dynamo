using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
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

        // states :: Dictionary<Guid, State>
        // It is a dictionary for the state of each UI node. We use 
        // OrderedDictionary here is because we want to keep the order of
        // state change. 
        private OrderedDictionary states = new OrderedDictionary();

        /// <summary>
        ///     Return graph sync data that will be executed by LiveRunner.
        /// </summary>
        /// <returns></returns>
        public GraphSyncData GetSyncData()
        {
            var added = GetSubtrees(State.Added).ToList();
            var modified = GetSubtrees(State.Modified).ToList();
            var deleted = GetSubtrees(State.Deleted).ToList();
            return new GraphSyncData(deleted, added, modified);
        }

        /// <summary>
        ///     Reset states of all nodes to State.NoChange. It should be called
        ///     before each running.
        /// </summary>
        public void ResetStates()
        {
            // Remove all thoses deleted nodes and mark the states of remaining
            // nodes as clean.
            var newStates = new OrderedDictionary();
            foreach (Guid guid in states.Keys)
            {
                State state = (State)states[guid];
                if (state != State.Deleted)
                {
                    newStates.Add(guid, State.Clean);
                }
            }

            states = newStates;
        }

        /// <summary>
        ///     Notify SyncDataManager that is going to add AST nodes.
        /// </summary>
        /// <param name="guid"></param>
        public void MarkForAdding(Guid guid)
        {
            if (states.Contains(guid))
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

        private IEnumerable<Subtree> GetSubtrees(State state)
        {
            return states.Keys
                         .Cast<Guid>()
                         .Where(guid => (State)states[guid] == state)
                         .Select(guid => new Subtree(nodes.GetItems(guid), guid));
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