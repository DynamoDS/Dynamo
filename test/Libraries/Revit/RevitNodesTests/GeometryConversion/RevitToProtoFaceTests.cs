using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using NUnit.Framework;
using Revit.Elements;
using Revit.GeometryConversion;

namespace DSRevitNodesTests.GeometryConversion
{
    [TestFixture]
    internal class RevitToProtoFaceTests : RevitNodeTestBase
    {
        [SetUp]
        public void Setup()
        {
            HostFactory.Instance.StartUp();
        }

        [TearDown]
        public void TearDown()
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
                .OfType<Autodesk.Revit.DB.Solid>();

            foreach (var solid in allSolidsInDoc)
            {
                var asmSolid = solid.ToProtoType();

                asmSolid.Volume.ShouldBeApproximately(solid.Volume);
                asmSolid.Area.ShouldBeApproximately(solid.SurfaceArea);
                Assert.AreEqual(allSolidsInDoc.Count(), asmSolid.Faces.Length);
                asmSolid.Centroid().ShouldBeApproximately(solid.ComputeCentroid());

            }

        }
    }
}
