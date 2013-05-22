using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Dynamo.Controls;
using System.Windows.Controls;
using Dynamo.Connectors;
using System.Windows;
using Dynamo.Commands;
using Microsoft.Practices.Prism;

namespace Dynamo.Utilities
{
    class NodeCollapser
    {
        /// <summary>
        ///     Collapse a set of nodes in a given workspace.  Has the side effects of prompting the user
        ///     first in order to obtain the name and category for the new node, 
        ///     writes the function to a dyf file, adds it to the FunctionDict, adds it to search, and compiles and 
        ///     places the newly created symbol (defining a lambda) in the Controller's FScheme Environment.  
        /// </summary>
        /// <param name="selectedNodes"> The function definition for the user-defined node </param>
        /// <param name="currentWorkspace"> The workspace where</param>
        internal static void Collapse(IEnumerable<dynNodeModel> selectedNodes, dynWorkspaceModel currentWorkspace)
        {
            var selectedNodeSet = new HashSet<dynNodeModel>(selectedNodes);

            #region Prompt

            //First, prompt the user to enter a name
            string newNodeName, newNodeCategory;
            string error = "";

            do
            {
                var dialog = new FunctionNamePrompt(dynSettings.Controller.SearchViewModel.Categories, error);
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                newNodeName = dialog.Text;
                newNodeCategory = dialog.Category;

                if (dynSettings.Controller.CustomNodeLoader.Contains(newNodeName))
                {
                    error = "A function with this name already exists.";
                }
                else if (newNodeCategory.Equals(""))
                {
                    error = "Please enter a valid category.";
                }
                else
                {
                    error = "";
                }
            } while (!error.Equals(""));

            var newNodeWorkspace = new FuncWorkspace(newNodeName, newNodeCategory, 0, 0);
            var newNodeDefinition = new FunctionDefinition(Guid.NewGuid());
            newNodeDefinition.Workspace = newNodeWorkspace;

            #endregion

            currentWorkspace.DisableReporting();

            #region Determine Inputs and Outputs

            //Step 1: determine which nodes will be inputs to the new node
            var inputs = new HashSet<Tuple<dynNodeModel, int, Tuple<int, dynNodeModel>>>(
                    selectedNodeSet
                        .SelectMany(node => Enumerable.Range(0, node.InPortData.Count)
                            .Where(node.HasInput)
                            .Select(data => Tuple.Create(node, data, node.Inputs[data]))
                                                 .Where(input => !selectedNodeSet.Contains(input.Item3.Item2))));

            var outputs = new HashSet<Tuple<dynNodeModel, int, Tuple<int, dynNodeModel>>>(
                selectedNodeSet.SelectMany(
                    node => Enumerable.Range(0, node.OutPortData.Count).Where(node.HasOutput).SelectMany(
                        data => node.Outputs[data]
                                    .Where(output => !selectedNodeSet.Contains(output.Item2))
                                    .Select(output => Tuple.Create(node, data, output)))));

            #endregion

            #region Detect 1-node holes (higher-order function extraction)

            var curriedNodeArgs =
                new HashSet<dynNodeModel>(
                    inputs
                        .Select(x => x.Item3.Item2)
                        .Intersect(outputs.Select(x => x.Item3.Item2)))
                    .Select(
                        outerNode =>
                        {
                            var node = new dynApply1();

                            //MVVM : Don't make direct reference to view here
                            //MVVM: no reference to view here
                            //dynNodeView nodeUI = node.NodeUI;

                            var elNameAttrib =
                                node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as
                                NodeNameAttribute;
                            if (elNameAttrib != null)
                            {
                                node.NickName = elNameAttrib.Name;
                            }

                            node.GUID = Guid.NewGuid();

                            //store the element in the elements list
                            newNodeWorkspace.Nodes.Add(node);
                            node.WorkSpace = newNodeWorkspace;

                            node.DisableReporting();

                            //MVVM : Can't set view location here

                            //dynSettings.Bench.WorkBench.Children.Add(nodeUI);

                            //Place it in an appropriate spot
                            //Canvas.SetLeft(nodeUI, Canvas.GetLeft(outerNode.NodeUI));
                            //Canvas.SetTop(nodeUI, Canvas.GetTop(outerNode.NodeUI));
                            node.X = outerNode.X;
                            node.Y = outerNode.Y;

                            //Fetch all input ports
                            // in order
                            // that have inputs
                            // and whose input comes from an inner node
                            List<int> inPortsConnected = Enumerable.Range(0, outerNode.InPortData.Count)
                                                                   .Where(
                                                                       x =>
                                                                       outerNode.HasInput(x) &&
                                                                       selectedNodeSet.Contains(
                                                                           outerNode.Inputs[x].Item2))
                                                                   .ToList();

                            var nodeInputs = outputs
                                .Where(output => output.Item3.Item2 == outerNode)
                                .Select(
                                    output =>
                                    new
                                    {
                                        InnerNodeInputSender = output.Item1,
                                        OuterNodeInPortData = output.Item3.Item1
                                    }).ToList();

                            nodeInputs.ForEach(_ => node.AddInput());

                            node.RegisterAllPorts();

                            //MVVM: don't call update layout here
                            //dynSettings.Bench.WorkBench.UpdateLayout();

                            return new
                            {
                                OuterNode = outerNode,
                                InnerNode = node,
                                Outputs = inputs.Where(input => input.Item3.Item2 == outerNode)
                                                .Select(input => input.Item3.Item1),
                                Inputs = nodeInputs,
                                OuterNodePortDataList = inPortsConnected
                            };
                        }).ToList();

            #endregion

            #region UI Positioning Calculations

            double avgX = selectedNodeSet.Average(node => node.X);
            double avgY = selectedNodeSet.Average(node => node.Y);

            double leftMost = selectedNodeSet.Min(node => node.X);
            double topMost = selectedNodeSet.Min(node => node.Y);
            double rightMost = selectedNodeSet.Max(node => node.X + node.Width);

            #endregion

            #region Move selection to new workspace

            var connectors = new HashSet<dynConnectorModel>(currentWorkspace.Connectors.Where(
                                                                conn => selectedNodeSet.Contains(conn.Start.Owner)
                                                                    && selectedNodeSet.Contains(conn.End.Owner)));

            //Step 2: move all nodes to new workspace
            //  remove from old
            foreach (var ele in selectedNodeSet)
            {
                currentWorkspace.Nodes.Remove(ele);
            }
            foreach (var ele in connectors)
            {
                currentWorkspace.Connectors.Remove(ele);
            }

            //  add to new
            newNodeWorkspace.Nodes.AddRange(selectedNodeSet);
            newNodeWorkspace.Connectors.AddRange(connectors);

            double leftShift = leftMost - 250;
            foreach (dynNodeModel node in newNodeWorkspace.Nodes)
            {
                node.X = node.X - leftShift;
                node.Y = node.Y - topMost;
            }

            #endregion

            #region Insert new node into the current workspace

            //Step 5: insert new node into original workspace
            var collapsedNode = dynSettings.Controller.DynamoViewModel.CreateFunction(
                inputs.Select(x => x.Item1.InPortData[x.Item2].NickName),
                outputs
                    .Where(x => !curriedNodeArgs.Any(y => y.OuterNode == x.Item3.Item2))
                    .Select(x => x.Item1.OutPortData[x.Item2].NickName),
                newNodeDefinition);

            collapsedNode.GUID = Guid.NewGuid();

            currentWorkspace.Nodes.Add(collapsedNode);
            collapsedNode.WorkSpace = currentWorkspace;

            collapsedNode.X = avgX;
            collapsedNode.Y = avgY;

            #endregion

            #region Destroy all hanging connectors

            //Step 6: connect inputs and outputs

            var removeConnectors = currentWorkspace.Connectors.Where(c =>
                                                                     selectedNodeSet.Contains(c.Start.Owner) ||
                                                                     selectedNodeSet.Contains(c.End.Owner))
                                                   .ToList();
            foreach (dynConnectorModel connector in removeConnectors)
            {
                connector.NotifyConnectedPortsOfDeletion();
                currentWorkspace.Connectors.Remove(connector);
            }

            #endregion

            newNodeWorkspace.Nodes.ToList().ForEach(x => x.DisableReporting());

            var inConnectors = new List<Tuple<dynNodeModel, int, int>>();

            #region Process inputs

            //Step 3: insert variables (reference step 1)
            foreach (var input in Enumerable.Range(0, inputs.Count).Zip(inputs, Tuple.Create))
            {
                int inputIndex = input.Item1;

                dynNodeModel inputReceiverNode = input.Item2.Item1;
                int inputReceiverData = input.Item2.Item2;

                dynNodeModel inputNode = input.Item2.Item3.Item2;
                int inputData = input.Item2.Item3.Item1;

//MVVM : replace NodeUI reference with node
                inConnectors.Add(new Tuple<dynNodeModel, int, int>(inputNode, inputData, inputIndex));

                //Create Symbol Node
                var node = new dynSymbol
                {
                    Symbol = inputReceiverNode.InPortData[inputReceiverData].NickName
                };

                //MVVM : Don't make direct reference to view here
                //dynNodeView nodeUI = node.NodeUI;
                
                var elNameAttrib =
                    node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), true)[0] as NodeNameAttribute;
                if (elNameAttrib != null)
                {
                    node.NickName = elNameAttrib.Name;
                }

