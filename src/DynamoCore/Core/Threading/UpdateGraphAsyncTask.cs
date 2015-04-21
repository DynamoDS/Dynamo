using System.Linq;
﻿using System;
using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoScript.Runners;

using BuildWarning = ProtoCore.BuildData.WarningEntry;
using RuntimeWarning = ProtoCore.Runtime.WarningEntry;

namespace Dynamo.Core.Threading
{
    class UpdateGraphAsyncTask : AsyncTask
    {
        #region Class Data Members and Properties

        private GraphSyncData graphSyncData;
        private EngineController engineController;
        private readonly bool verboseLogging;

        internal override TaskPriority Priority
        {
            get { return TaskPriority.AboveNormal; }
        }

        internal IEnumerable<NodeModel> ModifiedNodes { get; private set; }

        #endregion

        #region Public Class Operational Methods

        internal UpdateGraphAsyncTask(IScheduler scheduler, bool verboseLogging) : base(scheduler)
        {
            this.verboseLogging = verboseLogging;
        }

        /// <summary>
        /// This method is called by codes that intent to start a graph update.
        /// This method is called on the main thread where node collection in a 
        /// WorkspaceModel can be safely accessed.
        /// </summary>
        /// <param name="controller">Reference to an instance of EngineController 
        /// to assist in generating GraphSyncData object for the given set of nodes.
        /// </param>
        /// <param name="workspace">Reference to the WorkspaceModel from which a 
        /// set of updated nodes is computed. The EngineController generates the 
        /// resulting GraphSyncData from this list of updated nodes.</param>
        /// <returns>Returns true if there is any GraphSyncData, or false otherwise
        /// (in which case there will be no need to schedule UpdateGraphAsyncTask 
        /// for execution).</returns>
        /// 
        internal bool Initialize(EngineController controller, WorkspaceModel workspace)
        {
            try
            {
                engineController = controller;
                TargetedWorkspace = workspace;

                ModifiedNodes = ComputeModifiedNodes(workspace);
                graphSyncData = engineController.ComputeSyncData(workspace.Nodes, ModifiedNodes, verboseLogging);
                return graphSyncData != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            // Updating graph in the context of ISchedulerThread.
            engineController.UpdateGraphImmediate(graphSyncData);
        }

        protected override void HandleTaskCompletionCore()
        {
            // Retrieve warnings in the context of ISchedulerThread.
            BuildWarnings = engineController.GetBuildWarnings();
            RuntimeWarnings = engineController.GetRuntimeWarnings();

            // Mark all modified nodes as being updated (if the task has been 
            // successfully scheduled, executed and completed, it is expected 
            // for "modifiedNodes" to be both non-null and non-empty.
            // 
            // In addition to marking modified nodes as being updated, their 
            // warning states are cleared (which include the tool-tip). Any node
            // that has build/runtime warnings assigned to it will properly be 
            // restored to warning state when task completion handler sets the 
            // corresponding build/runtime warning on it.
            // 
            foreach (var modifiedNode in ModifiedNodes)
            {
                modifiedNode.IsUpdated = true;
                if (modifiedNode.State == ElementState.Warning)
                    modifiedNode.ClearRuntimeError();
            }
        }

        #endregion

        #region Public Class Properties

        internal WorkspaceModel TargetedWorkspace { get; private set; }
        internal IDictionary<Guid, List<BuildWarning>> BuildWarnings { get; private set; }
        internal IDictionary<Guid, List<RuntimeWarning>> RuntimeWarnings { get; private set; }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// This method is called to gather all the nodes whose cached values 
        /// should be updated after graph update is done. This set of nodes 
        /// includes nodes that are explicitly marked as requiring update, as 
        /// well as all its downstream nodes.
        /// </summary>
        /// <param name="workspace">The WorkspaceModel from which nodes are to
        /// be retrieved.</param>
        /// <returns>Returns a list of NodeModel whose cached values are to be 
        /// updated after the evaluation.</returns>
        /// 
        private static IEnumerable<NodeModel> ComputeModifiedNodes(WorkspaceModel workspace)
        {
            var nodesToUpdate = new List<NodeModel>();
            foreach (var node in workspace.Nodes.Where(n => n.IsModified))
            {
                GetDownstreamNodes(node, nodesToUpdate);
            }

            return nodesToUpdate;
        }

        /// <summary>
        /// Call this method to recursively gather downstream nodes of a given node.
        /// </summary>
        /// <param name="node">A NodeModel whose downstream nodes are to be gathered.</param>
        /// <param name="gathered">A list of all downstream nodes.</param>
        /// 
        private static void GetDownstreamNodes(NodeModel node, ICollection<NodeModel> gathered)
        {
            if (gathered.Contains(node)) // Considered this node before, bail.
                return;

            gathered.Add(node);

            var sets = node.OutputNodes.Values;
            var outputNodes = sets.SelectMany(set => set.Select(t => t.Item2)).Distinct();
            foreach (var outputNode in outputNodes)
            {
                // Recursively get all downstream nodes.
                GetDownstreamNodes(outputNode, gathered);
            }
        }

        #endregion
    }
}
