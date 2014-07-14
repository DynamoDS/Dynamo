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



        [TestFixture]
        public class InitialGraphTests
        {

           




            [Test]
            public  void GraphCanBeGeneratedFromCubeFaces() 
            {
                Solid testcube = SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                
                Assert.AreEqual(faces.Count, 6);

               var graph = UnfoldPlanar.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> face_objs = faces.Select(x => x as Object).ToList();

                GraphHasVertForEachFace(graph, face_objs);

                GraphHasCorrectNumberOfEdges(24, graph);
                
                var sccs = GraphUtilities.tarjansAlgo<UnfoldPlanar.EdgeLikeEntity,UnfoldPlanar.FaceLikeEntity>.CycleDetect(graph);

                IsOneStronglyConnectedGraph(sccs);
                //
            }


            [Test]
            public  void GraphCanBeGeneratedFromCubeSurfaces()
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

                IsAcylic<UnfoldPlanar.EdgeLikeEntity, UnfoldPlanar.FaceLikeEntity>(sccs,casttree);

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


    }
}