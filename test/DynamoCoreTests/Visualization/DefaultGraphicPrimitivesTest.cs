using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using NUnit.Framework;

namespace Dynamo.Tests.Visualization
{
    [TestFixture]
    class DefaultGraphicPrimitivesTest
    {
        /// <summary>
        /// This test method will execute the DefaultGraphicPrimitives constructor
        /// public DefaultGraphicPrimitives(IRenderPackage package)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void DefaultGraphicPrimitives_Constructor()
        {
            //Arrange
            var data = new List<IGraphicPrimitives>();
            var renderingFactory = new DefaultRenderPackageFactory();
            List<IRenderPackage> packages = new List<IRenderPackage>();
            packages.Add(renderingFactory.CreateRenderPackage());

            //Act
            var render = new DefaultRenderPackage();
            render.SetColors(WhiteByteArrayOfLength(2));//This will set the Colors property

            foreach (var package in packages)
            {
                data.Add(new DefaultGraphicPrimitives(render));
            }

            //Assert
            Assert.IsNotNull(render);
            Assert.AreEqual((render.Colors as byte[]).Length, 8);
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
