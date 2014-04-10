using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    class ProtoGeometryTest : RevitNodeTestBase
    {
        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
            base.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            HostFactory.Instance.ShutDown();
            base.TearDown();
        }

    }
}
