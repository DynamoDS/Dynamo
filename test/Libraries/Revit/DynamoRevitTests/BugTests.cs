using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using Dynamo.Models;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class BugTests:DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\Bugs\MAGN_66.rfa")]
        public void MAGN_66()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-66

            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Bugs\MAGN_66.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            AssertNoDummyNodes();

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void MAGN_102()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-102

            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Bugs\MAGN_102_projectPointsToFace_selfContained.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\Bugs\MAGN-122_wallsAndFloorsAndLevels.rvt")]
        public void MAGN_122()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-122
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Bugs\MAGN_122_wallsAndFloorsAndLevels.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(20, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(23, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

            // Check for Walls Creation
            var walls = "b7392d1d-6333-4bed-b10d-7b83520d2c3e";
            AssertPreviewCount(walls, 6);

            // I will un-comment this code once Ian fix the issue with Document.IsFamilyDocument 
            //var wallinst = GetPreviewValueAtIndex(walls, 3) as Floor;
            //Assert.IsNotNull(wallinst);
            //Assert.IsNotNullOrEmpty(wallinst.Name);

            // Check for Floor Creation
            var floors = "25392912-b625-4020-8dd1-81923c5e4823";
            AssertPreviewCount(floors, 6);

            // I will un-comment this code once Ian fix the issue with Document.IsFamilyDocument 
            //var floorInst = GetPreviewValueAtIndex(floors, 3) as Floor;
            //Assert.IsNotNull(floorInst);
            //Assert.IsNotNullOrEmpty(floorInst.Name);

        }

        [Test]
        [TestModel(@".\Bugs\MAGN-438_structuralFraming_simple.rvt")]
        public void MAGN_438()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-438
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\\Bugs\MAGN-438_structuralFraming_simple.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\Bugs\MAGN_2576_DataImport.rvt")]
        public void MAGN_2576()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2576
            var model = dynSettings.Controller.DynamoModel;
            var workspace = Controller.DynamoModel.CurrentWorkspace;

            string samplePath = Path.Combine(_testPath, @".\\Bugs\Defect_MAGN_2576.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            dynSettings.Controller.RunExpression();

            // there should not be any crash on running this graph.
            // below node should have an error because there is no selection for Floor Type.
            NodeModel nodeModel = workspace.NodeFromWorkspace("cc38d11d-cda2-4294-81dc-119776af7338");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);

        }
    }
}
