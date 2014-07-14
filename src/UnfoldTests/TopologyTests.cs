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


    public class HostSetupTest
    {
        [Test]
        public void HostSimple()
        {
            Assert.DoesNotThrow(() => HostFactory.Instance.StartUp());

            Assert.DoesNotThrow(() => HostFactory.Instance.ShutDown());
        }

    }



    public class TopologyTests : HostFactorySetup
    {
        #region TestUtilities
        public static Solid SetupCube()
        {
            var rect = Rectangle.ByWidthHeight(1, 1);
            return rect.ExtrudeAsSolid(1);
        }

        public static void GraphHasCorrectNumberOfEdges<K, T>(int expectedEdges, List<UnfoldPlanar.GraphVertex<K, T>> graph)
            where K : IUnfoldEdge
            where T : IUnfoldPlanarFace<K>
        {

            List<UnfoldPlanar.GraphEdge<K, T>> alledges = new List<UnfoldPlanar.GraphEdge<K, T>>();

            foreach (UnfoldPlanar.GraphVertex<K, T> vertex in graph)
            {
                foreach (UnfoldPlanar.GraphEdge<K, T> graphedge in vertex.Graph_Edges)
                {

                    alledges.Add(graphedge);
                }
            }

            Assert.AreEqual(expectedEdges, alledges.Count);
            Console.WriteLine("correct number of edges");

        }

        public static void IsOneStronglyConnectedGraph<K, T>(List<List<UnfoldPlanar.GraphVertex<K, T>>> sccs)
            where K : IUnfoldEdge
            where T : IUnfoldPlanarFace<K>
        {
            Assert.AreEqual(sccs.Count, 1);
            Console.WriteLine("This graph is one strongly connected component");
        }

        public static void IsAcylic<K, T>(List<List<UnfoldPlanar.GraphVertex<K, T>>> sccs, List<UnfoldPlanar.GraphVertex<K, T>> graph)
            where K : IUnfoldEdge
            where T : IUnfoldPlanarFace<K>
        {
            Console.WriteLine(graph.Count);
            Assert.AreEqual(graph.Count, sccs.Count);
            Console.WriteLine(" This graph is acyclic, each vertex is its own strongly connected comp");
        }

        public static void GraphHasVertForEachFace<K, T>(List<UnfoldPlanar.GraphVertex<K, T>> graph, List<Object> faces)
            where T : IUnfoldPlanarFace<K>
            where K : IUnfoldEdge
        {

            Assert.AreEqual(graph.Count, faces.Count);
            Console.WriteLine("same number of faces as verts");
            foreach (var vertex in graph)
            {
                var originalface = vertex.Face.OriginalEntity;
                Assert.Contains(originalface, faces);
            }

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



            Assert.AreEqual(Math.Abs(dotpro), 1);
            Console.WriteLine(dotpro);
            Console.WriteLine("was parallel");
            Console.WriteLine(cross1);
            Console.WriteLine(cross2);

        }

        public static void AssertRotatedSurfacesDoNotShareSameCenter(Surface surf1, Surface surf2)
        {


            var center1 = surf1.PointAtParameter(.5, .5);
            var center2 = surf2.PointAtParameter(.5, .5);


            Assert.IsFalse(center1.IsAlmostEqualTo(center2));

            Console.WriteLine("centers were not the same");
            Console.WriteLine(center1);
            Console.WriteLine(center2);
        }

        #endregion


        [TestFixture]
        public class InitialGraphTests
        {

            [Test]
            public void GraphCanBeGeneratedFromCubeFaces()
            {
                Solid testcube = SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                Assert.AreEqual(faces.Count, 6);

                var graph = UnfoldPlanar.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> face_objs = faces.Select(x => x as Object).ToList();

                GraphHasVertForEachFace(graph, face_objs);

                GraphHasCorrectNumberOfEdges(24, graph);

                var sccs = GraphUtilities.tarjansAlgo<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>.CycleDetect(graph);

                IsOneStronglyConnectedGraph(sccs);
                //
            }


            [Test]
            public void GraphCanBeGeneratedFromCubeSurfaces()
            {


                Solid testcube = SetupCube();
                List<Surface> surfaces = testcube.Faces.Select(x => x.SurfaceGeometry()).ToList();

                Assert.AreEqual(surfaces.Count, 6);

                var graph = UnfoldPlanar.ModelTopology.GenerateTopologyFromSurfaces(surfaces);

                List<Object> face_objs = surfaces.Select(x => x as Object).ToList();

                GraphHasVertForEachFace(graph, face_objs);

                GraphHasCorrectNumberOfEdges(24, graph);


                //
            }

        }
        [TestFixture]
        public class BFSTreeTests
        {





            [Test]
            public void GenBFSTreeFromCubeFaces()
            {

                Solid testcube = SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                var graph = UnfoldPlanar.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> face_objs = faces.Select(x => x as Object).ToList();


                GraphHasVertForEachFace(graph, face_objs);

                GraphHasCorrectNumberOfEdges(24, graph);

                var nodereturn = UnfoldPlanar.ModelGraph.BFS<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<UnfoldPlanar.GraphVertex<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>>;

                GraphHasVertForEachFace(casttree, face_objs);
                GraphHasCorrectNumberOfEdges(5, casttree);


                var sccs = GraphUtilities.tarjansAlgo<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>.CycleDetect(casttree);

                IsAcylic<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>(sccs, casttree);

            }


            [Test]
            public void TreeIsAcyclic()
            {
                //
            }

            [Test]
            public void TreeContainsAllFaces()
            {
                //
            }

            [Test]
            public void TreeContainsNoRepeatEdges()
            {
                //
            }

            [Test]
            public void AllFinishingTimesSet()
            {
                //
            }

        }


        [TestFixture]
        public class AlignPlanarTests
        {
            [Test]
            public void UnfoldEachPairOfFacesInACube_ParentAsRefFace()
            {


                Solid testcube = SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                //generate a graph of the cube
                var graph = UnfoldPlanar.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> face_objs = faces.Select(x => x as Object).ToList();



                //perform BFS on the graph and get back the tree
                var nodereturn = UnfoldPlanar.ModelGraph.BFS<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<UnfoldPlanar.GraphVertex<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>>;
                //perform tarjans algo and make sure that the tree is acylic before unfold
                var sccs = GraphUtilities.tarjansAlgo<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>.CycleDetect(casttree);

                IsAcylic<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>(sccs, casttree);

                // iterate through each vertex in the tree
                // make sure that the parent/child is not null (depends which direction we're traversing)
                // if not null, grab the next node and the tree edge
                // pass these to check normal consistencey and align.
                // be careful about the order of passed faces

                foreach (var parent in casttree)
                {
                    if (parent.Graph_Edges.Count > 0)
                    {
                        foreach (var edge in parent.Graph_Edges)
                        {

                            var child = edge.Head;




                            int nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.Real_Edge);
                            Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.Face, parent.Face, edge.Real_Edge) as Surface;

                            AssertSurfacesAreCoplanar(rotatedFace, parent.Face.SurfaceEntity);

                            AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace, parent.Face.SurfaceEntity);

                        }

                    }


                }

            }


            [Test]
            public void UnfoldEachPairOfFacesInACube_ChildAsRefFace()
            {


                Solid testcube = SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                //generate a graph of the cube
                var graph = UnfoldPlanar.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> face_objs = faces.Select(x => x as Object).ToList();



                //perform BFS on the graph and get back the tree
                var nodereturn = UnfoldPlanar.ModelGraph.BFS<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<UnfoldPlanar.GraphVertex<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>>;
                //perform tarjans algo and make sure that the tree is acylic before unfold
                var sccs = GraphUtilities.tarjansAlgo<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>.CycleDetect(casttree);

                IsAcylic<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>(sccs, casttree);

                // iterate through each vertex in the tree
                // make sure that the parent/child is not null (depends which direction we're traversing)
                // if not null, grab the next node and the tree edge
                // pass these to check normal consistencey and align.
                // be careful about the order of passed faces

                foreach (var parent in casttree)
                {
                    if (parent.Graph_Edges.Count > 0)
                    {
                        foreach (var edge in parent.Graph_Edges)
                        {

                            var child = edge.Head;


                            int nc = AlignPlanarFaces.CheckNormalConsistency(parent.Face, child.Face, edge.Real_Edge);
                            Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, parent.Face, child.Face, edge.Real_Edge) as Surface;

                            AssertSurfacesAreCoplanar(rotatedFace, child.Face.SurfaceEntity);

                            AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace, child.Face.SurfaceEntity);

                        }

                    }


                }

            }




        }
    }
}