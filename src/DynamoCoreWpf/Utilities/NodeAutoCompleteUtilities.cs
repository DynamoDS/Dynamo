using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;

using Dynamo.Selection;
using Dynamo.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Dynamo.Wpf.Utilities
{
    internal static class NodeAutoCompleteUtilities
    {
        // We want to perform an AutoLayout operation only after all nodes have updated their UI.
        // Therefore, we will queue the AutoLayout operation to execute during the next idle event.
        internal static void PostAutoLayoutNodes(WorkspaceModel wsModel,
            NodeModel queryNode,
            IEnumerable<NodeModel> misplacedNodes,
            bool clusterLayout,
            bool checkWorkspaceNodes,
            Action finalizer)
        {
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(() => AutoLayoutNodes(wsModel,
                    queryNode,
                    misplacedNodes,
                    clusterLayout,
                    checkWorkspaceNodes,
                    finalizer), DispatcherPriority.ApplicationIdle);
            }
        }

        internal static Rect2D GetNodesBoundingBox(IEnumerable<NodeModel> nodes)
        {
            if (nodes is null || nodes.Count() == 0)
                return Rect2D.Empty;

            double minX = nodes.Min(node => node.Rect.TopLeft.X);
            double maxX = nodes.Max(node => node.Rect.BottomRight.X);
            double minY = nodes.Min(node => node.Rect.TopLeft.Y);
            double maxY = nodes.Max(node => node.Rect.BottomRight.Y);

            return new Rect2D(minX, minY, maxX - minX, maxY - minY);
        }

        // Determines whether an AutoLayout operation is needed for a query node and other relevant nodes around it.
        // This is based on whether the relevant nodes intersect with other nodes in the model.
        // If intersections occur, the function identifies the newly intersected nodes and returns true considering
        // an additional AutoLayout operation is needed.
        internal static bool AutoLayoutNeeded(WorkspaceModel wsModel, NodeModel originalNode, IEnumerable<NodeModel> nodesToConsider, out List<NodeModel> intersectedNodes)
        {
            //Collect all connected input or output nodes from the original node.
            
            var nodesGuidsToConsider = nodesToConsider.Select(n => n.GUID).ToHashSet();
            nodesGuidsToConsider.Append(originalNode.GUID);

            Rect2D connectedNodesBBox = GetNodesBoundingBox(nodesToConsider);

            //See if there are other nodes that intersect with our bbbox.
            //If there are, check to see if they actually intersect with one of the
            //connected nodes and select them for auto layout.
            intersectedNodes = new List<NodeModel>();
            bool realIntersection = false;
            foreach (var node in wsModel.Nodes)
            {
                if (nodesGuidsToConsider.Contains(node.GUID))
                    continue;

                if (connectedNodesBBox.IntersectsWith(node.Rect) ||
                    connectedNodesBBox.Contains(node.Rect))
                {
                    intersectedNodes.Add(node);
                    if (!realIntersection)
                    {
                        foreach (var connectedNode in nodesToConsider)
                        {
                            if (node.Rect.IntersectsWith(connectedNode.Rect) ||
                                node.Rect.Contains(connectedNode.Rect) ||
                                connectedNode.Rect.Contains(node.Rect))
                            {
                                realIntersection = true;
                                break;
                            }
                        }
                    }
                }
            }

            return realIntersection;
        }

        /// <summary>
        /// Automatically arranges misplaced nodes based on the specified parameters.
        /// </summary>
        /// <param name="wsModel">The workspace model containing the nodes to be arranged.</param>
        /// <param name="queryNode">The node used as a starting point for the layout operation.</param>
        /// <param name="misplacedNodes">A collection of nodes that are not properly positioned and need to be arranged.</param>
        /// <param name="clusterLayout">Ensures misplaced nodes are positioned downstream. Nodes will be moved if necessary.</param>
        /// <param name="checkWorkspaceNodes">Specifies whether to consider existing nodes in the workspace during the layout operation.</param>
        /// <param name="finalizer">An action to be executed after the layout operation is complete, typically for cleanup or further adjustments.</param>
        internal static void AutoLayoutNodes(WorkspaceModel wsModel,
            NodeModel queryNode,
            IEnumerable<NodeModel> misplacedNodes,
            bool clusterLayout,
            bool checkWorkspaceNodes,
            Action finalizer)
        {
            DynamoSelection.Instance.Selection.AddRange(misplacedNodes);
            wsModel.DoGraphAutoLayout(true, true, queryNode.GUID);

            // For large clusters of nodes, auto-layout may place nodes on both sides of the query node.
            // While the arrangement is technically fine, we move the entire group downstream for better consistency.
            if (clusterLayout)
            {
                double offset = -1;
                foreach (var node in misplacedNodes)
                {
                    if (node.X < queryNode.X)
                    {
                        offset = Math.Max(offset, queryNode.X - node.X);
                    }
                }

                double balast = 50;
                if (offset > 0)
                {
                    foreach (var node in misplacedNodes)
                    {
                        node.X = node.X + offset + queryNode.Width + balast;
                    }

                    wsModel.DoGraphAutoLayout(true, true, queryNode.GUID);
                }
            }

            // Check if the newly added nodes are still intersecting with other nodes in the workspace.
            // If so, perform an additional auto-layout pass. We only do this once in order to minimize
            // disruption to the user's workspace.
            if (checkWorkspaceNodes)
            {
                bool redoAutoLayout = AutoLayoutNeeded(wsModel, queryNode, misplacedNodes, out List<NodeModel> intersectedNodes);
                if (redoAutoLayout)
                {
                    DynamoSelection.Instance.Selection.AddRange(intersectedNodes);
                    wsModel.DoGraphAutoLayout(true, true, queryNode.GUID);
                }
            }

            DynamoSelection.Instance.ClearSelection();

            if (finalizer != null)
            {
                finalizer();
            }
        }
    }
}
