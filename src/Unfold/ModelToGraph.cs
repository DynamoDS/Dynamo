using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold.Interfaces;

namespace Unfold
{
    

 

    public static class UnfoldPlanar
    {


        


        /// <summary>
        /// wrapper for edges and curves
        /// </summary>
        public class EdgeLikeEntity:IUnfoldEdge
        {

           

            public Point Start { get; set; }
            public Point End { get; set; }
            public Curve Curve { get; set; }
            public Edge Real_Edge { get; set; }

            public EdgeLikeEntity(Edge edge)
            {
                Start = edge.StartVertex.PointGeometry;
                End = edge.EndVertex.PointGeometry;
                Curve = edge.CurveGeometry;
                // not sure I should be storing this here as well, but if so, then should eliminate edgewrapper class
                Real_Edge = edge;
                

            }

            public EdgeLikeEntity(Curve curve)
            {
                Start = curve.StartPoint;
                End = curve.EndPoint;
                Curve = curve;
                // not sure I should be storing this here as well, but if so, then should eliminate edgewrapper class
                Real_Edge = null;


            }

          
         

            public int GetSpatialHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    //implementing XOR hashcode since a curve defined edge should hash to the same value even if its
                    // start and end points are stored in the reverse fields.  Is there something better?

                    int hash = this.Start.ToString().GetHashCode() ^ this.End.ToString().GetHashCode();
                    Console.WriteLine(this);
                    Console.WriteLine(hash);
                    return hash;

                }
            }

