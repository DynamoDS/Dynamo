using System.Diagnostics;
using Autodesk.DesignScript.Interfaces;

using Dynamo.DSEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Core.Threading
{
    /// <summary>
    /// This task is scheduled after one or more UpdateRenderPackageAsyncTask 
    /// objects are scheduled for execution. During execution this task gathers 
    /// render packages for a predefined set of NodeModel objects. This 
    /// predefined set of NodeModel objects includes all the NodeModel in the 
    /// given WorkspaceModel, if no specific NodeModel is specified during the 
    /// creation of this task; otherwise, the set only includes all upstream 
    /// NodeModel objects of the specified NodeModel.
    /// </summary>
    /// 
    class AggregateRenderPackageAsyncTask : AsyncTask
    {
        #region Class Data Members and Properties

        protected Guid targetedNodeId = Guid.Empty;
        private readonly List<IRenderPackage> normalRenderPackages;
        private readonly List<IRenderPackage> selectedRenderPackages;
        private IEnumerable<NodeModel> duplicatedNodeReferences;

        internal Guid NodeId { get { return targetedNodeId; } }

        internal IEnumerable<IRenderPackage> NormalRenderPackages
        {
            get { return normalRenderPackages; }
        }

        internal IEnumerable<IRenderPackage> SelectedRenderPackages
        {
            get { return selectedRenderPackages; }
        }

        internal override TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

        #endregion

        #region Public Class Operational Methods

        internal AggregateRenderPackageAsyncTask(IScheduler scheduler)
            : base(scheduler)
        {
            normalRenderPackages = new List<IRenderPackage>();
            selectedRenderPackages = new List<IRenderPackage>();
        }

        /// <summary>
        /// Call this method to determine if the task should be scheduled for 
        /// execution.
        /// </summary>
        /// <param name="workspaceModel">Render packages for all the nodes in 
        /// this workspaceModel will be extracted, if 'nodeModel' parameter is 
        /// null.</param>
        /// <param name="nodeModel">An optional NodeModel from which all upstream 
        /// nodes are to be examined for render packages. If this parameter is 
        /// null, render packages are extracted from all nodes in workspaceModel.
        /// </param>
        /// <returns>Returns true if the task should be scheduled for execution,
        /// or false otherwise.</returns>
        /// 
        internal bool Initialize(WorkspaceModel workspaceModel, NodeModel nodeModel)
        {
            if (workspaceModel == null)
                throw new ArgumentNullException("workspaceModel");

            if (nodeModel == null) // No node is specified, gather all nodes.
            {
                targetedNodeId = Guid.Empty;

                // Duplicate a list of all nodes for consumption later.
                var nodes = workspaceModel.Nodes.Where(n => n.IsVisible);
                duplicatedNodeReferences = nodes.ToList();
            }
            else
            {
                targetedNodeId = nodeModel.GUID;

                // Recursively gather all upstream nodes. Stop 
                // gathering if this node does not display upstream.
                var gathered = new List<NodeModel>();
                WorkspaceUtilities.GatherAllUpstreamNodes(nodeModel,
                    gathered, model => model.IsUpstreamVisible);

                duplicatedNodeReferences = gathered;
            }

            return duplicatedNodeReferences.Any();
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            if (duplicatedNodeReferences == null)
            {
                const string message = "Initialize method was not called";
                throw new InvalidOperationException(message);
            }

            foreach (var duplicatedNodeReference in duplicatedNodeReferences)
            {
                var rps = duplicatedNodeReference.RenderPackages;
                foreach (var renderPackage in rps.OfType<RenderPackage>())
                {
                    if (!renderPackage.IsNotEmpty())
                        continue;

                    if (renderPackage.Selected)
                        selectedRenderPackages.Add(renderPackage);
                    else
                        normalRenderPackages.Add(renderPackage);
                }
            }
        }

        protected override void HandleTaskCompletionCore()
        {
        }

        protected override TaskMergeInstruction CanMergeWithCore(AsyncTask otherTask)
        {
            var theOtherTask = otherTask as AggregateRenderPackageAsyncTask;
            if (theOtherTask == null)
                return base.CanMergeWithCore(otherTask);

            if (NodeId != theOtherTask.NodeId)
                return TaskMergeInstruction.KeepBoth;

            //// Comparing to another AggregateRenderPackageAsyncTask, the one 
            //// that gets scheduled more recently stay, while the earlier one 
            //// gets dropped. If this task has a higher tick count, keep this.
            //// 
            if (ScheduledTime.TickCount > theOtherTask.ScheduledTime.TickCount)
                return TaskMergeInstruction.KeepThis;

            return TaskMergeInstruction.KeepOther; // Otherwise, keep the other.
        }

        #endregion
    }
}

