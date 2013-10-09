﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using NUnit.Framework;

namespace Dynamo.Tests
{
    internal class CustomNodeTests : DynamoUnitTest
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

            Controller.RunExpression(null);

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
                 DynamoSelection.Instance.Selection.Where(x => x is NodeModel)
                    .Select(x => (x as NodeModel)),
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

            Controller.RunExpression(null);

            var valuePostCollapse = (watchNode.OldValue as FScheme.Value.Number).Item;

            Assert.AreEqual(valuePreCollapse, valuePostCollapse);
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
            
            model.Delete(model.CurrentWorkspace.FirstNodeFromWorkspace<Addition>());

            Assert.AreEqual(numNodes-1, model.CurrentWorkspace.Nodes.Count);
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

        [Test]
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
        /// Run an infinite loop for 10 seconds to confirm that it doesn't stack overflow
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
        /// Confirm that a custom node with multiple outputs evaluates successfully.
        /// </summary>
        [Test]
        public void MultipleOutputs()
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
        /// Can use SaveAs to get a new custom node
        /// </summary>
        [Test] public void SaveAsDoesNotDuplicateCustomNodeInSearch()
        {
            Assert.Inconclusive();
        }

        /// <summary>
        /// Can use SaveAs to get a new custom node
        /// </summary>
        [Test]
        public void SaveAsResultsInNewFunctionIdButMaintainsOriginal()
        {
            Assert.Inconclusive();
        }

        /// <summary>
        /// When placing an instance of a Custom Node inside of itself, confirm it has the
        /// infinity symbol output only if it is an entry point.
        /// </summary>
        [Test]
        public void RecursiveInstanceOfCustomNode_InfinityOutput()
        {
            // Make instance the top most, confirm it has infinity output

            // Confirm that if there is any other top most node, infinity output is gone
            Assert.Inconclusive();
        }
    }
}