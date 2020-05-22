using System.Collections.Generic;
using Dynamo.Visualization;
using NUnit.Framework;

namespace Dynamo.Tests.Visualization
{
    [TestFixture]
    class DefaultRenderPackageTest
    {
        /// <summary>
        /// This test will execute the next methods from the DefaultRenderPackage class
        ///  public void ApplyPointVertexColors(byte[] colors)
        ///  public void ApplyLineVertexColors(byte[] colors)
        ///  public void SetColors(byte[] colors)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void VertexColorsTest()
        {
            //Arrange
            var render = new DefaultRenderPackage();

            //Act
            render.ApplyPointVertexColors(WhiteByteArrayOfLength(3));
            render.ApplyLineVertexColors(WhiteByteArrayOfLength(2));
            render.SetColors(WhiteByteArrayOfLength(2));

            //Assert
            //Checking that the properties were populated correctly
            Assert.AreEqual((render.PointVertexColors as List<byte>).Count,12);
            Assert.AreEqual((render.LineStripVertexColors as List<byte>).Count,8);
            Assert.AreEqual((render.Colors as byte[]).Length,8);

            render.Clear();//Empty some properties of the DefaultRenderPackage class

            //Assert
            //Checking that the properties were cleaned
            Assert.AreEqual((render.PointVertexColors as List<byte>).Count,0);
            Assert.AreEqual((render.LineStripVertexColors as List<byte>).Count,0);

        }

        /// <summary>
        /// This test method will validate the next properties from the DefaultRenderPackage class:
        /// HasRenderingData
        /// LineStripIndices
        /// MeshIndices
        /// MeshTextureCoordinates
        /// PointIndices
        /// ColorsStride
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void DefaultRenderPackage_Properties()
        {
            //Arrange
            var render = new DefaultRenderPackage();

            //Act
            //This will populate the HasRenderingData property
            render.ApplyPointVertexColors(WhiteByteArrayOfLength(3));

            //This will populate the LineStripIndices property
            render.AddLineStripVertex(1, 2, 5);

            //This will populate the MeshIndices property
            render.AddTriangleVertex(1,4,5);

            //This will populate the MeshTextureCoordinates property
            render.AddTriangleVertexUV(2,5);

            //This will populate the PointIndices property
            render.AddPointVertex(3, 6, 4);

            //Assert
            //Checking that the properties have the right value
            Assert.IsTrue(render.HasRenderingData);
            Assert.AreEqual((render.LineStripIndices as List<int>).Count, 1);
            Assert.AreEqual((render.MeshIndices as List<int>).Count,1);
            Assert.AreEqual((render.MeshTextureCoordinates as List<double>).Count,2);
            Assert.AreEqual((render.PointIndices as List<int>).Count,1);
            Assert.AreEqual(render.ColorsStride,0);
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
}