                node.GUID = Guid.NewGuid();

                //store the element in the elements list
                newNodeWorkspace.Nodes.Add(node);
                node.WorkSpace = newNodeWorkspace;

                node.DisableReporting();

                //MVVM : Do not add view directly to canvas
                /*dynSettings.Bench.WorkBench.Children.Add(nodeUI);

                //Place it in an appropriate spot
                Canvas.SetLeft(nodeUI, 0);
                Canvas.SetTop(nodeUI, inputIndex * (50 + node.NodeUI.Height));

                dynSettings.Bench.WorkBench.UpdateLayout();*/
                node.X = 0;
                node.Y = inputIndex*(50 + node.Height);


                var curriedNode = curriedNodeArgs.FirstOrDefault(
                    x => x.OuterNode == inputNode);

                if (curriedNode == null)
                {
                    //Connect it (new dynConnector)
                    newNodeWorkspace.Connectors.Add(new dynConnectorModel(
                                                        node,
                                                        inputReceiverNode,
                                                        0,
                                                        inputReceiverData,
                                                        0,
                                                        false));
                }
                else
                {
                    //Connect it to the applier
                    newNodeWorkspace.Connectors.Add(new dynConnectorModel(
                                                        node,
                                                        curriedNode.InnerNode,
                                                        0,
                                                        0,
                                                        0,
                                                        false));

                    //Connect applier to the inner input receiver
                    newNodeWorkspace.Connectors.Add(new dynConnectorModel(
                                                        curriedNode.InnerNode,
                                                        inputReceiverNode,
                                                        0,
                                                        inputReceiverData,
                                                        0,
                                                        false));
                }
            }

