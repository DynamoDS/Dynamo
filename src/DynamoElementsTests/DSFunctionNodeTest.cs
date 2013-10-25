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
    }
}
