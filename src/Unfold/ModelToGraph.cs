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


                // do something with entry.Value or entry.Key
            }
            return null;
        }

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

        public class graph_vertex
        {

            public Face Face { get; set; }
            public List<graph_edge> Graph_Edges { get; set; }
            public graph_vertex Parent { get; set; }
            public Boolean Explored { get; set; }
            public List<Edge> Fold_Edge { get; set; }
            public int Finish_Time { get; set; }


            public graph_vertex(Face face)
            {
                Face = face;
                Graph_Edges = new List<graph_edge>();
            }
        }




        public static class ModelTopology
        {



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





        }
    }
}