using System;
using System.IO;
using Dynamo.Nodes;
using Dynamo.Selection;
using System.Linq;
using Dynamo.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using Dynamo.DSEngine;
using ProtoCore.Mirror;
using System.Collections;
using Dynamo.Models;
using DSCoreNodesUI;
using DSCore.File;
using RevitTestFramework;

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
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(11, model.CurrentWorkspace.Connectors.Count);

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
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
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
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

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
