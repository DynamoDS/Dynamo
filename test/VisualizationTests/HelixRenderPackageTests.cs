using System;
using System.Linq;
using System.Windows.Media;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Wpf.Rendering;
using NUnit.Framework;

namespace WpfVisualizationTests
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

        [Test]
        public void HelixRenderPackage_AppendMeshVertexColorRange_Success()
        {
            var p = new HelixRenderPackage();

            PushQuadIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(6);

            p.AppendMeshVertexColorRange(testColors);

            Assert.AreEqual(p.MeshVertexColors, testColors);
            
            PushQuadIntoPackage(p);
            
            p.AppendMeshVertexColorRange(testColors);

            var totalColors = WhiteByteArrayOfLength(12);
            
            Assert.AreEqual(p.MeshVertexColors, totalColors);
        }

        [Test]
        public void HelixRenderPackage_AppendMeshVertexColorRange_ThrowsForBadRange()
        {
            var p = new HelixRenderPackage();

            PushQuadIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(5);

            try
            {
                p.AppendMeshVertexColorRange(testColors);
            }
            catch(Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.MeshVertexColorCount);

            testColors = WhiteByteArrayOfLength(7);

            try
            {
                p.AppendMeshVertexColorRange(testColors);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.MeshVertexColorCount);
        }

        [Test]
        public void HelixRenderPackage_UpdateMeshVertexColorForRange_Success()
        {
            var p = new HelixRenderPackage();

            PushQuadIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(6);
            
            //Set initial colors
            p.AppendMeshVertexColorRange(testColors);

            //Update colors
            p.UpdateMeshVertexColorForRange(0,3,255, 255, 255, 255);

            Assert.AreEqual(p.MeshVertexColors, testColors);
        }

        [Test]
        public void HelixRenderPackage_UpdateMeshVertexColorForRange_ThrowsForBadRange()
        {
            var p = new HelixRenderPackage();

            PushQuadIntoPackage(p);

            //Try to update without any colors set
            try
            {
                p.UpdateMeshVertexColorForRange(0, 3, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.MeshVertexColorCount);

            var testColors = WhiteByteArrayOfLength(6);
            //Set colors
            p.AppendMeshVertexColorRange(testColors);

            //Try to update colors with wrong range
            try
            {
                p.UpdateMeshVertexColorForRange(0, 7, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(6, p.MeshVertexColorCount);

            //Try to update colors with index error
            try
            {
                p.UpdateMeshVertexColorForRange(6, 0, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(6, p.MeshVertexColorCount);
        }

        [Test]
        public void HelixRenderPackage_AppendLineVertexColorRange_Success()
        {
            var p = new HelixRenderPackage();

            PushLineIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(2);

            p.AppendLineVertexColorRange(testColors);

            Assert.AreEqual(p.LineStripVertexColors, testColors);

            PushLineIntoPackage(p);

            p.AppendLineVertexColorRange(testColors);

            var totalColors = WhiteByteArrayOfLength(4);

            Assert.AreEqual(p.LineStripVertexColors, totalColors);
        }

        [Test]
        public void HelixRenderPackage_AppendLineVertexColorRange_ThrowsForBadRange()
        {
            var p = new HelixRenderPackage();

            PushLineIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(1);

            try
            {
                p.AppendLineVertexColorRange(testColors);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.LineVertexColorCount);

            testColors = WhiteByteArrayOfLength(3);

            try
            {
                p.AppendLineVertexColorRange(testColors);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.LineVertexColorCount);
        }

        [Test]
        public void HelixRenderPackage_UpdateLineVertexColorForRange_Success()
        {
            var p = new HelixRenderPackage();

            PushLineIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(2);

            //Set initial colors
            p.AppendLineVertexColorRange(testColors);

            //Update colors
            p.UpdateLineVertexColorForRange(1, 1, 255, 255, 255, 255);

            Assert.AreEqual(p.LineStripVertexColors, testColors);
        }

        [Test]
        public void HelixRenderPackage_UpdateLineVertexColorForRange_ThrowsForBadRange()
        {
            var p = new HelixRenderPackage();

            PushLineIntoPackage(p);

            //Try to update without any colors set
            try
            {
                p.UpdateLineVertexColorForRange(0, 1, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.LineVertexColorCount);

            var testColors = WhiteByteArrayOfLength(2);
            //Set colors
            p.AppendLineVertexColorRange(testColors);

            //Try to update colors with wrong range
            try
            {
                p.UpdateLineVertexColorForRange(0, 3, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(2, p.LineVertexColorCount);

            //Try to update colors with index error
            try
            {
                p.UpdateLineVertexColorForRange(1, 0, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(2, p.LineVertexColorCount);
        }

        [Test]
        public void HelixRenderPackage_AppendPointVertexColorRange_Success()
        {
            var p = new HelixRenderPackage();

            PushPointIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(1);

            p.AppendPointVertexColorRange(testColors);

            Assert.AreEqual(p.PointVertexColors, testColors);

            PushPointIntoPackage(p);

            p.AppendPointVertexColorRange(testColors);

            var totalColors = WhiteByteArrayOfLength(2);

            Assert.AreEqual(p.PointVertexColors, totalColors);
        }

        [Test]
        public void HelixRenderPackage_AppendPointVertexColorRange_ThrowsForBadRange()
        {
            var p = new HelixRenderPackage();

            PushPointIntoPackage(p);
            PushPointIntoPackage(p);
            
            var testColors = WhiteByteArrayOfLength(1);

            try
            {
                p.AppendPointVertexColorRange(testColors);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.PointVertexColorCount);

            testColors = WhiteByteArrayOfLength(3);

            try
            {
                p.AppendPointVertexColorRange(testColors);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.PointVertexColorCount);
        }

        [Test]
        public void HelixRenderPackage_UpdatePointVertexColorForRange_Success()
        {
            var p = new HelixRenderPackage();

            PushPointIntoPackage(p);
            PushPointIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(2);

            //Set initial colors
            p.AppendPointVertexColorRange(testColors);

            //Update colors
            p.UpdatePointVertexColorForRange(1, 1, 255, 255, 255, 255);

            Assert.AreEqual(p.PointVertexColors, testColors);
        }

        [Test]
        public void HelixRenderPackage_UpdatePointVertexColorForRange_ThrowsForBadRange()
        {
            var p = new HelixRenderPackage();

            PushPointIntoPackage(p);
            PushPointIntoPackage(p);
            
            //Try to update without any colors set
            try
            {
                p.UpdatePointVertexColorForRange(0, 1, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(0, p.PointVertexColorCount);

            var testColors = WhiteByteArrayOfLength(2);
            //Set colors
            p.AppendPointVertexColorRange(testColors);

            //Try to update colors with wrong range
            try
            {
                p.UpdatePointVertexColorForRange(0, 3, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(2, p.PointVertexColorCount);

            //Try to update colors with index error
            try
            {
                p.UpdatePointVertexColorForRange(1, 0, 255, 255, 255, 255);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
            }

            Assert.AreEqual(2, p.PointVertexColorCount);
        }

        [Test]
        public void HelixRenderPackage_ObsoleteCheck()
        {
            var p = new HelixRenderPackage();

            PushQuadIntoPackage(p);
            PushLineIntoPackage(p);
            PushPointIntoPackage(p);

            var testColors = WhiteByteArrayOfLength(6);
            try
            {
                p.AllowLegacyColorOperations = false;
                p.ApplyMeshVertexColors(testColors);
            }
            catch(Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(LegacyRenderPackageMethodException));
            }
            
            Assert.AreEqual(0, p.MeshVertexColorCount);
        }

        [Test]
        public void SetBaseTessellationRange_SetsDataCorrectly()
        {
            var p = new HelixRenderPackage();
            var id = Guid.NewGuid();
            p.AddTriangleVertex(0, 0, 0);
            p.AddTriangleVertex(0, 0, 1);
            p.AddTriangleVertex(1, 0, 0);
            p.AddInstanceGuidForMeshVertexRange(0, 2, id);
            p.AddInstanceMatrix(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, id);
            p.AddInstanceMatrix(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, id);
            Assert.AreEqual(0, p.MeshVertexRangesAssociatedWithInstancing[id].Item1);
            Assert.AreEqual(2, p.MeshVertexRangesAssociatedWithInstancing[id].Item2);
            Assert.AreEqual(3, p.MeshVertexCount);
        }
        [Test]
        public void SetBaseTessellationRange_ThrowsWhenIDAlreadyExists()
        {
            var p = new HelixRenderPackage();
            var id = Guid.NewGuid();
            p.AddTriangleVertex(0, 0, 0);
            p.AddTriangleVertex(0, 0, 1);
            p.AddTriangleVertex(1, 0, 0);
            Assert.Throws<Exception>(() =>
            {
                p.AddInstanceGuidForMeshVertexRange(0, 2, id);
                p.AddInstanceGuidForMeshVertexRange(0, 2, id);
                p.AddInstanceMatrix(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, id);

            });

        }
        [Test]
        public void AddInstanceMatrix_ThrowsWhenIDDoesNotExist()
        {
            var p = new HelixRenderPackage();
            var id = Guid.NewGuid();
            p.AddTriangleVertex(0, 0, 0);
            p.AddTriangleVertex(0, 0, 1);
            p.AddTriangleVertex(1, 0, 0);
            Assert.Throws<Exception>(() =>
            {
                p.AddInstanceMatrix(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, id);
            });

        }
        [Test]
        public void SetBaseTessellationRange_ThrowsWithBadRange()
        {
            var p = new HelixRenderPackage();
            var id = Guid.NewGuid();
            Assert.Throws<Exception>(() =>
            {
                p.AddInstanceGuidForMeshVertexRange(0, 2, id);
                p.AddInstanceMatrix(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, id);

            });
          
        }
        [Test]
        public void InstancePackage_ContainsChecksAllInternalCollection()
        {
            var p = new HelixRenderPackage();
            var id = Guid.NewGuid();
            Assert.IsFalse(p.ContainsTessellationId(id));

            p.AddTriangleVertex(0, 0, 0);
            p.AddTriangleVertex(0, 0, 1);
            p.AddTriangleVertex(1, 0, 0);
            p.AddInstanceGuidForMeshVertexRange(0, 2, id);
            p.AddInstanceMatrix(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, id);
            Assert.IsTrue(p.ContainsTessellationId(id));

            p = new HelixRenderPackage();
            Assert.IsFalse(p.ContainsTessellationId(id));
            p.AddLineStripVertex(0, 0, 0);
            p.AddLineStripVertex(0, 0, 1);
            p.AddLineStripVertex(1, 0, 0);
            p.AddInstanceGuidForLineVertexRange(0, 2, id);
            p.AddInstanceMatrix(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, id);
            Assert.IsTrue(p.ContainsTessellationId(id));
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
