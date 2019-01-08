using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Notes;
using Dynamo.Selection;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Layout class contains methods for organizing graphs.
    /// </summary>
    public static class LayoutExtensions
    {
        /// <summary>
        /// This function wraps a few methods on the workspace model layer
        /// to set up and run the graph layout algorithm.
        /// </summary>
        internal static List<GraphLayout.Graph> DoGraphAutoLayout(this WorkspaceModel workspace)
        {
            if (workspace.Nodes.Count() < 2) return null;

            var selection = DynamoSelection.Instance.Selection;

            // Check if all the selected models are groups
            bool isGroupLayout = selection.Count > 0 &&
                selection.All(x => x is AnnotationModel ||
                    selection.OfType<AnnotationModel>().Any(g => g.Nodes.Contains(x)));

            List<GraphLayout.Graph> layoutSubgraphs;
            List<List<GraphLayout.Node>> subgraphClusters;

            GenerateCombinedGraph(workspace, isGroupLayout,out layoutSubgraphs, out subgraphClusters);

            RecordUndoGraphLayout(workspace, isGroupLayout);

            // Generate subgraphs separately for each cluster
            subgraphClusters.ForEach(
                x => GenerateSeparateSubgraphs(new HashSet<GraphLayout.Node>(x), layoutSubgraphs));

            // Deselect all nodes
            subgraphClusters.ForEach(c => c.ForEach(x => x.IsSelected = false));

            // Run layout algorithm for each subgraph
            layoutSubgraphs.Skip(1).ToList().ForEach(g => RunLayoutSubgraph(g, isGroupLayout));
            AvoidSubgraphOverlap(layoutSubgraphs);
            SaveLayoutGraph(workspace, layoutSubgraphs);

            // Restore the workspace model selection information
            selection.ToList().ForEach(x => x.Select());

            return layoutSubgraphs;
        }

        /// <summary>
        /// <param name="workspace">A <see cref="WorkspaceModel"/>.</param>
        /// This method extracts all models from the workspace and puts them
        /// into the combined graph object, LayoutSubgraphs.First()
        /// <param name="isGroupLayout">True if all the selected models are groups.</param>
        /// <param name="layoutSubgraphs"></param>
        /// <param name="subgraphClusters"></param>
        /// </summary>
        private static void GenerateCombinedGraph(this WorkspaceModel workspace, bool isGroupLayout, 
            out List<GraphLayout.Graph> layoutSubgraphs, out List<List<GraphLayout.Node>> subgraphClusters)
        {
            layoutSubgraphs = new List<GraphLayout.Graph>
            {
                new GraphLayout.Graph()
            };
            var combinedGraph = layoutSubgraphs.First();
            subgraphClusters = new List<List<GraphLayout.Node>>();

            if (!isGroupLayout)
            {
                foreach (AnnotationModel group in workspace.Annotations)
                {
                    // Treat a group as a graph layout node/vertex
                    combinedGraph.AddNode(group.GUID, group.Width, group.Height, group.X, group.Y,
                        group.IsSelected || DynamoSelection.Instance.Selection.Count == 0);
                }
            }

            foreach (NodeModel node in workspace.Nodes)
            {
                if (!isGroupLayout)
                {
                    AnnotationModel group = workspace.Annotations.Where(
                        g => g.Nodes.Contains(node)).ToList().FirstOrDefault();

                    // Do not process nodes within groups
                    if (group == null)
                    {
                        combinedGraph.AddNode(node.GUID, node.Width, node.Height, node.X, node.Y,
                            node.IsSelected || DynamoSelection.Instance.Selection.Count == 0);
                    }
                }
                else
                {
                    // Process all nodes inside the selection
                    combinedGraph.AddNode(node.GUID, node.Width, node.Height, node.X, node.Y,
                        node.IsSelected || DynamoSelection.Instance.Selection.Count == 0);
                }
            }

            foreach (ConnectorModel edge in workspace.Connectors)
            {
                if (!isGroupLayout)
                {
                    AnnotationModel startGroup = null, endGroup = null;
                    startGroup = workspace.Annotations.Where(
                        g => g.Nodes.Contains(edge.Start.Owner)).ToList().FirstOrDefault();
                    endGroup = workspace.Annotations.Where(
                        g => g.Nodes.Contains(edge.End.Owner)).ToList().FirstOrDefault();

                    // Treat a group as a node, but do not process edges within a group
                    if (startGroup == null || endGroup == null || startGroup != endGroup)
                    {
                        combinedGraph.AddEdge(
                            startGroup == null ? edge.Start.Owner.GUID : startGroup.GUID,
                            endGroup == null ? edge.End.Owner.GUID : endGroup.GUID,
                            edge.Start.Center.X, edge.Start.Center.Y, edge.End.Center.X, edge.End.Center.Y);
                    }
                }
                else
                {
                    // Edges within a group are also processed
                    combinedGraph.AddEdge(edge.Start.Owner.GUID, edge.End.Owner.GUID,
                        edge.Start.Center.X, edge.Start.Center.Y, edge.End.Center.X, edge.End.Center.Y);
                }
            }

            foreach (NoteModel note in workspace.Notes)
            {
                AnnotationModel group = workspace.Annotations.Where(
                    g => g.Nodes.Contains(note)).ToList().FirstOrDefault();

                GraphLayout.Node nd = null;

                if (!isGroupLayout || group == null)
                {
                    // If note is not part of a group, link to the nearest node in the graph
                    nd = combinedGraph.Nodes.OrderBy(node =>
                        Math.Pow(node.X + node.Width / 2 - note.X - note.Width / 2, 2) +
                        Math.Pow(node.Y + node.Height / 2 - note.Y - note.Height / 2, 2)).FirstOrDefault();
                }
                else
                {
                    // If note is part of a group, link to the nearest node in the group
                    NodeModel ndm = group.Nodes.OfType<NodeModel>().OrderBy(node =>
                        Math.Pow(node.X + node.Width / 2 - note.X - note.Width / 2, 2) +
                        Math.Pow(node.Y + node.Height / 2 - note.Y - note.Height / 2, 2)).FirstOrDefault();

                    // Skip processing the group if there is no node in the group
                    if (ndm == null) continue;

                    // If the nearest point is a node model
                    nd = combinedGraph.FindNode(ndm.GUID);

                    // If the nearest point is a group model
                    nd = nd ?? combinedGraph.FindNode(group.GUID);
                }

                // Otherwise, leave the note unchanged
                if (nd != null)
                {
                    nd.LinkNote(note, note.Width, note.Height);
                }
            }

            if (!isGroupLayout)
            {
                // Add all nodes to one big cluster
                List<GraphLayout.Node> bigcluster = new List<GraphLayout.Node>();
                bigcluster.AddRange(combinedGraph.Nodes);
                subgraphClusters.Add(bigcluster);
            }
            else
            {
                // Each group becomes one cluster
                foreach (AnnotationModel group in DynamoSelection.Instance.Selection.OfType<AnnotationModel>())
                {
                    List<GraphLayout.Node> cluster = new List<GraphLayout.Node>();
                    cluster.AddRange(group.Nodes.OfType<NodeModel>().Select(x => combinedGraph.FindNode(x.GUID)));
                    subgraphClusters.Add(cluster);
                }
            }

        }

        /// <summary>
        /// This method adds relevant models to the undo recorder.
        /// </summary>
        /// <param name="workspace">A <see cref="WorkspaceModel"/>.</param>
        /// <param name="isGroupLayout">True if all the selected models are groups.</param>
        private static void RecordUndoGraphLayout(this WorkspaceModel workspace, bool isGroupLayout)
        {
            List<ModelBase> undoItems = new List<ModelBase>();

            if (!isGroupLayout)
            {
                // Add all selected items to the undo recorder
                undoItems.AddRange(workspace.Nodes);
                undoItems.AddRange(workspace.Notes);
                if (DynamoSelection.Instance.Selection.Count > 0)
                {
                    undoItems = undoItems.Where(x => x.IsSelected).ToList();
                }
            }
            else
            {
                // Add all models inside selected groups
                foreach (var group in workspace.Annotations)
                {
                    if (group.IsSelected)
                    {
                        group.Nodes.OfType<NodeModel>().ToList().ForEach(x => x.IsSelected = false);
                        undoItems.AddRange(group.Nodes);
                    }
                }
            }

            WorkspaceModel.RecordModelsForModification(undoItems, workspace.UndoRecorder);
        }

        /// <summary>
        /// This method repeatedly takes a selected node in the combined graph and
        /// uses breadth-first search to find all other nodes in the same subgraph
        /// until all selected nodes have been processed.
        /// </summary>
        /// <param name="nodes">A cluster of nodes to be separated into subgraphs.</param>
        /// <param name="layoutSubgraphs">A collection of layout subgraphs.</param>
        private static void GenerateSeparateSubgraphs(HashSet<GraphLayout.Node> nodes, List<GraphLayout.Graph> layoutSubgraphs)
        {
            int processed = 0;
            var combinedGraph = layoutSubgraphs.First();
            GraphLayout.Graph graph = new GraphLayout.Graph();
            Queue<GraphLayout.Node> queue = new Queue<GraphLayout.Node>();

            while (nodes.Count(n => n.IsSelected) > 0)
            {
                GraphLayout.Node currentNode;

                if (queue.Count == 0)
                {
                    if (graph.Nodes.Count > 0)
                    {
                        // Save the subgraph and subtract these nodes from the combined graph

                        layoutSubgraphs.Add(graph);
                        nodes.ExceptWith(graph.Nodes);
                        combinedGraph.Nodes.ExceptWith(graph.Nodes);
                        graph = new GraphLayout.Graph();
                    }
                    if (nodes.Count(n => n.IsSelected) == 0) break;

                    currentNode = nodes.FirstOrDefault(n => n.IsSelected);
                    graph.Nodes.Add(currentNode);
                }
                else
                {
                    currentNode = queue.Dequeue();
                }

                // Find all nodes in the selection which are connected directly
                // to the left or to the right to the currentNode

                var selectedNodes = currentNode.RightEdges.Select(e => e.EndNode)
                    .Union(currentNode.LeftEdges.Select(e => e.StartNode))
                    .Where(x => nodes.Contains(x) && x.IsSelected)
                    .Except(graph.Nodes).ToList();
                graph.Nodes.UnionWith(selectedNodes);
                graph.Edges.UnionWith(currentNode.RightEdges);
                graph.Edges.UnionWith(currentNode.LeftEdges);

                // If any of the incident edges are connected to unselected (outside) nodes
                // then mark these edges as anchors.

                graph.AnchorRightEdges.UnionWith(currentNode.RightEdges.Where(e => !e.EndNode.IsSelected));
                graph.AnchorLeftEdges.UnionWith(currentNode.LeftEdges.Where(e => !e.StartNode.IsSelected));

                foreach (var node in selectedNodes)
                {
                    queue.Enqueue(node);
                    processed++;
                }
            }
        }

        /// <summary>
        /// This function calls the graph layout algorithm methods.
        /// </summary>
        /// <param name="graph">The subgraph to be processed.</param>
        /// <param name="isGroupLayout">True if all selected models are groups.</param>
        private static void RunLayoutSubgraph(GraphLayout.Graph graph, bool isGroupLayout)
        {
            // Select relevant nodes
            graph.Nodes.ToList().ForEach(x => x.IsSelected = true);

            // Save subgraph position before running the layout
            graph.RecordInitialPosition();

            // Sugiyama algorithm steps
            graph.RemoveCycles();
            graph.AssignLayers();
            graph.OrderNodes();

            // Node and graph positioning
            graph.DistributeNodePosition();
            graph.SetGraphPosition(isGroupLayout);

            // Reset layer information and deselect nodes
            graph.ResetLayers();
            graph.Nodes.ToList().ForEach(x => x.IsSelected = false);
        }

        /// <summary>
        /// This method repeatedly shifts subgraphs away vertically from each other
        /// when there are any two nodes from different subgraphs overlapping.
        /// </summary>
        private static void AvoidSubgraphOverlap(List<GraphLayout.Graph> layoutSubgraphs)
        {
            bool done;

            do
            {
                done = true;

                foreach (var g1 in layoutSubgraphs.Skip(1))
                {
                    foreach (var g2 in layoutSubgraphs.Skip(1))
                    {
                        // The first subgraph's center point must be higher than the second subgraph
                        if (!g1.Equals(g2) && (g1.GraphCenterY + g1.OffsetY <= g2.GraphCenterY + g2.OffsetY))
                        {
                            var g1nodes = g1.Nodes.OrderBy(n => n.Y + n.TotalHeight);
                            var g2nodes = g2.Nodes.OrderBy(n => n.Y);

                            foreach (var node1 in g1nodes)
                            {
                                foreach (var node2 in g2nodes)
                                {
                                    // If any two nodes from these two different subgraphs overlap
                                    if ((node1.Y + node1.TotalHeight + GraphLayout.Graph.VerticalNodeDistance + g1.OffsetY > node2.Y + g2.OffsetY) &&
                                        (((node1.X <= node2.X) && (node1.X + node1.Width + GraphLayout.Graph.HorizontalNodeDistance > node2.X)) ||
                                        ((node2.X <= node1.X) && (node2.X + node2.Width + GraphLayout.Graph.HorizontalNodeDistance > node1.X))))
                                    {
                                        // Shift the first subgraph to the top and the second subgraph to the bottom
                                        g1.OffsetY -= 5;
                                        g2.OffsetY += 5;
                                        done = false;
                                    }
                                    if (!done) break;
                                }
                                if (!done) break;
                            }
                        }
                    }
                }
            } while (!done);
        }

        /// <summary>
        /// This method pushes changes from the GraphLayout.Graph objects
        /// back to the workspace models.
        /// </summary>
        private static void SaveLayoutGraph(this WorkspaceModel workspace, List<GraphLayout.Graph> layoutSubgraphs)
        {
            // Assign coordinates to nodes inside groups
            foreach (var group in workspace.Annotations)
            {
                GraphLayout.Graph graph = layoutSubgraphs
                    .FirstOrDefault(g => g.FindNode(group.GUID) != null);

                if (graph != null)
                {
                    GraphLayout.Node n = graph.FindNode(group.GUID);

                    double deltaX = n.X - group.X;
                    double deltaY = n.Y - group.Y + graph.OffsetY;
                    foreach (var node in group.Nodes.OfType<NodeModel>())
                    {
                        node.X += deltaX;
                        node.Y += deltaY;
                        node.ReportPosition();
                    }

                    foreach (NoteModel note in n.LinkedNotes)
                    {
                        if (note.IsSelected || DynamoSelection.Instance.Selection.Count == 0)
                        {
                            note.X += deltaX;
                            note.Y += deltaY;
                            note.ReportPosition();
                        }
                    }
                }
            }

            // Assign coordinates to nodes outside groups
            foreach (var node in workspace.Nodes)
            {
                GraphLayout.Graph graph = layoutSubgraphs
                    .FirstOrDefault(g => g.FindNode(node.GUID) != null);

                if (graph != null)
                {
                    GraphLayout.Node n = graph.FindNode(node.GUID);
                    double offsetY = graph.OffsetY;

                    node.X = n.X;
                    node.Y = n.Y + n.NotesHeight + offsetY;
                    node.ReportPosition();
                    workspace.HasUnsavedChanges = true;

                    double noteOffset = -n.NotesHeight;
                    foreach (NoteModel note in n.LinkedNotes)
                    {
                        if (note.IsSelected || DynamoSelection.Instance.Selection.Count == 0)
                        {
                            note.X = node.X;
                            note.Y = node.Y + noteOffset;
                            noteOffset += note.Height + GraphLayout.Graph.VerticalNoteDistance;
                            note.ReportPosition();
                        }
                    }
                }
            }
        }
    }
}
