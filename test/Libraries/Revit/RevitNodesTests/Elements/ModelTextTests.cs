using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using NUnit.Framework;
using Revit.GeometryConversion;

using RevitServices.Persistence;

using RevitTestServices;

using RTF.Framework;
using ModelText = Revit.Elements.ModelText;
using ModelTextType = Revit.Elements.ModelTextType;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;
using SketchPlane = Revit.Elements.SketchPlane;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class ModelTextTests : RevitNodeTestBase
    {
        internal double InternalDepth(Revit.Elements.ModelText text)
        {
            return (text.InternalElement as Autodesk.Revit.DB.ModelText).Depth;
        }

        internal XYZ InternalLocation(Revit.Elements.ModelText text)
        {
            return ((LocationPoint)text.InternalElement.Location).Point;
        }

        [Test]
        [TestModel(@".\withModelText.rfa")]
        public void ByTextSketchPlaneAndPosition_ValidArgs()
        {
            var origin = Point.ByCoordinates(1, 2, 3);
            var normal = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByOriginNormal(origin, normal);
            var text = "Snickers - why wait?";

            var name = "Model Text 1";
            var modelTextType = ModelTextType.ByName(name);

            var sketchPlane = SketchPlane.ByPlane(plane);
            var depth = 1;
            var x = 10;
            var y = 3;
            var mt = ModelText.ByTextSketchPlaneAndPosition(text, sketchPlane, x, y, depth, modelTextType);

            Assert.NotNull(mt);
            Assert.NotNull(mt.InternalElement);
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID( mt.InternalElement.UniqueId)));

            mt.Depth.ShouldBeApproximately(depth);

            // with unit conversion
            InternalDepth(mt).ShouldBeApproximately(depth * UnitConverter.DynamoToHostFactor);

            var expectedInternalLoc =
                origin.InHostUnits()
                    .Add(Vector.XAxis().Scale(x*UnitConverter.DynamoToHostFactor))
                    .Add(Vector.YAxis().Scale(y*UnitConverter.DynamoToHostFactor));
            InternalLocation(mt).ShouldBeApproximately(expectedInternalLoc);

        }

        [Test]
        [TestModel(@".\withModelText.rfa")]
        public void ByTextSketchPlaneAndPosition_BadArgs()
        {
            var origin = Point.ByCoordinates(1, 2, 3);
            var normal = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByOriginNormal(origin, normal);
            var text = "Snickers - why wait?";

            var name = "Model Text 1";
            var modelTextType = ModelTextType.ByName(name);

            var sketchPlane = SketchPlane.ByPlane(plane);

            Assert.Throws(typeof(System.ArgumentNullException), () => ModelText.ByTextSketchPlaneAndPosition(null, sketchPlane, 0, 0, 1, modelTextType));
            Assert.Throws(typeof(System.ArgumentNullException), () => ModelText.ByTextSketchPlaneAndPosition(text, null, 0, 0, 1, modelTextType));
            Assert.Throws(typeof(System.ArgumentNullException), () => ModelText.ByTextSketchPlaneAndPosition(text, sketchPlane, 0, 0, 1, null));
        }

    }
}
