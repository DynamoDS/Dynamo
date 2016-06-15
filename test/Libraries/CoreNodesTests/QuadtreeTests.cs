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
        [Test]
        public void QuadTree_FindPointsInRectangle()
        {
            var uvs = new List<UV>();
            uvs.Add(UV.ByCoordinates(0.1, 0.1));
            uvs.Add(UV.ByCoordinates(0.3, 0.03));
            uvs.Add(UV.ByCoordinates(0.7, 0.7));
            var qt = Quadtree.ByUVs(uvs);
 
            UVRect uvRect = new UVRect(UV.ByCoordinates(0,0),UV.ByCoordinates(0.5,0.5));
            var points = qt.FindPointsInRectangle(uvRect);
            Assert.AreEqual(points.Count, 2);   
        }
        [Test]
        public void UVExtensions_Area()
        {
            Assert.AreEqual(0.5,DSCore.UVExtensions.Area(UV.ByCoordinates(0,0),UV.ByCoordinates(1,0.5)));
        }
        [Test]
        public void Node_FindAllNodesUpLevel_FromRootNode()
        {
            var uvs = new List<UV>();
            uvs.Add(UV.ByCoordinates(0, 0));
            uvs.Add(UV.ByCoordinates(1, 1));
            var qt = Quadtree.ByUVs(uvs);
            var nodeList = qt.Root.FindAllNodesUpLevel(3);
            Assert.AreEqual(4, nodeList.Count);
        }
        [Test]
        public void Node_FindNodeWhichContains_GoodArgs()
        {
            var uvs = new List<UV>();
            uvs.Add(UV.ByCoordinates(0, 0));
            uvs.Add(UV.ByCoordinates(1, 1));
            uvs.Add(UV.ByCoordinates(0.2, 0.2));
            var qt = Quadtree.ByUVs(uvs);
            var node = qt.Root.FindNodeWhichContains(UV.ByCoordinates(0.2, 0.2));
            Assert.AreEqual(0.2, node.Point.U);
            Assert.AreEqual(0.2, node.Point.V);

        }
        [Test]
        public void Node_TryFind_GoodArgs()
        {
            var uvs = new List<UV>();
            uvs.Add(UV.ByCoordinates(0, 0));
            uvs.Add(UV.ByCoordinates(1, 1));
            uvs.Add(UV.ByCoordinates(0.2, 0.2));
            var qt = Quadtree.ByUVs(uvs);
            Node node;
            Assert.IsTrue( qt.Root.TryFind(UV.ByCoordinates(0.2, 0.2),out node));
            Assert.AreEqual(0.2, node.Point.U);
            Assert.AreEqual(0.2, node.Point.V);
        }
        [Test]
        public void Node_TryFind_BadArgs()
        {
            var uvs = new List<UV>();
            uvs.Add(UV.ByCoordinates(0, 0));
            uvs.Add(UV.ByCoordinates(1, 1));
            uvs.Add(UV.ByCoordinates(0.2, 0.2));
            var qt = Quadtree.ByUVs(uvs);
            Node node;
            Assert.IsFalse(qt.Root.TryFind(UV.ByCoordinates(-1,-1), out node));
            Assert.IsNull(node);
        }
        [Test]
        public void Node_FindNodeWhichContains_BadArgs()
        {
            var uvs = new List<UV>();
            uvs.Add(UV.ByCoordinates(0, 0));
            uvs.Add(UV.ByCoordinates(0.2, 0.2));
            var qt = Quadtree.ByUVs(uvs);
            Assert.IsNull(qt.Root.FindNodeWhichContains(UV.ByCoordinates(-1, -1)));
        }

        [Test]
        public void Node_FindAllNodesUpLevel_FromLeafNode()
        {
            var uvs = new List<UV>();
            uvs.Add(UV.ByCoordinates(0, 0));
            uvs.Add(UV.ByCoordinates(1, 1));
            var qt = Quadtree.ByUVs(uvs);
            var neNode = qt.Root.NE;
            var nodeList = neNode.FindAllNodesUpLevel(3);
            Assert.AreEqual(4, nodeList.Count);
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
