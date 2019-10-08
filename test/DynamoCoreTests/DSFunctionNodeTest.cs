using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;
using ProtoCore.Mirror;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class DSFunctionNodeTest : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        public void TestLoadingFunctions()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsfunction\dsfunctions.dyn");
            OpenModel(openPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
        }

        [Test]
        public void TestAddFunction()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsfunction\add.dyn");
            RunModel(openPath);

            // get add node
            var addNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("c969ebda-d77e-4cd3-985e-187dd1dccb03");
            string var = addNode.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = CurrentDynamoModel.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);

            Assert.AreEqual(mirror.GetData().Data, 5.0);
        }

        [Test]
        public void TestAbs()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsfunction\abs.dyn");
            RunModel(openPath);

            // get abs node
            var absNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("2c26388d-3d14-443a-ac41-c2bb0987a58e");
            string var = absNode.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = CurrentDynamoModel.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);

            Assert.AreEqual(mirror.GetData().Data, 10.0);

            var mulNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("0c85072f-f9c5-45f3-8099-832161dfcacb");
            var = mulNode.GetAstIdentifierForOutputIndex(0).Name;
            mirror = null;
            Assert.DoesNotThrow(() => mirror = CurrentDynamoModel.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);

            Assert.AreEqual(mirror.GetData().Data, 100.0);
        }

        [Test]
        public void TestCount()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsfunction\count.dyn");
            RunModel(openPath);

            // get count node
            var count = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace("007b5942-12b0-4cea-aa05-b43531b6ccb8");
            string var = count.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = CurrentDynamoModel.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);

            var value = (Int64)mirror.GetData().Data;
            Assert.AreEqual(value, 10);
        }

        [Test, Category("RegressionTests")]
        public void TestGetKeys()
        {
            string openPath = Path.Combine(TestDirectory, @"core\dsfunction\GetKeys.dyn");
            OpenModel(openPath);

            // no crash
            Assert.DoesNotThrow(BeginRun);
        }
    }
}