            #endregion

            #region Process outputs

            //List of all inner nodes to connect an output. Unique.
            var outportList = new List<Tuple<dynNodeModel, int>>();

            var outConnectors = new List<Tuple<dynNodeModel, int, int>>();

            int i = 0;
            foreach (var output in outputs)
            {
                if (outportList.All(x => !(x.Item1 == output.Item1 && x.Item2 == output.Item2)))
                {
                    dynNodeModel outputSenderNode = output.Item1;
                    int outputSenderData = output.Item2;
                    dynNodeModel outputReceiverNode = output.Item3.Item2;

                    if (curriedNodeArgs.Any(x => x.OuterNode == outputReceiverNode))
                        continue;

                    outportList.Add(Tuple.Create(outputSenderNode, outputSenderData));

                    //Create Symbol Node
                    var node = new dynOutput
                    {
                        Symbol = outputSenderNode.OutPortData[outputSenderData].NickName
                    };

                    //dynNodeView nodeUI = node.NodeUI;

                    var elNameAttrib =
                        node.GetType().GetCustomAttributes(typeof(NodeNameAttribute), false)[0] as NodeNameAttribute;
                    if (elNameAttrib != null)
                    {
                        node.NickName = elNameAttrib.Name;
                    }

                    node.GUID = Guid.NewGuid();

                    //store the element in the elements list
                    newNodeWorkspace.Nodes.Add(node);
                    node.WorkSpace = newNodeWorkspace;

                    node.DisableReporting();

                    //MVVM : Do not add view directly to canvas
                    /*dynSettings.Bench.WorkBench.Children.Add(nodeUI);

                    //Place it in an appropriate spot
                    Canvas.SetLeft(nodeUI, rightMost + 75 - leftShift);
                    Canvas.SetTop(nodeUI, i * (50 + node.NodeUI.Height));

                    dynSettings.Bench.WorkBench.UpdateLayout();*/

                    node.X = rightMost + 75 - leftShift;
                    node.Y = i*(50 + node.Height);

                    newNodeWorkspace.Connectors.Add(new dynConnectorModel(
                                                        outputSenderNode,
                                                        node,
                                                        outputSenderData,
                                                        0,
                                                        0,
                                                        false));



                    i++;
                }
            }

