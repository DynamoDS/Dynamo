using Autodesk.DesignScript.Geometry;
using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    public abstract class GeometricRevitNodeTest : RevitNodeTestBase
    {
        [SetUp]
        public void SetUpHostFactory()
        {
            AssemblyResolver.Setup();
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void ShutDownHostFactory()
        {
            HostFactory.Instance.ShutDown();
        }

    }
}
