using NUnit.Framework;
using DSRevitNodes;
using Autodesk.LibG;

namespace DSRevitNodesTests
{
    [TestFixture]
    internal class GeometryTests
    {
        [SetUp]
        public void Init()
        {
            Autodesk.LibG.LibG.start_asm_library();
        }

        [TearDown]
        public void Cleanup()
        {
            Autodesk.LibG.LibG.end_asm_library();
        }

        [Test]
        public void Domain()
        {
            //create a domain object
            var dom = new Domain(Vector.by_coordinates(0, 0), Vector.by_coordinates(1, 1));

            //assert the min and max of the domain
            Assert.IsTrue(dom.Min.x()==0);
            Assert.IsTrue(dom.Min.y()==0);
            Assert.IsTrue(dom.Max.x()==1);
            Assert.IsTrue(dom.Max.y()==1);

            //assert the spans of the domain
            Assert.IsTrue(dom.USpan == 1);
            Assert.IsTrue(dom.VSpan == 1);

        }

    }
}
