using System.Linq;

using Autodesk.DesignScript.Geometry;

using NUnit.Framework;

using Revit.Elements;
using Revit.GeometryConversion;

using RevitTestServices;

using RTF.Framework;

namespace RevitNodesTests.GeometryConversion
{
    [TestFixture]
    public class RevitToProtoMeshTests : RevitNodeTestBase
    {
        [Test]
        [TestModel(@".\topography.rvt")]
        public void ToProtoType_ShouldReturnCorrectlyScaledMesh()
        {
            var allMeshesInDoc =
                ElementSelector.ByType<Autodesk.Revit.DB.Architecture.TopographySurface>(true)
                    .Cast<Revit.Elements.Topography>()
                    .SelectMany(x => x.InternalGeometry())
                    .OfType<Autodesk.Revit.DB.Mesh>()
                    .ToList();

            Assert.AreEqual(4, allMeshesInDoc.Count, "There should be 4 meshes in topography.rvt.  Has the file changed?");

            // get the big mesh in the doc
            var bigMesh = allMeshesInDoc.First(x => x.NumTriangles > 5000);

            // convert, performing unit conversion
            var convertedMesh = bigMesh.ToProtoType();

            var bb = BoundingBox.ByGeometry(convertedMesh.VertexPositions);

            var bbvol = BoundingBoxTests.BoundingBoxVolume(bb);
            var bbmin = bb.MinPoint;
            var bbmax = bb.MaxPoint;

            bbvol.ShouldBeApproximately(560845.185, 1e-2);
            bbmin.X.ShouldBeApproximately(-114.688, 1e-2);
            bbmax.X.ShouldBeApproximately(204.378, 1e-2);

            convertedMesh.Dispose();
        }
    }
}
