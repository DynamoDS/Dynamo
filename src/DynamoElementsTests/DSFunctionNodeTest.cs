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
    class DSFunctionNodeTest: DynamoUnitTest
    {
        [Test]
        public void TestLoadingFunctions()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\dsfunctions.dyn");
            model.Open(openPath);
            
            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
        }

        [Test]
        public void TestAddFunction()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\add.dyn");
            model.Open(openPath);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));

            // get add node
            var addNode = model.CurrentWorkspace.NodeFromWorkspace("c969ebda-d77e-4cd3-985e-187dd1dccb03");
            string var = (addNode.AstIdentifier as ProtoCore.AST.AssociativeAST.IdentifierNode).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = LiveRunnerServices.Instance.GetMirror(var));
            Assert.IsNotNull(mirror);

            StackValue value = mirror.GetData().GetStackValue();
            Assert.AreEqual(value.opdata_d, 5.0);
        }

        [Test]
        public void TestAbs()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\abs.dyn");
            model.Open(openPath);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));

            // get abs node
            var absNode = model.CurrentWorkspace.NodeFromWorkspace("2c26388d-3d14-443a-ac41-c2bb0987a58e");
            string var = (absNode.AstIdentifier as ProtoCore.AST.AssociativeAST.IdentifierNode).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = LiveRunnerServices.Instance.GetMirror(var));
            Assert.IsNotNull(mirror);

            StackValue value = mirror.GetData().GetStackValue();
            Assert.AreEqual(value.opdata_d, 10.0);

            var mulNode = model.CurrentWorkspace.NodeFromWorkspace("0c85072f-f9c5-45f3-8099-832161dfcacb");
            var = (mulNode.AstIdentifier as ProtoCore.AST.AssociativeAST.IdentifierNode).Name;
            mirror = null;
            Assert.DoesNotThrow(() => mirror = LiveRunnerServices.Instance.GetMirror(var));
            Assert.IsNotNull(mirror);

            value = mirror.GetData().GetStackValue();
            Assert.AreEqual(value.opdata_d, 100.0);
        }

        [Test]
        public void TestCount()
        {
            var model = dynSettings.Controller.DynamoModel;

            string openPath = Path.Combine(GetTestDirectory(), @"core\dsfunction\count.dyn");
            model.Open(openPath);

            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(null));

            // get count node
            var count = model.CurrentWorkspace.NodeFromWorkspace("007b5942-12b0-4cea-aa05-b43531b6ccb8");
            string var = (count.AstIdentifier as ProtoCore.AST.AssociativeAST.IdentifierNode).Name;
            RuntimeMirror mirror = null;
            Assert.DoesNotThrow(() => mirror = LiveRunnerServices.Instance.GetMirror(var));
            Assert.IsNotNull(mirror);

            StackValue value = mirror.GetData().GetStackValue();
            Assert.AreEqual(value.opdata, 10);
        }
    }
}
