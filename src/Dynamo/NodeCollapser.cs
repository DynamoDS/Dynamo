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
        internal static void Collapse(IEnumerable<dynNode> selectedNodes, dynWorkspace currentWorkspace)
        {
            var selectedNodeSet = new HashSet<dynNode>(selectedNodes);

            // TODO: this code needs refactoring
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
            var inputs = new HashSet<Tuple<dynNode, int, Tuple<int, dynNode>>>(
                    selectedNodeSet
                        .SelectMany(node => Enumerable.Range(0, node.InPortData.Count)
                            .Where(node.HasInput)
                            .Select(data => Tuple.Create(node, data, node.Inputs[data]))
                                                 .Where(input => !selectedNodeSet.Contains(input.Item3.Item2))));

            var outputs = new HashSet<Tuple<dynNode, int, Tuple<int, dynNode>>>(
                selectedNodeSet.SelectMany(
                    node => Enumerable.Range(0, node.OutPortData.Count).Where(node.HasOutput).SelectMany(
                        data => node.Outputs[data]
                                    .Where(output => !selectedNodeSet.Contains(output.Item2))
                                    .Select(output => Tuple.Create(node, data, output)))));

            #endregion

            #region Detect 1-node holes (higher-order function extraction)

            var curriedNodeArgs =
                new HashSet<dynNode>(
                    inputs
                        .Select(x => x.Item3.Item2)
                        .Intersect(outputs.Select(x => x.Item3.Item2)))
                    .Select(
                        outerNode =>
                        {
                            var node = new dynApply1();

                            //MVVM : Don't make direct reference to view here
                            //MVVM: no reference to view here
                            //dynNodeUI nodeUI = node.NodeUI;

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

//MVVM : use node positioning calculations to set a property and bind that to the view
            /*double avgX = selectedNodeSet.Average(node => Canvas.GetLeft(node.NodeUI));
            double avgY = selectedNodeSet.Average(node => Canvas.GetTop(node.NodeUI));

            double leftMost = selectedNodeSet.Min(node => Canvas.GetLeft(node.NodeUI)) + 24;
            double topMost = selectedNodeSet.Min(node => Canvas.GetTop(node.NodeUI));
            double rightMost = selectedNodeSet.Max(node => Canvas.GetLeft(node.NodeUI) + node.NodeUI.Width);*/

            double avgX = selectedNodeSet.Average(node => node.X);
            double avgY = selectedNodeSet.Average(node => node.Y);

            double leftMost = selectedNodeSet.Min(node => node.X);
            double topMost = selectedNodeSet.Min(node => node.Y);
            double rightMost = selectedNodeSet.Max(node => node.X + node.Width);

            #endregion

            #region Move selection to new workspace

            var connectors = new HashSet<dynConnector>(
                currentWorkspace.Connectors.Where(
                    conn => selectedNodeSet.Contains(conn.Start.Owner)
                            && selectedNodeSet.Contains(conn.End.Owner)));

            //Step 2: move all nodes to new workspace
            //  remove from old
            //MVVM: confirm that the extension we've defined here does what we expect.
            currentWorkspace.Nodes.RemoveAll(selectedNodeSet.Contains);
            currentWorkspace.Connectors.RemoveAll(connectors.Contains);

            //  add to new
            newNodeWorkspace.Nodes.AddRange(selectedNodeSet);
            newNodeWorkspace.Connectors.AddRange(connectors);

//MVVM : do not set location of view directly
            double leftShift = leftMost - 250;
            foreach (dynNode node in newNodeWorkspace.Nodes)
            {
                //Canvas.SetLeft(node, Canvas.GetLeft(node) - leftShift);
                //Canvas.SetTop(node, Canvas.GetTop(node) - topMost);
                node.X = node.X - leftShift;
                node.Y = node.Y - topMost;
            }

            #endregion

            #region Insert new node into the current workspace

            //Step 5: insert new node into original workspace
            var collapsedNode = new dynFunction(
                inputs.Select(x => x.Item1.InPortData[x.Item2].NickName),
                outputs
                    .Where(x => !curriedNodeArgs.Any(y => y.OuterNode == x.Item3.Item2))
                    .Select(x => x.Item1.OutPortData[x.Item2].NickName),
                newNodeDefinition);

            collapsedNode.GUID = Guid.NewGuid();

            currentWorkspace.Nodes.Add(collapsedNode);
            collapsedNode.WorkSpace = currentWorkspace;

//MVVM : do not set view in collection here
            /*dynSettings.Bench.WorkBench.Children.Add(collapsedNode.NodeUI);

            Canvas.SetLeft(collapsedNode.NodeUI, avgX);
            Canvas.SetTop(collapsedNode.NodeUI, avgY);*/
            collapsedNode.X = avgX;
            collapsedNode.Y = avgY;

            #endregion

            #region Destroy all hanging connectors

            //Step 6: connect inputs and outputs
            foreach (dynConnector connector in currentWorkspace.Connectors
                                                           .Where(
                                                               c =>
                                                               selectedNodeSet.Contains(c.Start.Owner) &&
                                                               !selectedNodeSet.Contains(c.End.Owner))
                                                           .ToList())
            {
                connector.Kill();
            }

            foreach (dynConnector connector in currentWorkspace.Connectors
                                                           .Where(
                                                               c =>
                                                               !selectedNodeSet.Contains(c.Start.Owner) &&
                                                               selectedNodeSet.Contains(c.End.Owner)).ToList()
                )
            {
                connector.Kill();
            }

            #endregion

            newNodeWorkspace.Nodes.ToList().ForEach(x => x.DisableReporting());

            var inConnectors = new List<Tuple<dynNode, int, int>>();

            #region Process inputs

            //Step 3: insert variables (reference step 1)
            foreach (var input in Enumerable.Range(0, inputs.Count).Zip(inputs, Tuple.Create))
            {
                int inputIndex = input.Item1;

                dynNode inputReceiverNode = input.Item2.Item1;
                int inputReceiverData = input.Item2.Item2;

                dynNode inputNode = input.Item2.Item3.Item2;
                int inputData = input.Item2.Item3.Item1;

//MVVM : replace NodeUI reference with node
                inConnectors.Add(new Tuple<dynNode, int, int>(inputNode, inputData, inputIndex));

                //Create Symbol Node
                var node = new dynSymbol
                {
                    Symbol = inputReceiverNode.InPortData[inputReceiverData].NickName
                };

                //MVVM : Don't make direct reference to view here
                //dynNodeUI nodeUI = node.NodeUI;
                
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
                    newNodeWorkspace.Connectors.Add(new dynConnector(
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
                    newNodeWorkspace.Connectors.Add(new dynConnector(
                                                        node,
                                                        curriedNode.InnerNode,
                                                        0,
                                                        0,
                                                        0,
                                                        false));

                    //Connect applier to the inner input receiver
                    newNodeWorkspace.Connectors.Add(new dynConnector(
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
            var outportList = new List<Tuple<dynNode, int>>();

            var outConnectors = new List<Tuple<dynNode, int, int>>();

            int i = 0;
            foreach (var output in outputs)
            {
                if (outportList.All(x => !(x.Item1 == output.Item1 && x.Item2 == output.Item2)))
                {
                    dynNode outputSenderNode = output.Item1;
                    int outputSenderData = output.Item2;
                    dynNode outputReceiverNode = output.Item3.Item2;

                    if (curriedNodeArgs.Any(x => x.OuterNode == outputReceiverNode))
                        continue;

                    outportList.Add(Tuple.Create(outputSenderNode, outputSenderData));

                    //Create Symbol Node
                    var node = new dynOutput
                    {
                        Symbol = outputSenderNode.OutPortData[outputSenderData].NickName
                    };

                    //dynNodeUI nodeUI = node.NodeUI;

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

                    newNodeWorkspace.Connectors.Add(new dynConnector(
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
                dynNode outputSenderNode = output.Item1;

                //Port to be connected to on outPutNode_outer
                int outputSenderData = output.Item2;

                int outputReceiverData = output.Item3.Item1;
                dynNode outputReceiverNode = output.Item3.Item2;

                var curriedNode = curriedNodeArgs.FirstOrDefault(
                    x => x.OuterNode == outputReceiverNode);

                if (curriedNode == null)
                {
                    // we create the connectors in the current space later
//MVVM : replaced multiple dynNodeUI refrences with dynNode
                    outConnectors.Add(new Tuple<dynNode, int, int>(outputReceiverNode,
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
                    newNodeWorkspace.Connectors.Add(new dynConnector(
                                                        outputSenderNode,
                                                        curriedNode.InnerNode,
                                                        outputSenderData,
                                                        targetPortIndex + 1,
                                                        0));
                }
            }

            #endregion

            #region Make new workspace invisible

//MVVM : Do not set view visibility here. It will be controlled by active workspace binding.
            //Step 4: make nodes invisible
            // and update positions
            /*foreach (dynNodeUI node in newNodeWorkspace.Nodes.Select(x => x.NodeUI))
                node.Visibility = Visibility.Hidden;

            foreach (dynConnector connector in newNodeWorkspace.Connectors)
                connector.Visible = false;*/

            #endregion

            //set the name on the node
            collapsedNode.NickName = newNodeName;

            currentWorkspace.Nodes.Remove(collapsedNode);

//MVVM : Don't remove the view directly
            //dynSettings.Bench.WorkBench.Children.Remove(collapsedNode.NodeUI);

            // save and load the definition from file
            var path = dynSettings.Controller.DynamoViewModel.SaveFunctionOnly(newNodeDefinition);
            dynSettings.Controller.CustomNodeLoader.SetNodeInfo(newNodeName, newNodeCategory, newNodeDefinition.FunctionId, path);
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

            newlyPlacedCollapsedNode.DisableReporting();

            //MVVM: don't call update layout here.
            //dynSettings.Bench.WorkBench.UpdateLayout(); // without doing this, connectors fail to be created

            foreach (var nodeTuple in inConnectors)
            {
                currentWorkspace.Connectors.Add(
                    new dynConnector(
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
                    new dynConnector(
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
