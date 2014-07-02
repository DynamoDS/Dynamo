using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
namespace Unfold
{


    public static class Unfold
    {
        /// <summary>
        /// A wrapper for an Edge object so that it can be hashed
        /// </summary>
        public class EdgeWrapper
        {

            public override bool Equals(object obj)
            {
                EdgeWrapper objitem = obj as EdgeWrapper;
                var otherval = objitem.Start.ToString() + objitem.End.ToString();
                return otherval == this.Start.ToString() + this.End.ToString();


            }

            public Point Start { get; set; }
            public Point End { get; set; }
            public Edge Real_Edge { get; set; }
            public EdgeWrapper(Edge edge)
            {
                Start = edge.StartVertex.PointGeometry;
                End = edge.EndVertex.PointGeometry;
                Real_Edge = edge;


            }


            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + Start.ToString().GetHashCode();
                    hash = hash * 23 + End.ToString().GetHashCode();
                    return hash;
                }
            }

        }

        /// <summary>
        /// method for finding a shared edge given a list of faces
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="faces"></param>
        /// <param name="edgedict"></param>
        /// <returns></returns>
        public static Edge find_real_edge_by_two_faces(List<graph_vertex> graph, List<Face> faces, Dictionary<EdgeWrapper, List<Face>> edgedict)
        {
            foreach (KeyValuePair<EdgeWrapper, List<Face>> entry in edgedict)
            {
                var match = 0;
                foreach (Face face in faces)
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
                if (match == faces.Count)
                {
                    return entry.Key.Real_Edge;
                }



            }
            return null;
        }

        /// <summary>
        /// method to find a list of nodes that represent a list of faces
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        public static List<graph_vertex> find_nodes_by_matching_faces(List<graph_vertex> nodes, List<Face> faces)
        {

            List<graph_vertex> output = new List<graph_vertex>();

            foreach (var face in faces)
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
        /// a graph edge_ stores head and tail and the real geometry edge that this graph edge represents
        /// </summary>
        public class graph_edge
        {

            public graph_vertex Tail { get; set; }
            public graph_vertex Head { get; set; }
            public Edge Real_Edge { get; set; }

            public graph_edge(Edge edge, graph_vertex tail, graph_vertex head)
            {
                Tail = tail;
                Head = head;
                Real_Edge = edge;
            }





        }

        /// <summary>
        /// graph vertex, represents a face, stores list of outoging edges
        /// parent,explored,finishtime, and fold edge will be set during BFS or another traversal method
        /// </summary>
        public class graph_vertex
        {

            public Face Face { get; set; }
            public HashSet<graph_edge> Graph_Edges { get; set; }
            public graph_vertex Parent { get; set; }
            public Boolean Explored { get; set; }
            public List<Edge> Fold_Edge { get; set; }
            public int Finish_Time { get; set; }


            public graph_vertex(Face face)
            {
                Face = face;
                Graph_Edges = new HashSet<graph_edge>();
            }


            public override bool Equals(object obj)
            {
                graph_vertex objitem = obj as graph_vertex;
                var otherval = objitem.Face;
                return otherval.Equals(this.Face);
            }
            
            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    // Suitable nullity checks etc, of course :)
                    hash = hash * 23 + Face.GetHashCode();
                    hash = hash * 23 + Face.GetHashCode();
                    return hash;
                }
            }

        }




        public static class ModelTopology
        {


            /// <summary>
            /// main user facing method for generation of graph from list of faces
            /// </summary>
            /// <param name="faces"></param>
            /// <returns></returns>
            public static List<graph_vertex> GenerateTopologyFromFaces(List<Face> faces)
            {
                Dictionary<EdgeWrapper, List<Face>> edgeDict = new Dictionary<EdgeWrapper, List<Face>>();
                foreach (Face face in faces)
                {

                    foreach (Edge edge in face.Edges)
                    {
                        EdgeWrapper edgekey = new EdgeWrapper(edge);
                        if (edgeDict.ContainsKey(edgekey))
                        {
                            edgeDict[edgekey].Add(face);
                        }
                        else
                        {
                            edgeDict.Add(edgekey, new List<Face>() { face });
                        }
                    }
                }

                List<graph_vertex> graph = ModelGraph.GenGraph(faces, edgeDict);
                return graph;
            }


        }
        public static class ModelGraph
        {




            /// <summary>
            /// actually construct list of graph verts with stored faces and edges
            /// </summary>
            /// <param name="faces"></param>
            /// <param name="edge_dict"></param>
            /// <returns></returns>
            public static List<graph_vertex> GenGraph(List<Face> faces, Dictionary<EdgeWrapper, List<Face>> edge_dict)
            {
                List<graph_vertex> graph = new List<graph_vertex>();
                // first build the graph nodes, just referencing faces
                foreach (Face face in faces)
                {
                    var current_vertex = new graph_vertex(face);
                    graph.Add(current_vertex);
                }

                // then build edges, need to use edge dict to do this
                foreach (graph_vertex vertex in graph)
                {
                    Face face = vertex.Face;
                    foreach (Edge edge in face.Edges)
                    {

                        var edgekey = new EdgeWrapper(edge);
                        // find adjacent faces in the dict
                        var subfaces = edge_dict[edgekey];
                        // find the graph verts that represent these faces
                        var verts = Unfold.find_nodes_by_matching_faces(graph, subfaces);
                        //remove dupe faces, not sure if these should really be removed
                        verts = verts.Distinct().ToList();
                        //need to remove self loops
                        //build list of toremove, then remove them
                        var toremove = new List<graph_vertex>();
                        foreach (graph_vertex testvert in verts)
                        {
                            if (testvert == vertex)
                            {
                                toremove.Add(testvert);
                            }
                        }
                        verts = verts.Except(toremove).ToList();


                        // these are the verts this edge connects
                        foreach (var vert_to_connect_to in verts)
                        {
                            Edge real_edge_on_this_graph_edge = Unfold.find_real_edge_by_two_faces(graph, subfaces, edge_dict);
                            var current_graph_edge = new graph_edge(real_edge_on_this_graph_edge, vertex, vert_to_connect_to);
                            vertex.Graph_Edges.Add(current_graph_edge);
                        }

                    }



                }
                return graph;
            }

            // be careful here, modifying the verts may causing issues,
            // might be better to create new verts, or implement Icloneable

            [MultiReturn(new[] {"tree geo","BFS tree"})]
            public static Dictionary<string,object> BFS(List<graph_vertex> graph)
            {



                ///this is really a clone method in next 3 for loops
                //create new verts and store them in a new list
                List<graph_vertex> graph_to_traverse = new List<graph_vertex>();
                foreach (graph_vertex vert_to_copy in graph)
                {
                    var vert = new graph_vertex(vert_to_copy.Face);
                    graph_to_traverse.Add(vert);
                }

                
                // build the rest of the graphcopy - set the other properties of the verts correctly
                foreach (graph_vertex vert_to_copy in graph)
                {   
                    List<graph_vertex> vertlist = find_nodes_by_matching_faces(graph_to_traverse, new List<Face>() { vert_to_copy.Face });
                    graph_vertex vert = vertlist[0];
                    vert.Explored = false;
                    vert.Finish_Time = 1000000000;//infin
                    vert.Parent = null;

                    foreach (graph_edge edge_to_copy in vert_to_copy.Graph_Edges)
                    {

                        // find the same faces in the new graph, the nodes that represent these faces...// but we must make sure that these nodes
                        // that are returned are the ones inside the new graph
                        // may make sense to add a property that either is a name , id, or graph owner ...
                        List<graph_vertex> newtail = find_nodes_by_matching_faces(graph_to_traverse, new List<Face>() { edge_to_copy.Tail.Face });
                        List<graph_vertex> newhead = find_nodes_by_matching_faces(graph_to_traverse, new List<Face>() { edge_to_copy.Head.Face });
                        graph_edge edge = new graph_edge(edge_to_copy.Real_Edge, newtail[0], newhead[0]);
                        vert.Graph_Edges.Add(edge);
                    }



                }

                // now can start actually traversing the graph and building a tree.

                Queue<graph_vertex> Q = new Queue<graph_vertex>();

                List<Autodesk.DesignScript.Geometry.DesignScriptEntity> tree = new List<Autodesk.DesignScript.Geometry.DesignScriptEntity>();

                graph_vertex root = graph_to_traverse.First();
                root.Finish_Time = 0;
                root.Parent = null;
                Q.Enqueue(root);

                while (Q.Count > 0)
                {

                    graph_vertex current_vertex = Q.Dequeue();

                    //generate some geometry to visualize the BFS tree
                    Point center = current_vertex.Face.SurfaceGeometry().PointAtParameter(.5, .5);
                    Sphere nodecenter = Sphere.ByCenterPointRadius(center, 1);
                    tree.Add(nodecenter);

                    foreach (graph_edge vedge in current_vertex.Graph_Edges)
                    {

                        graph_vertex V = vedge.Head;
                        if (V.Explored == false)
                        {
                            V.Explored = true;
                            V.Finish_Time = current_vertex.Finish_Time + 1;
                            V.Parent = current_vertex;

                            V.Fold_Edge = new List<Edge>() { vedge.Real_Edge };

                            Point child_center = V.Face.SurfaceGeometry().PointAtParameter(.5, .5);
                            Line line = Line.ByStartPointEndPoint(center, child_center);
                            tree.Add(line);
                            Q.Enqueue(V);
                        }

                    }
                    // look at BFS implementation again - CLRS ? colors? I am mutating verts too many times?
                    // check how many times this loop is running with acounter
                    current_vertex.Explored = true;

                }
                return new Dictionary<string, object> 
                {   
                    { "tree geo", (tree)},
                    {"BFS tree",(graph_to_traverse)}
                
                };
            }
        }
    }
}