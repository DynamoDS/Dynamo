using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Revit.AnalysisDisplay;
using Revit.Elements;
using NUnit.Framework;
using RevitServices.Persistence;

namespace DSRevitNodesTests.Elements
{
    [TestFixture]
    public class ModelTextTests
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
        public void ByTextSketchPlaneAndPosition_ValidArgs()
        {
            var origin = Point.ByCoordinates(1, 2, 3);
            var normal = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByOriginNormal(origin, normal);
            var text = "Snickers - why wait?";

            var name = "Model Text 1";
            var modelTextType = ModelTextType.ByName(name);

            var sketchPlane = SketchPlane.ByPlane(plane);

            var structure = ModelText.ByTextSketchPlaneAndPosition(text, sketchPlane, 0, 0, 1, modelTextType);

            Assert.NotNull(structure);
            Assert.NotNull(structure.InternalElement);
            Assert.IsTrue(DocumentManager.Instance.ElementExistsInDocument(
                 new ElementUUID( structure.InternalElement.UniqueId)));
        }

        [Test]
        public void ByTextSketchPlaneAndPosition_BadArgs()
        {
            var origin = Point.ByCoordinates(1, 2, 3);
            var normal = Vector.ByCoordinates(0, 0, 1);
            var plane = Plane.ByOriginNormal(origin, normal);
            var text = "Snickers - why wait?";

            var name = "Model Text 1";
            var modelTextType = ModelTextType.ByName(name);

            var sketchPlane = SketchPlane.ByPlane(plane);

            Assert.Throws(typeof(System.ArgumentNullException), () => ModelText.ByTextSketchPlaneAndPosition(null, sketchPlane, 0, 0, 1, modelTextType));
            Assert.Throws(typeof(System.ArgumentNullException), () => ModelText.ByTextSketchPlaneAndPosition(text, null, 0, 0, 1, modelTextType));
            Assert.Throws(typeof(System.ArgumentNullException), () => ModelText.ByTextSketchPlaneAndPosition(text, sketchPlane, 0, 0, 1, null));

        }

    }
}