            public bool SpatialEquals(ISpatialEquatable obj)
            {
                IUnfoldEdge objitem = obj as IUnfoldEdge;
                var otherval = objitem.Start.ToString() + objitem.End.ToString();

                //equals will return true even if the start and end point are reversed since on one object this should be
                // the same edge, if this is equal is not implemented this way surface perimeter curves fail to be located correctly 
                // in hash tables

                if (otherval == this.Start.ToString() + this.End.ToString() || otherval == this.End.ToString() + this.Start.ToString())
                {
                    return true;
                }
                else
                {

                    return false;
                }
            }
        }

        /// <summary>
        /// This is a wrapper type for face like entities so that unfolding methods can operate both on faces/edges or surfaces/adjacent-curves
        /// There are overloads for building this class from faces or surfaces
        /// </summary>
        public class FaceLikeEntity
        {

            public Object OriginalEntity { get; set; }
            public Surface SurfaceEntity {get; set;}
            public List<EdgeLikeEntity> EdgeLikeEntities { get; set; }
           // public List<Point> Points { get; set; }

            public FaceLikeEntity(Surface surface)
            {
                // wrap up the curves or edges
                var pericurves = surface.PerimeterCurves();
               List<EdgeLikeEntity> ees =  pericurves.Select(x => new EdgeLikeEntity(x)).ToList();
                EdgeLikeEntities = ees;
               //store the surface
                SurfaceEntity = surface;

                OriginalEntity = surface;

             //   Points = Surface.point

            }

            public FaceLikeEntity(Face face)
            {
                //grab the surface from the face
                SurfaceEntity = face.SurfaceGeometry();

                OriginalEntity = face;

                List<Edge> orgedges = face.Edges.ToList();

                

                EdgeLikeEntities = orgedges.ConvertAll(x=> new EdgeLikeEntity(x)).ToList();

            }


        
        }
        
       

        /// <summary>
        /// method for finding a shared edge given a list of faces
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="facelikes"></param>
        /// <param name="edgedict"></param>
        /// <returns></returns>
        public static EdgeLikeEntity find_real_edge_by_two_faces(List<graph_vertex> graph, List<FaceLikeEntity> facelikes, Dictionary<EdgeLikeEntity, List<FaceLikeEntity>> edgedict)
        {
            foreach (KeyValuePair<EdgeLikeEntity, List<FaceLikeEntity>> entry in edgedict)
            {
                var match = 0;
                foreach (FaceLikeEntity face in facelikes)
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
            return null;
        }

        /// <summary>
        /// method to find a list of nodes that represent a list of faces
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="faces"></param>
        /// <returns></returns>
        public static List<graph_vertex> find_nodes_by_matching_faces(List<graph_vertex> nodes, List<FaceLikeEntity> facelikes)
        {

            List<graph_vertex> output = new List<graph_vertex>();

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
        /// a graph edge_ stores head and tail and the  wrapped geometry edgeLikeEntity that this graph edge represents
        /// </summary>
        public class graph_edge
        {

            public graph_vertex Tail { get; set; }
            public graph_vertex Head { get; set; }
            public EdgeLikeEntity Real_Edge { get; set; }

            public graph_edge(EdgeLikeEntity edge, graph_vertex tail, graph_vertex head)
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

            public FaceLikeEntity Face{ get; set; }
            public HashSet<graph_edge> Graph_Edges { get; set; }
            public graph_vertex Parent { get; set; }
            public Boolean Explored { get; set; }
            public List<EdgeLikeEntity> Fold_Edge { get; set; }
            public int Finish_Time { get; set; }

            // for cycle detection using tarjans
            public int Index { get; set; }
            public int LowLink { get; set; }

            public graph_vertex(FaceLikeEntity face)
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
            /// overloads exposed into dynamo that wrap up the appropriate input types and call generate topology
            /// </summary>
            /// <param name="faces"></param>
            /// <returns></returns>


            public static List<graph_vertex> GenerateTopologyFromFaces(List<Face> faces)
            {

                List<FaceLikeEntity> wrappedFaces = faces.Select(x => new FaceLikeEntity(x)).ToList();



                return GenerateTopology(wrappedFaces);
            }

            public static List<graph_vertex> GenerateTopologyFromSurfaces(List<Surface> surfaces)
            {

                List<FaceLikeEntity> wrappedSurfaces = surfaces.Select(x => new FaceLikeEntity(x)).ToList();



                return GenerateTopology(wrappedSurfaces);
            }


           



            /// <summary>
            /// main user facing method for generation of graph from list of faces
            /// </summary>
            /// <param name="faces"></param>
            /// <returns></returns>
            private static List<graph_vertex> GenerateTopology(List<FaceLikeEntity> facelikes)
            {
                Dictionary<EdgeLikeEntity, List<FaceLikeEntity>> edgeDict = new Dictionary<EdgeLikeEntity, List<FaceLikeEntity>>();
                foreach (FaceLikeEntity facelike in facelikes)
                {

                    foreach (EdgeLikeEntity edgelike in facelike.EdgeLikeEntities)
                    {
                         // EdgeLikeEntity edgekey = new EdgeLikeEntity(edgelike);
                        // no longer need to wrap up the key, it's already wrapped up
                        // watch this...

                        var edgekey = edgelike;

                        if (edgeDict.ContainsKey(edgekey))
                        {
                            edgeDict[edgekey].Add(facelike);
                        }
                        else
                        {
                            edgeDict.Add(edgekey, new List<FaceLikeEntity>() { facelike });
                        }
                    }
                }

                List<graph_vertex> graph = ModelGraph.GenGraph(facelikes, edgeDict);
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
            public static List<graph_vertex> GenGraph(List<FaceLikeEntity> facelikes, Dictionary<EdgeLikeEntity, List<FaceLikeEntity>> edge_dict)
            {
                List<graph_vertex> graph = new List<graph_vertex>();
                // first build the graph nodes, just referencing faces
                foreach (FaceLikeEntity face in facelikes)
                {
                    var current_vertex = new graph_vertex(face);
                    graph.Add(current_vertex);
                }

                // then build edges, need to use edge dict to do this
                foreach (graph_vertex vertex in graph)
                {
                    FaceLikeEntity facelike = vertex.Face;
                    foreach (EdgeLikeEntity edgelike in facelike.EdgeLikeEntities)
                    {

                        //var edgekey = new EdgeLikeEntity(edge);
                        // again we dont need to generate a new key with this wrapper - already stored on the face in this form

                        var edgekey = edgelike;

                        // find adjacent faces in the dict
                        var subfaces = edge_dict[edgekey];
                        // find the graph verts that represent these faces
                        var verts = UnfoldPlanar.find_nodes_by_matching_faces(graph, subfaces);
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
                            EdgeLikeEntity wrapped_edge_on_this_graph_edge = UnfoldPlanar.find_real_edge_by_two_faces(graph, subfaces, edge_dict);
                            var current_graph_edge = new graph_edge(wrapped_edge_on_this_graph_edge, vertex, vert_to_connect_to);
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


                // *refactor this out as a clone method or graph utility/ext method on List<graph_vertex> types
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
                    List<graph_vertex> vertlist = find_nodes_by_matching_faces(graph_to_traverse, new List<FaceLikeEntity>() { vert_to_copy.Face });
                    graph_vertex vert = vertlist[0];
                    vert.Explored = false;
                    vert.Finish_Time = 1000000000;//infin
                    vert.Parent = null;

                    foreach (graph_edge edge_to_copy in vert_to_copy.Graph_Edges)
                    {

                        // find the same faces in the new graph, the nodes that represent these faces...// but we must make sure that these nodes
                        // that are returned are the ones inside the new graph
                        // may make sense to add a property that either is a name , id, or graph owner ...
                        List<graph_vertex> newtail = find_nodes_by_matching_faces(graph_to_traverse, new List<FaceLikeEntity>() { edge_to_copy.Tail.Face });
                        List<graph_vertex> newhead = find_nodes_by_matching_faces(graph_to_traverse, new List<FaceLikeEntity>() { edge_to_copy.Head.Face });
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
                    Point center = current_vertex.Face.SurfaceEntity.PointAtParameter(.5, .5);
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

                            V.Fold_Edge = new List<EdgeLikeEntity>() { vedge.Real_Edge };

                            Point child_center = V.Face.SurfaceEntity.PointAtParameter(.5, .5);
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