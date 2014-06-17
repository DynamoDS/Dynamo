using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class GeometricRevitNodeTest : RevitNodeTestBase
    {
        [SetUp]
        public override void Setup()
        {
            HostFactory.Instance.StartUp();
            base.Setup();
        }

        [TearDown]
        public override void TearDown()
        {
            HostFactory.Instance.ShutDown();
            base.TearDown();
        }

    }
}
