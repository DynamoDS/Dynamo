using System;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using NUnit.Framework;
using Solid = Revit.Elements.Solid;

namespace DSRevitNodesTests.Elements
{

    [TestFixture]
    public class FreeFormTests
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
        public void BySolid_ValidArgs()
        {
            // construct a triangle
            var pts1 = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(0.4,0,0),
                Point.ByCoordinates(0.8,0,0),
                Point.ByCoordinates(1,0,0),
            };

            var pts2 = new[]
            {
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(1,0.4,0),
                Point.ByCoordinates(1,0.8,0),
                Point.ByCoordinates(1,1,0)
            };

            var pts3 = new[]
            {
                Point.ByCoordinates(1,1,0),
                Point.ByCoordinates(0.8,0.8,0),
                Point.ByCoordinates(0.4,0.4,0),
                Point.ByCoordinates(0.0,0,0)
            };

            var crvs = new[]
            {
                NurbsCurve.ByPoints(pts1),
                NurbsCurve.ByPoints(pts2),
                NurbsCurve.ByPoints(pts3)
            };

            var dir = Vector.ByCoordinates(0, 0, 1);
            var dist = 5;

            // construct the extrusion
            var extrusion = Solid.ByExtrusion(crvs, dir, dist);
            Assert.NotNull(extrusion);

            // construct the freeform element
            var freeForm = FreeForm.BySolid(extrusion);
            Assert.NotNull(freeForm);

        }

        [Test]
        public void BySolid_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => FreeForm.BySolid(null));
        }

    }

}
