using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using NUnit.Framework;
using Autodesk.DesignScript.Interfaces;
using Unfold;

namespace UnfoldTests
{
    [TestFixture]
    public static class TopologyTests
    {

        public class InitialGraphTests
        {

            public Solid SetupCube() 
            {
               var rect = Rectangle.ByWidthHeight(1,1);
               return rect.ExtrudeAsSolid(1);
            }





            [Test]
            public void GraphCanBeGeneratedFromFaces()
            {
                HostFactory.Instance.StartUp();
                Solid testcube =  SetupCube();
                List<Face> faces  = testcube.Faces.ToList();

                Assert.AreEqual(faces.Count,6);

               List<Unfold_Planar.graph_vertex>  graph =  Unfold_Planar.ModelTopology.GenerateTopologyFromFaces(faces);

               List<Object> face_objs = faces.Select(x => x as Object).ToList();
               
                GraphHasVertForEachFace(graph, face_objs);

               HostFactory.Instance.ShutDown();
                //
            }
            

             [Test]
            public void GraphCanBeGeneratedFromSurfaces()
            {

                HostFactory.Instance.StartUp();
                Solid testcube = SetupCube();
                List<Surface> surfaces = testcube.Faces.Select(x => x.SurfaceGeometry()).ToList();

                Assert.AreEqual(surfaces.Count, 6);

                List<Unfold_Planar.graph_vertex> graph = Unfold_Planar.ModelTopology.GenerateTopologyFromSurfaces(surfaces);

                List<Object> face_objs = surfaces.Select(x => x as Object).ToList();

                GraphHasVertForEachFace(graph, face_objs);

                HostFactory.Instance.ShutDown();
                //
            }

           
            public void GraphHasVertForEachFace(List<Unfold_Planar.graph_vertex> graph, List<Object> faces)
            {

                Assert.AreEqual(graph.Count, faces.Count);

                foreach (var vertex in graph)
                {
                    var originalface = vertex.Face.OriginalEntity;
                    Assert.Contains(originalface, faces);
                }

            }

           
            public void EveryFaceIsReachable()
            {
                //
            }
           
            


        }

        public class BFSTreeTests
        {
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
