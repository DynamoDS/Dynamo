using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unfold.Interfaces;

namespace Unfold
{
    public static class GraphUtilities
    {



        /// <summary>
        /// method to find a list of nodes that represent a list of faces
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        public static List<UnfoldPlanar.graph_vertex<K,T>> find_nodes_by_matching_faces<K,T>(List<UnfoldPlanar.graph_vertex<K,T>> nodes, List<T> facelikes) where T:IUnfoldPlanarFace<K> where K:IUnfoldEdge
        {

            List<UnfoldPlanar.graph_vertex<K,T>> output = new List<UnfoldPlanar.graph_vertex<K,T>>();

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
        public static K find_real_edge_by_two_faces<T,K>(List<UnfoldPlanar.graph_vertex<K,T>> graph, List<T> facelikes, Dictionary<K, List<T>> edgedict)
            where T:IUnfoldPlanarFace<K> 
            where K:IUnfoldEdge
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


        public static List<UnfoldPlanar.graph_vertex<K,T>> CloneGraph<K,T>(List<UnfoldPlanar.graph_vertex<K,T>> graph ) where T:IUnfoldPlanarFace<K> where K:IUnfoldEdge
        {
            ///this is really a clone method in next 3 for loops
            //create new verts and store them in a new list
            List<UnfoldPlanar.graph_vertex<K,T>> graph_to_traverse = new List<UnfoldPlanar.graph_vertex<K,T>>();
            foreach (UnfoldPlanar.graph_vertex<K,T> vert_to_copy in graph)
            {
                var vert = new UnfoldPlanar.graph_vertex<K,T>(vert_to_copy.Face);
                graph_to_traverse.Add(vert);
            }


            // build the rest of the graphcopy - set the other properties of the verts correctly
            foreach (UnfoldPlanar.graph_vertex<K,T> vert_to_copy in graph)
            {
                List<UnfoldPlanar.graph_vertex<K,T>> vertlist = find_nodes_by_matching_faces(graph_to_traverse, new List<T>() { vert_to_copy.Face });
                UnfoldPlanar.graph_vertex<K,T> vert = vertlist[0];
                vert.Explored = false;
                vert.Finish_Time = 1000000000;//infin
                vert.Parent = null;

                foreach (Unfold.UnfoldPlanar.graph_edge<K,T> edge_to_copy in vert_to_copy.Graph_Edges)
                {

                    // find the same faces in the new graph, the nodes that represent these faces...// but we must make sure that these nodes
                    // that are returned are the ones inside the new graph
                    // may make sense to add a property that either is a name , id, or graph owner ...
                    List<UnfoldPlanar.graph_vertex<K,T>> newtail = find_nodes_by_matching_faces(graph_to_traverse, new List<T>() { edge_to_copy.Tail.Face });
                    List<UnfoldPlanar.graph_vertex<K,T>> newhead = find_nodes_by_matching_faces(graph_to_traverse, new List<T>() { edge_to_copy.Head.Face });
                    UnfoldPlanar.graph_edge<K,T> edge = new UnfoldPlanar.graph_edge<K,T>(edge_to_copy.Real_Edge, newtail[0], newhead[0]);
                    vert.Graph_Edges.Add(edge);
                }



            }

            return graph_to_traverse;
        }

        //   http://stackoverflow.com/questions/6643076/tarjan-cycle-detection-help-c-sharp
        //  http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        public static class tarjansAlgo<K,T> where T:IUnfoldPlanarFace<K> where K:IUnfoldEdge
        {

            static int Index;
            static Stack<UnfoldPlanar.graph_vertex<K,T>> VertStack;
            static List<UnfoldPlanar.graph_vertex<K,T>> graphcopy;
            static List<List<UnfoldPlanar.graph_vertex<K,T>>> stronglyConnectedComponents;


            //use for cycle detection to assert that the BFS tree has no cycles and is a tree
            public static List<List<UnfoldPlanar.graph_vertex<K,T>>> CycleDetect(List<UnfoldPlanar.graph_vertex<K,T>> graph)
            {

                var GraphWithTags = CloneGraph<K,T>(graph);
                foreach (var vert in GraphWithTags)
                {
                    // initialize to -1
                    vert.Index = -1;
                    vert.LowLink = -1;
                }

                stronglyConnectedComponents = new List<List<UnfoldPlanar.graph_vertex<K,T>>>();
                Index = 0;
                VertStack = new Stack<UnfoldPlanar.graph_vertex<K,T>>();

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

            private static void checkStrongConnect(UnfoldPlanar.graph_vertex<K,T> vertex)
            {
                vertex.Index = Index;
                vertex.LowLink = Index;
                Index = Index + 1;
                VertStack.Push(vertex);

                var adjlist = vertex.Graph_Edges.Select(x => x.Head).ToList();

                foreach (UnfoldPlanar.graph_vertex<K,T> AdjVert in adjlist)
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

                    var components = new List<UnfoldPlanar.graph_vertex<K,T>>();
                    UnfoldPlanar.graph_vertex<K,T> X;

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
}
