using Autodesk.LibG;
using DSCoreNodes;
using NUnit.Framework;

namespace DSCoreNodesTests
{
    [TestFixture]
    class GeometryTests
    {
        [Test]
        public void Domain2D()
        {
            //create a domain object
            var dom = new Domain2D(Vector.by_coordinates(0, 0), Vector.by_coordinates(1, 1));

            //assert the min and max of the domain
            Assert.IsTrue(dom.Min.x() == 0);
            Assert.IsTrue(dom.Min.y() == 0);
            Assert.IsTrue(dom.Max.x() == 1);
            Assert.IsTrue(dom.Max.y() == 1);

            //assert the spans of the domain
            Assert.IsTrue(dom.USpan == 1);
            Assert.IsTrue(dom.VSpan == 1);
        }

        [Test]
        public void Domain()
        {
            //create a domain object
            var dom = new Domain(0, 1);

            //assert the min and max of the domain
            Assert.IsTrue(dom.Min == 0);
            Assert.IsTrue(dom.Max == 1);

            //assert the spans of the domain
            Assert.IsTrue(dom.Span == 1);
        }

    }
}
