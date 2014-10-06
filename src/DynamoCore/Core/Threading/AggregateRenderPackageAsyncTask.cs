#if ENABLE_DYNAMO_SCHEDULER

using Autodesk.DesignScript.Interfaces;

using Dynamo.DSEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.Models;

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

        private readonly List<IRenderPackage> normalRenderPackages;
        private readonly List<IRenderPackage> selectedRenderPackages;
        private IEnumerable<NodeModel> duplicatedNodeReferences;

        internal IEnumerable<IRenderPackage> NormalRenderPackages
        {
            get { return normalRenderPackages; }
        }

        internal IEnumerable<IRenderPackage> SelectedRenderPackages
        {
            get { return selectedRenderPackages; }
        }

        #endregion

        #region Public Class Operational Methods

        internal AggregateRenderPackageAsyncTask(DynamoScheduler scheduler)
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
                // Duplicate a list of all nodes for consumption later.
                duplicatedNodeReferences = workspaceModel.Nodes.ToList();
            }
            else
            {
                // Recursively gather all upstream nodes.
                var gathered = new List<NodeModel>();
                GatherAllUpstreamNodes(nodeModel, gathered);
                duplicatedNodeReferences = gathered;
            }

            return duplicatedNodeReferences.Any();
        }

        #endregion

        #region Protected Overridable Methods

        protected override void ExecuteCore()
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

        #endregion

        #region Private Class Helper Methods

        private static void GatherAllUpstreamNodes(NodeModel nodeModel, List<NodeModel> gathered)
        {
            if ((nodeModel == null) || gathered.Contains(nodeModel))
                return; // Look no further, node is already in the list.

            gathered.Add(nodeModel); // Add to list first, avoiding re-entrant.

            foreach (var upstreamNode in nodeModel.Inputs)
            {
                // Add all the upstream nodes found into the list.
                GatherAllUpstreamNodes(upstreamNode.Value.Item2, gathered);
            }
        }

        #endregion
    }
}

#endif
