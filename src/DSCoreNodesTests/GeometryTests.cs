using System;
using Autodesk.LibG;
using DSCoreNodes;
using NUnit.Framework;
using Dynamo.Utilities;

namespace DSCoreNodesTests
{
    [SetUpFixture]
    internal class Setup
    {
        [SetUp]
        public void Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelper.CurrentDomain_AssemblyResolve;
        }

        [TearDown]
        public void Cleanup()
        {

        }     
    }

    [TestFixture]
    internal class GeometryTests
    {
        
        [TestFixtureSetUp]
        public void Init()
        {
            LibG.start_asm_library();
        }

        [TestFixtureTearDown]
        public void Cleanup()
        {
            LibG.end_asm_library();
        }

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
