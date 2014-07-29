using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unfold.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Unfold
{
    [SupressImportIntoVM]
   
    public static class GraphUtilities
    {



        /// <summary>
        /// method to find a list of nodes that represent a list of faces
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        public static List<GeneratePlanarUnfold.GraphVertex<K,T>> FindNodesByMatchingFaces<K,T>(List<GeneratePlanarUnfold.GraphVertex<K,T>> nodes, List<T> facelikes) 
            where T:IUnfoldablePlanarFace<K> 
            where K:IUnfoldableEdge
        {

            List<GeneratePlanarUnfold.GraphVertex<K,T>> output = new List<GeneratePlanarUnfold.GraphVertex<K,T>>();

            foreach (var face in facelikes)
            {
                foreach (var node in nodes)
                {
                    if (face.Equals(node.Face) && (output.Contains(node) == false))
                    {
                        output.Add(node);
                    }
                }
            }
            return output;
        }


        /// <summary>
        /// method for finding a shared edge given a list of faces
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="facelikes"></param>
        /// <param name="edgedict"></param>
        /// <returns></returns>
        public static K FindRealEdgeByTwoFaces<T,K>(List<GeneratePlanarUnfold.GraphVertex<K,T>> graph, List<T> facelikes, Dictionary<K, List<T>> edgedict)
            where T:IUnfoldablePlanarFace<K> 
            where K:IUnfoldableEdge
        {
            foreach (KeyValuePair<K, List<T>> entry in edgedict) 
                    
            {
                var match = 0;
                foreach (T face in facelikes)
                {
                    if (entry.Value.Contains(face))
                    {
                        match = match + 1;
                    }
                    else
                    {
                        break;
                    }


                }
                if (match == facelikes.Count)
                {
                    return entry.Key;
                }



            }
            return default(K);
        }


        public static List<GeneratePlanarUnfold.GraphVertex<K,T>> CloneGraph<K,T>(List<GeneratePlanarUnfold.GraphVertex<K,T>> graph ) 
            where T:IUnfoldablePlanarFace<K> 
            where K:IUnfoldableEdge
        {
            ///this is really a clone method in next 3 for loops
            //create new verts and store them in a new list
            List<GeneratePlanarUnfold.GraphVertex<K,T>> graphToTraverse = new List<GeneratePlanarUnfold.GraphVertex<K,T>>();
            foreach (GeneratePlanarUnfold.GraphVertex<K,T> vertToCopy in graph)
            {
                var vert = new GeneratePlanarUnfold.GraphVertex<K,T>(vertToCopy.Face);
                graphToTraverse.Add(vert);
            }


            // build the rest of the graphcopy - set the other properties of the verts correctly
            foreach (GeneratePlanarUnfold.GraphVertex<K,T> vertToCopy in graph)
            {
                List<GeneratePlanarUnfold.GraphVertex<K,T>> vertlist = FindNodesByMatchingFaces(graphToTraverse, new List<T>() { vertToCopy.Face });
                GeneratePlanarUnfold.GraphVertex<K,T> vert = vertlist[0];
                vert.Explored = vertToCopy.Explored;
                vert.FinishTime = vertToCopy.FinishTime;
                if (vertToCopy.Parent != null)
                {
                    vert.Parent = graphToTraverse.Where(x => x.Equals(vertToCopy.Parent)).First();
                }
                foreach (Unfold.GeneratePlanarUnfold.GraphEdge<K,T> edgeToCopy in vertToCopy.GraphEdges)
                {

                    // find the same faces in the new graph, the nodes that represent these faces...// but we must make sure that these nodes
                    // that are returned are the ones inside the new graph
                    // may make sense to add a property that either is a name , id, or graph owner ...
                    List<GeneratePlanarUnfold.GraphVertex<K, T>> newtail = FindNodesByMatchingFaces(graphToTraverse, new List<T>() { edgeToCopy.Tail.Face });
                    List<GeneratePlanarUnfold.GraphVertex<K, T>> newhead = FindNodesByMatchingFaces(graphToTraverse, new List<T>() { edgeToCopy.Head.Face });
                    GeneratePlanarUnfold.GraphEdge<K,T> edge = new GeneratePlanarUnfold.GraphEdge<K,T>(edgeToCopy.GeometryEdge, newtail[0], newhead[0]);
                    vert.GraphEdges.Add(edge);
                }

                // same logic for tree edges

                foreach (Unfold.GeneratePlanarUnfold.GraphEdge<K, T> edgeToCopy in vertToCopy.TreeEdges)
                {

                    // find the same faces in the new graph, the nodes that represent these faces...// but we must make sure that these nodes
                    // that are returned are the ones inside the new graph
                    // may make sense to add a property that either is a name , id, or graph owner ...
                    List<GeneratePlanarUnfold.GraphVertex<K, T>> newtail = FindNodesByMatchingFaces(graphToTraverse, new List<T>() { edgeToCopy.Tail.Face });
                    List<GeneratePlanarUnfold.GraphVertex<K, T>> newhead = FindNodesByMatchingFaces(graphToTraverse, new List<T>() { edgeToCopy.Head.Face });
                    GeneratePlanarUnfold.GraphEdge<K, T> edge = new GeneratePlanarUnfold.GraphEdge<K, T>(edgeToCopy.GeometryEdge, newtail[0], newhead[0]);
                    vert.TreeEdges.Add(edge);
                }



            }

            return graphToTraverse;
        }

        //   http://stackoverflow.com/questions/6643076/tarjan-cycle-detection-help-c-sharp
        //  http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        public static class tarjansAlgo<K,T> where T:IUnfoldablePlanarFace<K> where K:IUnfoldableEdge
       {

            static int Index;
            static Stack<GeneratePlanarUnfold.GraphVertex<K,T>> VertStack;
            static List<GeneratePlanarUnfold.GraphVertex<K,T>> graphcopy;
            static List<List<GeneratePlanarUnfold.GraphVertex<K,T>>> stronglyConnectedComponents;


            //use for cycle detection to assert that the BFS tree has no cycles and is a tree
            public static List<List<GeneratePlanarUnfold.GraphVertex<K,T>>> CycleDetect(List<GeneratePlanarUnfold.GraphVertex<K,T>> graph)
            {

                var GraphWithTags = CloneGraph<K,T>(graph);
                foreach (var vert in GraphWithTags)
                {
                    // initialize to -1
                    vert.Index = -1;
                    vert.LowLink = -1;
                }

                stronglyConnectedComponents = new List<List<GeneratePlanarUnfold.GraphVertex<K,T>>>();
                Index = 0;
                VertStack = new Stack<GeneratePlanarUnfold.GraphVertex<K,T>>();

                graphcopy = GraphWithTags;
                foreach (var vert in GraphWithTags)
                {
                    if (vert.Index < 0)
                    {
                        checkStrongConnect(vert);

                    }

                }
                return stronglyConnectedComponents;
            }

            private static void checkStrongConnect(GeneratePlanarUnfold.GraphVertex<K,T> vertex)
            {
                vertex.Index = Index;
                vertex.LowLink = Index;
                Index = Index + 1;
                VertStack.Push(vertex);

                var adjlist = vertex.GraphEdges.Select(x => x.Head).ToList();

                foreach (GeneratePlanarUnfold.GraphVertex<K,T> AdjVert in adjlist)
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

                    var components = new List<GeneratePlanarUnfold.GraphVertex<K,T>>();
                    GeneratePlanarUnfold.GraphVertex<K,T> X;

                    do
                    {

                        X = VertStack.Pop();
                        components.Add(X);
                    } while (vertex != X);

                    stronglyConnectedComponents.Add(components);




                }

            }
        }
            public static List<GeneratePlanarUnfold.GraphEdge<K, T>> GetAllGraphEdges<K,T>(List<GeneratePlanarUnfold.GraphVertex<K, T>> graph) 
                where K:IUnfoldableEdge 
                where T:IUnfoldablePlanarFace<K> 
            {
             var alledges = new List<GeneratePlanarUnfold.GraphEdge<K, T>>();
        
            foreach (GeneratePlanarUnfold.GraphVertex<K, T> vertex in graph)
            {
                foreach (GeneratePlanarUnfold.GraphEdge<K, T> graphedge in vertex.GraphEdges)
                {

                    alledges.Add(graphedge);
                }

            }
            return alledges;
          } 
        }

    }

