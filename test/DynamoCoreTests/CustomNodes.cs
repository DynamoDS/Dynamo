using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using NUnit.Framework;
using System.Text;
using Dynamo.DSEngine;
using ProtoCore.DSASM;
using ProtoCore.Mirror;
using System.Collections;

namespace Dynamo.Tests
{
    internal class CustomNodes : DSEvaluationViewModelUnitTest
    {
        [Test]
        public void CanCollapseNodesAndGetSameResult()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");

            string openPath = Path.Combine(examplePath, "collapse.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var watchNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            var numNodesPreCollapse = model.CurrentWorkspace.Nodes.Count;

            ViewModel.Model.RunExpression();

            var valuePreCollapse = watchNode.CachedValue;

            var nodesToCollapse = new[]
            {
                "1da395b9-2539-4705-a479-1f6e575df01d", 
                "b8130bf5-dd14-4784-946d-9f4705df604e",
                "a54c7cfa-450a-4edc-b7a5-b3e15145a9e1"
            };

            foreach (var guid in nodesToCollapse)
            {
                var node = model.Nodes.First(x => x.GUID == Guid.Parse(guid));
                model.AddToSelection(node);
            }

            NodeCollapser.Collapse(ViewModel.Model,
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                model.CurrentWorkspace,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            var numNodesPostCollapse = model.CurrentWorkspace.Nodes.Count;

            Assert.AreNotEqual(numNodesPreCollapse, numNodesPostCollapse);
            Assert.AreEqual(nodesToCollapse.Length, numNodesPreCollapse - numNodesPostCollapse + 1);

            ViewModel.Model.RunExpression();

            var valuePostCollapse = watchNode.CachedValue;

            // Ensure the values are equal and both 65.
            Assert.AreEqual(65, valuePreCollapse);
            Assert.AreEqual(valuePreCollapse, valuePostCollapse);

        }

        [Test]
        public void CanCollapseNodesWithDefaultValues()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");

            string openPath = Path.Combine(examplePath, "collapse-defaults.dyn");
            RunModel(openPath);

            //Confirm that everything is working OK.
            ViewModel.Model.RunExpression();

            var minNode = model.CurrentWorkspace.NodeFromWorkspace("13f58ca4-4e48-4757-b16a-45b971a6d7fc");
            var numNode = model.CurrentWorkspace.NodeFromWorkspace("4b6487e1-1bcf-47a6-a6fb-ea3122a303af");

            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(1, model.CurrentWorkspace.Connectors.Count);

            AssertPreviewValue("13f58ca4-4e48-4757-b16a-45b971a6d7fc", 10);

            model.AddToSelection(minNode);
            model.AddToSelection(numNode);

            NodeCollapser.Collapse(ViewModel.Model,
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                model.CurrentWorkspace,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            Assert.AreEqual(1, model.CurrentWorkspace.Nodes.Count);

            ViewModel.Model.RunExpression();

            var collapsedNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>();

            AssertPreviewValue(collapsedNode.GUID.ToString(), 10);
        }

        [Test]
        public void CanCollapseWith1NodeHoleInSelection()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");

            string openPath = Path.Combine(examplePath, "collapse-function.dyn");
            RunModel(openPath);

            //Confirm that everything is working OK.
            ViewModel.Model.RunExpression();

            var mulNode = model.CurrentWorkspace.NodeFromWorkspace("7bae9908-6e44-41a4-8b9a-e6cd58791194");

            AssertPreviewValue(mulNode.GUID.ToString(), 0);

            foreach (var node in
                model.CurrentWorkspace.Nodes.Where(
                    x => x.GUID.ToString() != "34d7a656-338d-43bd-bb3d-224515a855eb"))
            {
                model.AddToSelection(node);
            }

            NodeCollapser.Collapse(ViewModel.Model,
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                model.CurrentWorkspace,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);

            ViewModel.Model.RunExpression();

            var collapsedNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>();

            AssertPreviewValue(collapsedNode.GUID.ToString(), 0);
        }

