using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using DSRevitNodes.Elements;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Tests
{
    [TestFixture]
    class SelectionTests
    {
        [Test]
        public void SelectElementASTGeneration()
        {
            //create an element in revit
            var sphere = DSSolid.Sphere(Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0), 5);
            var element = DSFreeForm.BySolid(sphere);
            var sel = DSSelection<Element>.SelectElement();
            sel.SelectedElement = element.InternalElement;

            var buildOutput = sel.BuildOutputAst(new List<AssociativeNode>());

            var funCall = (FunctionCallNode)((BinaryExpressionNode)buildOutput.First()).RightNode;

            Assert.IsInstanceOf<IdentifierNode>(funCall.Function);
            Assert.AreEqual(1, funCall.FormalArguments.Count);
            Assert.IsInstanceOf<IntNode>(funCall.FormalArguments[0]);
            Assert.AreEqual(element.InternalElement.Id.IntegerValue.ToString(), ((IntNode)funCall.FormalArguments[0]).value);
        }
    }
}
