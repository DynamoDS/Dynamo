﻿using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements;
using Revit.GeometryConversion;
using NUnit.Framework;
using RevitTestFramework;

namespace DSRevitNodesTests.Conversion
{
    [TestFixture]
    class BoundingBoxTests : ProtoGeometryTest
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

            max.AssertShouldBeApproximately(Point.ByCoordinates(15, 16, 32));
            min.AssertShouldBeApproximately(Point.ByCoordinates(-15, -14, 2));

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

            bbxyz.Max.AssertShouldBeApproximately(Point.ByCoordinates(15, 16, 32));
            bbxyz.Min.AssertShouldBeApproximately(Point.ByCoordinates(-15, -14, 2));

        }
    }
}
