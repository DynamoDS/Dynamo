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

namespace UnfoldTests
{

    public static class GraphUtilities
    {


        public static List<UnfoldPlanar.graph_vertex> CloneGraph(List<UnfoldPlanar.graph_vertex> graph)
             
        {
            ///this is really a clone method in next 3 for loops
                //create new verts and store them in a new list
                List<UnfoldPlanar.graph_vertex> graph_to_traverse = new List<UnfoldPlanar.graph_vertex>();
                foreach (UnfoldPlanar.graph_vertex vert_to_copy in graph)
                {
                    var vert = new UnfoldPlanar.graph_vertex(vert_to_copy.Face);
                    graph_to_traverse.Add(vert);
                }

                
                // build the rest of the graphcopy - set the other properties of the verts correctly
                foreach (UnfoldPlanar.graph_vertex vert_to_copy in graph)
                {   
                    List<UnfoldPlanar.graph_vertex> vertlist = Unfold.UnfoldPlanar.find_nodes_by_matching_faces(graph_to_traverse, new List<UnfoldPlanar.FaceLikeEntity>() { vert_to_copy.Face });
                    UnfoldPlanar.graph_vertex vert = vertlist[0];
                    vert.Explored = false;
                    vert.Finish_Time = 1000000000;//infin
                    vert.Parent = null;

                    foreach (Unfold.UnfoldPlanar.graph_edge edge_to_copy in vert_to_copy.Graph_Edges)
                    {

                        // find the same faces in the new graph, the nodes that represent these faces...// but we must make sure that these nodes
                        // that are returned are the ones inside the new graph
                        // may make sense to add a property that either is a name , id, or graph owner ...
                        List<UnfoldPlanar.graph_vertex> newtail = UnfoldPlanar.find_nodes_by_matching_faces(graph_to_traverse, new List<UnfoldPlanar.FaceLikeEntity>() { edge_to_copy.Tail.Face });
                        List<UnfoldPlanar.graph_vertex> newhead = UnfoldPlanar.find_nodes_by_matching_faces(graph_to_traverse, new List<UnfoldPlanar.FaceLikeEntity>() { edge_to_copy.Head.Face });
                        UnfoldPlanar.graph_edge edge = new UnfoldPlanar.graph_edge(edge_to_copy.Real_Edge, newtail[0], newhead[0]);
                        vert.Graph_Edges.Add(edge);
                    }



                }

            return graph_to_traverse;
        }

       //   http://stackoverflow.com/questions/6643076/tarjan-cycle-detection-help-c-sharp
     //  http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        public static class tarjansAlgo
        {

        static int Index;
        static Stack<UnfoldPlanar.graph_vertex> VertStack;
        static   List<UnfoldPlanar.graph_vertex> graphcopy;
        static List<List<UnfoldPlanar.graph_vertex>> stronglyConnectedComponents;

      
        //use for cycle detection to assert that the BFS tree has no cycles and is a tree
        public static List<List<UnfoldPlanar.graph_vertex>> CycleDetect(List<UnfoldPlanar.graph_vertex> graph)
        
        {
          
           var GraphWithTags = CloneGraph(graph);
         foreach (var vert in GraphWithTags) 
            {
               // initialize to -1
                vert.Index = -1;
             vert.LowLink = -1;
            }

             stronglyConnectedComponents = new List<List<UnfoldPlanar.graph_vertex>>();
             Index = 0;
            VertStack = new Stack<UnfoldPlanar.graph_vertex>();
           
            graphcopy = GraphWithTags;
            foreach (var vert in GraphWithTags) 
            {
               if(vert.Index <0)
               
               {
                   checkStrongConnect(vert);
               
               }
         
           }
        return stronglyConnectedComponents;
        }
        
            private static void checkStrongConnect(UnfoldPlanar.graph_vertex vertex){
                vertex.Index = Index;
                vertex.LowLink = Index;
                Index = Index + 1;
                VertStack.Push(vertex);

                var adjlist = vertex.Graph_Edges.Select(x=>x.Head).ToList();

                foreach (UnfoldPlanar.graph_vertex AdjVert in adjlist)
                {

                    if (AdjVert.Index < 0)
                    {
                        checkStrongConnect(AdjVert);
                        vertex.LowLink = Math.Min(vertex.LowLink, AdjVert.LowLink);

                    }

                    else if (VertStack.Contains(AdjVert))
                    {
                        vertex.LowLink = Math.Min(vertex.LowLink, AdjVert.Index);


                    }

                }

                if (vertex.LowLink == vertex.Index)
                {

                    var components = new List<UnfoldPlanar.graph_vertex>();
                    UnfoldPlanar.graph_vertex X;

                    do
                    {

                        X = VertStack.Pop();
                        components.Add(X);
                    } while (vertex != X);
                    
                        stronglyConnectedComponents.Add(components);
                    

                    

                }

            }

        }

    }
    

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

        [TestFixture]
        public class InitialGraphTests
        {

            public Solid SetupCube()
            {
                var rect = Rectangle.ByWidthHeight(1, 1);
                return rect.ExtrudeAsSolid(1);
            }





            [Test]
            public  void GraphCanBeGeneratedFromFaces()
            {
                Solid testcube = SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                
                Assert.AreEqual(faces.Count, 6);

                List<UnfoldPlanar.graph_vertex> graph = UnfoldPlanar.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> face_objs = faces.Select(x => x as Object).ToList();

                GraphHasVertForEachFace(graph, face_objs);

                GraphHasCorrectNumberOfEdges(24, graph);
                
                var sccs = GraphUtilities.tarjansAlgo.CycleDetect(graph);
                //
            }


            [Test]
            public  void GraphCanBeGeneratedFromSurfaces()
            {


                Solid testcube = SetupCube();
                List<Surface> surfaces = testcube.Faces.Select(x => x.SurfaceGeometry()).ToList();

                Assert.AreEqual(surfaces.Count, 6);

                List<UnfoldPlanar.graph_vertex> graph = UnfoldPlanar.ModelTopology.GenerateTopologyFromSurfaces(surfaces);

                List<Object> face_objs = surfaces.Select(x => x as Object).ToList();

                GraphHasVertForEachFace(graph, face_objs);

                GraphHasCorrectNumberOfEdges(24, graph);

                //
            }


            public  void GraphHasVertForEachFace(List<UnfoldPlanar.graph_vertex> graph, List<Object> faces)
            {
                
                Assert.AreEqual(graph.Count, faces.Count);
                Console.WriteLine("same number of faces as verts");
                foreach (var vertex in graph)
                {
                    var originalface = vertex.Face.OriginalEntity;
                    Assert.Contains(originalface, faces);
                }

            }


            public void GraphHasCorrectNumberOfEdges(int expectedEdges, List<Unfold.UnfoldPlanar.graph_vertex> graph)
            {

                List<UnfoldPlanar.graph_edge> alledges = new List<UnfoldPlanar.graph_edge>();

                foreach (UnfoldPlanar.graph_vertex vertex in graph)
                {
                    foreach(UnfoldPlanar.graph_edge graphedge in vertex.Graph_Edges){

                        alledges.Add(graphedge);
                    }
                }

                Assert.AreEqual(expectedEdges, alledges.Count);
                Console.WriteLine("correct number of edges");
            
            }

            public  void EveryFaceIsReachable()
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