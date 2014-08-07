using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Unfold.Interfaces;
using DynamoText;

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

        public class UnfoldableFaceLabel<K,T> 
            where T : IUnfoldablePlanarFace<K>
            where K : IUnfoldableEdge
        {

            public string Label { get; set; }
            public List<Curve> LabelGeometry {get; set;}
            public List<Curve> AlignedLabelGeometry { get; set; }
            public int ID { get;set; }
            public T UnfoldableFace { get; set; }


            private IEnumerable<Curve> GenLabelGeometryFromId(int id){

                List<List<Curve>> textgeo = TextFromString.FromStringOriginAndScale(id.ToString(), Point.ByCoordinates(0, 0, 0), 1.0) as List<List<Curve>>;

                List<Curve> flattextgeo = textgeo.SelectMany(x => x).ToList();

                return flattextgeo;
                
                
            }

            private IEnumerable<Curve> AlignGeoToFace(IEnumerable<Curve> geo)
            {
                var ApproxCenter = Tessellate.MeshHelpers.SurfaceAsPolygonCenter(UnfoldableFace.SurfaceEntity);
                var norm = UnfoldableFace.SurfaceEntity.NormalAtPoint(ApproxCenter);

                var facePlane = Plane.ByOriginNormal(ApproxCenter, norm);

                var finalCordSystem = CoordinateSystem.ByPlane(facePlane);

                return geo.Select(x => x.Transform(finalCordSystem)).Cast<Curve>().AsEnumerable();

            }

            public UnfoldableFaceLabel(T face)
            {
                ID = face.ID;
                LabelGeometry = GenLabelGeometryFromId(ID).ToList();
                Label = ID.ToString();
                UnfoldableFace = face;
                AlignedLabelGeometry = AlignGeoToFace(LabelGeometry).ToList();

            }


        
}

       

          public static List<G> MapGeometryToUnfoldingByID<K,T,G>(PlanarUnfolder.PlanarUnfolding<K, T> unfolding, List<G> geometryToTransform, int id)
      where T : IUnfoldablePlanarFace<K>
      where K : IUnfoldableEdge
      where G : Geometry
          {
                // find bounding box of set of curves
                var myBox =  BoundingBox.ByGeometry(geometryToTransform);

                  // find the center of this box and use as start point
                  var geoStartPoint = myBox.MinPoint.Add((myBox.MaxPoint.Subtract(myBox.MinPoint.AsVector()).AsVector().Scale(.5)));
             
              // transform each curve using this new center as an offset so it ends up translated correctly to the surface center
         var  transformedgeo = geometryToTransform.Select(x => MapGeometryToUnfoldingByID(unfolding, x, id, geoStartPoint)).ToList();

         return transformedgeo;
       }


          public static G MapGeometryToUnfoldingByID<K, T, G>(PlanarUnfolder.PlanarUnfolding<K, T> unfolding, G geometryToTransform, int id)

              where T : IUnfoldablePlanarFace<K>
              where K : IUnfoldableEdge
              where G : Geometry
          {

              // grab all transforms that were applied to this surface id
              var map = unfolding.Maps;
              var applicableTransforms = map.Where(x => x.IDS.Contains(id));
              var transforms = applicableTransforms.Select(x => x.CS).ToList();


              // set the geometry to the first applicable transform
              geometryToTransform = geometryToTransform.Transform(transforms.First()) as G;

              // get bb of geo to transform
              var myBox = geometryToTransform.BoundingBox;
              // find the center of this box and use as start point
              var geoStartPoint = myBox.MinPoint.Add((myBox.MaxPoint.Subtract(myBox.MinPoint.AsVector()).AsVector().Scale(.5)));
              //create vector from unfold surface center startpoint and the current geo center and translate to this start position
              geometryToTransform = geometryToTransform.Translate(Vector.ByTwoPoints(geoStartPoint, unfolding.StartingPoints[id])) as G;

              // at this line, geo to transform is in the CS of the 
              //unfold surface and is that the same position, so following the transform
              // chain will bring the geo to a similar final location as the unfold

              G aggregatedGeo = geometryToTransform;
              for (int i = 0; i + 1 < transforms.Count; i++)
              {
                  aggregatedGeo = aggregatedGeo.Transform(transforms[i + 1]) as G;

              }

              return aggregatedGeo;

          }


          public static G MapGeometryToUnfoldingByID<K, T, G>(PlanarUnfolder.PlanarUnfolding<K, T> unfolding, G geometryToTransform, int id, Point offset)

              where T : IUnfoldablePlanarFace<K>
              where K : IUnfoldableEdge
              where G : Geometry
          {

              // grab all transforms that were applied to this surface id
              var map = unfolding.Maps;
              var applicableTransforms = map.Where(x => x.IDS.Contains(id));
              var transforms = applicableTransforms.Select(x => x.CS).ToList();


              // set the geometry to the first applicable transform
              geometryToTransform = geometryToTransform.Transform(transforms.First()) as G;

              var geoStartPoint = offset;
              //create vector from unfold surface center startpoint and the current geo center and translate to this start position
              geometryToTransform = geometryToTransform.Translate(Vector.ByTwoPoints(geoStartPoint, unfolding.StartingPoints[id])) as G;

              // at this line, geo to transform is in the CS of the 
              //unfold surface and is that the same position, so following the transform
              // chain will bring the geo to a similar final location as the unfold

              G aggregatedGeo = geometryToTransform;
              for (int i = 0; i + 1 < transforms.Count; i++)
              {
                  aggregatedGeo = aggregatedGeo.Transform(transforms[i + 1]) as G;

              }

              return aggregatedGeo;

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


        private static bool checkBoundingBoxIntersections(List<BoundingBox> bbs)
        {
            foreach (var bb in bbs)
            {
                foreach (var bb2 in bbs)
                {
                    if (bb != bb2)
                    {
                        if (bb.Intersects(bb2))
                        {
                            return true;
                        }

                    }
                }
            }
            return false;
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


                    // put the child ids back into the new translated geo container
                    translatedGeoContainer.IDS.AddRange(movedUnfoldBranch.IDS);
                    translatedGeoContainer.IDS.Add(child.Face.ID);

                    transforms.Add(new FaceTransformMap(
                   translatedGeoContainer.SurfaceEntity.ContextCoordinateSystem, translatedGeoContainer.IDS));
                  
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

                    // need to extract the parentIDchain, this is previous faces that been made coplanar with the parent
                    // we need to grab them before the parent unfoldchain is replaced
                    var parentIDchain = parent.UnfoldPolySurface.IDS;
                    var newParentSurface = PolySurface.ByJoinedSurfaces(subsurblist);

                    // replace the surface in the parent with the wrapped chain of surfaces
                    var wrappedChainOfUnfolds = new T();
                    wrappedChainOfUnfolds.SurfaceEntity = newParentSurface;
                    wrappedChainOfUnfolds.OriginalEntity = wrappedChainOfUnfolds.SurfaceEntity;

                    parent.UnfoldPolySurface = wrappedChainOfUnfolds;

                    // as we rotate up the chain we'll add the new IDs entry to the list on the parent.
                   
                    var rotatedFaceIDs = child.UnfoldPolySurface.IDS;
                    // add the child ids to the parent id list
                    parent.UnfoldPolySurface.IDS.AddRange(rotatedFaceIDs);
                    parent.UnfoldPolySurface.IDS.Add(child.Face.ID);
                    

                    // note that we add the parent ID chain to the parent unfold chain, replacing it
                    // but that we DO NOT add these ids to the current transformation map, since we're not transforming them
                    // right now, we just need to keep them from being deleted while adding the new ids.
                   
                    parent.UnfoldPolySurface.IDS.AddRange(parentIDchain);
                    // now add the coordinate system for the rotatedface to the transforms list

                    var currentIDsToStoreTransforms = new List<int>();

                    currentIDsToStoreTransforms.Add(child.Face.ID);
                    currentIDsToStoreTransforms.AddRange(rotatedFaceIDs);

                    transforms.Add(new FaceTransformMap(
                    rotatedFace.ContextCoordinateSystem, currentIDsToStoreTransforms));


                }

                child.RemoveFromGraph(sortedtree);


            }
            // at this point we may have a main trunk with y nodes in it, and x disconnected branches
            //step 1 is to align all sets down to horizontal plane and record this transform
            // step 2 we will dilate all branches with repulsion forces between centers of boundingboxes 
            // until there are NO intersections between any boundingboxes
            // we must record the transforms at each step, or start and final atleast
          
            // collect all polysurfaces
            var masterFacelikeSet = sortedtree.Select(x => x.UnfoldPolySurface).ToList();
            masterFacelikeSet.AddRange(disconnectedSet);
            var rnd = new Random();
            //align all surfaces down
            foreach (var facelike in masterFacelikeSet)
            {
                var surfaceToAlignDown = facelike.SurfaceEntity;
                
                // get the coordinate system defined by the face normal
                var somepoint = facelike.SurfaceEntity.PointAtParameter(.5, .5);
                var norm = facelike.SurfaceEntity.NormalAtParameter(.5,.5);

                var facePlane = Plane.ByOriginNormal(somepoint, norm);

                var startCoordSystem = CoordinateSystem.ByPlane(facePlane);

                var randomPoint = Point.ByCoordinates(rnd.Next(2), rnd.Next(2), 0);

                // transform surface to horizontal plane at 0,0,0
                facelike.SurfaceEntity = surfaceToAlignDown.Transform(startCoordSystem,CoordinateSystem.ByPlane(Plane.ByOriginXAxisYAxis(randomPoint, Vector.XAxis(), Vector.YAxis()))) as Surface;

                // save transformation for each set, this should have all the ids present
                transforms.Add(new FaceTransformMap(
                        facelike.SurfaceEntity.ContextCoordinateSystem, facelike.IDS));

            }
           
            // now begin loop

            var bbs = masterFacelikeSet.Select(x=>BoundingBox.ByGeometry(x.SurfaceEntity)).ToList();


            while (checkBoundingBoxIntersections(bbs))
            {
                var centers = bbs.Select(x => x.MinPoint.Add((x.MaxPoint.Subtract(x.MinPoint.AsVector()).AsVector().Scale(.5)))).ToList();

                var centroid = centers.Aggregate((workingsum, next) =>
                                                  workingsum.Add(next.AsVector())).Scale(1.0/centers.Count) as Point;
                foreach(var facelike in masterFacelikeSet){

                    var index = masterFacelikeSet.IndexOf(facelike);
                    var displacement = Vector.ByTwoPoints(centroid,centers[index]);

                    facelike.SurfaceEntity = facelike.SurfaceEntity.Translate(displacement) as Surface;

                    transforms.Add(new FaceTransformMap(
                        facelike.SurfaceEntity.ContextCoordinateSystem, facelike.IDS));

                }

                bbs = masterFacelikeSet.Select(x => BoundingBox.ByGeometry(x.SurfaceEntity)).ToList();
            }



            // merge the main trunk and the disconnected sets
            var maintree = sortedtree.Select(x => x.UnfoldPolySurface.SurfaceEntity).ToList();
            maintree.AddRange(disconnectedSet.Select(x => x.SurfaceEntity).ToList());
            return new PlanarUnfolding<K, T>(allfaces, maintree, transforms);
        }

    }
}
