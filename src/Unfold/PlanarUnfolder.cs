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
    /// <summary>
    /// class that contains the unfolding methods and algorithms that perform unfolding
    /// </summary>
    public static class PlanarUnfolder
    {

        public class FaceTransformMap
        {

            public CoordinateSystem CS { get; set; }
            public List<int> IDS { get; set; }


            public FaceTransformMap(CoordinateSystem cs, List<int> ids)
            {
                IDS = ids;
                CS = cs;

            }

        }

        public class PlanarUnfolding<K, T>
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {
            public List<T> StartingUnfoldableFaces { get; set; }
            public List<Surface> UnfoldedSurfaceSet { get; set; }
            public List<FaceTransformMap> Maps { get; set; }
            public Dictionary<int, Point> StartingPoints { get; set; }

            public PlanarUnfolding(List<T> originalFaces, List<Surface> finalFaces, List<FaceTransformMap> transforms)
            {
                StartingUnfoldableFaces = originalFaces;
                UnfoldedSurfaceSet = finalFaces;
                Maps = transforms;

                StartingPoints = StartingUnfoldableFaces.ToDictionary(x => x.ID, x => Tessellate.MeshHelpers.SurfaceAsPolygonCenter(x.SurfaceEntity));
            }

        }



        // these overloads are called from exposed dynamo nodes depending on input type
        // These methods now return unfolding objects which will need to be split up
        // and returned in dynamo with a multiout node, in the future after generic bug fixed or after refactor
        // I would like to expose PlanarunfoldingResult object with query methods
        // might be able to make the rest of these methods generic now....


        public static PlanarUnfolding<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity> DSPLanarUnfold(List<Face> faces)
        {
            var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromFaces(faces);

            //perform BFS on the graph and get back the tree
            var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
            object tree = nodereturn["BFS finished"];

            var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;


            return PlanarUnfold(casttree);

        }

        public static PlanarUnfolding<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity> DSPLanarUnfold(List<Surface> surfaces)
        {
            var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromSurfaces(surfaces);

            //perform BFS on the graph and get back the tree
            var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
            object tree = nodereturn["BFS finished"];

            var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;


            return PlanarUnfold(casttree);

        }






        /// <summary>
        /// method that performs the main planar unfolding
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static PlanarUnfolding<K, T>
            PlanarUnfold<K,T>(List<GeneratePlanarUnfold.GraphVertex<K, T>> tree)
        where K:IUnfoldableEdge
        where T : IUnfoldablePlanarFace<K>, new()
        
        { 
            // this algorithm is a first test of recursive unfolding - overlapping is expected

            //algorithm pseudocode follows:
            //Find the last ranked finishing time node in the BFS tree
            //Find the parent of this vertex
            //Fold them over their shared edge ( rotate child to be coplanar with parent)
            //merge the resulting faces into a polysurface
            // make this new surface the Face property in parent node
            //remove the child node we started with from the tree
            //repeat, until there is only one node in the tree.
            // at this point all faces should be coplanar with this surface
            var allfaces = tree.Select(x => x.Face).ToList();



            var sortedtree = tree.OrderBy(x => x.FinishTime).ToList();

            var disconnectedSet = new List<T>();

            List<FaceTransformMap> transforms = new List<FaceTransformMap>();


            // as an initial set, we'll record the starting coordinate system of each surface
            transforms.AddRange(allfaces.Select(x => new FaceTransformMap(x.SurfaceEntity.ContextCoordinateSystem, x.IDS)).ToList());


            while (sortedtree.Count > 1)
            {
                // if the tree only has nodes with no parents
                // then all branches have been folded into these
                //nodes and we should just return
                if (sortedtree.All(x => x.Parent == null))
                {
                    break;
                }

                // child is the highest finish time remaining in the list.
                var child = sortedtree.Last();
                var parent = child.Parent;
                //weak code, shoould have a method for this - find edge that leads to
                var edge = parent.GraphEdges.Where(x => x.Head.Equals(child)).First();

                double nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.GeometryEdge);
                Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.UnfoldPolySurface, parent.Face, edge.GeometryEdge) as Surface;



                //at this point need to check if the rotated face has intersected with any other face that has been been
                // folded already, all of these already folded faces should exist either in the parent unfoldedpolysurface
                // or they should have been moved away, we should only need to check if the rotated face hits the polysurface.

                // the srflist is either a single surface or all surfaces containeed in the polysurface, if any of these
                // surfaces intersected with the rotatedface returns a new surface then there is a real overlap and 
                // we need to move the rotated face away .. it will need to be oriented horizontal later

                List<Surface> srfList = null;
                if (parent.UnfoldPolySurface.SurfaceEntity is PolySurface)
                {
                    srfList = (parent.UnfoldPolySurface.SurfaceEntity as PolySurface).Surfaces().ToList();
                }
                else
                {
                    srfList = new List<Surface>() { parent.UnfoldPolySurface.SurfaceEntity };
                }

                // same logic rotation face
                List<Surface> rotFaceSubSurfaces = null;
                if (rotatedFace is PolySurface)
                {

                    rotFaceSubSurfaces = (rotatedFace as PolySurface).Surfaces().ToList();
                }
                else
                {

                    rotFaceSubSurfaces = new List<Surface>() { rotatedFace };
                }

                // perfrom the intersection test, from surfaces against all surfaces

                bool overlapflag = false;
                foreach (var surfaceToIntersect in srfList)
                {
                    foreach (var rotsubface in rotFaceSubSurfaces)
                    {
                        var resultantGeo = surfaceToIntersect.Intersect(rotatedFace);
                        foreach (var geo in resultantGeo)
                        {
                            if (geo is Surface)
                            {
                                overlapflag = true;
                            }
                        }
                    }
                }

                if (overlapflag)
                {
                    //TODO must fix this, need to organize final output and align all final subsets to some plane
                    // this randomness should be removed and replaced with logic for layout searching the current bounding boxes
                    // of existing branches
                    var r = new Random();
                    // if any result was a surface then we overlapped we need to move the folded branch far away and pick a new
                    // branch to start the unfold from

                    // wrap up the translated geometry as a new facelike
                    // when this transformation occurs we need to save the coordinate system as well to the transformation map
                    var translatedGeoContainer = new T();
                    translatedGeoContainer.SurfaceEntity = (child.UnfoldPolySurface.SurfaceEntity.Translate((r.NextDouble() * 10) + 5, 0, 0) as Surface);
                    translatedGeoContainer.OriginalEntity = translatedGeoContainer.SurfaceEntity;

                    var movedUnfoldBranch = translatedGeoContainer;
                    movedUnfoldBranch.IDS = child.UnfoldPolySurface.IDS;
                    disconnectedSet.Add(movedUnfoldBranch);

                  
                }

                else
                {
                    // if there is no overlap we need to merge the rotated chain into the parent

                    List<Surface> subsurblist = null;
                    if (rotatedFace is PolySurface)
                    {
                        subsurblist = (rotatedFace as PolySurface).Surfaces().ToList();
                    }
                    else
                    {
                        subsurblist = new List<Surface>() { rotatedFace };
                    }



                    //below section is in flux - the idea is to push the rotatedface - which might be a polysurface or surface into
                    // the parent vertex's unfoldpolysurface property, then to contract the graph, removing the child node.
                    // at the same time we are trying to build a map of all the rotation transformations we are producing
                    // and to which faces they have been applied, we must push the intermediate coordinate systems
                    // as well as the ids to which they apply through the graph as well.

                    // add the parent surface into this list of surfaces we'll use to create a new polysurface
                    subsurblist.Add(parent.UnfoldPolySurface.SurfaceEntity);


                    var newParentSurface = PolySurface.ByJoinedSurfaces(subsurblist);

                    // replace the surface in the parent with the wrapped chain of surfaces
                    var wrappedChainOfUnfolds = new T();
                    wrappedChainOfUnfolds.SurfaceEntity = newParentSurface;
                    wrappedChainOfUnfolds.OriginalEntity = wrappedChainOfUnfolds.SurfaceEntity;

                    parent.UnfoldPolySurface = wrappedChainOfUnfolds;

                    // as we rotate up the chain we'll add each new ID entry to the list on the parent.
                    var rotatedFaceIDs = child.UnfoldPolySurface.IDS;
                    // add the child ids to the parent id list
                    parent.UnfoldPolySurface.IDS.AddRange(rotatedFaceIDs);
                    parent.UnfoldPolySurface.IDS.Add(child.Face.ID);

                    // now add the coordinate system for the rotatedface to the transforms list

                    transforms.Add(new FaceTransformMap(
                    rotatedFace.ContextCoordinateSystem, parent.UnfoldPolySurface.IDS));


                }

                child.RemoveFromGraph(sortedtree);


            }
            // merge the main trunk and the disconnected sets
            var maintree = sortedtree.Select(x => x.UnfoldPolySurface.SurfaceEntity).ToList();
            maintree.AddRange(disconnectedSet.Select(x => x.SurfaceEntity).ToList());
            return new PlanarUnfolding<K, T>(allfaces, maintree, transforms);
        }

    }
}
