using Autodesk.Revit.DB;
using DSRevitNodes.Elements;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Tests
{
    [TestFixture]
    class InteractivityTests
    {
        [Test]
        public void SelectElementASTGeneration()
        {
            //create an element in revit
            var sphere = DSSolid.Sphere(Autodesk.DesignScript.Geometry.Point.ByCoordinates(0, 0, 0), 5);
            var element = DSFreeForm.BySolid(sphere);

            var sel = DSElementSelection<Element>.SelectElement();
            sel.SelectedElement = element.InternalElement;

            var buildOutput = sel.BuildAst();

            Assert.IsInstanceOf<FunctionCallNode>(buildOutput);

            var funCall = buildOutput as FunctionCallNode;

            Assert.IsInstanceOf<IdentifierNode>(funCall.Function);
            Assert.AreEqual(1, funCall.FormalArguments.Count);
            Assert.IsInstanceOf<IntNode>(funCall.FormalArguments[0]);
            Assert.AreEqual("123", (funCall.FormalArguments[0] as IntNode).value);
        }
    }
}
