using System;
using System.Collections.Generic;
using System.Diagnostics;

using Autodesk.DesignScript.Geometry;

using DSCore;

using NUnit.Framework;

using TestServices;

using Math = DSCore.Math;

namespace DSCoreNodesTests
{
    [TestFixture]
    class QuadtreeTests : GeometricTestBase
    {
        [Test]
        public void QuadtreeByUVs_GoodArgs()
        {
            var sw = new Stopwatch();
            sw.Start();
            var qt = Quadtree.ByUVs(SetupSampleUVs());
            Assert.NotNull(qt);
            sw.Stop();
            Console.WriteLine("{0} ellapsed for creating quadtree.", sw.Elapsed);
        }

        [Test]
        public void QuadtreeByUVs_BadArgs()
        {
            Assert.Throws<ArgumentNullException>(() => Quadtree.ByUVs(null));
            Assert.Throws<ArgumentException>(() => Quadtree.ByUVs(new UV[] { }));
        }

        [Test]
        public void Quadtree_FindPointsWithinRadius_GoodArgs()
        {
            var uvs = new List<UV>();
            uvs.AddRange(SetupSampleUVs());
            uvs.Add(UV.ByCoordinates(0.01,0.01)); // ensure at least one UV in range
            var qt = Quadtree.ByUVs(uvs);
            Assert.NotNull(qt);

            var foundUVs = qt.FindPointsWithinRadius(UV.ByCoordinates(), .25);
            Assert.Greater(foundUVs.Count, 1);
        }

        [Test]
        public void Quadtree_FindPointsWithinRadius_BadArgs()
        {
            var uvs = new List<UV>();
            uvs.AddRange(SetupSampleUVs());
            uvs.Add(UV.ByCoordinates(0.01, 0.01)); // ensure at least one UV in range
            var qt = Quadtree.ByUVs(uvs);
            Assert.NotNull(qt);

            Assert.Throws<ArgumentNullException>(
                () => qt.FindPointsWithinRadius(null, 0.25));
            Assert.Throws<ArgumentException>(
                () => qt.FindPointsWithinRadius(UV.ByCoordinates(), 0.0));
        }

        private static IList<UV> SetupSampleUVs()
        {
            var uvs = new List<UV>();
            for (int i = 0; i < 100; i++)
            {
                uvs.Add(UV.ByCoordinates(Math.Rand(), Math.Rand()));
            }

            return uvs;
        } 
    }
}
