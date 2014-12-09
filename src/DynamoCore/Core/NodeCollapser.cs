using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;

namespace Dynamo.Utilities
{
    public static class NodeCollapser
    {
        /// <summary>
        ///     Collapse a set of nodes in a given workspace.
        /// </summary>
        /// <param name="dynamoModel">The current DynamoModel</param>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        /// <param name="currentWorkspace"> The workspace where</param>
        /// <param name="args"></param>
        public static void Collapse(DynamoModel dynamoModel, 
                                    IEnumerable<NodeModel> selectedNodes, 
                                    WorkspaceModel currentWorkspace, 
                                    FunctionNamePromptEventArgs args = null)
        {
            if (args == null || !args.Success)
            {
                args = new FunctionNamePromptEventArgs();
                dynamoModel.OnRequestsFunctionNamePrompt(null, args);

                if (!args.Success)
                {
                    return;
                }
            }

            var selectedNodeSet = new HashSet<NodeModel>(selectedNodes);
            // Note that undoable actions are only recorded for the "currentWorkspace", 
            // the nodes which get moved into "newNodeWorkspace" are not recorded for undo,
            // even in the new workspace. Their creations will simply be treated as part of
            // the opening of that new workspace (i.e. when a user opens a file, she will 
            // not expect the nodes that show up to be undoable).
            // 
            // After local nodes are moved into "newNodeWorkspace" as the result of 
            // conversion, if user performs an undo, new set of nodes will be created in 
            // "currentWorkspace" (not moving those nodes in the "newNodeWorkspace" back 
            // into "currentWorkspace"). In another word, undo recording is on a per-
            // workspace basis, it does not work across different workspaces.
            // 
            UndoRedoRecorder undoRecorder = currentWorkspace.UndoRecorder;
            using (undoRecorder.BeginActionGroup())
            {
                var newNodeWorkspace = new CustomNodeWorkspaceModel(
                    dynamoModel,
                    args.Name,
                    args.Category,
                    args.Description,
                    0,
                    0) { WatchChanges = false, HasUnsavedChanges = true };

                var newNodeDefinition = new CustomNodeDefinition(Guid.NewGuid())
                {
                    WorkspaceModel = newNodeWorkspace
                };

                currentWorkspace.DisableReporting();

                #region Determine Inputs and Outputs

                //Step 1: determine which nodes will be inputs to the new node
                var inputs =
                    new HashSet<Tuple<NodeModel, int, Tuple<int, NodeModel>>>(
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.InPortData.Count)
                                .Where(node.HasConnectedInput)
                                .Select(data => Tuple.Create(node, data, node.Inputs[data]))
                                .Where(input => !selectedNodeSet.Contains(input.Item3.Item2))));

