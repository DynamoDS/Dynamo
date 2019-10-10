using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreNodeModels;
using DesignScript.Builtin;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class CustomNodes : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void CanCollapseNodesAndGetSameResult()
        {
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse.dyn");
            OpenModel(openPath);

            var watchNode = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            var numNodesPreCollapse = CurrentDynamoModel.CurrentWorkspace.Nodes.Count();

            BeginRun();

            var valuePreCollapse = watchNode.CachedValue;

            var nodesToCollapse = new[]
            {
                "1da395b9-2539-4705-a479-1f6e575df01d",
                "b8130bf5-dd14-4784-946d-9f4705df604e",
                "a54c7cfa-450a-4edc-b7a5-b3e15145a9e1"
            };

            foreach (
                var node in nodesToCollapse.Select(CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace))
            {
                CurrentDynamoModel.AddToSelection(node);
            }

            CurrentDynamoModel.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            var numNodesPostCollapse = CurrentDynamoModel.CurrentWorkspace.Nodes.Count();

            Assert.AreNotEqual(numNodesPreCollapse, numNodesPostCollapse);
            Assert.AreEqual(nodesToCollapse.Length, numNodesPreCollapse - numNodesPostCollapse + 1);

            BeginRun();

            var valuePostCollapse = watchNode.CachedValue;

            // Ensure the values are equal and both 65.
            Assert.AreEqual(65, valuePreCollapse);
            Assert.AreEqual(65, valuePostCollapse);
        }

        [Test]
        public void CanCollapseNodesWithDefaultValues()
        {
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse-defaults.dyn");
            RunModel(openPath);

            var minNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("13f58ca4-4e48-4757-b16a-45b971a6d7fc");
            var numNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("4b6487e1-1bcf-47a6-a6fb-ea3122a303af");

            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            AssertPreviewValue("13f58ca4-4e48-4757-b16a-45b971a6d7fc", 10);

            CurrentDynamoModel.AddToSelection(minNode);
            CurrentDynamoModel.AddToSelection(numNode);

            CurrentDynamoModel.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            BeginRun();

            var collapsedNode = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>();

            AssertPreviewValue(collapsedNode.GUID.ToString(), 10);
        }

        [Test]
        [Category("Failure")]
        public void CanCollapseWith1NodeHoleInSelection()
        {
            //   http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5603
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse-function.dyn");
            RunModel(openPath);

            var mulNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("7bae9908-6e44-41a4-8b9a-e6cd58791194");

            AssertPreviewValue(mulNode.GUID.ToString(), 0);

            foreach (var node in
                CurrentDynamoModel.CurrentWorkspace.Nodes.Where(
                    x => x.GUID.ToString() != "34d7a656-338d-43bd-bb3d-224515a855eb"))
            {
                CurrentDynamoModel.AddToSelection(node);
            }

            CurrentDynamoModel.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            Assert.AreEqual(2, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            BeginRun();

            var collapsedNode = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>();

            AssertPreviewValue(collapsedNode.GUID.ToString(), 0);
        }

        [Test]
        public void CanCollapseAndUndoRedo()
        {
            OpenModel(Path.Combine(TestDirectory, @"core\collapse\collapse-number-chain.dyn"));

            // Ensure all the nodes we are looking for are actually there.
            Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
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
            VerifyModelExistence(existenceMap);

            string[] guids =
            {
                "5e0d6637-5156-4b60-b49d-3c9aedd71884", // Addition
                "98350887-4839-4ece-a4ad-37137cb11f52", // Addition
                "28cff154-ef78-43fa-bcc9-f86e00ce2ced", // Double input
                "7ad3d045-c620-4817-8723-afd3c266555b"  // Double input
            };

            var selectionSet =
                guids.Select(guid => CurrentDynamoModel.CurrentWorkspace.GetModelInternal(Guid.Parse(guid)))
                    .Cast<NodeModel>()
                    .ToList();

            // Making sure we do not have any Function node at this point.
            Assert.IsNull(CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>());
            Assert.AreEqual(false, CurrentDynamoModel.CurrentWorkspace.CanUndo);
            Assert.AreEqual(false, CurrentDynamoModel.CurrentWorkspace.CanRedo);

            CurrentDynamoModel.CustomNodeManager.Collapse(
                selectionSet.AsEnumerable(),
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            // Making sure we have a Function node after the conversion.
            Assert.IsNotNull(CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>());

            // Make sure we have 8 nodes left (11 - 4 + 1).
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
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
            VerifyModelExistence(existenceMap);

            // Try undoing the conversion operation.
            Assert.AreEqual(true, CurrentDynamoModel.CurrentWorkspace.CanUndo);
            Assert.AreEqual(false, CurrentDynamoModel.CurrentWorkspace.CanRedo);
            CurrentDynamoModel.CurrentWorkspace.Undo();
            Assert.AreEqual(false, CurrentDynamoModel.CurrentWorkspace.CanUndo);
            Assert.AreEqual(true, CurrentDynamoModel.CurrentWorkspace.CanRedo);

            // Now it should have gone back to 11 nodes.
            Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
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
            VerifyModelExistence(existenceMap);

            // Try redoing the conversion.
            CurrentDynamoModel.CurrentWorkspace.Redo();
            Assert.AreEqual(true, CurrentDynamoModel.CurrentWorkspace.CanUndo);
            Assert.AreEqual(false, CurrentDynamoModel.CurrentWorkspace.CanRedo);

            // It should have gone back to 8 nodes.
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
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
            VerifyModelExistence(existenceMap);
        }

        [Test]
        public void GitHub_461_DeleteNodesFromCustomNodeWorkspaceAfterCollapse()
        {
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse.dyn");
            OpenModel(openPath);

            var nodesToCollapse = new[]
            {
                "1da395b9-2539-4705-a479-1f6e575df01d",
                "b8130bf5-dd14-4784-946d-9f4705df604e",
                "a54c7cfa-450a-4edc-b7a5-b3e15145a9e1"
            };

            foreach (
                var node in
                    nodesToCollapse.Select(CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace))
            {
                CurrentDynamoModel.AddToSelection(node);
            }

            var ws = CurrentDynamoModel.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest2__",
                    Success = true
                });

            CurrentDynamoModel.AddCustomNodeWorkspace(ws);

            SelectTabByGuid(ws.CustomNodeId);

            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            var modelsToDelete = new List<ModelBase>();
            var addition = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<DSFunction>();
            Assert.IsNotNull(addition);
            Assert.AreEqual("+", addition.Name);

            modelsToDelete.Add(addition);
            CurrentDynamoModel.DeleteModelInternal(modelsToDelete);
            Assert.AreEqual(5, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        public void ReduceAndRecursion()
        {
            string openPath = Path.Combine(TestDirectory, @"core\reduce_and_recursion\reduce-example.dyn");
            OpenModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(11, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            // run the expression
            BeginRun();

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
            var examplePath = Path.Combine(TestDirectory, @"core\filter\");

            CustomNodeInfo info;
            Assert.IsTrue(CurrentDynamoModel.CustomNodeManager.AddUninitializedCustomNode(Path.Combine(examplePath, "IsOdd.dyf"), true, out info));

            string openPath = Path.Combine(examplePath, "filter-example.dyn");
            OpenModel(openPath);

            // run the expression
            BeginRun();

            // check the output values are correctly computed
            var watchNode = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Assert.IsNotNull(watchNode);

            // odd numbers between 0 and 5
            Assert.IsNotNull(watchNode.CachedValue);
            Assert.IsTrue(watchNode.CachedValue is ICollection);
            var list = ((ICollection)watchNode.CachedValue).Cast<long>();

            Assert.AreEqual(new[] { 1, 3, 5 }, list.ToList());
        }

        /// <summary>
        /// Run an infinite tail-recursive loop for 10 seconds to confirm that it doesn't stack overflow
        /// </summary>
        [Test]
        public void TailCallOptimization()
        {
            Assert.Inconclusive();
        }

        [Test]
        public void CanEvaluateCustomNodeWithDuplicateInputs()
        {
            var openPath = Path.Combine(TestDirectory, @"core\CustomNodes\duplicate-input.dyn");
            OpenModel(openPath);
            BeginRun();

            var addNode = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>();
            AssertPreviewValue(addNode.GUID.ToString(), 3);
        }

        [Test]
        public void CanCreateAndPlaceNewCustomNode()
        {
            const string name = "Custom Node Creation Test";
            const string description = "Description";
            const string category = "Custom Node Category";

            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.CreateCustomNodeCommand(
                    Guid.NewGuid(),
                    name,
                    category,
                    description,
                    true));

            Assert.IsInstanceOf<CustomNodeWorkspaceModel>(CurrentDynamoModel.CurrentWorkspace);
            var customNodeWs = CurrentDynamoModel.CurrentWorkspace as CustomNodeWorkspaceModel;
            var customNodeDef = customNodeWs.CustomNodeDefinition;
            Assert.AreEqual(name, customNodeDef.DisplayName);
            Assert.AreEqual(description, customNodeWs.Description);
            Assert.AreEqual(category, customNodeWs.Category);

            var home = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            Assert.NotNull(home);
            SelectTabByGuid(home.Guid);

            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(
                    CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(customNodeDef.FunctionId),
                    0,
                    0,
                    true,
                    true));

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.IsInstanceOf<Function>(CurrentDynamoModel.CurrentWorkspace.Nodes.First());
            Assert.AreEqual(
                customNodeDef.FunctionId,
                ((Function)CurrentDynamoModel.CurrentWorkspace.Nodes.First()).Definition.FunctionId);
        }


        /// <summary>
        /// Run a custom node, change parameter/output/function names, run again to verify consistency
        /// </summary>
        [Test]
        public void ModificationUITesting()
        {
            // Re-use code for creating a custom node
            CanCreateAndPlaceNewCustomNode();

            var instance = CurrentDynamoModel.CurrentWorkspace.Nodes.First() as Function;
            SelectTabByGuid(instance.Definition.FunctionId);

            var currentInPortAmt = 0;
            var currentOutPortAmt = 0;

            #region Adding
            Func<string, Symbol> addInput = label =>
            {
                var node = new Symbol();
                CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, 0, 0, true, true));
                node.InputSymbol = label;

                Assert.AreEqual(++currentInPortAmt, instance.InPorts.Count);
                Assert.AreEqual(label, instance.InPorts.Last().Name);

                return node;
            };

            Func<string, Output> addOutput = label =>
            {
                var node = new Output();

                CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, 0, 0, true, true));
                node.Symbol = label;

                Assert.AreEqual(++currentOutPortAmt, instance.OutPorts.Count);
                Assert.AreEqual(label, instance.OutPorts.Last().Name);

                return node;
            };
            #endregion

            #region Renaming
            Action<Symbol, int, string> renameInput = (input, idx, s) =>
            {
                input.InputSymbol = s;
                Assert.AreEqual(s, instance.InPorts[idx].Name);
            };

            Action<Output, int, string> renameOutput = (output, idx, s) =>
            {
                output.Symbol = s;
                Assert.AreEqual(s, instance.OutPorts[idx].Name);
            };
            #endregion

            #region Deleting
            Action<NodeModel> deleteNode = nodeModel =>
            {
                DynamoSelection.Instance.ClearSelection();
                CurrentDynamoModel.AddToSelection(nodeModel);
                var command = new DynamoModel.DeleteModelCommand(Guid.Empty);
                CurrentDynamoModel.ExecuteCommand(command);
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
            string openPath = Path.Combine(TestDirectory, @"core\multiout\multi-custom.dyn");
            OpenModel(openPath);

            BeginRun();

            var splitListVal = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>().CachedValue;

            var val = Dictionary.ByKeysValues(new[] { "item", "rest" }, new object[] { 0, new object[] { } });
            AssertValue(splitListVal, val);
        }

        [Test]
        public void TestPartialCustomMultiOutputNode()
        {
            RunModel(@"core\multiout\partial-multiout-customnode.dyn");
            AssertPreviewValue("0ce0d95c-a644-4268-8820-065d7edb2778", new[] { 2, 0.5, 0 });

            var guid = Guid.Parse("0ce0d95c-a644-4268-8820-065d7edb2778");
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == guid);
            Assert.IsTrue(node.State != ElementState.Warning);

        }

        [Test]
        public void PartialApplicationWithMultipleOutputs()
        {
            string openPath = Path.Combine(TestDirectory, @"core\multiout\partial-multi-custom.dyn");
            OpenModel(openPath);

            BeginRun();

            var firstWatch = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Watch>("d824e8dd-1009-449f-b5d6-1cd83bd180d6");

            Assert.AreEqual(new ArrayList { 0 }, firstWatch.CachedValue);

            var restWatch = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<Watch>("af7ada9a-4316-475b-8582-742acc40fc1b");

            Assert.IsTrue(restWatch.CachedValue is ICollection);
            Assert.IsTrue((restWatch.CachedValue as ICollection).Cast<object>().First() is ICollection);
            Assert.IsFalse(((restWatch.CachedValue as ICollection).Cast<object>().First() as ICollection)
                    .Cast<object>().Any());
        }

        [Test]
        public void CollapsedNodeWOrkspaceIsAddedToDynamoWithUnsavedChanges()
        {
            NodeModel node;
            if (!CurrentDynamoModel.NodeFactory.CreateNodeFromTypeName("CoreNodeModels.Input.DoubleInput", out node))
            {
                throw new Exception("Failed to create node!");
            }

            var selectionSet = new[] { node };

            DynamoModel.FunctionNamePromptRequestHandler del = (sender, args) =>
            {
                args.Category = "Testing";
                args.Description = "";
                args.Name = "__CollapseTest__";
                args.Success = true;
            };

            CurrentDynamoModel.RequestsFunctionNamePrompt += del;

            var arg = new FunctionNamePromptEventArgs();
            CurrentDynamoModel.OnRequestsFunctionNamePrompt(null, arg);
            Assert.IsTrue(arg.Success);

            CurrentDynamoModel.AddCustomNodeWorkspace(
                CurrentDynamoModel.CustomNodeManager.Collapse(
                    selectionSet, Enumerable.Empty<NoteModel>(),
                    CurrentDynamoModel.CurrentWorkspace, DynamoModel.IsTestMode, arg));

            Assert.IsNotNull(CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>());

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(2, CurrentDynamoModel.Workspaces.Count());

            var customWorkspace = CurrentDynamoModel.Workspaces.ElementAt(1);
            Assert.AreEqual("__CollapseTest__", customWorkspace.Name);
            Assert.IsTrue(customWorkspace.HasUnsavedChanges);
        }

        [Test]
        public void CollapsedNodeShouldHaveNewIdentfifer()
        {
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse-newname.dyn");
            OpenModel(openPath);

            // Convert a DSFunction node Point.ByCoordinates to custom node.
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSFunction>().First();

            var originalGuid = node.GUID;
            var originalIdentifierName = node.AstIdentifierBase;
            var originalIdentifier = node.AstIdentifierForPreview;

            var selectionSet = new[] { node };
            var customWorkspace = CurrentDynamoModel.CustomNodeManager.Collapse(
                selectionSet,
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            // Making sure we have a Function node after the conversion.
            Assert.IsNotNull(CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>());

            // As there is only one node is converted to custom node, get
            // the first one
            var collapsedNode = customWorkspace.Nodes.OfType<DSFunction>().First();

            // Node -> custom node just copy node from home workspace to 
            // custom workspace, so they are the same node
            Assert.AreSame(node, collapsedNode);

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
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\missing_custom_node.dyn");

            OpenModel(dynFilePath);
            BeginRun();

            AssertPreviewValue("1b8b309b-ee2e-44fe-ac98-2123b2711bea", 1);
            AssertPreviewValue("08db7d60-845c-439c-b7ca-c2a06664a948", 2);
        }

        [Test]
        public void TestCustomNodeInputType()
        {
            // Custom node's signature is add(x:int, y:int)
            // Test type conversion happens.
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\TestTypeConversion.dyn");

            OpenModel(dynFilePath);
            BeginRun();

            // add(1.49, 3.49) => 4
            AssertPreviewValue("fe515852-8e88-496b-8f17-005d97c7fa19", 4);
        }

        [Test]
        public void TestCustomNodeLacing()
        {
            // Test lacing works ofr custom node. 
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\TestLacing.dyn");
            OpenModel(dynFilePath);

            var instance = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Function>().First();
            instance.UpdateValue(new UpdateValueParams("ArgumentLacing", "CrossProduct"));
            BeginRun();

            // {1,2} + {3,4}
            AssertPreviewValue("fe515852-8e88-496b-8f17-005d97c7fa19", new object[] { new object[] { 4, 5 }, new object[] { 5, 6 } });

            instance.UpdateValue(new UpdateValueParams("ArgumentLacing", "Longest"));
            BeginRun();
            AssertPreviewValue("fe515852-8e88-496b-8f17-005d97c7fa19", new object[] { 4, 6 });
        }

        [Test]
        public void TestCustomNodeDefaultValue()
        {
            // Test custom node default value works
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\TestDefaultValue.dyn");

            OpenModel(dynFilePath);
            BeginRun();

            AssertPreviewValue("405d0c03-6b22-466e-a2b9-b9bf602e1762", 142);
        }

        [Test]
        public void TestCustomNodeDefaultValueJson()
        {
            // load xml dyf test file
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\CNDefault.dyf");
            OpenModel(dynFilePath);

            // get custom node ws as json
            var cws = CurrentDynamoModel.Workspaces.FirstOrDefault(ws => ws is CustomNodeWorkspaceModel);
            Assert.NotNull(cws);
            var customNodeWorkspace = (CustomNodeWorkspaceModel)cws;
            var json = customNodeWorkspace.ToJson(CurrentDynamoModel.EngineController);
            var jObject = JObject.Parse(json);

            // check first input defaults
            string concreteType = (string)jObject.SelectToken("Nodes[1].ConcreteType");
            string name = (string)jObject.SelectToken("Nodes[1].Parameter.Name");
            string typeName = (string)jObject.SelectToken("Nodes[1].Parameter.TypeName");
            int typeRank = (int)jObject.SelectToken("Nodes[1].Parameter.TypeRank");
            string defaultValue = (string)jObject.SelectToken("Nodes[1].Parameter.DefaultValue");

            string type = cws.Nodes.Where(node => node.GUID.ToString("N") == "4b3ef2466ef649ea95db607b1f083d0c").ToArray().First().GetType().ToString();
            Assert.IsTrue(concreteType.Contains(type));
            Assert.IsTrue(name == "center");
            Assert.IsTrue(typeName == "Autodesk.DesignScript.Geometry.Point");
            Assert.IsTrue(typeRank == 0);
            Assert.IsTrue(defaultValue == "Autodesk.DesignScript.Geometry.Point.ByCoordinates(10, 10, 10)");

            // check second input defaults
            type = cws.Nodes.Where(node => node.GUID.ToString("N") == "e2efe8b186cd477995f6fb4cf28038a5").ToArray().First().GetType().ToString();
            concreteType = (string)jObject.SelectToken("Nodes[2].ConcreteType");
            name = (string)jObject.SelectToken("Nodes[2].Parameter.Name");
            typeName = (string)jObject.SelectToken("Nodes[2].Parameter.TypeName");
            typeRank = (int)jObject.SelectToken("Nodes[2].Parameter.TypeRank");
            defaultValue = (string)jObject.SelectToken("Nodes[2].Parameter.DefaultValue");

            Assert.IsTrue(concreteType.Contains(type));
            Assert.IsTrue(name == "radius");
            Assert.IsTrue(typeName == "double");
            Assert.IsTrue(typeRank == 0);
            Assert.IsTrue(defaultValue == "5.5");
        }

        [Test]
        public void TestCustomNodeDefaultValueOpenedinJson()
        {
            // load json dyf test file
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\CNDefault_json.dyf");
            OpenModel(dynFilePath);

            // get custom node ws as json
            var cws = CurrentDynamoModel.Workspaces.FirstOrDefault(ws => ws is CustomNodeWorkspaceModel);
            Assert.NotNull(cws);
            var customNodeWorkspace = (CustomNodeWorkspaceModel)cws;
            var json = customNodeWorkspace.ToJson(CurrentDynamoModel.EngineController);
            var jObject = JObject.Parse(json);

            // check first input defaults
            string concreteType = (string)jObject.SelectToken("Nodes[1].ConcreteType");
            string name = (string)jObject.SelectToken("Nodes[1].Parameter.Name");
            string typeName = (string)jObject.SelectToken("Nodes[1].Parameter.TypeName");
            int typeRank = (int)jObject.SelectToken("Nodes[1].Parameter.TypeRank");
            string defaultValue = (string)jObject.SelectToken("Nodes[1].Parameter.DefaultValue");

            string type = cws.Nodes.Where(node => node.GUID.ToString("N") == "4b3ef2466ef649ea95db607b1f083d0c").ToArray().First().GetType().ToString();
            Assert.IsTrue(concreteType.Contains(type));
            Assert.IsTrue(name == "center");
            Assert.IsTrue(typeName == "Autodesk.DesignScript.Geometry.Point");
            Assert.IsTrue(typeRank == 0);
            Assert.IsTrue(defaultValue == "Autodesk.DesignScript.Geometry.Point.ByCoordinates(10, 10, 10)");

            // check second input defaults
            type = cws.Nodes.Where(node => node.GUID.ToString("N") == "e2efe8b186cd477995f6fb4cf28038a5").ToArray().First().GetType().ToString();
            concreteType = (string)jObject.SelectToken("Nodes[2].ConcreteType");
            name = (string)jObject.SelectToken("Nodes[2].Parameter.Name");
            typeName = (string)jObject.SelectToken("Nodes[2].Parameter.TypeName");
            typeRank = (int)jObject.SelectToken("Nodes[2].Parameter.TypeRank");
            defaultValue = (string)jObject.SelectToken("Nodes[2].Parameter.DefaultValue");

            Assert.IsTrue(concreteType.Contains(type));
            Assert.IsTrue(name == "radius");
            Assert.IsTrue(typeName == "double");
            Assert.IsTrue(typeRank == 0);
            Assert.IsTrue(defaultValue == "5.5");
        }

        [Test]
        public void TestCustomNodeInvalidType()
        {
            // Custom node has invalid type, which should be captured by Input node
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\invalidType.dyf");

            OpenModel(dynFilePath);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Symbol>().First();
            Assert.IsTrue(node.State == ElementState.Warning);
        }

        [Test]
        public void TestCustomNodeInvalidInput()
        {
            // Custom node has invalid input like "x = f(x)", but the evalution should continue
            // so that old custom node won't be broken
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\TestInvalidInput.dyn");
            OpenModel(dynFilePath);
            BeginRun();

            AssertPreviewValue("7134638a-26f4-4a13-affb-857ed519db5f", 84);
        }


        [Test]
        public void TestCustomNodeFromCollapsedNodeHasTypes()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");
            OpenModel(Path.Combine(examplePath, "simpleGeometry.dyn"));

            // Convert a DSFunction node Line.ByStartPointEndPoint to custom node.
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSFunction>().First();

            var selectionSet = new List<NodeModel> { node };
            CurrentDynamoModel.CustomNodeManager.Collapse(
                selectionSet,
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            // Get custom node instance
            var instance = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Function>();
            // All its input types are Point
            Assert.IsTrue(instance.Controller.Definition.Parameters.All(t => t.Name.Contains("Point")));
        }

        [Test]
        public void TestCustomNodeFromCollapsedNodeHasSpecificTypes()
        {
            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");
            OpenModel(Path.Combine(examplePath, "simpleGeometry.dyn"));

            // Convert a DSFunction node Line.ByStartPointEndPoint to custom node.
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSFunction>().First();

            var selectionSet = new List<NodeModel> { node };
            var ws = CurrentDynamoModel.CustomNodeManager.Collapse(
                selectionSet,
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            CurrentDynamoModel.AddCustomNodeWorkspace(ws);
            CurrentDynamoModel.OpenCustomNodeWorkspace(ws.CustomNodeId);
            var inputs = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Symbol>().ToList();
            Assert.IsNotNull(inputs);

            inputs.ForEach(x => Assert.NotNull(x));

            Assert.IsTrue(inputs.All(t => t.InputSymbol.EndsWith(":Point")));
        }

        [Test]
        public void TestCustomNodeFromCollapsedNodeHasSpecificTypes_WithConflict()
        {
            //load the FFI target lib so we have some namespace conflicts
            string libraryPath = "FFITarget.dll";
            if (!CurrentDynamoModel.EngineController.LibraryServices.IsLibraryLoaded(libraryPath))
            {
                CurrentDynamoModel.EngineController.LibraryServices.ImportLibrary(libraryPath);
            }

            var examplePath = Path.Combine(TestDirectory, @"core\CustomNodes\");
            OpenModel(Path.Combine(examplePath, "simpleGeometry.dyn"));

            // Convert a DSFunction node Line.ByStartPointEndPoint to custom node.
            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<DSFunction>().First();

            var selectionSet = new List<NodeModel> { node };
            var ws = CurrentDynamoModel.CustomNodeManager.Collapse(
                selectionSet,
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest__",
                    Success = true
                });

            CurrentDynamoModel.AddCustomNodeWorkspace(ws);
            CurrentDynamoModel.OpenCustomNodeWorkspace(ws.CustomNodeId);
            var inputs = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Symbol>().ToList();
            Assert.IsNotNull(inputs);

            inputs.ForEach(x => Assert.NotNull(x));

            Assert.IsTrue(inputs.All(t =>
            {
                return t.InputSymbol.EndsWith(":Autodesk.Point");
            }));
        }

        [Test]
        public void TestCustomNodeInSyncWithDefinition()
        {
            var basePath = Path.Combine(TestDirectory, @"core\CustomNodes\");

            OpenModel(Path.Combine(basePath, "testCustomNodeSync.dyn"));
            BeginRun();

            var customInstance = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(x => x is Function) as Function;
            Assert.AreEqual("int", customInstance.InPorts.First().ToolTip);

            OpenModel(Path.Combine(basePath, "inputWithType.dyf"));
            var customWorkspace = CurrentDynamoModel.Workspaces.FirstOrDefault(x => x is CustomNodeWorkspaceModel) as CustomNodeWorkspaceModel;
            var inputNode = customWorkspace.Nodes.FirstOrDefault(x => x is Symbol) as Symbol;

            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    customWorkspace.Guid,
                    inputNode.GUID,
                    "InputSymbol",
                    "x : bool"));

            customInstance.ResyncWithDefinition(customWorkspace.CustomNodeDefinition);

            Assert.AreEqual("x", customInstance.InPorts.First().Name);
            Assert.AreEqual("bool", customInstance.InPorts.First().ToolTip);
        }

        [Test]
        public void TestGroupsOnCustomNodes()
        {
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse.dyn");
            OpenModel(openPath);

            var nodesToCollapse = new[]
            {
                "1da395b9-2539-4705-a479-1f6e575df01d",
                "b8130bf5-dd14-4784-946d-9f4705df604e",
                "a54c7cfa-450a-4edc-b7a5-b3e15145a9e1"
            };

            foreach (
                var node in
                    nodesToCollapse.Select(CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace))
            {
                CurrentDynamoModel.AddToSelection(node);
            }

            //create the group around selected nodes
            Guid groupid = Guid.NewGuid();
            var annotation = CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", groupid);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Annotations.Count(), 1);
            Assert.AreEqual(CurrentDynamoModel.CurrentWorkspace.Annotations.First().Nodes.Count(), 3);

            CurrentDynamoModel.AddToSelection(annotation);

            var ws = CurrentDynamoModel.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTest2__",
                    Success = true
                });

            CurrentDynamoModel.AddCustomNodeWorkspace(ws);

            SelectTabByGuid(ws.CustomNodeId);

            Assert.AreEqual(6, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());

            //Check whether the group is copied to custom workspace
            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Annotations.Count());
        }

        [Test]
        public void TestCustomNodeInputType2()
        {
            string openPath = Path.Combine(TestDirectory, @"core\collapse\collapse-input-type.dyn");
            OpenModel(openPath);

            var nodesToCollapse = new[]
            {
                "fb066324-1ca0-400f-8dee-cbb0e1d27719",
            };

            foreach (
                var node in
                    nodesToCollapse.Select(CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace))
            {
                CurrentDynamoModel.AddToSelection(node);
            }

            var ws = CurrentDynamoModel.CustomNodeManager.Collapse(
                DynamoSelection.Instance.Selection.OfType<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                CurrentDynamoModel.CurrentWorkspace,
                true,
                new FunctionNamePromptEventArgs
                {
                    Category = "Testing",
                    Description = "",
                    Name = "__CollapseTestForInputType1__",
                    Success = true
                });

            CurrentDynamoModel.AddCustomNodeWorkspace(ws);
            CurrentDynamoModel.OpenCustomNodeWorkspace(ws.CustomNodeId);
            var inputs = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Symbol>();
            Assert.IsNotNull(inputs);

            var curveParam = inputs.FirstOrDefault();
            Assert.IsNotNull(curveParam);

            Assert.AreEqual("Curve", curveParam.Parameter.DisplayTypeName);
        }

        [Test]
        public void CanAddNodesWithTheSameNameAndEmptyPath2Times()
        {
            const string nodeName = "NodeName";
            const string catName1 = "CatName1";
            const string catName2 = "CatName2";

            CurrentDynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName1, "");
            CurrentDynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName2, "");

            Assert.AreEqual(2, CurrentDynamoModel.SearchModel.SearchEntries.Where(entry => entry.Name == nodeName).Count());
        }

        [Test]
        public void TestInputOutputNodeDocumentation()
        {
            var filePath = Path.Combine(TestDirectory, @"core\CustomNodes\test_input_output_comment.dyn");
            OpenModel(filePath);

            var customInstance = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(x => x is Function) as Function;
            Assert.AreEqual(5, customInstance.InPorts.Count());
            Assert.AreEqual(3, customInstance.OutPorts.Count());

            Assert.AreEqual("", customInstance.InPorts[0].Name);
            Assert.AreEqual(@"var[]..[]", customInstance.InPorts[0].ToolTip);

            Assert.AreEqual("x1", customInstance.InPorts[1].Name);
            Assert.AreEqual("x1\n\nvar[]..[]", customInstance.InPorts[1].ToolTip);

            Assert.AreEqual("x2", customInstance.InPorts[2].Name);
            Assert.AreEqual("x2:var\n\nvar", customInstance.InPorts[2].ToolTip);

            Assert.AreEqual("x3", customInstance.InPorts[3].Name);
            Assert.AreEqual("x3:var\n\nvar[]\nDefault value : [1]", customInstance.InPorts[3].ToolTip);

            Assert.AreEqual("x4", customInstance.InPorts[4].Name);
            Assert.AreEqual("comment1\ncomment2\ncomment3\n\nvar[]..[]", customInstance.InPorts[4].ToolTip);

            Assert.AreEqual("", customInstance.OutPorts[0].Name);
            Assert.AreEqual("return value", customInstance.OutPorts[0].ToolTip);

            Assert.AreEqual("y1", customInstance.OutPorts[1].Name);
            Assert.AreEqual("y1", customInstance.OutPorts[1].ToolTip);

            Assert.AreEqual("y2", customInstance.OutPorts[2].Name);
            Assert.AreEqual("comment1\ncomment2\ncomment3", customInstance.OutPorts[2].ToolTip);
        }

        [Test]
        public void TestOuputNodes()
        {
            var filePath = Path.Combine(TestDirectory, @"core\CustomNodes\test_outputs.dyn");
            OpenModel(filePath);

            var customInstance = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(x => x is Function) as Function;
            Assert.AreEqual(4, customInstance.OutPorts.Count());

            Assert.AreEqual("x", customInstance.OutPorts[0].Name);
            Assert.AreEqual("y", customInstance.OutPorts[1].Name);
            Assert.AreEqual("def foo() {}", customInstance.OutPorts[2].Name);
            Assert.AreEqual("class bar {}", customInstance.OutPorts[3].Name);
        }

        [Test]
        public void Regress9548_OutputNameIsClassName()
        {
            var filePath = Path.Combine(TestDirectory, @"core\CustomNodes\PointAsOutput.dyn");
            OpenModel(filePath);

            var customInstance = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(x => x is Function) as Function;
            Assert.AreEqual("comment", customInstance.OutPorts[0].ToolTip);
            Assert.AreEqual("Point", customInstance.OutPorts[0].Name);

            var previewValue = GetPreviewValue(customInstance.GUID.ToString());
            Assert.AreEqual(21, previewValue);
        }

        [Test]
        public void InputNodesShouldMaintainCommentsWhenOpening()
        {
            var filePath = Path.Combine(TestDirectory, @"core\CustomNodes\inputNodesWithComments.dyf");
            OpenModel(filePath);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(x => x is Symbol) as Symbol;
            Assert.IsNotNull(node);
            Assert.AreEqual("/* commentsssss*/" + Environment.NewLine + "inputName: string = \"a def string\"", node.Parameter.ToCommentNameString());
            Assert.AreEqual(" commentsssss", node.Parameter.Summary);
        }

        [Test]
        public void InputNodesShouldMaintainCommentsWhenSaving()
        {
            var filePath = Path.Combine(TestDirectory, @"core\CustomNodes\inputNodesWithComments.dyf");
            OpenModel(filePath);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(x => x is Symbol) as Symbol;
            Assert.IsNotNull(node);
            Assert.AreEqual("/* commentsssss*/" + Environment.NewLine + "inputName: string = \"a def string\"", node.Parameter.ToCommentNameString());
            Assert.AreEqual(" commentsssss", node.Parameter.Summary);

            //modify the comment:
            node.InputSymbol = "/* a new and better comment*/" + Environment.NewLine + "inputName: string = \"a def string\"";
            var tempPath = GetNewFileNameOnTempPath(".dyf");
            CurrentDynamoModel.CurrentWorkspace.Save(tempPath, false, this.CurrentDynamoModel.EngineController);

            OpenModel(tempPath);
            node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(x => x is Symbol) as Symbol;
            Assert.AreEqual("/* a new and better comment*/" + Environment.NewLine + "inputName: string = \"a def string\"", node.InputSymbol);
            Assert.AreEqual("/* a new and better comment*/" + Environment.NewLine + "inputName: string = \"a def string\"", node.Parameter.ToCommentNameString());
            Assert.AreEqual(" a new and better comment", node.Parameter.Summary);
        }

        [Test]
        public void InputNodesShouldMaintainCommentsWhenSavingBrandNewCustomNode()
        {
            const string nodeName = "someCustomNode";
            const string catName1 = "CatName1";

            var cnworkspace = CurrentDynamoModel.CustomNodeManager.CreateCustomNode(nodeName, catName1, "a node with an input with comments", null);
            var inputNode = new Symbol();
            inputNode.InputSymbol = "/* a new and better comment*/" + Environment.NewLine + "inputName: string = \"a def string\"";
            cnworkspace.AddAndRegisterNode(inputNode);

            var tempPath = GetNewFileNameOnTempPath(".dyf");
            cnworkspace.Save(tempPath, false, this.CurrentDynamoModel.EngineController);

            //lets load this workpace's json and assert that the comment is saved.
            var rawText = File.ReadAllText(tempPath);

            var jobject = JObject.Parse(rawText);
            Assert.AreEqual(" a new and better comment", jobject["Nodes"][0]["Parameter"]["Description"].Value<string>());
        }

        [Test]
        public void Regress3786_InputNodesDeserilization()
        {
            var filePath = Path.Combine(TestDirectory, @"core\CustomNodes\QNTM3786_InputNodes.dyf");
            OpenModel(filePath);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(x => x is Symbol) as Symbol;
            Assert.IsNotNull(node);
            Assert.AreEqual("var1: Autodesk.DesignScript.Geometry.Point", node.Parameter.ToCommentNameString());
            Assert.AreEqual(string.Empty, node.Parameter.Summary);
        }

        [Test]
        public void Regress3872_InputNodesDeserilization()
        {
            var filePath = Path.Combine(TestDirectory, @"core\CustomNodes\QNTM3872_InputNodesUnknownTypes.dyf");
            OpenModel(filePath);

            var customNodeWs = CurrentDynamoModel.CurrentWorkspace as CustomNodeWorkspaceModel;
            Assert.IsNotNull(customNodeWs);
            Assert.AreEqual("Clockwork.Core.DateTime.Actions", customNodeWs.CustomNodeInfo.Category);

            var node1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("ccf64b20-2625-4c98-a556-da8de12e81a4") as Symbol;
            Assert.IsNotNull(node1);
            Assert.AreEqual("var1: System.DateTime", node1.Parameter.ToCommentNameString());

            var node2 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("6653114d-f6c0-4a90-af9a-848c142f0b9b") as Symbol;
            Assert.IsNotNull(node2);
            Assert.AreEqual("var2: xxx.yyy", node2.Parameter.ToCommentNameString());
        }

        [Test]
        public void Regress3872_XMLInputNodesDeserilization()
        {
            var filePath = Path.Combine(TestDirectory, @"core\custom_node_serialization\GraphFunction.dyf");
            OpenModel(filePath);
            var customNodeWs = CurrentDynamoModel.CurrentWorkspace as CustomNodeWorkspaceModel;
            Assert.IsNotNull(customNodeWs);
            var node1 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("e058111a-0c58-4dbf-9293-6ab711f530bf") as Symbol;
            Assert.IsNotNull(node1);
            Assert.AreEqual("x_start: var[]..[]", node1.Parameter.ToCommentNameString());
            var node2 = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("2601b801-c8af-413b-9c58-f8b100d62ed8") as Symbol;
            Assert.IsNotNull(node2);
            Assert.AreEqual("x_step: var[]..[]", node2.Parameter.ToCommentNameString());
        }

        [Test]
        public void CreatingAndEditingCustomNodeShouldNotDuplicateNotes()
        {

            const string name = "Custom Node Edit Test";
            const string description = "Description";
            const string category = "Custom Node Category";

            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.CreateCustomNodeCommand(
                    Guid.NewGuid(),
                    name,
                    category,
                    description,
                    true));

            Assert.IsInstanceOf<CustomNodeWorkspaceModel>(CurrentDynamoModel.CurrentWorkspace);
            var customNodeWs = CurrentDynamoModel.CurrentWorkspace as CustomNodeWorkspaceModel;
            var customNodeDef = customNodeWs.CustomNodeDefinition;
            Assert.AreEqual(name, customNodeDef.DisplayName);
            Assert.AreEqual(description, customNodeWs.Description);
            Assert.AreEqual(category, customNodeWs.Category);

            //we're now in the custom node, add a note and annotation
            //add a node to the currentWorkspace
            var addNode = new DSFunction(CurrentDynamoModel.LibraryServices.GetFunctionDescriptor("+"));
            this.CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(addNode);
            //put the node in a group
            DynamoSelection.Instance.Selection.Add(addNode);
            //create the group around selected node
            Guid groupid = Guid.NewGuid();
            var annotation = this.CurrentDynamoModel.CurrentWorkspace.AddAnnotation("This is a test group", groupid);

            Assert.AreEqual(this.CurrentDynamoModel.CurrentWorkspace.Annotations.Count(), 1);

            //add a note the current workspace
            var newNote = new NoteModel(100, 100, "someText", Guid.NewGuid());
            this.CurrentDynamoModel.CurrentWorkspace.AddNote(newNote, false);

            //now switch home

            var home = CurrentDynamoModel.Workspaces.OfType<HomeWorkspaceModel>().FirstOrDefault();
            Assert.NotNull(home);
            SelectTabByGuid(home.Guid);


            CurrentDynamoModel.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(
                    CurrentDynamoModel.CustomNodeManager.CreateCustomNodeInstance(customNodeDef.FunctionId),
                    0,
                    0,
                    true,
                    true));

            Assert.AreEqual(1, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.IsInstanceOf<Function>(CurrentDynamoModel.CurrentWorkspace.Nodes.First());
            Assert.AreEqual(
                customNodeDef.FunctionId,
                ((Function)CurrentDynamoModel.CurrentWorkspace.Nodes.First()).Definition.FunctionId);

            //close tab
            this.CurrentDynamoModel.RemoveWorkspace(customNodeWs);

            //edit the custom node
            var found = this.CurrentDynamoModel.OpenCustomNodeWorkspace(customNodeWs.CustomNodeId);
            Assert.IsTrue(found);
            SelectTabByGuid(customNodeWs.Guid);

            Assert.IsTrue(this.CurrentDynamoModel.CurrentWorkspace is CustomNodeWorkspaceModel);
            Assert.AreEqual(1, this.CurrentDynamoModel.CurrentWorkspace.Notes.Count());
            Assert.AreEqual(1, this.CurrentDynamoModel.CurrentWorkspace.Annotations.Count());

        }

        [Test]
        public void InputNameWithSpacesShouldCauseErrorStateAndReadOnly()
        {
            // Custom node has input symbol with a name that contains spaces
            var dyfFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\invalidInputName.dyf");

            OpenModel(dyfFilePath);

            var inputSymbolNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Symbol>().First();

            // The input symbol is invalid, so the input symbol node should show an error
            Assert.IsTrue(inputSymbolNode.State == ElementState.Error);
            // The workspace should know that it contains invalid input symbols
            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.containsInvalidInputSymbols());
            // The workspace should be readonly because it contains invalid input symbols
            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.IsReadOnly);
        }

        [Test]
        public void InputNameWithSpacesShouldCauseWarningInGraph()
        {
            // Dyn file contains a custom node with an invalid input symbol
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\invalidInputName.dyn");

            OpenModel(dynFilePath);
            var functionNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Function>().First();

            // The custom node should display a warning that it contains invalid input symbols
            Assert.IsTrue(functionNode.State == ElementState.PersistentWarning);
            // Despite the invalid input symbol, the custom node should still execute and return the correct value
            AssertPreviewValue("f91658aeafe84cbaa39d370dec283b53", 8.0);
        }

        [Test]
        public void LooseCustomNodeShouldNotHavePackageInfoOrPackageMember()
        {
            //load any loose custom node
            var dynFilePath = Path.Combine(TestDirectory, @"core\CustomNodes\invalidInputName.dyn");

            OpenModel(dynFilePath);
        var functionNode = CurrentDynamoModel.CurrentWorkspace.Nodes.OfType<Function>().First();

            var nodeInfo =CurrentDynamoModel.CustomNodeManager.NodeInfos.Where(x => x.Value.Name == "invalidInputName").FirstOrDefault();
            Assert.IsNotNull(nodeInfo.Value);
            Assert.IsFalse(nodeInfo.Value.IsPackageMember);
            Assert.IsNull(nodeInfo.Value.PackageInfo);
        }
    }
}