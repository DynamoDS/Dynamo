using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using Revit.Elements;
using Revit.GeometryConversion;

using RTF.Framework;

namespace DSRevitNodesTests.GeometryConversion
{
    [TestFixture]
    internal class RevitToProtoSolidTests : GeometricRevitNodeTest
    {
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
                var asmSolid = solid.ToProtoType(false);

                asmSolid.Volume.ShouldBeApproximately(solid.Volume);
                asmSolid.Area.ShouldBeApproximately(solid.SurfaceArea);
                Assert.AreEqual(solid.Faces.Size, asmSolid.Faces.Length);
                asmSolid.Centroid().ShouldBeApproximately(solid.ComputeCentroid());

            }

        }
    }
}