                var outputs =
                    new HashSet<Tuple<NodeModel, int, Tuple<int, NodeModel>>>(
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.OutPortData.Count)
                                .Where(node.HasOutput)
                                .SelectMany(
                                    data =>
                                        node.Outputs[data].Where(
                                            output => !selectedNodeSet.Contains(output.Item2))
                                        .Select(output => Tuple.Create(node, data, output)))));

                #endregion

                #region Detect 1-node holes (higher-order function extraction)

                dynamoModel.Logger.LogWarning("Could not repair 1-node holes", WarningLevel.Mild);

                // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5603
                //var curriedNodeArgs =
                //    new HashSet<NodeModel>(
                //        inputs.Select(x => x.Item3.Item2)
                //            .Intersect(outputs.Select(x => x.Item3.Item2))).Select(
                //                outerNode =>
                //                {
                //                    //var node = new Apply1();
                //                    var node = newNodeWorkspace.AddNode<Apply1>();
                //                    node.SetNickNameFromAttribute();

                //                    node.DisableReporting();

                //                    node.X = outerNode.X;
                //                    node.Y = outerNode.Y;

                //                    //Fetch all input ports
                //                    // in order
                //                    // that have inputs
                //                    // and whose input comes from an inner node
                //                    List<int> inPortsConnected =
                //                        Enumerable.Range(0, outerNode.InPortData.Count)
                //                            .Where(
                //                                x =>
                //                                    outerNode.HasInput(x)
                //                                        && selectedNodeSet.Contains(
                //                                            outerNode.Inputs[x].Item2))
                //                            .ToList();

                //                    var nodeInputs =
                //                        outputs.Where(output => output.Item3.Item2 == outerNode)
                //                            .Select(
                //                                output =>
                //                                    new
                //                                    {
                //                                        InnerNodeInputSender = output.Item1,
                //                                        OuterNodeInPortData = output.Item3.Item1
                //                                    })
                //                            .ToList();

                //                    nodeInputs.ForEach(_ => node.AddInput());

                //                    node.RegisterAllPorts();

                //                    return
                //                        new
                //                        {
                //                            OuterNode = outerNode,
                //                            InnerNode = node,
                //                            Outputs =
                //                                inputs.Where(
                //                                    input => input.Item3.Item2 == outerNode)
                //                                    .Select(input => input.Item3.Item1),
                //                            Inputs = nodeInputs,
                //                            OuterNodePortDataList = inPortsConnected
                //                        };
                //                }).ToList();

                #endregion

                #region UI Positioning Calculations

                double avgX = selectedNodeSet.Average(node => node.X);
                double avgY = selectedNodeSet.Average(node => node.Y);

                double leftMost = selectedNodeSet.Min(node => node.X);
                double topMost = selectedNodeSet.Min(node => node.Y);
                double rightMost = selectedNodeSet.Max(node => node.X + node.Width);

                #endregion

                #region Handle full selected connectors

                // Step 2: Determine all the connectors whose start/end owners are 
                // both in the selection set, and then move them from the current 
                // workspace into the new workspace.

                var fullySelectedConns =
                    new HashSet<ConnectorModel>(
                        currentWorkspace.Connectors.Where(
                            conn =>
                            {
                                bool startSelected = selectedNodeSet.Contains(conn.Start.Owner);
                                bool endSelected = selectedNodeSet.Contains(conn.End.Owner);
                                return startSelected && endSelected;
                            }));

                foreach (var ele in fullySelectedConns)
                {
                    undoRecorder.RecordDeletionForUndo(ele);
                    currentWorkspace.Connectors.Remove(ele);
                }

                #endregion

                #region Handle partially selected connectors

                // Step 3: Partially selected connectors (either one of its start 
                // and end owners is in the selection) are to be destroyed.

                var partiallySelectedConns =
                    currentWorkspace.Connectors.Where(
                        conn =>
                            selectedNodeSet.Contains(conn.Start.Owner)
                                || selectedNodeSet.Contains(conn.End.Owner)).ToList();

                foreach (ConnectorModel connector in partiallySelectedConns)
                {
                    undoRecorder.RecordDeletionForUndo(connector);
                    connector.NotifyConnectedPortsOfDeletion();
                    currentWorkspace.Connectors.Remove(connector);
                }

                #endregion

                #region Transfer nodes and connectors to new workspace

                // Step 4: move all nodes to new workspace remove from old
                // PB: This could be more efficiently handled by a copy paste, but we
                // are preservering the node 
                foreach (var ele in selectedNodeSet)
                {
                    undoRecorder.RecordDeletionForUndo(ele);
                    ele.SaveResult = false;
                    currentWorkspace.Nodes.Remove(ele);

                    // VisualizationManager and EngineController subscribe
                    // NodeDeleted event, so notify them that this node has
                    // been deleted.
                    dynamoModel.OnNodeDeleted(ele);

                    // Assign a new guid to this node, otherwise when node is
                    // compiled to AST, literally it is still in global scope
                    // instead of in function scope.
                    ele.GUID = Guid.NewGuid();
                    ele.RenderPackages.Clear();
                    ele.Workspace = newNodeWorkspace;
                }

                //  add to new
                newNodeWorkspace.Nodes.AddRange(selectedNodeSet);
                newNodeWorkspace.Connectors.AddRange(fullySelectedConns);

                foreach (var node in newNodeWorkspace.Nodes)
                    node.DisableReporting();

                double leftShift = leftMost - 250;
                foreach (NodeModel node in newNodeWorkspace.Nodes)
                {
                    node.X = node.X - leftShift;
                    node.Y = node.Y - topMost;
                }

                #endregion


                #region Process inputs

                var inConnectors = new List<Tuple<NodeModel, int>>();
                var uniqueInputSenders = new Dictionary<Tuple<NodeModel, int>, Symbol>();

                //Step 3: insert variables (reference step 1)
                foreach (var input in Enumerable.Range(0, inputs.Count).Zip(inputs, Tuple.Create))
                {
                    int inputIndex = input.Item1;

                    NodeModel inputReceiverNode = input.Item2.Item1;
                    int inputReceiverData = input.Item2.Item2;

                    NodeModel inputNode = input.Item2.Item3.Item2;
                    int inputData = input.Item2.Item3.Item1;

                    Symbol node;

                    var key = Tuple.Create(inputNode, inputData);
                    if (uniqueInputSenders.ContainsKey(key))
                    {
                        node = uniqueInputSenders[key];
                    }
                    else
                    {
                        inConnectors.Add(Tuple.Create(inputNode, inputData));

                        node = newNodeWorkspace.AddNode<Symbol>();
                        node.InputSymbol = inputReceiverNode.InPortData[inputReceiverData].NickName;

                        node.SetNickNameFromAttribute();

                        node.DisableReporting();

                        node.X = 0;
                        node.Y = inputIndex*(50 + node.Height);

                        uniqueInputSenders[key] = node;
                    }

                    //var curriedNode = curriedNodeArgs.FirstOrDefault(x => x.OuterNode == inputNode);

                    //if (curriedNode == null)
                    //{
                        newNodeWorkspace.AddConnection(
                            node,
                            inputReceiverNode,
                            0,
                            inputReceiverData);
                    //}
                    //else
                    //{
                    //    //Connect it to the applier
                    //    newNodeWorkspace.AddConnection(node, curriedNode.InnerNode, 0, 0);

                    //    //Connect applier to the inner input receive
                    //    newNodeWorkspace.AddConnection(
                    //        curriedNode.InnerNode,
                    //        inputReceiverNode,
                    //        0,
                    //        inputReceiverData);
                    //}
                }

                #endregion

                #region Process outputs

                //List of all inner nodes to connect an output. Unique.
                var outportList = new List<Tuple<NodeModel, int>>();

                var outConnectors = new List<Tuple<NodeModel, int, int>>();

                int i = 0;
                if (outputs.Any())
                {
                    foreach (var output in outputs)
                    {
                        if (
                            outportList.All(
                                x => !(x.Item1 == output.Item1 && x.Item2 == output.Item2)))
                        {
                            NodeModel outputSenderNode = output.Item1;
                            int outputSenderData = output.Item2;
                            NodeModel outputReceiverNode = output.Item3.Item2;

                            //if (curriedNodeArgs.Any(x => x.OuterNode == outputReceiverNode))
                            //    continue;

                            outportList.Add(Tuple.Create(outputSenderNode, outputSenderData));

                            //Create Symbol Node
                            var node = newNodeWorkspace.AddNode<Output>();
                            node.Symbol = outputSenderNode.OutPortData[outputSenderData].NickName;

                            node.SetNickNameFromAttribute();
                            node.DisableReporting();

                            node.X = rightMost + 75 - leftShift;
                            node.Y = i*(50 + node.Height);

                            newNodeWorkspace.AddConnection(
                                outputSenderNode,
                                node,
                                outputSenderData,
                                0);

                            i++;
                        }
                    }

                    //Connect outputs to new node
                    foreach (var output in outputs)
                    {
                        //Node to be connected to in CurrentWorkspace
                        NodeModel outputSenderNode = output.Item1;

                        //Port to be connected to on outPutNode_outer
                        int outputSenderData = output.Item2;

                        int outputReceiverData = output.Item3.Item1;
                        NodeModel outputReceiverNode = output.Item3.Item2;

                        //var curriedNode =
                        //    curriedNodeArgs.FirstOrDefault(x => x.OuterNode == outputReceiverNode);

                        //if (curriedNode == null)
                        //{
                            // we create the connectors in the current space later
                            outConnectors.Add(
                                Tuple.Create(
                                    outputReceiverNode,
                                    outportList.FindIndex(
                                        x =>
                                            x.Item1 == outputSenderNode
                                                && x.Item2 == outputSenderData),
                                    outputReceiverData));
                        //}
                        //else
                        //{
                        //    int targetPort =
                        //        curriedNode.Inputs.First(
                        //            x => x.InnerNodeInputSender == outputSenderNode)
                        //            .OuterNodeInPortData;

                        //    int targetPortIndex =
                        //        curriedNode.OuterNodePortDataList.IndexOf(targetPort);

                        //    //Connect it (new dynConnector)
                        //    newNodeWorkspace.AddConnection(
                        //        outputSenderNode,
                        //        curriedNode.InnerNode,
                        //        outputSenderData,
                        //        targetPortIndex + 1);

                        //}
                    }
                }
                else
                {
                    foreach (var hanging in
                        selectedNodeSet.SelectMany(
                            node =>
                                Enumerable.Range(0, node.OutPortData.Count)
                                .Where(port => !node.HasOutput(port))
                                .Select(port => new { node, port })).Distinct())
                    {
                        //Create Symbol Node
                        var node = newNodeWorkspace.AddNode<Output>();
                        node.Symbol = hanging.node.OutPortData[hanging.port].NickName;

                        node.SetNickNameFromAttribute();

                        //store the element in the elements list
                        node.DisableReporting();

                        node.X = rightMost + 75 - leftShift;
                        node.Y = i*(50 + node.Height);

                        newNodeWorkspace.AddConnection(hanging.node, node, hanging.port, 0);

                        i++;
                    }
                }

                #endregion

                // save and load the definition from file
                newNodeDefinition.SyncWithWorkspace(dynamoModel, true, true);
                dynamoModel.Workspaces.Add(newNodeWorkspace);

                string name = newNodeDefinition.FunctionId.ToString();
                var collapsedNode = currentWorkspace.AddNode(avgX, avgY, name);
                undoRecorder.RecordCreationForUndo(collapsedNode);

                // place the node as intended, not centered
                collapsedNode.X = avgX;
                collapsedNode.Y = avgY;

                collapsedNode.DisableReporting();

                foreach (
                    var nodeTuple in
                        inConnectors.Select(
                            (x, idx) => new { node = x.Item1, from = x.Item2, to = idx }))
                {
                    var conn = currentWorkspace.AddConnection(
                        nodeTuple.node,
                        collapsedNode,
                        nodeTuple.from,
                        nodeTuple.to);

                    if (conn != null)
                    {
                        undoRecorder.RecordCreationForUndo(conn);
                    }
                }

                foreach (var nodeTuple in outConnectors)
                {

                    var conn = currentWorkspace.AddConnection(
                        collapsedNode,
                        nodeTuple.Item1,
                        nodeTuple.Item2,
                        nodeTuple.Item3);

                    if (conn != null)
                    {
                        undoRecorder.RecordCreationForUndo(conn);
                    }
                }

                collapsedNode.EnableReporting();
                currentWorkspace.EnableReporting();

                foreach (var node in newNodeWorkspace.Nodes)
                    node.EnableReporting();

                newNodeWorkspace.WatchChanges = true;
            }
        }

        private static void SetNickNameFromAttribute(this NodeModel node)
        {
            var elNameAttrib =
                node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), false)[0] as
                    NodeNameAttribute;

            if (elNameAttrib != null)
            {
                node.NickName = elNameAttrib.Name;
            }
        }

    }
}
