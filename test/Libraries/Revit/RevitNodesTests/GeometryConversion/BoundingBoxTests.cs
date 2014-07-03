using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using Revit.GeometryConversion;
using NUnit.Framework;
using RTF.Framework;

namespace DSRevitNodesTests.Conversion
{
    [TestFixture]
    class BoundingBoxTests : GeometricRevitNodeTest
    {

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void CanConvertRevitToProtoType()
        {
            var famSym = FamilySymbol.ByName("Box");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var bbox = famInst.BoundingBox;
            Assert.NotNull(bbox);

            var max = bbox.MaxPoint;
            var min = bbox.MinPoint;

            max.ShouldBeApproximately(Point.ByCoordinates(15, 16, 32).InDynamoUnits());
            min.ShouldBeApproximately(Point.ByCoordinates(-15, -14, 2).InDynamoUnits());

        }

        [Test]
        [TestModel(@".\MassWithBoxAndCone.rfa")]
        public void CanConvertProtoToRevitType()
        {
            var famSym = FamilySymbol.ByName("Box");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var bbox = famInst.BoundingBox;

            var bbxyz = bbox.ToRevitType();

            bbxyz.Max.ShouldBeApproximately(Point.ByCoordinates(15, 16, 32));
            bbxyz.Min.ShouldBeApproximately(Point.ByCoordinates(-15, -14, 2));

        }
    }
}
