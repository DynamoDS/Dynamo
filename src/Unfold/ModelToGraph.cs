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
        public class FaceLikeEntity:IUnfoldPlanarFace<EdgeLikeEntity>
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
        /// a graph edge_ stores head and tail and the  wrapped geometry edgeLikeEntity that this graph edge represents
        /// </summary>
        public class GraphEdge<K,T> where K:IUnfoldEdge where T:IUnfoldPlanarFace<K>
        {

            public GraphVertex<K,T> Tail { get; set; }
            public GraphVertex<K,T> Head { get; set; }
            public K Real_Edge { get; set; }

            public GraphEdge (K edge, GraphVertex<K,T> tail, GraphVertex<K,T> head)
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
        public class GraphVertex<K,T> where T: IUnfoldPlanarFace<K> where K:IUnfoldEdge
        {

            public T Face{ get; set; }
            public HashSet<GraphEdge<K,T>> Graph_Edges { get; set; }
            public GraphVertex<K,T> Parent { get; set; }
            public Boolean Explored { get; set; }
            public int Finish_Time { get; set; }
            public HashSet<GraphEdge<K, T>> TreeEdges { get; set; }

            // for cycle detection using tarjans
            public int Index { get; set; }
            public int LowLink { get; set; }

            public GraphVertex(T face)
            {
                Face = face;
                Graph_Edges = new HashSet<GraphEdge<K,T>>();
                TreeEdges = new HashSet<GraphEdge<K, T>>();
            }

           

            public override bool Equals(object obj)
            {
                GraphVertex<K,T> objitem = obj as GraphVertex<K,T>;
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

            // These user facing methods will need to be wrapped to return different types 
            // since we do not want to expose generic types to dynamo

            public static List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>> GenerateTopologyFromFaces(List<Face> faces)  
               
            {

                List<FaceLikeEntity> wrappedFaces = faces.Select(x => new FaceLikeEntity(x)).ToList();



                return GenerateTopology<EdgeLikeEntity, FaceLikeEntity>(wrappedFaces);
            }

            public static List<GraphVertex<EdgeLikeEntity, FaceLikeEntity>> GenerateTopologyFromSurfaces(List<Surface> surfaces)
               
            {

                List<FaceLikeEntity> wrappedSurfaces = surfaces.Select(x => new FaceLikeEntity(x)).ToList();



                return GenerateTopology<EdgeLikeEntity, FaceLikeEntity>(wrappedSurfaces);
            }


           



            /// <summary>
            /// main user facing method for generation of graph from list of faces
            /// </summary>
            /// <param name="faces"></param>
            /// <returns></returns>
            private static List<GraphVertex<K,T>> GenerateTopology<K,T>(List<T> facelikes) where T:IUnfoldPlanarFace<K> where K:IUnfoldEdge
            {
                Dictionary<K, List<T>> edgeDict = new Dictionary<K, List<T>>(new Unfold.Interfaces.SpatiallyEquatableComparer<K>());
                foreach (T facelike in facelikes)
                {

                    foreach (K edgelike in facelike.EdgeLikeEntities)
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
                            edgeDict.Add(edgekey, new List<T>() { facelike });
                        }
                    }
                }

                var graph = ModelGraph.GenGraph(facelikes, edgeDict);
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
            public static List<GraphVertex<K,T>> GenGraph<T,K>(List<T> facelikes, Dictionary<K, List<T>> edge_dict) where K:IUnfoldEdge where T:IUnfoldPlanarFace<K>
            {
                List<GraphVertex<K,T>> graph = new List<GraphVertex<K,T>>();
                // first build the graph nodes, just referencing faces
                foreach (T face in facelikes)
                {
                    var current_vertex = new GraphVertex<K,T>(face);
                    graph.Add(current_vertex);
                }

                // then build edges, need to use edge dict to do this
                foreach (GraphVertex<K,T> vertex in graph)
                {
                    T facelike = vertex.Face;
                    foreach (K edgelike in facelike.EdgeLikeEntities)
                    {

                        //var edgekey = new EdgeLikeEntity(edge);
                        // again we dont need to generate a new key with this wrapper - already stored on the face in this form

                        var edgekey = edgelike;

                        // find adjacent faces in the dict
                        var subfaces = edge_dict[edgekey];
                        // find the graph verts that represent these faces
                        var verts = GraphUtilities.find_nodes_by_matching_faces(graph, subfaces);
                        //remove dupe faces, not sure if these should really be removed
                        verts = verts.Distinct().ToList();
                        //need to remove self loops
                        //build list of toremove, then remove them
                        var toremove = new List<GraphVertex<K,T>>();
                        foreach (GraphVertex<K,T> testvert in verts)
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
                            K wrapped_edge_on_this_graph_edge = GraphUtilities.find_real_edge_by_two_faces<T,K>(graph, subfaces, edge_dict);
                            var current_graph_edge = new GraphEdge<K,T>(wrapped_edge_on_this_graph_edge, vertex, vert_to_connect_to);
                            vertex.Graph_Edges.Add(current_graph_edge);
                        }

                    }



                }
                return graph;
            }

            // be careful here, modifying the verts may causing issues,
            // might be better to create new verts, or implement Icloneable

            [MultiReturn(new[] {"tree geo","BFS tree"})]
            public static Dictionary<string,object> BFS<K,T>(List<GraphVertex<K,T>> graph) where T:IUnfoldPlanarFace<K> where K:IUnfoldEdge
            {
               
              // this is a clone of the graph that we modify to store the tree edges on
              var graphToTraverse =  GraphUtilities.CloneGraph<K,T>(graph);
                // this is the final form of the graph, we can replace references to the graph edges with only tree edges
                // and treat this tree as a graph
              List<GraphVertex<K, T>> TreeTransformedToGraph;
                
                // now can start actually traversing the graph and building a tree.

                Queue<GraphVertex<K,T>> Q = new Queue<GraphVertex<K,T>>();

                List<Autodesk.DesignScript.Geometry.DesignScriptEntity> tree = new List<Autodesk.DesignScript.Geometry.DesignScriptEntity>();

                GraphVertex<K, T> root = graphToTraverse.First();
                root.Finish_Time = 0;
                root.Parent = null;
                Q.Enqueue(root);

                while (Q.Count > 0)
                {

                    GraphVertex<K,T> current_vertex = Q.Dequeue();
              
                    //generate some geometry to visualize the BFS tree
                    Point center = current_vertex.Face.SurfaceEntity.PointAtParameter(.5, .5);
                    Sphere nodecenter = Sphere.ByCenterPointRadius(center, 1);
                    tree.Add(nodecenter);

                    foreach (GraphEdge<K,T> vedge in current_vertex.Graph_Edges)
                    {

                        GraphVertex<K,T> V = vedge.Head;
                        if (V.Explored == false)
                        {
                            V.Explored = true;
                            V.Finish_Time = current_vertex.Finish_Time + 1;
                            V.Parent = current_vertex;

                            current_vertex.TreeEdges.Add(vedge);

                          
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


                TreeTransformedToGraph = GraphUtilities.CloneGraph<K, T>(graphToTraverse);

                foreach (var Vertex in TreeTransformedToGraph)
                {
                    Vertex.Graph_Edges = Vertex.TreeEdges;
                    //Vertex.TreeEdges.Clear();

                }


                return new Dictionary<string, object> 
                {   
                    { "tree geo", (tree)},
                    {"BFS intermediate",(graphToTraverse)},
                    {"BFS finished", (TreeTransformedToGraph)}
                };
            }
        }
    }
}