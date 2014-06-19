using Autodesk.DesignScript.Geometry;

using DynamoUnits;

using NUnit.Framework;

namespace DSRevitNodesTests
{
    [TestFixture]
    public class GeometricRevitNodeTest : RevitNodeTestBase
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyResolver.Setup();

            HostFactory.Instance.StartUp();
            SetUpHostUnits();
        }

        private void SetUpHostUnits()
        {
            BaseUnit.HostApplicationInternalAreaUnit = DynamoAreaUnit.SquareFoot;
            BaseUnit.HostApplicationInternalLengthUnit = DynamoLengthUnit.DecimalFoot;
            BaseUnit.HostApplicationInternalVolumeUnit = DynamoVolumeUnit.CubicFoot;
        }

        [TearDown]
        public void ShutDownHostFactory()
        {
            HostFactory.Instance.ShutDown();
        }

    }
}
