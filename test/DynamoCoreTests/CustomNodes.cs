using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class CustomNodes : DynamoUnitTest
    {
        [Test]
        public void CanCollapseNodesAndGetSameResult()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");

            string openPath = Path.Combine(examplePath, "collapse.dyn");
            model.Open(openPath);

            var watchNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            var numNodesPreCollapse = model.CurrentWorkspace.Nodes.Count;

            Controller.RunExpression();

            var valuePreCollapse = (watchNode.OldValue as FScheme.Value.Number).Item;

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

            NodeCollapser.Collapse(
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

            Controller.RunExpression();

            var valuePostCollapse = (watchNode.OldValue as FScheme.Value.Number).Item;

            Assert.AreEqual(valuePreCollapse, valuePostCollapse);
        }

        [Test]
        public void CanCollapseNodesWithDefaultValues()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");

            string openPath = Path.Combine(examplePath, "collapse-defaults.dyn");
            model.Open(openPath);

            //Confirm that everything is working OK.
            Controller.RunExpression();

            var minNode = model.CurrentWorkspace.FirstNodeFromWorkspace<ListMin>();
            var numNode = model.CurrentWorkspace.FirstNodeFromWorkspace<DoubleInput>();

            Assert.IsInstanceOf<FScheme.Value.Number>(minNode.OldValue);
            Assert.AreEqual(10, (minNode.OldValue as FScheme.Value.Number).Item);

            model.AddToSelection(minNode);
            model.AddToSelection(numNode);

            NodeCollapser.Collapse(
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

            Controller.RunExpression();

            var collapsedNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>();

            Assert.IsInstanceOf<FScheme.Value.Number>(collapsedNode.OldValue);
            Assert.AreEqual(10, (collapsedNode.OldValue as FScheme.Value.Number).Item);
        }

        [Test]
        public void CanCollapseWith1NodeHoleInSelection()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");

            string openPath = Path.Combine(examplePath, "collapse-function.dyn");
            model.Open(openPath);

            //Confirm that everything is working OK.
            Controller.RunExpression();

            var mulNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Multiplication>();

            Assert.IsInstanceOf<FScheme.Value.Number>(mulNode.OldValue);
            Assert.AreEqual(0, (mulNode.OldValue as FScheme.Value.Number).Item);

            foreach (var node in model.CurrentWorkspace.Nodes.Where(x => !(x is Addition)))
            {
                model.AddToSelection(node);
            }

            NodeCollapser.Collapse(
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

            Controller.RunExpression();

            var collapsedNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>();

            Assert.IsInstanceOf<FScheme.Value.Number>(collapsedNode.OldValue);
            Assert.AreEqual(0, (collapsedNode.OldValue as FScheme.Value.Number).Item);
        }

        [Test]
        public void CanCollapseAndUndoRedo()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");
            model.Open(Path.Combine(examplePath, "collapse-number-chain.dyn"));

            // Ensure all the nodes we are looking for are actually there.
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);
            var existenceMap = new Dictionary<string, bool>();
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

            NodeCollapser.Collapse(
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
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\collapse\");

            string openPath = Path.Combine(examplePath, "collapse.dyn");
            model.Open(openPath);

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

            NodeCollapser.Collapse(
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

            Controller.DynamoViewModel.GoToWorkspace(
                Controller.CustomNodeManager.GetGuidFromName("__CollapseTest2__"));

            var numNodes = model.CurrentWorkspace.Nodes.Count;

            List<ModelBase> modelsToDelete = new List<ModelBase>();
            modelsToDelete.Add(model.CurrentWorkspace.FirstNodeFromWorkspace<Addition>());
            model.DeleteModelInternal(modelsToDelete);

            Assert.AreEqual(numNodes - 1, model.CurrentWorkspace.Nodes.Count);
        }

        [Test]
        public void CombineWithCustomNodes()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\combine\");

            string openPath = Path.Combine(examplePath, "combine-with-three.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            while (Controller.Running)
                Thread.Sleep(100);

            // [[0,3,6], [2,5,4], [0,5,8]]

            // check the output values are correctly computed
            var watchNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Assert.IsNotNull(watchNode);

            var expected = new List<List<double>>
            {
                new List<double> { 0, 3, 6 },
                new List<double> { 1, 4, 7 },
                new List<double> { 2, 5, 8 },
            };

            // 50 elements between -1 and 1
            Assert.IsAssignableFrom(typeof(FScheme.Value.List), watchNode.OldValue);
            var outerList = (watchNode.OldValue as FScheme.Value.List).Item;

            Assert.AreEqual(3, outerList.Count());
            int i = 0;
            foreach (var innerList in outerList)
            {
                var fList = innerList.GetListFromFSchemeValue();
                int j = 0;
                foreach (var ele in fList)
                {
                    var num = (ele as FScheme.Value.Number).Item;
                    Assert.AreEqual(num, expected[i][j], 0.001);
                    j++;
                }
                i++;
            }

        }

        [Test, Ignore]
        public void ReduceAndRecursion()
        {
            var model = Controller.DynamoModel;

            var examplePath = Path.Combine(GetTestDirectory(), @"core\reduce_and_recursion\");

            string openPath = Path.Combine(examplePath, "reduce-example.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watch =
                model.CurrentWorkspace.NodeFromWorkspace<Watch>(
                    "157557d2-2452-413a-9944-1df3df793cee");
            var doubleWatchVal = watch.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(doubleWatchVal, 15.0, 0.001);

            var watch2 =
                model.CurrentWorkspace.NodeFromWorkspace<Watch>(
                    "068dd555-a5d5-4f11-af05-e4fa0cc015c9");
            var doubleWatchVal1 = watch2.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(doubleWatchVal1, 15.0, 0.001);

            var watch3 =
                model.CurrentWorkspace.NodeFromWorkspace<Watch>(
                    "1aca382d-ca81-4955-a6c1-0f549df19fd7");
            var doubleWatchVal2 = watch3.GetValue(0).GetDoubleFromFSchemeValue();
            Assert.AreEqual(doubleWatchVal2, 15.0, 0.001);

        }

        [Test]
        public void FilterWithCustomNode()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\filter\");

            Assert.IsTrue(
                Controller.CustomNodeManager.AddFileToPath(Path.Combine(examplePath, "IsOdd.dyf"))
                != null);

            string openPath = Path.Combine(examplePath, "filter-example.dyn");
            model.Open(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);

            // run the expression
            dynSettings.Controller.RunExpression(null);

            // wait for the expression to complete
            Thread.Sleep(500);

            // check the output values are correctly computed
            var watchNode = model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            Assert.IsNotNull(watchNode);

            // odd numbers between 0 and 5
            Assert.IsAssignableFrom(typeof(FScheme.Value.List), watchNode.OldValue);
            var list = (watchNode.OldValue as FScheme.Value.List).Item;

            Assert.AreEqual(3, list.Count());
            var count = 1;
            list.ToList().ForEach(
                x =>
                {
                    Assert.IsAssignableFrom(typeof(FScheme.Value.Number), x);
                    var val = (x as FScheme.Value.Number).Item;
                    Assert.AreEqual(count, val, 0.0001);
                    count += 2;
                });
        }

        /// <summary>
        /// Run an infinite recursive loop for 10 seconds to confirm that it doesn't stack overflow
        /// </summary>
        [Test]
        public void TailCallOptimization()
        {
            Assert.Inconclusive();
        }

        /// <summary>
        /// Run a custom node, change parameter/output/function names, run again to verify consistency
        /// </summary>
        [Test]
        public void RenameConsistency()
        {
            Assert.Inconclusive();
        }

        /// <summary>
        /// Modification of a recursive custom node results in UI updating for all instances.
        /// </summary>
        [Test]
        public void ModificationUITesting()
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
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\multiout");

            string openPath = Path.Combine(examplePath, "multi-custom.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression();

            var splitListVal = model.CurrentWorkspace.FirstNodeFromWorkspace<Function>().OldValue;

            Assert.IsInstanceOf<FScheme.Value.List>(splitListVal);

            var outs = (splitListVal as FScheme.Value.List).Item;

            Assert.AreEqual(2, outs.Length);

            var out1 = outs[0];
            Assert.IsInstanceOf<FScheme.Value.Number>(out1);
            Assert.AreEqual(0, (out1 as FScheme.Value.Number).Item);

            var out2 = outs[1];
            Assert.IsInstanceOf<FScheme.Value.List>(out2);
            Assert.IsTrue((out2 as FScheme.Value.List).Item.IsEmpty);
        }

        [Test]
        public void PartialApplicationWithMultipleOutputs()
        {
            var model = Controller.DynamoModel;
            var examplePath = Path.Combine(GetTestDirectory(), @"core\multiout");

            string openPath = Path.Combine(examplePath, "partial-multi-custom.dyn");
            model.Open(openPath);

            dynSettings.Controller.RunExpression();

            var firstWatch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("d824e8dd-1009-449f-b5d6-1cd83bd180d6");

            Assert.IsInstanceOf<FScheme.Value.List>(firstWatch.OldValue);
            Assert.IsInstanceOf<FScheme.Value.Number>((firstWatch.OldValue as FScheme.Value.List).Item[0]);
            Assert.AreEqual(0, ((firstWatch.OldValue as FScheme.Value.List).Item[0] as FScheme.Value.Number).Item);

            var restWatch = model.CurrentWorkspace.NodeFromWorkspace<Watch>("af7ada9a-4316-475b-8582-742acc40fc1b");

            Assert.IsInstanceOf<FScheme.Value.List>(restWatch.OldValue);
            Assert.IsInstanceOf<FScheme.Value.List>((restWatch.OldValue as FScheme.Value.List).Item[0]);
            Assert.IsTrue(((restWatch.OldValue as FScheme.Value.List).Item[0] as FScheme.Value.List).Item.IsEmpty);
        }

        //[Test]
        //public void CanGetDependenciesFromFunctionDefinition()
        //{



        //    if (CustomNodeDefinition.WorkspaceModel.Nodes.Any(x => x is RevitTransactionNode)
        //        || CustomNodeDefinition.Dependencies.Any(d => d.WorkspaceModel.Nodes.Any(x => x is RevitTransactionNode)))
        //    {
        //        return new FunctionWithRevit(inputs, outputs, CustomNodeDefinition);
        //    }
        //    return base.CreateFunction(inputs, outputs, CustomNodeDefinition);
        //}


    }
}