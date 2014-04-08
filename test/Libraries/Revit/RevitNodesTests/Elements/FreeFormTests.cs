using System;
using Autodesk.DesignScript.Geometry;
using Dynamo.Tests;
using Revit.Elements;
using NUnit.Framework;

namespace DSRevitNodesTests.Elements
{

    [TestFixture]
    public class FreeFormTests : RevitNodeTestBase
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
        [TestModel(@".\empty.rfa")]
        public void BySolid_ValidArgs()
        {
            // construct a triangle
            var pts1 = new[]
            {
                Point.ByCoordinates(0,0,0),
                Point.ByCoordinates(1,0,0),
            };

            var pts2 = new[]
            {
                Point.ByCoordinates(1,0,0),
                Point.ByCoordinates(1,1,0)
            };

            var pts3 = new[]
            {
                Point.ByCoordinates(1,1,0),
                Point.ByCoordinates(0.0,0,0)
            };

            var crvs = new[]
            {
                Line.ByStartPointEndPoint(pts1[0], pts1[1]),
                Line.ByStartPointEndPoint(pts2[0], pts2[1]),
                Line.ByStartPointEndPoint(pts3[0], pts3[1]),
            };

            var dir = Vector.ByCoordinates(0, 0, 1);
            var dist = 5;

            // construct the extrusion
            var extrusion = Revit.GeometryObjects.Solid.ByExtrusion(crvs, dir, dist);
            Assert.NotNull(extrusion);

            // construct the freeform element
            var freeForm = FreeForm.BySolid(extrusion);
            Assert.NotNull(freeForm);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void BySolid_NullArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => FreeForm.BySolid(null));
        }

    }

}
