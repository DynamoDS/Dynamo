using System.Linq;
using System.Windows.Media;
using Autodesk.DesignScript.Interfaces;

using Dynamo.Wpf;
using Dynamo.Wpf.Rendering;
using NUnit.Framework;

namespace DynamoCoreUITests
{
    [TestFixture]
    public class HelixRenderPackageTests
    {
        [Test]
        public void HelixRenderPackage_Construction_IsInitializedCorrectly()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);
        }

        [Test]
        public void HelixRenderPackage_PushMesh()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            PushQuadIntoPackage(p);
            Assert.AreEqual(6, p.Mesh.Positions.Count);
            Assert.AreEqual(6, p.Mesh.TextureCoordinates.Count);
            Assert.AreEqual(6, p.Mesh.Indices.Count);
            Assert.AreEqual(0, p.Mesh.Colors.Count);
        }

        [Test]
        public void HelixRenderPackage_PushLine()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            PushLineIntoPackage(p);

            Assert.AreEqual(2, p.Lines.Positions.Count);
            Assert.AreEqual(2, p.Lines.Indices.Count);
            Assert.AreEqual(0, p.Lines.Colors.Count);
        }

        [Test]
        public void HelixRenderPackage_PushPoint()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            PushPointIntoPackage(p);

            Assert.AreEqual(1, p.Points.Positions.Count);
            Assert.AreEqual(1, p.Points.Indices.Count);
            Assert.AreEqual(0, p.Points.Colors.Count);
        }

        [Test]
        public void HelixRenderPackage_PushData_CountsAreCorrect()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            PushQuadIntoPackage(p);
            PushPointIntoPackage(p);
            PushLineIntoPackage(p);

            Assert.AreEqual(2, p.LineVertexCount);
            Assert.AreEqual(6, p.MeshVertexCount);
            Assert.AreEqual(1, p.PointVertexCount);
        }

        [Test]
        public void HelixRenderPackage_ApplyPointVertexColors_Array_Success()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            PushPointIntoPackage(p);
            p.ApplyPointVertexColors(WhiteByteArrayOfLength(1));
            Assert.AreEqual(1, p.Points.Colors.Count);
        }

        [Test]
        public void HelixRenderPackage_ApplyLineVertexColors_Array_Success()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            PushLineIntoPackage(p);
            p.ApplyLineVertexColors(WhiteByteArrayOfLength(2));
            Assert.AreEqual(2, p.Lines.Colors.Count);
        }

        [Test]
        public void HelixRenderPackage_ApplyMeshVertexColors_Success()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            PushQuadIntoPackage(p);

            p.ApplyMeshVertexColors(WhiteByteArrayOfLength(6));
            Assert.AreEqual(6, p.Mesh.Colors.Count);
        }

        [Test]
        public void HelixRenderPackage_Clear_IsClearedCompletely()
        {
            var p = new HelixRenderPackage();

            PushQuadIntoPackage(p);

            p.Clear();

            Assert.IsEmpty(p.Points.Positions);
            Assert.IsEmpty(p.Points.Indices);
            Assert.IsEmpty(p.Points.Colors);

            Assert.IsEmpty(p.Mesh.Positions);
            Assert.IsEmpty(p.Mesh.Normals);
            Assert.IsEmpty(p.Mesh.Indices);
            Assert.IsEmpty(p.Mesh.TextureCoordinates);
            Assert.IsEmpty(p.Mesh.Colors);

            Assert.IsEmpty(p.Lines.Positions);
            Assert.IsEmpty(p.Lines.Indices);
            Assert.IsEmpty(p.Lines.Colors);
        }

        [Test]
        public void HelixRenderPackage_MeshVertexBufferCheck()
        {
            var p = new HelixRenderPackage();

            PushQuadIntoPackage(p);

            var test1 = p.MeshVertices.Skip(3).Take(3).ToArray();
            var test2 = p.MeshNormals.Take(3).ToArray();

            // Expect Y Up.
            Assert.AreEqual(test1, new double[]{1,0,0});
            Assert.AreEqual(test2, new double[]{0,1,0});
        }

        [Test]
        public void HelixRenderPackage_MeshColorBufferCheck()
        {
            var p = new HelixRenderPackage();

            PushQuadIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(6);

            p.ApplyMeshVertexColors(testColors);

            Assert.AreEqual(p.MeshVertexColors, testColors);
        }

        /// <summary>
        /// Pushes an uncolored quad into a package.
        /// </summary>
        private static void PushQuadIntoPackage(IRenderPackage package)
        {
            package.AddTriangleVertex(0,0,0);
            package.AddTriangleVertex(1,0,0);
            package.AddTriangleVertex(1,1,0);

            package.AddTriangleVertex(0,0,0);
            package.AddTriangleVertex(1,1,0);
            package.AddTriangleVertex(0,1,0);

            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);
            package.AddTriangleVertexNormal(0, 0, 1);

            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
        }

        /// <summary>
        /// Pushes an uncolored line into a package.
        /// </summary>
        internal static void PushLineIntoPackage(IRenderPackage package)
        {
            package.AddLineStripVertex(0,0,0);
            package.AddLineStripVertex(1,1,1);
            package.AddLineStripVertexCount(2);
        }

        /// <summary>
        /// Pushes a point into a package.
        /// </summary>
        private static void PushPointIntoPackage(IRenderPackage package)
        {
            package.AddPointVertex(0,0,0);
        }

        private byte[] WhiteByteArrayOfLength(int numberOfVertices)
        {
            var colors = new byte[4 * numberOfVertices];
            for (int i = 0; i < 4 * numberOfVertices; i++)
            {
                colors[i] = 255;
            }

            return colors;
        }
    }

    [TestFixture]
    public class HelixRenderPackageExtensionTests
    {
        [Test]
        public void HelixRenderPackage_SingleLineVertexColor_AllLineStripVerticesHaveColor_True()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            // Same line strip vertex colors.
            HelixRenderPackageTests.PushLineIntoPackage(p);
            p.AddLineStripVertexColor(255,0,0,255);
            p.AddLineStripVertexColor(255,0,0,255);

            Assert.True(p.AllLineStripVerticesHaveColor(Color.FromArgb(255,255,0,0)));
        }

        [Test]
        public void HelixRenderPackage_ManyLineVertexColors_AllLineStripVerticesHaveColor_False()
        {
            var p = new HelixRenderPackage();
            Assert.NotNull(p);

            // Different line strip vertex colors.
            HelixRenderPackageTests.PushLineIntoPackage(p);
            p.AddLineStripVertexColor(255, 0, 0, 255);
            p.AddLineStripVertexColor(255, 255, 0, 0);

            Assert.False(p.AllLineStripVerticesHaveColor(Color.FromArgb(255, 255, 0, 0)));
        }
    }
}
