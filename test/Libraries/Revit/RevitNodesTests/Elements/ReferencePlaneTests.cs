using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.Elements
{
    [TestFixture]
    public class ReferencePlaneTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByLine_ValidArgs()
        {
            var line = Line.ByStartPointEndPoint(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(1, 0, 0));
            Assert.NotNull(line);

            var refPlane = ReferencePlane.ByLine(line);

            Assert.NotNull(refPlane);
            Assert.NotNull(refPlane.Plane);
            Assert.NotNull(refPlane.ElementPlaneReference);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByStartPointEndPoint_ValidArgs()
        {
            var refPlane = ReferencePlane.ByStartPointEndPoint(Point.ByCoordinates(1, 0, 0),
                Point.ByCoordinates(1, 1, 1));

            Assert.NotNull(refPlane);
            Assert.NotNull(refPlane.Plane);
            Assert.NotNull(refPlane.ElementPlaneReference);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByLine_NullInput()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePlane.ByLine(null));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByStartPointEndPoint_NullInputBoth()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePlane.ByStartPointEndPoint(null, null));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByStartPointEndPoint_NullInput2()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePlane.ByStartPointEndPoint(Point.ByCoordinates(1, 1, 1), null));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ByStartPointEndPoint_NullInput1()
        {
            Assert.Throws(typeof(System.ArgumentNullException), () => ReferencePlane.ByStartPointEndPoint(Point.ByCoordinates(1, 1, 1), null));
        }
    }
}
