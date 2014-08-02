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

    public static class GeneratePlanarUnfold
    {
        [SupressImportIntoVM]
        /// <summary>
        /// wrapper for edges and curves
        /// </summary>
        public class EdgeLikeEntity : IUnfoldableEdge
        {

            public Point Start { get; set; }
            public Point End { get; set; }
            public Curve Curve { get; set; }
            public Edge RealEdge { get; set; }

            public EdgeLikeEntity(Edge edge)
            {
                Start = edge.StartVertex.PointGeometry;
                End = edge.EndVertex.PointGeometry;
                Curve = edge.CurveGeometry;
                // not sure I should be storing this here as well, but if so, then should eliminate edgewrapper class
                RealEdge = edge;

            }

            public EdgeLikeEntity(Curve curve)
            {
                Start = curve.StartPoint;
                End = curve.EndPoint;
                Curve = curve;
                // not sure I should be storing this here as well, but if so, then should eliminate edgewrapper class
                RealEdge = null;


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
                IUnfoldableEdge objitem = obj as IUnfoldableEdge;
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

        [SupressImportIntoVM]
        /// <summary>
        /// This is a wrapper type for face like entities so that unfolding methods can operate both on faces/edges or surfaces/adjacent-curves
        /// There are overloads for building this class from faces or surfaces
        /// </summary>
        public class FaceLikeEntity : IUnfoldablePlanarFace<EdgeLikeEntity>
        {

            public Object OriginalEntity { get; set; }

            private Surface _surface;

            public Surface SurfaceEntity
            {
                get { return _surface; }
                set
                {
                    _surface = value;
                    EdgeLikeEntities = ExtractSurfaceEdges(value);
                }
            }
            public List<EdgeLikeEntity> EdgeLikeEntities { get; set; }
            public int ID { get; set; }
            public List<int> IDS { get; set; }


            private List<EdgeLikeEntity> ExtractSurfaceEdges(Surface surface)
            {
                List<Curve> pericurves = null;
                if (surface is PolySurface)
                {
                    pericurves = (surface as PolySurface).PerimeterCurves().ToList();
                }
                else
                {
                    pericurves = surface.PerimeterCurves().ToList();
                }
                //wrap them
                List<EdgeLikeEntity> ees = pericurves.Select(x => new EdgeLikeEntity(x)).ToList();
                return ees;
            }


            public FaceLikeEntity(Surface surface)
            {

                //store the surface
                SurfaceEntity = surface;
                //store surface
                OriginalEntity = surface;
                // new blank ids
                IDS = new List<int>();

            }

            public FaceLikeEntity(Face face)
            {
                //grab the surface from the face
                SurfaceEntity = face.SurfaceGeometry();
                // org entity is the face
                OriginalEntity = face;
                // grab edges
                List<Edge> orgedges = face.Edges.ToList();
                //wrap edges
                EdgeLikeEntities = orgedges.ConvertAll(x => new EdgeLikeEntity(x)).ToList();
                // new blank ids list
                IDS = new List<int>();


            }

            public FaceLikeEntity()
            {
                IDS = new List<int>();
            }



        }

        [SupressImportIntoVM]
        /// <summary>
        /// a graph edge_ stores head and tail and the  wrapped geometry edgeLikeEntity that this graph edge represents
        /// </summary>
        public class GraphEdge<K, T>
            where K : IUnfoldableEdge
            where T : IUnfoldablePlanarFace<K>
        {

            public GraphVertex<K, T> Tail { get; set; }
            public GraphVertex<K, T> Head { get; set; }
            public K GeometryEdge { get; set; }

            public GraphEdge(K edge, GraphVertex<K, T> tail, GraphVertex<K, T> head)
            {
                Tail = tail;
                Head = head;
                GeometryEdge = edge;
            }

        }

        [SupressImportIntoVM]
        /// <summary>
        /// graph vertex, represents a face, stores list of outoging edges
        /// parent,explored,finishtime, and fold edge will be set during BFS or another traversal method
        /// </summary>
        public class GraphVertex<K, T>
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {
            public T UnfoldPolySurface { get; set; }
            public T Face { get; set; }
            public HashSet<GraphEdge<K, T>> GraphEdges { get; set; }
            public GraphVertex<K, T> Parent { get; set; }
            public Boolean Explored { get; set; }
            public int FinishTime { get; set; }
            public HashSet<GraphEdge<K, T>> TreeEdges { get; set; }


            // for cycle detection using tarjans
            public int Index { get; set; }
            public int LowLink { get; set; }

            public GraphVertex(T face)
            {
                Face = face;
                UnfoldPolySurface = face;
                GraphEdges = new HashSet<GraphEdge<K, T>>();
                TreeEdges = new HashSet<GraphEdge<K, T>>();
            }

            // Method to remove this graphvertex from graph and to remove all edges which point to it
            // from other nodes in the graph
            public void RemoveFromGraph(List<GeneratePlanarUnfold.GraphVertex<K, T>> graph)
            {

                //collect all edges
                var allGraphEdges = GraphUtilities.GetAllGraphEdges(graph);
                var edgesToRemove = new List<GraphEdge<K, T>>();

                // mark all edges we need to remove
                foreach (var edge in allGraphEdges)
                {
                    if (edge.Head == this)
                    {
                        edgesToRemove.Add(edge);
                    }
                }
                // iterate the graph again, if during traversal we see 
                // a marked edge, remove it

                foreach (var vertex in graph)
                {

                    vertex.GraphEdges.ExceptWith(edgesToRemove);
                }
                //finally remove the node
                graph.Remove(this);
            }



            public override bool Equals(object obj)
            {
                GraphVertex<K, T> objitem = obj as GraphVertex<K, T>;
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



        [SupressImportIntoVM]
        public static class ModelTopology
        {
            //TODO(Mike) need to actually get this to expose in dynamo, bug in importer prevents generics from importing
            // along with non generic code, unfold clas in corenodes project currently replicates below code
            // also need to discuss design with Zach,Peter to see if this should be exposed, it may be useful for
            // other things besides unfolding



            // These user facing methods will need to be wrapped to return different types 
            // since we do not want to expose generic types to dynamo

            // this could be made generic 

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
            /// main method for generation of graph from list of faceslikes
            /// builds initial dict of edges:faces using spatiallyequatablecomparer
            /// </summary>
            /// <param name="faces"></param>
            /// <returns></returns>
            private static List<GraphVertex<K, T>> GenerateTopology<K, T>(List<T> facelikes)
                where T : IUnfoldablePlanarFace<K>
                where K : IUnfoldableEdge
            {
                // assign some ids to the faces that will operate on, we use the ids to create text and perform other mappings
                AssignIDs<K, T>(facelikes);

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

            //method that iterates each face in a list and generates and assigns its id

            private static void AssignIDs<K, T>(List<T> facelikes)
                where T : IUnfoldablePlanarFace<K>
                where K : IUnfoldableEdge
            {
                for (int i = 0; i < facelikes.Count; i++)
                {
                    // simple id just equals index
                    // possibly this method should be
                    // user assignable so custom ids can be generated
                    //that has some meaning during refolding
                    facelikes[i].ID = i;
                    facelikes[i].IDS = new List<int>() { i };

                }
            }


        }
        [SupressImportIntoVM]
        public static class ModelGraph
        {


            /// <summary>
            /// actually construct list of graph verts with stored faces and edges
            /// iterates dictionary of edges to faces and building graph
            /// </summary>
            /// <param name="faces"></param>
            /// <param name="edgeDict"></param>
            /// <returns></returns>
            public static List<GraphVertex<K, T>> GenGraph<T, K>(List<T> facelikes, Dictionary<K, List<T>> edgeDict)
                where K : IUnfoldableEdge
                where T : IUnfoldablePlanarFace<K>
            {
                List<GraphVertex<K, T>> graph = new List<GraphVertex<K, T>>();
                // first build the graph nodes, just referencing faces
                foreach (T face in facelikes)
                {
                    var CurrentVertex = new GraphVertex<K, T>(face);
                    graph.Add(CurrentVertex);
                }

                // then build edges, need to use edge dict to do this
                foreach (GraphVertex<K, T> vertex in graph)
                {
                    T facelike = vertex.Face;
                    foreach (K edgelike in facelike.EdgeLikeEntities)
                    {

                        //var edgekey = new EdgeLikeEntity(edge);
                        // again we dont need to generate a new key with this wrapper - already stored on the face in this form

                        var edgekey = edgelike;

                        // find adjacent faces in the dict
                        var subfaces = edgeDict[edgekey];
                        // find the graph verts that represent these faces
                        var verts = GraphUtilities.FindNodesByMatchingFaces(graph, subfaces);
                        //remove dupe faces, not sure if these should really be removed
                        verts = verts.Distinct().ToList();
                        //need to remove self loops
                        //build list of toremove, then remove them
                        var toremove = new List<GraphVertex<K, T>>();
                        foreach (GraphVertex<K, T> testvert in verts)
                        {
                            if (testvert == vertex)
                            {
                                toremove.Add(testvert);
                            }
                        }
                        verts = verts.Except(toremove).ToList();


                        // these are the verts this edge connects
                        foreach (var VertToConnectTo in verts)
                        {
                            K wrappedEdgeOnThisGraphEdge = GraphUtilities.FindRealEdgeByTwoFaces<T, K>(graph, subfaces, edgeDict);
                            var CurrentGraphEdge = new GraphEdge<K, T>(wrappedEdgeOnThisGraphEdge, vertex, VertToConnectTo);
                            vertex.GraphEdges.Add(CurrentGraphEdge);
                        }

                    }



                }
                return graph;
            }

            // be careful here, modifying the verts may causing issues,
            // might be better to create new verts, or implement Icloneable

            [MultiReturn(new[] { "tree geo", "BFS tree", "BFS finished" })]
            public static Dictionary<string, object> BFS<K, T>(List<GraphVertex<K, T>> graph)
                where T : IUnfoldablePlanarFace<K>
                where K : IUnfoldableEdge
            {

                // this is a clone of the graph that we modify to store the tree edges on
                var graphToTraverse = GraphUtilities.CloneGraph<K, T>(graph);
                // this is the final form of the graph, we can replace references to the graph edges with only tree edges
                // and treat this tree as a graph
                List<GraphVertex<K, T>> TreeTransformedToGraph;

                // now can start actually traversing the graph and building a tree.

                Queue<GraphVertex<K, T>> Q = new Queue<GraphVertex<K, T>>();

                List<Autodesk.DesignScript.Geometry.DesignScriptEntity> tree = new List<Autodesk.DesignScript.Geometry.DesignScriptEntity>();

                foreach (var node in graphToTraverse)
                {
                    if (node.Explored == false)
                    {
                        GraphVertex<K, T> root = node;
                        root.FinishTime = 0;
                        root.Parent = null;
                        Q.Enqueue(root);
                    }
                    while (Q.Count > 0)
                    {

                        GraphVertex<K, T> CurrentVertex = Q.Dequeue();

                        //generate some geometry to visualize the BFS tree

                        //create a polygon from verts, grab center, project center towards 
                        Point center = Tessellate.MeshHelpers.SurfaceAsPolygonCenter(CurrentVertex.Face.SurfaceEntity);
                        //  Point center = current_vertex.Face.SurfaceEntity.PointAtParameter(.5, .5);
                        Sphere nodecenter = Sphere.ByCenterPointRadius(center, .1);
                        tree.Add(nodecenter);

                        foreach (GraphEdge<K, T> vedge in CurrentVertex.GraphEdges)
                        {

                            GraphVertex<K, T> V = vedge.Head;
                            if (V.Explored == false)
                            {
                                V.Explored = true;
                                V.FinishTime = CurrentVertex.FinishTime + 1;
                                V.Parent = CurrentVertex;

                                CurrentVertex.TreeEdges.Add(vedge);


                                //Point child_center = V.Face.SurfaceEntity.PointAtParameter(.5, .5);
                                Point childCenter = Tessellate.MeshHelpers.SurfaceAsPolygonCenter(V.Face.SurfaceEntity);
                                Line line = Line.ByStartPointEndPoint(center, childCenter);
                                tree.Add(line);
                                Q.Enqueue(V);
                            }

                        }
                        // look at BFS implementation again - CLRS ? colors? I am mutating verts too many times?
                        // check how many times this loop is running with acounter
                        CurrentVertex.Explored = true;


                    }

                }

                TreeTransformedToGraph = GraphUtilities.CloneGraph<K, T>(graphToTraverse);

                foreach (var Vertex in TreeTransformedToGraph)
                {
                    Vertex.GraphEdges = Vertex.TreeEdges;
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