        [Test]
        public void CanCollapseAndUndoRedo()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");
            ViewModel.OpenCommand.Execute(Path.Combine(examplePath, "collapse-number-chain.dyn"));

            // Ensure all the nodes we are looking for are actually there.
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);
            var existenceMap = new Dictionary<string, bool>
            {
                { "5a503c02-13a7-4def-9fb6-52101117219e", true },
                { "6e7bdd5a-6c3c-4588-bb7d-bb49c969812b", true },
                { "da2cbc80-a278-4699-96fa-a22d7762a42d", true },
                { "28cff154-ef78-43fa-bcc9-f86e00ce2ced", true },
                { "7ad3d045-c620-4817-8723-afd3c266555b", true },
                { "e8388f0d-2438-4b8b-87d1-f473c9e2c9a8", true },
                { "fed04d43-aad6-4782-a3c4-a86925e6b538", true },
                { "5e0d6637-5156-4b60-b49d-3c9aedd71884", true },
                { "98350887-4839-4ece-a4ad-37137cb11f52", true },
                { "8f4a460d-dada-4ecd-a0ca-9adb32d36f12", true },
                { "9cbbfa9c-fb5d-4a18-8d4b-5a02d842724e", true }
            };
            this.VerifyModelExistence(existenceMap);

            string[] guids =
            {
                "5e0d6637-5156-4b60-b49d-3c9aedd71884", // Addition
                "98350887-4839-4ece-a4ad-37137cb11f52", // Addition
                "28cff154-ef78-43fa-bcc9-f86e00ce2ced", // Double input
                "7ad3d045-c620-4817-8723-afd3c266555b", // Double input
            };

            List<NodeModel> selectionSet = new List<NodeModel>();
            var workspace = model.CurrentWorkspace;

            foreach (string guid in guids)
            {
                var m = workspace.GetModelInternal(Guid.Parse(guid));
                selectionSet.Add(m as NodeModel);
            }

            // Making sure we do not have any Function node at this point.
            Assert.IsNull(model.CurrentWorkspace.FirstNodeFromWorkspace<Function>());
            Assert.AreEqual(false, model.CurrentWorkspace.CanUndo);
            Assert.AreEqual(false, model.CurrentWorkspace.CanRedo);

            NodeCollapser.Collapse(ViewModel.Model,
                selectionSet.AsEnumerable(),
                model.CurrentWorkspace,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            // Making sure we have a Function node after the conversion.
            Assert.IsNotNull(model.CurrentWorkspace.FirstNodeFromWorkspace<Function>());

            // Make sure we have 8 nodes left (11 - 4 + 1).
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            existenceMap.Clear();
            existenceMap.Add("5a503c02-13a7-4def-9fb6-52101117219e", true);
            existenceMap.Add("6e7bdd5a-6c3c-4588-bb7d-bb49c969812b", true);
            existenceMap.Add("da2cbc80-a278-4699-96fa-a22d7762a42d", true);
            existenceMap.Add("28cff154-ef78-43fa-bcc9-f86e00ce2ced", false);
            existenceMap.Add("7ad3d045-c620-4817-8723-afd3c266555b", false);
            existenceMap.Add("e8388f0d-2438-4b8b-87d1-f473c9e2c9a8", true);
            existenceMap.Add("fed04d43-aad6-4782-a3c4-a86925e6b538", true);
            existenceMap.Add("5e0d6637-5156-4b60-b49d-3c9aedd71884", false);
            existenceMap.Add("98350887-4839-4ece-a4ad-37137cb11f52", false);
            existenceMap.Add("8f4a460d-dada-4ecd-a0ca-9adb32d36f12", true);
            existenceMap.Add("9cbbfa9c-fb5d-4a18-8d4b-5a02d842724e", true);
            this.VerifyModelExistence(existenceMap);

            // Try undoing the conversion operation.
            Assert.AreEqual(true, model.CurrentWorkspace.CanUndo);
            Assert.AreEqual(false, model.CurrentWorkspace.CanRedo);
            model.CurrentWorkspace.Undo();
            Assert.AreEqual(false, model.CurrentWorkspace.CanUndo);
            Assert.AreEqual(true, model.CurrentWorkspace.CanRedo);

            // Now it should have gone back to 11 nodes.
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);
            existenceMap.Clear();
            existenceMap.Add("5a503c02-13a7-4def-9fb6-52101117219e", true);
            existenceMap.Add("6e7bdd5a-6c3c-4588-bb7d-bb49c969812b", true);
            existenceMap.Add("da2cbc80-a278-4699-96fa-a22d7762a42d", true);
            existenceMap.Add("28cff154-ef78-43fa-bcc9-f86e00ce2ced", true);
            existenceMap.Add("7ad3d045-c620-4817-8723-afd3c266555b", true);
            existenceMap.Add("e8388f0d-2438-4b8b-87d1-f473c9e2c9a8", true);
            existenceMap.Add("fed04d43-aad6-4782-a3c4-a86925e6b538", true);
            existenceMap.Add("5e0d6637-5156-4b60-b49d-3c9aedd71884", true);
            existenceMap.Add("98350887-4839-4ece-a4ad-37137cb11f52", true);
            existenceMap.Add("8f4a460d-dada-4ecd-a0ca-9adb32d36f12", true);
            existenceMap.Add("9cbbfa9c-fb5d-4a18-8d4b-5a02d842724e", true);
            this.VerifyModelExistence(existenceMap);

