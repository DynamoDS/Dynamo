using System;
using Analysis;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Visualization;
using NUnit.Framework;

namespace AnalysisTests
{
    [TestFixture]
    class LabelTests
    {
        /// <summary>
        /// This test method will execute the next methods from the Label class:
        /// private Label(Point point, string label)
        /// public static Label ByPointAndString(Point point, string label)
        /// public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        /// </summary>
        [Test, Category("UnitTests")]
        public void LabelByPointAndStringTest()
        {
            //Arrange
            var labelTest = Label.ByPointAndString(Point.ByCoordinates(100,100), "TestingLabel");
            var factory = new DefaultRenderPackageFactory();
            var package = factory.CreateRenderPackage();

            //Act
            Assert.IsNotNull(labelTest);
            labelTest.Tessellate(package, new TessellationParameters());

            //Assert
            //Validates when Point is null
            Assert.Throws<ArgumentNullException>( () => Label.ByPointAndString(null, "TestingLabel"));
            //Validates when the label string is null
            Assert.Throws<ArgumentNullException>(() => Label.ByPointAndString(Point.ByCoordinates(100, 100), null));
        }
    }
}