            //Connect outputs to new node
            foreach (var output in outputs)
            {
                //Node to be connected to in CurrentSpace
                dynNodeModel outputSenderNode = output.Item1;

                //Port to be connected to on outPutNode_outer
                int outputSenderData = output.Item2;

                int outputReceiverData = output.Item3.Item1;
                dynNodeModel outputReceiverNode = output.Item3.Item2;

                var curriedNode = curriedNodeArgs.FirstOrDefault(
                    x => x.OuterNode == outputReceiverNode);

                if (curriedNode == null)
                {
                    // we create the connectors in the current space later
//MVVM : replaced multiple dynNodeView refrences with dynNode
                    outConnectors.Add(new Tuple<dynNodeModel, int, int>(outputReceiverNode,
                                                                     outportList.FindIndex(x => x.Item1 == outputSenderNode && x.Item2 == outputSenderData),
                                                                     outputReceiverData));
                }
                else
                {
                    int targetPort = curriedNode.Inputs
                                                .First(
                                                    x => x.InnerNodeInputSender == outputSenderNode)
                                                .OuterNodeInPortData;

                    int targetPortIndex = curriedNode.OuterNodePortDataList.IndexOf(targetPort);
                    
                    //Connect it (new dynConnector)
                    newNodeWorkspace.Connectors.Add(new dynConnectorModel(
                                                        outputSenderNode,
                                                        curriedNode.InnerNode,
                                                        outputSenderData,
                                                        targetPortIndex + 1,
                                                        0));
                }
            }

            #endregion

            //set the name on the node
            collapsedNode.NickName = newNodeName;

            currentWorkspace.Nodes.Remove(collapsedNode);

            // save and load the definition from file
            dynSettings.Controller.CustomNodeLoader.SetNodeInfo(newNodeName, newNodeCategory, newNodeDefinition.FunctionId, "");
            var path = dynSettings.Controller.DynamoViewModel.SaveFunctionOnly(newNodeDefinition);
            dynSettings.Controller.CustomNodeLoader.SetNodePath(newNodeDefinition.FunctionId, path);
            dynSettings.Controller.SearchViewModel.Add(newNodeName, newNodeCategory, newNodeDefinition.FunctionId);

            dynSettings.Controller.DynamoViewModel.CreateNodeCommand.Execute(new Dictionary<string, object>()
                {
                    {"name", collapsedNode.Definition.FunctionId.ToString() },
                    {"x", avgX },
                    {"y", avgY }
                });

            var newlyPlacedCollapsedNode = currentWorkspace.Nodes
                                            .Where(node => node is dynFunction)
                                            .First(node => ((dynFunction)node).Definition.FunctionId == newNodeDefinition.FunctionId);

            // place the node as intended, not centered
            newlyPlacedCollapsedNode.X = avgX;
            newlyPlacedCollapsedNode.Y = avgY;

            newlyPlacedCollapsedNode.DisableReporting();

            foreach (var nodeTuple in inConnectors)
            {
                currentWorkspace.Connectors.Add(
                    new dynConnectorModel(
                        nodeTuple.Item1,
                        newlyPlacedCollapsedNode,
                        nodeTuple.Item2,
                        nodeTuple.Item3,
                        0,
                        true));
            }

            foreach (var nodeTuple in outConnectors)
            {
                currentWorkspace.Connectors.Add(
                    new dynConnectorModel(
                        newlyPlacedCollapsedNode,
                        nodeTuple.Item1,
                        nodeTuple.Item2,
                        nodeTuple.Item3,
                        0,
                        true));
            }

            newlyPlacedCollapsedNode.EnableReporting();
            currentWorkspace.EnableReporting();

        }

    }
}
