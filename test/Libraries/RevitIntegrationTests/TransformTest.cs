using System.IO;
using System.Linq;

using Autodesk.DesignScript.Geometry;

using DSCoreNodesUI;

using Dynamo.Models;
using Dynamo.Tests;

using NUnit.Framework;

using ProtoCore.Mirror;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class TransformTest : SystemTest
    {
        [Test]
        [TestModel(@".\empty.rvt")]
        public void BasisX()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Transform\BasisX.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            // Check that all nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            var nodes = ViewModel.Model.Nodes;

            double dummyNodesCount = nodes.OfType<DummyNode>().Count();

            // Check if there are dummy nodes
            if (dummyNodesCount >= 1)
            {
                Assert.Fail("Number of dummy nodes found: " + dummyNodesCount);
            }

            // Run the model
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            // Check node output
            NodeModel node = model.CurrentWorkspace.NodeFromWorkspace("58d488dd-b668-467f-b3ac-d46b5a97fabe");
            RuntimeMirror mirror = ViewModel.Model.EngineController.GetMirror(node.AstIdentifierBase);
            Assert.IsNotNull(mirror);

            var point = mirror.GetData().Data as Point;
            Assert.IsNotNull(point);
            Assert.AreEqual(point, Point.ByCoordinates(1, 0, 0));
        }

        [Test]
        [TestModel(@".\empty.rvt")]
        public void BasisY()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Transform\BasisY.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            // Check that all nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            var nodes = ViewModel.Model.Nodes;

            double dummyNodesCount = nodes.OfType<DummyNode>().Count();

            // Check if there are dummy nodes
            if (dummyNodesCount >= 1)
            {
                Assert.Fail("Number of dummy nodes found: " + dummyNodesCount);
            }

            // Run the model
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            // Check node output
            NodeModel node = model.CurrentWorkspace.NodeFromWorkspace("7fdb538d-22a3-412c-b646-d0fb23ca2dc6");
            RuntimeMirror mirror = ViewModel.Model.EngineController.GetMirror(node.AstIdentifierBase);
            Assert.IsNotNull(mirror);

            var point = mirror.GetData().Data as Point;
            Assert.IsNotNull(point);
            Assert.AreEqual(point, Point.ByCoordinates(0, 1, 0));
        }

        [Test]
        [TestModel(@".\empty.rvt")]
        public void BasisZ()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Transform\BasisZ.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            // Check that all nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            var nodes = ViewModel.Model.Nodes;

            double dummyNodesCount = nodes.OfType<DummyNode>().Count();

            // Check if there are dummy nodes
            if (dummyNodesCount >= 1)
            {
                Assert.Fail("Number of dummy nodes found: " + dummyNodesCount);
            }

            // Run the model
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            // Check node output
            NodeModel node = model.CurrentWorkspace.NodeFromWorkspace("783ce70c-789d-4c2a-ad40-c16d6d933fd4");
            RuntimeMirror mirror = ViewModel.Model.EngineController.GetMirror(node.AstIdentifierBase);
            Assert.IsNotNull(mirror);

            var point = mirror.GetData().Data as Point;
            Assert.IsNotNull(point);
            Assert.AreEqual(point, Point.ByCoordinates(0, 0, 1));
        }
    }
}
