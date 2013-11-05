using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Tests
{
    class NewNodeSampleTests
    {
        [Test]
        public void SelectElementASTGeneration()
        {
            var selectNode = new SelectElement { SelectedElement = new ElementId(123) };
            var buildOutput = selectNode.BuildAst();

            Assert.IsInstanceOf<FunctionCallNode>(buildOutput);

            var funCall = buildOutput as FunctionCallNode;

            Assert.IsInstanceOf<IdentifierNode>(funCall.Function);
            Assert.AreEqual(1, funCall.FormalArguments.Count);
            Assert.IsInstanceOf<IntNode>(funCall.FormalArguments[0]);
            Assert.AreEqual("123", (funCall.FormalArguments[0] as IntNode).value);
        }
    }
}
