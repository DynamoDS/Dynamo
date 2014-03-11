using System.Collections.ObjectModel;
using NUnit.Framework;
using System.IO;
using Dynamo.Nodes;
using Dynamo.ViewModels;
using ProtoCore.Mirror;
using Dynamo.Models;
using System.Collections.Generic;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class WatchNodeTests : DynamoUnitTest
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
            Assert.DoesNotThrow(() => mirror = Controller.EngineController.GetMirror(var));
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
        
        
        [Test]
        public void WatchLiterals()
        {
            var model = Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\watch\WatchLiterals.dyn");
            model.Open(openPath);

            Assert.DoesNotThrow(() => Controller.RunExpression(null));

            // get count node
            Watch watchNumber = model.CurrentWorkspace.NodeFromWorkspace("eed0b6aa-0d82-44c5-aab6-2bf131044940") as Watch;
            Watch watchBoolean = model.CurrentWorkspace.NodeFromWorkspace("8c5a87db-2d6a-4d3c-8c01-5ff326aef321") as Watch;
            Watch watchPoint = model.CurrentWorkspace.NodeFromWorkspace("f1581148-9318-40fa-9402-61557255162a") as Watch;

            WatchViewModel node = watchNumber.GetWatchNode();
            Assert.AreEqual("5", node.NodeLabel);

            node = watchBoolean.GetWatchNode();
            Assert.AreEqual("False", node.NodeLabel);

            node = watchPoint.GetWatchNode();
            var pointNode = model.CurrentWorkspace.NodeFromWorkspace("64f10a92-3297-448b-be7a-03dbe1e8a90a");
            
            //Validate using the point node connected to watch node.
            AssertWatchContent(node, pointNode);
        }

        [Test]
        public void Watch1DCollections()
        {
            var model = Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\watch\Watch1DCollections.dyn");
            model.Open(openPath);

            Assert.DoesNotThrow(() => Controller.RunExpression(null));

            // get count node
            Watch watchNumbers = model.CurrentWorkspace.NodeFromWorkspace("f79b65d9-8cda-449c-a8fa-8a44166eec12") as Watch;
            Watch watchBooleans = model.CurrentWorkspace.NodeFromWorkspace("4ea56d78-68a9-400f-88b8-c6875365fa54") as Watch;
            Watch watchVectors = model.CurrentWorkspace.NodeFromWorkspace("73b35c1f-2dfb-4ce0-8609-c0bac9f3033c") as Watch;

            WatchViewModel node = watchNumbers.GetWatchNode();
            Assert.AreEqual("List", node.NodeLabel);
            Assert.AreEqual(10, node.Children.Count);
            AssertWatchTreeBranchContent(node.Children, new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            node = watchBooleans.GetWatchNode();
            Assert.AreEqual("List", node.NodeLabel);
            Assert.AreEqual(6, node.Children.Count);
            AssertWatchTreeBranchContent(node.Children, new object[] { true, false, true, false, true, false });

            node = watchVectors.GetWatchNode();
            Assert.AreEqual("List", node.NodeLabel);
            Assert.AreEqual(10, node.Children.Count);
            var vectorNode = model.CurrentWorkspace.NodeFromWorkspace("9aedc16c-da20-4584-a53f-7dd4f01dc5ee");

            //Validate using vecotr node connected to watch node.
            AssertWatchContent(node, vectorNode);
        }
    }
}
