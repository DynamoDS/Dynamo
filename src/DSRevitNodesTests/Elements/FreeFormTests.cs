using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using DSRevitNodes;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{

    [TestFixture]
    public class FreeFormTests
    {
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
                BSplineCurve.ByPoints(pts1),
                BSplineCurve.ByPoints(pts2),
                BSplineCurve.ByPoints(pts3)
            };

            var dir = Vector.ByCoordinates(0, 0, 1);
            var dist = 5;

            // construct the extrusion
            var extrusion = DSSolid.ByExtrusion(crvs, dir, dist);
            Assert.NotNull(extrusion);

            // construct the freeform element
            var freeForm = DSFreeForm.BySolid(extrusion);
            Assert.NotNull(freeForm);

        }

        [Test]
        public void BySolid_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => DSFreeForm.BySolid(null));
        }

    }

}
