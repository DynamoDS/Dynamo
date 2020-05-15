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
            Assert.AreEqual((render.PointVertexColors as List<byte>).Count,12);
            Assert.AreEqual((render.LineStripVertexColors as List<byte>).Count,8);
            Assert.AreEqual((render.Colors as byte[]).Length,8);

            render.Clear();

            //Assert
            Assert.AreEqual((render.PointVertexColors as List<byte>).Count,0);
            Assert.AreEqual((render.LineStripVertexColors as List<byte>).Count,0);

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
