using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class ReferencePlaneTests
    {
        [Test]
        public void ByLine_ValidArgs()
        {
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 1, 1));
            Assert.NotNull(line);

            var refPlane = ReferencePlane.ByLine(line);

            Assert.NotNull(refPlane);
            Assert.NotNull(refPlane.Plane);
            Assert.NotNull(refPlane.PlaneReference);
        }

        [Test]
        public void ByStartPointEndPoint_ValidArgs()
        {
            var refPlane = ReferencePlane.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0),
                Point.ByCoordinates(1, 1, 1));

            Assert.NotNull(refPlane);
            Assert.NotNull(refPlane.Plane);
            Assert.NotNull(refPlane.PlaneReference);
        }


        [Test]
        public void ByLine_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePlane.ByLine(null));
        }

        [Test]
        public void ByStartPointEndPoint_NullInputBoth()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePlane.ByStartPointEndPoint(null, null));
        }

        [Test]
        public void ByStartPointEndPoint_NullInput2()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePlane.ByStartPointEndPoint(Point.ByCoordinates(1, 1, 1), null));
        }

        [Test]
        public void ByStartPointEndPoint_NullInput1()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePlane.ByStartPointEndPoint(Point.ByCoordinates(1, 1, 1), null));
        }
    }
}
