using System.Linq;

using NUnit.Framework;
using Revit.Elements;
using Revit.GeometryConversion;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.GeometryConversion
{
    [TestFixture]
    internal class RevitToProtoFaceTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\Revolve.rfa")]
        public void ToProtoType_SucceedsForRevolvedEllipse()
        {
            // extract revolved solid from doc
            var revolvedEllipse = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true)
                .Cast<Revit.Elements.Form>()
                .SelectMany(x => x.InternalGeometry())
                .OfType<Autodesk.Revit.DB.Solid>()
                .First();

            // get the faces from the solid
            var faces = revolvedEllipse.Faces.Cast<Autodesk.Revit.DB.Face>();

            Assert.AreEqual(2, faces.Count());

            var face = faces.First();

            var r = face.ToProtoType(false);

            Assert.AreEqual(1, r.Count());

            r.First().Area.ShouldBeApproximately(face.Area, 1e-1);
         
        }
    }
}
