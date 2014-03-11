using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit;
using Revit.Elements;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using NUnit.Framework;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodesTests.Conversion
{
    [TestFixture]
    class BoundingBoxTests : ProtoGeometryTest
    {

        [Test]
        public void CanConvertRevitToProtoType()
        {
            var famSym = FamilySymbol.ByName("Box");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var bbox = famInst.BoundingBox;
            Assert.NotNull(bbox);

            var max = bbox.MaxPoint;
            var min = bbox.MinPoint;

            max.ShouldBeApproximately(Point.ByCoordinates(15, 16, 32));
            min.ShouldBeApproximately(Point.ByCoordinates(-15, -14, 2));

        }

        [Test]
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