            // Try redoing the conversion.
            model.CurrentWorkspace.Redo();
            Assert.AreEqual(true, model.CurrentWorkspace.CanUndo);
            Assert.AreEqual(false, model.CurrentWorkspace.CanRedo);

            // It should have gone back to 8 nodes.
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            existenceMap.Clear();
            existenceMap.Add("5a503c02-13a7-4def-9fb6-52101117219e", true);
            existenceMap.Add("6e7bdd5a-6c3c-4588-bb7d-bb49c969812b", true);
            existenceMap.Add("da2cbc80-a278-4699-96fa-a22d7762a42d", true);
            existenceMap.Add("28cff154-ef78-43fa-bcc9-f86e00ce2ced", false);
            existenceMap.Add("7ad3d045-c620-4817-8723-afd3c266555b", false);
            existenceMap.Add("e8388f0d-2438-4b8b-87d1-f473c9e2c9a8", true);
            existenceMap.Add("fed04d43-aad6-4782-a3c4-a86925e6b538", true);
            existenceMap.Add("5e0d6637-5156-4b60-b49d-3c9aedd71884", false);
            existenceMap.Add("98350887-4839-4ece-a4ad-37137cb11f52", false);
            existenceMap.Add("8f4a460d-dada-4ecd-a0ca-9adb32d36f12", true);
            existenceMap.Add("9cbbfa9c-fb5d-4a18-8d4b-5a02d842724e", true);
            this.VerifyModelExistence(existenceMap);
        }

        [Test]
        public void GitHub_461_DeleteNodesFromCustomNodeWorkspaceAfterCollapse()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");

            string openPath = Path.Combine(examplePath, "collapse.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            var nodesToCollapse = new[]
            {
                "1da395b9-2539-4705-a479-1f6e575df01d", 
                "b8130bf5-dd14-4784-946d-9f4705df604e",
                "a54c7cfa-450a-4edc-b7a5-b3e15145a9e1"
            };

            foreach (
                var node in
                    nodesToCollapse.Select(guid => model.CurrentWorkspace.NodeFromWorkspace(guid)))
            {
                model.AddToSelection(node);
            }

            NodeCollapser.Collapse(ViewModel.Model,
                 DynamoSelection.Instance.Selection.Where(x => x is NodeModel)
                    .Select(x => (x as NodeModel)),
                    model.CurrentWorkspace,
                    new FunctionNamePromptEventArgs
                    {
                        Category = "Testing",
                        Description = "",
                        Name = "__CollapseTest2__",
                        Success = true
                    });

            ViewModel.GoToWorkspace(
                ViewModel.Model.CustomNodeManager.GetGuidFromName("__CollapseTest2__"));

            var workspace = model.CurrentWorkspace;
            Assert.AreEqual(6, workspace.Nodes.Count);

            var modelsToDelete = new List<ModelBase>();
            var addition = workspace.FirstNodeFromWorkspace<DSFunction>();
            Assert.IsNotNull(addition);
            Assert.AreEqual("+", addition.NickName);

            modelsToDelete.Add(addition);
            model.DeleteModelInternal(modelsToDelete);
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
        }

        [Test]
        public void ReduceAndRecursion()
        {
            var model = ViewModel.Model;

            var examplePath = Path.Combine(GetTestDirectory(), @"core\reduce_and_recursion\");

            string openPath = Path.Combine(examplePath, "reduce-example.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            ViewModel.Model.RunExpression();

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            AssertPreviewValue("157557d2-2452-413a-9944-1df3df793cee", 15.0);
            AssertPreviewValue("068dd555-a5d5-4f11-af05-e4fa0cc015c9", 15.0);
            AssertPreviewValue("1aca382d-ca81-4955-a6c1-0f549df19fd7", 15.0);
        }

        [Test]
        public void FilterWithCustomNode()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\filter\");

            Assert.IsTrue(
                ViewModel.Model.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "IsOdd.dyf"))
                != null);

            string openPath = Path.Combine(examplePath, "filter-example.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            // run the expression
            ViewModel.Model.RunExpression();

            // check the output values are correctly computed
            var watchNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Assert.IsNotNull(watchNode);

            // odd numbers between 0 and 5
            Assert.IsNotNull(watchNode.CachedValue);
            Assert.IsTrue(watchNode.CachedValue is ICollection);
            var list = (watchNode.CachedValue as ICollection).Cast<object>();

            Assert.AreEqual(new[] { 1, 3, 5 }, list.ToList());
        }

        /// <summary>
        /// Run an infinite recursive loop for 10 seconds to confirm that it doesn't stack overflow
        /// </summary>
        [Test]
        public void TailCallOptimization()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void CanEvaluateCustomNodeWithDuplicateInputs()
        {
            var examplePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\duplicate-input.dyn");
            ViewModel.OpenCommand.Execute(examplePath);
            ViewModel.Model.RunExpression();

            var addNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Function>();
            AssertPreviewValue(addNode.GUID.ToString(), 3);
        }
        
        [Test]
        public void CanCreateAndPlaceNewCustomNode()
        {
            const string name = "Custom Node Creation Test";
            const string description = "Description";
            const string category = "Custom Node Category";

            ViewModel.ExecuteCommand(
                new DynamoModel.CreateCustomNodeCommand(
                    Guid.NewGuid(),
                    name,
                    category,
                    description,
                    true));

            Assert.IsInstanceOf<CustomNodeWorkspaceModel>(ViewModel.Model.CurrentWorkspace);
            var customNodeWs = ViewModel.Model.CurrentWorkspace as CustomNodeWorkspaceModel;
            var customNodeDef = customNodeWs.CustomNodeDefinition;
            Assert.AreEqual(name, customNodeDef.DisplayName);
            Assert.AreEqual(description, customNodeWs.Description);
            Assert.AreEqual(category, customNodeWs.Category);

            ViewModel.HomeCommand.Execute(null);

            ViewModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(
                    Guid.NewGuid(),
                    customNodeDef.FunctionId.ToString(),
                    0,
                    0,
                    true,
                    true));

            Assert.AreEqual(1, ViewModel.Model.HomeSpace.Nodes.Count);
            Assert.IsInstanceOf<Function>(ViewModel.Model.HomeSpace.Nodes.First());
            Assert.AreSame(customNodeDef, (ViewModel.Model.HomeSpace.Nodes.First() as Function).Definition);
        }


        /// <summary>
        /// Run a custom node, change parameter/output/function names, run again to verify consistency
        /// </summary>
        [Test]
        public void ModificationUITesting()
        {
            // Re-use code for creating a custom node
            CanCreateAndPlaceNewCustomNode();

            var instance = ViewModel.Model.HomeSpace.Nodes.First() as Function;

            ViewModel.GoToWorkspaceCommand.Execute(instance.Definition.FunctionId);

            var currentInPortAmt = 0;
            var currentOutPortAmt = 0;

            #region Reflection code for instantiating Input and Output nodes
            var inputType = typeof(Symbol);
            var outputType = typeof(Output);

            var inputName =
                inputType.GetCustomAttributes(typeof(NodeNameAttribute), false)
                    .OfType<NodeNameAttribute>()
                    .First()
                    .Name;

            var outputName =
                outputType.GetCustomAttributes(typeof(NodeNameAttribute), false)
                    .OfType<NodeNameAttribute>()
                    .First()
                    .Name;
            #endregion

            #region Adding
            Func<string, Symbol> addInput = label =>
            {
                var guid = Guid.NewGuid();

                ViewModel.ExecuteCommand(
                    new DynamoModel.CreateNodeCommand(
                        guid,
                        inputName,
                        0,
                        0,
                        true,
                        true));

                var node = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace<Symbol>(guid);
                node.InputSymbol = label;

                Assert.AreEqual(++currentInPortAmt, instance.InPorts.Count);
                Assert.AreEqual(label, instance.InPorts.Last().PortName);

                return node;
            };

            Func<string, Output> addOutput = label =>
            {
                var guid = Guid.NewGuid();

                ViewModel.ExecuteCommand(
                    new DynamoModel.CreateNodeCommand(
                        guid,
                        outputName,
                        0,
                        0,
                        true,
                        true));

                var node = ViewModel.Model.CurrentWorkspace.NodeFromWorkspace<Output>(guid);
                node.Symbol = label;

                Assert.AreEqual(++currentOutPortAmt, instance.OutPorts.Count);
                Assert.AreEqual(label, instance.OutPorts.Last().PortName);

                return node;
            };
            #endregion

            #region Renaming
            Action<Symbol, int, string> renameInput = (input, idx, s) =>
            {
                input.InputSymbol = s;
                Assert.AreEqual(s, instance.InPorts[idx].PortName);
            };

            Action<Output, int, string> renameOutput = (output, idx, s) =>
            {
                output.Symbol = s;
                Assert.AreEqual(s, instance.OutPorts[idx].PortName);
            };
            #endregion

            #region Deleting
            Action<NodeModel> deleteNode = nodeModel =>
            {
                DynamoSelection.Instance.ClearSelection();
                ViewModel.Model.AddToSelection(nodeModel);
                ViewModel.DeleteCommand.Execute(null);
            };

            Action<Symbol> deleteInput = input =>
            {
                deleteNode(input);
                Assert.AreEqual(--currentInPortAmt, instance.InPorts.Count);
            };

            Action<Output> deleteOutput = output =>
            {
                deleteNode(output);
                Assert.AreEqual(--currentOutPortAmt, instance.OutPorts.Count);
            };
            #endregion

            //Add some outputs
            var out1 = addOutput("output1");
            var out2 = addOutput("output2");

            //Add some inputs
            var in1 = addInput("input1");
            var in2 = addInput("input2");

            //Change some names
            renameInput(in1, 0, "test");
            renameOutput(out2, 1, "something");

            //Delete some ports
            deleteInput(in2);
            deleteOutput(out1);
        }

        /// <summary>
        /// Modification of a recursive custom node results in UI updating for all instances.
        /// </summary>
        [Test]
        public void ModificationUITesting_Recursive()
        {
            // var homeNode = custom node instance in the home workspace
            // var recNode = recursive custom node instance in the custom node workspace
              
            // change input/output names in the custom node definition, make sure instances were updated
            // change name of the custom node, make sure instances were updated
            // increase amount of inputs/outputs, make sure instances were updated
            // decrease amount of inputs/outputs
            //      make sure instances were updated
            //      make sure any connectors attached to removed ports were deleted
            Assert.Inconclusive();
        }

        /// <summary>
        /// Confirm that a custom node with multiple outputs evaluates successfully.
        /// </summary>
        [Test]
        public void MultipleOutputs()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\multiout");

            string openPath = Path.Combine(examplePath, "multi-custom.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            ViewModel.Model.RunExpression();

            var splitListVal = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>().CachedValue;

            Assert.IsTrue(splitListVal.IsCollection);

            var outs = splitListVal.GetElements();

            Assert.AreEqual(2, outs.Count);

            var out1 = outs[0];
            Assert.AreEqual(0, out1.Data);

            var out2 = outs[1];
            Assert.IsTrue(out2.IsCollection);
            Assert.IsFalse(out2.GetElements().Any());
        }

        [Test]
        public void PartialApplicationWithMultipleOutputs()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\multiout");

            string openPath = Path.Combine(examplePath, "partial-multi-custom.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            ViewModel.Model.RunExpression();

            var firstWatch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("d824e8dd-1009-449f-b5d6-1cd83bd180d6");

            Assert.AreEqual(new ArrayList { 0 }, firstWatch.CachedValue);

            var restWatch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("af7ada9a-4316-475b-8582-742acc40fc1b");

            Assert.IsTrue(restWatch.CachedValue is ICollection);
            Assert.IsTrue((restWatch.CachedValue as ICollection).Cast<object>().First() is ICollection);
            Assert.IsFalse(
                ((restWatch.CachedValue as ICollection).Cast<object>().First() as ICollection)
                    .Cast<object>().Any());
        }

        [Test]
        public void CollapsedNodeShouldHaveNewIdentfifer()
        {
            var model = ViewModel.Model;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");
            ViewModel.OpenCommand.Execute(Path.Combine(examplePath, "collapse-newname.dyn"));

            // Convert a DSFunction node Point.ByCoordinates to custom node.
            var workspace = model.CurrentWorkspace;
            var node = workspace.Nodes.OfType<DSFunction>().First();

            var originalGuid = node.GUID;
            var originalIdentifierName = node.AstIdentifierBase;
            var originalIdentifier = node.AstIdentifierForPreview;

            List<NodeModel> selectionSet = new List<NodeModel>() { node };
            NodeCollapser.Collapse(ViewModel.Model,
                selectionSet.AsEnumerable(),
                model.CurrentWorkspace,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            // Making sure we have a Function node after the conversion.
            Assert.IsNotNull(model.CurrentWorkspace.FirstNodeFromWorkspace<Function>());

            var customWorkspace = model.Workspaces.Where(w => w is CustomNodeWorkspaceModel).First() 
                                    as CustomNodeWorkspaceModel;
            // As there is only one node is converted to custom node, get
            // the first one
            var collapsedNode = customWorkspace.Nodes.OfType<DSFunction>().First();
            
            // Node -> custom node just copy node from home workspace to 
            // custom workspace, so they are the same node
            Assert.IsTrue(object.ReferenceEquals(node, collapsedNode));

            // But they should have different guid and different identifier name
            Assert.AreNotEqual(originalGuid, collapsedNode.GUID);
            Assert.AreNotEqual(originalIdentifierName, collapsedNode.AstIdentifierBase);
            Assert.AreNotEqual(originalIdentifier, collapsedNode.AstIdentifierForPreview);
        }

        [Test]
        public void EvaluateProxyCustomNodeInstances()
        {
            // Defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5555
            // Dyn file contains a proxy custom node. Evaluating the whole graph
            // should evaluate all nodes except those proxy custom node instance. 
            var model = ViewModel.Model;
            var dynFilePath = Path.Combine(GetTestDirectory(), @"core\CustomNodes\missing_custom_node.dyn");

            ViewModel.OpenCommand.Execute(dynFilePath);
            ViewModel.Model.RunExpression();

            AssertPreviewValue("1b8b309b-ee2e-44fe-ac98-2123b2711bea", 1);
            AssertPreviewValue("08db7d60-845c-439c-b7ca-c2a06664a948", 2);
        }
    }
}