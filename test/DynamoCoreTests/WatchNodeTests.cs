﻿using System.Collections.ObjectModel;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using NUnit.Framework;
using System.IO;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using ProtoCore.Mirror;
using Dynamo.Models;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class WatchNodeTests : DynamoViewModelUnitTest
    {
        /// <summary>
        /// Validates the watch content of a WatchViewModel branch with the 
        /// input content.
        /// </summary>
        /// <param name="branch">Collection of WatchViewModel</param>
        /// <param name="content">Array of objects as watch content</param>
        public void AssertWatchTreeBranchContent(ICollection<WatchViewModel> branch, object[] content)
        {
            Assert.AreEqual(content.Length, branch.Count);
            int count = 0;
            foreach (var item in branch)
            {
                string nodeLabel = string.Format("{0}", content[count]);

                // DesignScript bool string reps are lower case, C# are upper case
                if (content[count] is bool) nodeLabel = nodeLabel.ToLower();

                Assert.AreEqual(nodeLabel, item.NodeLabel);
                ++count;
            }
        }

        /// <summary>
        /// Validates the watch content with the source nodes output.
        /// </summary>
        /// <param name="watch">WatchViewModel of the watch node</param>
        /// <param name="sourceNode">NodeModel for source to watch node</param>
        public void AssertWatchContent(WatchViewModel watch, NodeModel sourceNode)
        {
            string var = sourceNode.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);
            AssertWatchContent(watch, mirror.GetData());
        }

        /// <summary>
        /// Validates the watch content with given mirror data.
        /// </summary>
        /// <param name="watch">WatchViewModel of the watch node</param>
        /// <param name="mirrorData">MirrorData to be shown in watch</param>
        private void AssertWatchContent(WatchViewModel watch, MirrorData mirrorData)
        {
            Assert.IsNotNull(mirrorData);
            if (mirrorData.IsCollection)
                AssertWatchTreeBranchContent(watch.Children, mirrorData.GetElements());
            else if (mirrorData.IsNull)
                Assert.AreEqual("null", watch.NodeLabel);
            else
            {
                string nodeLabel = string.Format("{0}", mirrorData.Data);
                Assert.AreEqual(nodeLabel, watch.NodeLabel);
            }
        }

        /// <summary>
        /// Validates the watch content of a WatchViewModel branch with the list
        /// of mirror data.
        /// </summary>
        /// <param name="branch">Collection of WatchViewModel</param>
        /// <param name="data">List of MirrorData for validation</param>
        private void AssertWatchTreeBranchContent(ICollection<WatchViewModel> branch, IList<MirrorData> data)
        {
            Assert.AreEqual(branch.Count, data.Count);
            int count = 0;
            foreach (var item in branch)
            {
                AssertWatchContent(item, data[count]);
                ++count;
            }
        }

        /// <summary>
        /// Construct a WatchViewModel from a Watch node
        /// </summary>
        private WatchViewModel GetWatchViewModel(Watch watch)
        {
            var inputVar = watch.IsPartiallyApplied
                ? watch.AstIdentifierForPreview.Name
                : watch.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview.Name;

            var core = ViewModel.Model.EngineController.LiveRunnerCore;
            var watchHandler = ViewModel.WatchHandler;

            return watchHandler.GenerateWatchViewModelForData(
                watch.CachedValue,
                core,
                inputVar,
                false );
        }
        
        [Test]
        public void WatchLiterals()
        {
            var model = ViewModel.Model;

            var openPath = Path.Combine(GetTestDirectory(), @"core\watch\WatchLiterals.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());

            ViewModel.Model.PreferenceSettings.NumberFormat = "f0";

            // get count node
            var watchNumber = model.CurrentWorkspace.NodeFromWorkspace("eed0b6aa-0d82-44c5-aab6-2bf131044940") as Watch;
            var watchBoolean = model.CurrentWorkspace.NodeFromWorkspace("8c5a87db-2d6a-4d3c-8c01-5ff326aef321") as Watch;
            var watchPoint = model.CurrentWorkspace.NodeFromWorkspace("f1581148-9318-40fa-9402-61557255162a") as Watch;

            var node = GetWatchViewModel(watchNumber);
            Assert.AreEqual("5", node.NodeLabel);

            node = GetWatchViewModel(watchBoolean);
            Assert.AreEqual("false", node.NodeLabel);

            node = GetWatchViewModel(watchPoint);
            var pointNode = model.CurrentWorkspace.NodeFromWorkspace("64f10a92-3297-448b-be7a-03dbe1e8a90a");
            
            //Validate using the point node connected to watch node.
            AssertWatchContent(node, pointNode);
        }

        [Test]
        public void Watch1DCollections()
        {
            var model = ViewModel.Model;

            var openPath = Path.Combine(GetTestDirectory(), @"core\watch\Watch1DCollections.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());

            ViewModel.Model.PreferenceSettings.NumberFormat = "f0";

            // get count node
            var watchNumbers = model.CurrentWorkspace.NodeFromWorkspace("f79b65d9-8cda-449c-a8fa-8a44166eec12") as Watch;
            var watchBooleans = model.CurrentWorkspace.NodeFromWorkspace("4ea56d78-68a9-400f-88b8-c6875365fa54") as Watch;
            var watchVectors = model.CurrentWorkspace.NodeFromWorkspace("73b35c1f-2dfb-4ce0-8609-c0bac9f3033c") as Watch;

            var node = GetWatchViewModel(watchNumbers);
            Assert.AreEqual("List", node.NodeLabel);
            Assert.AreEqual(10, node.Children.Count);
            AssertWatchTreeBranchContent(node.Children, new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            node = GetWatchViewModel(watchBooleans);
            Assert.AreEqual("List", node.NodeLabel);
            Assert.AreEqual(6, node.Children.Count);
            AssertWatchTreeBranchContent(node.Children, new object[] { true, false, true, false, true, false });

            node = GetWatchViewModel(watchVectors);
            Assert.AreEqual("List", node.NodeLabel);
            Assert.AreEqual(10, node.Children.Count);
            var vectorNode = model.CurrentWorkspace.NodeFromWorkspace("9aedc16c-da20-4584-a53f-7dd4f01dc5ee");

            //Validate using vecotr node connected to watch node.
            AssertWatchContent(node, vectorNode);
        }

        [Test]
        public void WatchFunctionObject()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\watch\watchfunctionobject.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            ViewModel.HomeSpace.Run();

            var watchNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            var watchVM = ViewModel.WatchHandler.GenerateWatchViewModelForData(
                watchNode.CachedValue,
                ViewModel.Model.EngineController.LiveRunnerCore,
                watchNode.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview.Name);

            Assert.AreEqual("_SingleFunctionObject", watchVM.NodeLabel);
        }

        [Test]
        public void WatchFunctionPointer()
        {
            string openPath = Path.Combine(GetTestDirectory(), @"core\watch\watchFunctionPointer.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            ViewModel.HomeSpace.Run();

            var watchNodes = ViewModel.Model.CurrentWorkspace.Nodes.OfType<Watch>();
            foreach (var watchNode in watchNodes)
            {
                var watchVM = ViewModel.WatchHandler.GenerateWatchViewModelForData(
                    watchNode.CachedValue,
                    ViewModel.Model.EngineController.LiveRunnerCore,
                    watchNode.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview.Name);
                Assert.IsTrue(watchVM.NodeLabel.StartsWith("function"));
            }
        }
        [Test]
        public void WatchFunctionObject_collection_5033()
        {
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-5033
            // Watch value for a partially-applied function should say "function" and not "null"
            
            string openPath = Path.Combine(GetTestDirectory(), @"core\watch\watchfunctionobject_2.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            ViewModel.HomeSpace.Run();

            var watchNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();
            var watchVM = ViewModel.WatchHandler.GenerateWatchViewModelForData(
                watchNode.CachedValue,
               ViewModel.Model.EngineController.LiveRunnerCore,
                watchNode.InPorts[0].Connectors[0].Start.Owner.AstIdentifierForPreview.Name);

            Assert.AreEqual("_SingleFunctionObject", watchVM.NodeLabel);
        }
    }
}
