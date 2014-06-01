using System.Linq;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using NUnit.Framework;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitTestFramework;

namespace DSRevitNodesTests.GeometryConversion
{
    [TestFixture]
    internal class RevitToProtoFaceTests : RevitNodeTestBase
    {
        [SetUp]
        public override void Setup()
        {
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public override void TearDown()
        {
            HostFactory.Instance.ShutDown();
        }

        [Test]
        [TestModel(@".\Solids.rfa")]
        public void AllSolidsConvert()
        {

            var allSolidsInDoc = ElementSelector.ByType<Autodesk.Revit.DB.Form>(true)
                .Cast<Revit.Elements.Form>()
                .SelectMany(x => x.InternalGeometry())
                .OfType<Autodesk.Revit.DB.Solid>()
                .ToList();

            foreach (var solid in allSolidsInDoc)
            {
                var asmSolid = solid.ToProtoType();

                asmSolid.Volume.ShouldBeApproximately(solid.Volume);
                asmSolid.Area.ShouldBeApproximately(solid.SurfaceArea);
                Assert.AreEqual(solid.Faces.Size, asmSolid.Faces.Length);
                asmSolid.Centroid().ShouldBeApproximately(solid.ComputeCentroid());

            }

        }
    }
}
