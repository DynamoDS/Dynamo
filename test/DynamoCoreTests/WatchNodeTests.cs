using System.Collections.ObjectModel;
using NUnit.Framework;
using System.IO;
using Dynamo.Nodes;
using Dynamo.ViewModels;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class WatchNodeTests : DynamoUnitTest
    {
        private void AssertWatchTreeBranchContent(ObservableCollection<WatchItem> branch, object[] content)
        {
            Assert.AreEqual(content.Length, branch.Count);
            int count = 0;
            foreach (var item in branch)
            {
                string nodeLabel = string.Format("[{0}] {1}", count, content[count]);
                Assert.AreEqual(nodeLabel, item.NodeLabel);
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

            WatchItem node = watchNumber.GetWatchNode();
            Assert.AreEqual("5", node.NodeLabel);

            node = watchBoolean.GetWatchNode();
            Assert.AreEqual("False", node.NodeLabel);

            node = watchPoint.GetWatchNode();
            Assert.AreEqual("Point", node.NodeLabel);
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

            WatchItem node = watchNumbers.GetWatchNode();
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
            string vector = "Vector";
            AssertWatchTreeBranchContent(node.Children, new object[] { vector, vector, vector, vector, vector, vector, vector, vector, vector, vector });
        }
    }
}
