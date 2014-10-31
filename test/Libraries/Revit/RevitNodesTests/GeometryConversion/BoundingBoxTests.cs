using System;

using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

using Revit.Elements;
using Revit.GeometryConversion;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.GeometryConversion
{
    [TestFixture]
    public class BoundingBoxTests : RevitNodeTestBase
    {

        public static double BoundingBoxVolume(BoundingBox bb)
        {
            var val = bb.MaxPoint.Subtract(bb.MinPoint.AsVector());
            return Math.Abs(val.X * val.Y * val.Z);
        }

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

            // the box is 30ft x 30ft x 30ft
            // the placement point is the center of the bottom face of the box
            var boxOffsetTop = Vector.ByCoordinates(15,15,30).AsPoint().InDynamoUnits().AsVector();
            var boxOffsetBottom = Vector.ByCoordinates(-15,-15,0).AsPoint().InDynamoUnits().AsVector();

            min.ShouldBeApproximately((Point)pt.Translate(boxOffsetBottom));
            max.ShouldBeApproximately((Point)pt.Translate(boxOffsetTop));

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

            var max = bbxyz.Max;
            var min = bbxyz.Min;

            // the box is 30ft x 30ft x 30ft
            // the placement point is the center of the bottom face of the box
            var boxOffsetTop = Vector.ByCoordinates(15, 15, 30);
            var boxOffsetBottom = Vector.ByCoordinates(-15, -15, 0);

            min.ShouldBeApproximately((Point)pt.InHostUnits().Translate(boxOffsetBottom));
            max.ShouldBeApproximately((Point)pt.InHostUnits().Translate(boxOffsetTop));

        }

        [Test]
        public void ToRevitBoundingBox_Method_PreservesValues()
        {
            var csOrigin = Point.ByCoordinates(1, 2, 3);
            var csX = Vector.ByCoordinates(0, 1, 0);
            var csY = Vector.ByCoordinates(0, 0, 1);

            var cs = CoordinateSystem.ByOriginVectors(csOrigin, csX, csY);

            var minPoint = Point.ByCoordinates(-2, -2, -2);
            var maxPoint = Point.ByCoordinates(2, 2, 2);

            Autodesk.Revit.DB.BoundingBoxXYZ boundingBoxXYZ = GeometryPrimitiveConverter.ToRevitBoundingBox(
                cs,
                minPoint,
                maxPoint);

            boundingBoxXYZ.Min.ShouldBeApproximately(minPoint.InHostUnits());
            boundingBoxXYZ.Max.ShouldBeApproximately(maxPoint.InHostUnits());

            boundingBoxXYZ.Transform.Origin.ShouldBeApproximately(csOrigin.InHostUnits());

            boundingBoxXYZ.Transform.BasisX.ShouldBeApproximately(csX);
            boundingBoxXYZ.Transform.BasisY.ShouldBeApproximately(csY);
        }
    }
}
