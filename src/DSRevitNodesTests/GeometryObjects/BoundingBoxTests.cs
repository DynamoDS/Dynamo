using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit;
using Revit.Elements;
using Revit.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.GeometryObjects
{
    [TestFixture]
    public class BoundingBoxTests
    {
        [Test]
        public void BoundingBoxPropertyOnAbstractElement()
        {
            var famSym = FamilySymbol.ByName("Box");
            var pt = Point.ByCoordinates(0, 1, 2);
            var famInst = FamilyInstance.ByPoint(famSym, pt);

            var bbox = famInst.BoundingBox;
            Assert.NotNull(bbox);

            var max = bbox.Max;
            var min = bbox.Min;

            Assert.AreEqual(-15, min.X, 1e-6);
            Assert.AreEqual(-14, min.Y, 1e-6);
            Assert.AreEqual(2, min.Z, 1e-6);

            Assert.AreEqual(15, max.X, 1e-6);
            Assert.AreEqual(16, max.Y, 1e-6);
            Assert.AreEqual(32, max.Z, 1e-6);
        }
    }
}
