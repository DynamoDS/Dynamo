using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using Autodesk.DesignScript.Interfaces;
using Unfold;
using System.Threading;
using Unfold.Interfaces;

namespace UnfoldTests
{
    public class  UnfoldTestUtils : HostFactorySetup
    {
        #region TestUtilities
        public static Solid SetupCube()
        {
            var rect = Rectangle.ByWidthHeight(1, 1);
            return rect.ExtrudeAsSolid(1);
        }

        public static Solid SetupLargeCone()
        {
            var cone = Cone.ByPointsRadius(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 0, 8), 15);
            return cone;
        }

        public static Solid SetupTallCone()
        {
            var cone = Cone.ByPointsRadius(Point.ByCoordinates(0, 0, 0), Point.ByCoordinates(0, 0, 8), 4);
            return cone;
        }

        public static void GraphHasCorrectNumberOfEdges<K, T>(int expectedEdges, List<GeneratePlanarUnfold.GraphVertex<K, T>> graph)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {

            var alledges = GraphUtilities.GetAllGraphEdges(graph);

            Assert.AreEqual(expectedEdges, alledges.Count);
            Console.WriteLine("correct number of edges");

        }

        public static void IsOneStronglyConnectedGraph<K, T>(List<List<GeneratePlanarUnfold.GraphVertex<K, T>>> sccs)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            Assert.AreEqual(sccs.Count, 1);
            Console.WriteLine("This graph is one strongly connected component");
        }

        public static void IsAcylic<K, T>(List<List<GeneratePlanarUnfold.GraphVertex<K, T>>> sccs, List<GeneratePlanarUnfold.GraphVertex<K, T>> graph)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {
            Console.WriteLine(graph.Count);
            Assert.AreEqual(graph.Count, sccs.Count);
            Console.WriteLine(" This graph is acyclic, each vertex is its own strongly connected comp");
        }

        public static void GraphHasVertForEachFace<K, T>(List<GeneratePlanarUnfold.GraphVertex<K, T>> graph, List<Object> faces)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {

            Assert.AreEqual(graph.Count, faces.Count);
            Console.WriteLine("same number of faces as verts");
            foreach (var vertex in graph)
            {
                var originalface = vertex.Face.OriginalEntity;
                Assert.Contains(originalface, faces);
            }

        }

        public static void AssertEdgeCoincidentWithBothFaces<K, T>(T face1, T face2, K edge)
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {
            var intersected = face1.SurfaceEntity.Intersect(face2.SurfaceEntity).ToList();

            bool foundflag = false;
            foreach (var geo in intersected)
            {
                if (geo is Curve)
                {
                    Curve geoAsCurve = geo as Curve;
                    var edgeLikeRepresentation = new GeneratePlanarUnfold.EdgeLikeEntity(geoAsCurve);

                    if (edgeLikeRepresentation.SpatialEquals(edge))
                    {
                        foundflag = true;
                    }

                }
            }

            Assert.IsTrue(foundflag);
            Console.WriteLine("the edge input was a shared edge between the 2 surfaces");
        }

        public static void AssertSurfacesAreCoplanar(Surface surf1, Surface surf2)
        {
            // 3 random points on each surface
            //assumption that domain of surface is [0,1]
            Random random = new Random();


            var A = surf1.PointAtParameter(random.NextDouble(), random.NextDouble());
            var B = surf1.PointAtParameter(random.NextDouble(), random.NextDouble());
            var C = surf1.PointAtParameter(random.NextDouble(), random.NextDouble());

            var D = surf2.PointAtParameter(random.NextDouble(), random.NextDouble());
            var E = surf2.PointAtParameter(random.NextDouble(), random.NextDouble());
            var F = surf2.PointAtParameter(random.NextDouble(), random.NextDouble());

            // generate 2 vectors for each surface

            var AB = Vector.ByTwoPoints(A, B).Normalized();
            var BC = Vector.ByTwoPoints(B, C).Normalized();

            var DE = Vector.ByTwoPoints(D, E).Normalized();
            var EF = Vector.ByTwoPoints(E, F).Normalized();

            var cross1 = AB.Cross(BC).Normalized();

            var cross2 = DE.Cross(EF).Normalized();

            var dotpro = cross1.Dot(cross2);

            Console.WriteLine(dotpro);

            Assert.IsTrue(Math.Abs(Math.Abs(dotpro) - 1) < .0001);
            Console.WriteLine(dotpro);
            Console.WriteLine("was parallel");
            Console.WriteLine(cross1);
            Console.WriteLine(cross2);

        }

        public static void AssertRotatedSurfacesDoNotShareSameCenter(Surface surf1, Surface surf2)
        {


            //var center1 = surf1.PointAtParameter(.5, .5);
            //var center2 = surf2.PointAtParameter(.5, .5);

            //solution to fnding centers of of trimmed surfaces like triangles as polygon projections
            var center1 = Tessellate.MeshHelpers.SurfaceAsPolygonCenter(surf1);
            var center2 = Tessellate.MeshHelpers.SurfaceAsPolygonCenter(surf2);

            Console.WriteLine(center1);
            Console.WriteLine(center2);

            Assert.IsFalse(center1.IsAlmostEqualTo(center2));



            Console.WriteLine("centers were not the same");

        }


        public static void AssertAllFinishingTimesSet<K, T>(List<GeneratePlanarUnfold.GraphVertex<K, T>> graph)
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {

            foreach (var vert in graph)
            {

                Assert.NotNull(vert.FinishTime);

            }


            Console.WriteLine("all nodes have been visited by BFS or acted as a root");

        }

        public static void AssertNoSurfaceIntersections(PolySurface TestPolysurface)
        {
            
             var SubSurfaces = (TestPolysurface as PolySurface).Surfaces().ToList();
            
           
            // perfrom the intersection test

            bool overlapflag = false;
            foreach (var surfaceToIntersect in SubSurfaces)
            {
                foreach (var rotsubface in SubSurfaces)
                {
                   
                    if (surfaceToIntersect.Equals(rotsubface))
                    {
                        continue;
                    }
                    var resultantGeo = surfaceToIntersect.Intersect(rotsubface);
                    foreach (var geo in resultantGeo)
                    {
                        if (geo is Surface)
                        {
                            overlapflag = true;
                        }
                    }
                }
            }
            Assert.IsFalse(overlapflag);
        }

        public delegate void Condition(Surface s1, Surface s2);
        public static void AssertConditionForEverySurfaceAgainstEverySurface(PolySurface TestPolysurface, Condition condition)
        {


            var SubSurfaces = (TestPolysurface as PolySurface).Surfaces().ToList();


            // perfrom the intersection test

           
            foreach (var surfaceToIntersect in SubSurfaces)
            {
                foreach (var rotsubface in SubSurfaces)
                {

                    if (surfaceToIntersect.Equals(rotsubface))
                    {
                        continue;
                    }
                    condition(surfaceToIntersect, rotsubface);
                    
                }
            }
            
        }
        #endregion

    }
}
