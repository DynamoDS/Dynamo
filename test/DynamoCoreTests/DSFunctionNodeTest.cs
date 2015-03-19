using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dynamo.DSEngine;
using Dynamo.Utilities;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoCore.Mirror;

namespace Dynamo.Tests
{
    [Category("DSExecution")]
    class DSFunctionNodeTest : DynamoViewModelUnitTest
    {
        [Test]
        public void TestLoadingFunctions()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\dsfunctions.dyn");
            ViewModel.OpenCommand.Execute(openPath);
            
            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count());
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
        }

        [Test]
        public void TestAddFunction()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\add.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());

            // get add node
            var addNode = model.CurrentWorkspace.NodeFromWorkspace("c969ebda-d77e-4cd3-985e-187dd1dccb03");
            string var = addNode.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);

            Assert.AreEqual(mirror.GetData().Data, 5.0);
        }

        [Test]
        public void TestAbs()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\abs.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());

            // get abs node
            var absNode = model.CurrentWorkspace.NodeFromWorkspace("2c26388d-3d14-443a-ac41-c2bb0987a58e");
            string var = absNode.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);

            Assert.AreEqual(mirror.GetData().Data, 10.0);

            var mulNode = model.CurrentWorkspace.NodeFromWorkspace("0c85072f-f9c5-45f3-8099-832161dfcacb");
            var = mulNode.GetAstIdentifierForOutputIndex(0).Name;
            mirror = null;
            Assert.DoesNotThrow(() => mirror = ViewModel.Model.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);

            Assert.AreEqual(mirror.GetData().Data, 100.0);
        }

        [Test]
        public void TestCount()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\count.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());

            // get count node
            var count = model.CurrentWorkspace.NodeFromWorkspace("007b5942-12b0-4cea-aa05-b43531b6ccb8");
            string var = count.GetAstIdentifierForOutputIndex(0).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror =  ViewModel.Model.EngineController.GetMirror(var));
            Assert.IsNotNull(mirror);

            var value = (Int64)mirror.GetData().Data;
            Assert.AreEqual(value, 10);
        }

        [Test, Category("RegressionTests")]
        public void TestGetKeys()
        {
            var model = ViewModel.Model;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\GetKeys.dyn");
            ViewModel.OpenCommand.Execute(openPath);

            // no crash
            Assert.DoesNotThrow(() => ViewModel.HomeSpace.Run());
        }
    }
}
