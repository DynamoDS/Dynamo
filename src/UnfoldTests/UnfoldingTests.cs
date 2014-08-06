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
using Unfold.Interfaces;


namespace UnfoldTests
{
    class UnfoldingTests : HostFactorySetup
    {

        [TestFixture]
        public class AlignPlanarTests
        {
            [Test]
            public void UnfoldEachPairOfFacesInACubeParentAsRefFace()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                //generate a graph of the cube
                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> faceobjs = faces.Select(x => x as Object).ToList();



                //perform BFS on the graph and get back the tree
                var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;
                //perform tarjans algo and make sure that the tree is acylic before unfold
                var sccs = GraphUtilities.tarjansAlgo<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(sccs, casttree);

                // iterate through each vertex in the tree
                // make sure that the parent/child is not null (depends which direction we're traversing)
                // if not null, grab the next node and the tree edge
                // pass these to check normal consistencey and align.
                // be careful about the order of passed faces

                foreach (var parent in casttree)
                {
                    if (parent.GraphEdges.Count > 0)
                    {
                        foreach (var edge in parent.GraphEdges)
                        {

                            var child = edge.Head;




                            double nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.GeometryEdge);
                            Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.Face, parent.Face, edge.GeometryEdge) as Surface;

                            UnfoldTestUtils.AssertSurfacesAreCoplanar(rotatedFace, parent.Face.SurfaceEntity);

                            UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace, parent.Face.SurfaceEntity);

                        }

                    }


                }

            }


            [Test]
            public void UnfoldEachPairOfFacesInACubeChildAsRefFace()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                //generate a graph of the cube
                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> faceobjs = faces.Select(x => x as Object).ToList();



                //perform BFS on the graph and get back the tree
                var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;
                //perform tarjans algo and make sure that the tree is acylic before unfold
                var sccs = GraphUtilities.tarjansAlgo<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(sccs, casttree);

                // iterate through each vertex in the tree
                // make sure that the parent/child is not null (depends which direction we're traversing)
                // if not null, grab the next node and the tree edge
                // pass these to check normal consistencey and align.
                // be careful about the order of passed faces

                foreach (var parent in casttree)
                {
                    if (parent.GraphEdges.Count > 0)
                    {
                        foreach (var edge in parent.GraphEdges)
                        {

                            var child = edge.Head;


                            double nc = AlignPlanarFaces.CheckNormalConsistency(parent.Face, child.Face, edge.GeometryEdge);
                            Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, parent.Face, child.Face, edge.GeometryEdge) as Surface;

                            UnfoldTestUtils.AssertSurfacesAreCoplanar(rotatedFace, child.Face.SurfaceEntity);

                            UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace, child.Face.SurfaceEntity);

                        }

                    }


                }

            }



            [Test]
            public void UnfoldEachPairOfSurfacesInACubeParentAsRefFace()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                //generate a graph of the cube
                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromSurfaces(surfaces);


                //perform BFS on the graph and get back the tree
                var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;
                //perform tarjans algo and make sure that the tree is acylic before unfold
                var sccs = GraphUtilities.tarjansAlgo<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(sccs, casttree);

                // iterate through each vertex in the tree
                // make sure that the parent/child is not null (depends which direction we're traversing)
                // if not null, grab the next node and the tree edge
                // pass these to check normal consistencey and align.
                // be careful about the order of passed faces

                foreach (var parent in casttree)
                {
                    if (parent.GraphEdges.Count > 0)
                    {
                        foreach (var edge in parent.GraphEdges)
                        {

                            var child = edge.Head;

                            UnfoldTestUtils.AssertEdgeCoincidentWithBothFaces<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(parent.Face, child.Face, edge.GeometryEdge);


                            double nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.GeometryEdge);
                            Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.Face, parent.Face, edge.GeometryEdge) as Surface;

                            UnfoldTestUtils.AssertSurfacesAreCoplanar(rotatedFace, parent.Face.SurfaceEntity);

                            UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace, parent.Face.SurfaceEntity);

                        }

                    }


                }

            }
            [Test]
            public void UnfoldEachPairOfTriangularSurfacesInACubeParentAsRefFace()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tessellate.Tesselate(surfaces);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                //generate a graph of the cube
                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);


                //perform BFS on the graph and get back the tree
                var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;
                //perform tarjans algo and make sure that the tree is acylic before unfold
                var sccs = GraphUtilities.tarjansAlgo<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(sccs, casttree);

                // iterate through each vertex in the tree
                // make sure that the parent/child is not null (depends which direction we're traversing)
                // if not null, grab the next node and the tree edge
                // pass these to check normal consistencey and align.
                // be careful about the order of passed faces

                foreach (var parent in casttree)
                {
                    if (parent.GraphEdges.Count > 0)
                    {
                        foreach (var edge in parent.GraphEdges)
                        {

                            var child = edge.Head;

                            UnfoldTestUtils.AssertEdgeCoincidentWithBothFaces<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(parent.Face, child.Face, edge.GeometryEdge);


                            double nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.GeometryEdge);
                            Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.Face, parent.Face, edge.GeometryEdge) as Surface;

                            UnfoldTestUtils.AssertSurfacesAreCoplanar(rotatedFace, parent.Face.SurfaceEntity);

                            UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace, parent.Face.SurfaceEntity);

                        }

                    }


                }

            }

            [Test]
            public void UnfoldEachPairOfTriangularSurfacesInAConeWideParentAsRefFace()
            {


                Solid testCone = UnfoldTestUtils.SetupLargeCone();
                List<Face> faces = testCone.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tessellate.Tesselate(surfaces);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                //generate a graph of the cube
                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);


                //perform BFS on the graph and get back the tree
                var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;
                //perform tarjans algo and make sure that the tree is acylic before unfold
                var sccs = GraphUtilities.tarjansAlgo<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(sccs, casttree);

                // iterate through each vertex in the tree
                // make sure that the parent/child is not null (depends which direction we're traversing)
                // if not null, grab the next node and the tree edge
                // pass these to check normal consistencey and align.
                // be careful about the order of passed faces

                foreach (var parent in casttree)
                {
                    if (parent.GraphEdges.Count > 0)
                    {
                        foreach (var edge in parent.GraphEdges)
                        {

                            var child = edge.Head;

                            UnfoldTestUtils.AssertEdgeCoincidentWithBothFaces<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(parent.Face, child.Face, edge.GeometryEdge);


                            double nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.GeometryEdge);
                            Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.Face, parent.Face, edge.GeometryEdge) as Surface;

                            UnfoldTestUtils.AssertSurfacesAreCoplanar(rotatedFace, parent.Face.SurfaceEntity);

                            UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace, parent.Face.SurfaceEntity);

                        }

                    }


                }

            }

            [Test]
            public void UnfoldEachPairOfTriangularSurfacesInAConeTallParentAsRefFace()
            {


                Solid testCone = UnfoldTestUtils.SetupTallCone();
                List<Face> faces = testCone.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tessellate.Tesselate(surfaces);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();

                //generate a graph of the cube
                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromSurfaces(trisurfaces);


                //perform BFS on the graph and get back the tree
                var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;
                //perform tarjans algo and make sure that the tree is acylic before unfold
                var sccs = GraphUtilities.tarjansAlgo<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(sccs, casttree);

                // iterate through each vertex in the tree
                // make sure that the parent/child is not null (depends which direction we're traversing)
                // if not null, grab the next node and the tree edge
                // pass these to check normal consistencey and align.
                // be careful about the order of passed faces

                foreach (var parent in casttree)
                {
                    if (parent.GraphEdges.Count > 0)
                    {
                        foreach (var edge in parent.GraphEdges)
                        {

                            var child = edge.Head;

                            UnfoldTestUtils.AssertEdgeCoincidentWithBothFaces<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(parent.Face, child.Face, edge.GeometryEdge);


                            double nc = AlignPlanarFaces.CheckNormalConsistency(child.Face, parent.Face, edge.GeometryEdge);
                            Surface rotatedFace = AlignPlanarFaces.MakeGeometryCoPlanarAroundEdge(nc, child.Face, parent.Face, edge.GeometryEdge) as Surface;

                            UnfoldTestUtils.AssertSurfacesAreCoplanar(rotatedFace, parent.Face.SurfaceEntity);

                            UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter(rotatedFace, parent.Face.SurfaceEntity);

                        }

                    }


                }

            }
        }

        [TestFixture]
        public class FullUnfoldTests
        {
            [Test]
            public void FullyUnfoldCubeFromFaces()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                var unfoldsurfaces = PlanarUnfolder.DSPLanarUnfold(faces).UnfoldedSurfaceSet;

                // must check each surface in each polysurface
                // against everyother surface inside this polysurface and check coplanarity and centers
                // can also test intersections, and make sure no overlaps

                foreach (var srf in unfoldsurfaces)
                {

                    if (srf is PolySurface)
                    {
                        UnfoldTestUtils.AssertNoSurfaceIntersections(srf as PolySurface);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertSurfacesAreCoplanar);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter);
                    }
                }

            }

            [Test]
            public void FullyUnfoldCubeFromSurfaces()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                var unfoldsurfaces = PlanarUnfolder.DSPLanarUnfold(surfaces).UnfoldedSurfaceSet;

                // must check each surface in each polysurface
                // against everyother surface inside this polysurface and check coplanarity and centers
                // can also test intersections, and make sure no overlaps

                foreach (var srf in unfoldsurfaces)
                {

                    if (srf is PolySurface)
                    {
                        UnfoldTestUtils.AssertNoSurfaceIntersections(srf as PolySurface);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertSurfacesAreCoplanar);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter);
                    }
                }

            }

            [Test]
            public void FullyUnfoldCubeFromTriSurfaces()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tessellate.Tesselate(surfaces);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldsurfaces = PlanarUnfolder.DSPLanarUnfold(trisurfaces).UnfoldedSurfaceSet;

                // must check each surface in each polysurface
                // against everyother surface inside this polysurface and check coplanarity and centers
                // can also test intersections, and make sure no overlaps

                foreach (var srf in unfoldsurfaces)
                {

                    if (srf is PolySurface)
                    {
                        UnfoldTestUtils.AssertNoSurfaceIntersections(srf as PolySurface);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertSurfacesAreCoplanar);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter);
                    }
                }

            }

            [Test]
            public void FullyUnfoldConeWideFromTriSurfaces()
            {


                Solid testCone = UnfoldTestUtils.SetupLargeCone();
                List<Face> faces = testCone.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tessellate.Tesselate(surfaces);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldsurfaces = PlanarUnfolder.DSPLanarUnfold(trisurfaces).UnfoldedSurfaceSet;

                // must check each surface in each polysurface
                // against everyother surface inside this polysurface and check coplanarity and centers
                // can also test intersections, and make sure no overlaps

                foreach (var srf in unfoldsurfaces)
                {

                    if (srf is PolySurface)
                    {
                        UnfoldTestUtils.AssertNoSurfaceIntersections(srf as PolySurface);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertSurfacesAreCoplanar);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter);
                    }
                }

            }

            [Test]
            public void FullyUnfoldConeTallFromTriSurfaces()
            {


                Solid testCone = UnfoldTestUtils.SetupTallCone();
                List<Face> faces = testCone.Faces.ToList();
                List<Surface> surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();

                //handle tesselation here
                var pointtuples = Tessellate.Tesselate(surfaces);
                //convert triangles to surfaces
                List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                var unfoldsurfaces = PlanarUnfolder.DSPLanarUnfold(trisurfaces).UnfoldedSurfaceSet;

                // must check each surface in each polysurface
                // against everyother surface inside this polysurface and check coplanarity and centers
                // can also test intersections, and make sure no overlaps

                foreach (var srf in unfoldsurfaces)
                {

                    if (srf is PolySurface)
                    {
                        UnfoldTestUtils.AssertNoSurfaceIntersections(srf as PolySurface);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertSurfacesAreCoplanar);

                        UnfoldTestUtils.AssertConditionForEverySurfaceAgainstEverySurface(srf as PolySurface, UnfoldTestUtils.AssertRotatedSurfacesDoNotShareSameCenter);
                    }
                }

            }

        }


        [TestFixture]
        public class MappingFaceTransformsTests
        {
            // not a test, may not belong in general utilities
             public void MapLabelsToFinalFaceLocations(List<Face> faces)
            {


            }

             public void MapLabelsToFinalFaceLocations(List<Surface> surfaces)
            {


            }


            


             public void AssertLabelsGoodFinalLocationAndOrientation(List<PlanarUnfolder.UnfoldableFaceLabel
                  <GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>> labels, List<List<Curve>> 
                 translatedgeo,PlanarUnfolder.PlanarUnfolding<GeneratePlanarUnfold.EdgeLikeEntity,GeneratePlanarUnfold.FaceLikeEntity> unfoldingObject)
             {

                 // assert that the final geometry bounding boxes intersect with the 
                 //bounding boxes of the orginal surfaces(transformed through their transformation histories)

                 foreach (var label in labels)
                 {
                     var index = labels.IndexOf(label);
                     var bb = BoundingBox.ByGeometry(translatedgeo[index]);

                     //transform the  inital surface by its transform map

                     var transformedInitialSurfaceToFinal = PlanarUnfolder.MapGeometryToUnfoldingByID
                         <GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity, Surface>
                         (unfoldingObject, label.UnfoldableFace.SurfaceEntity, label.ID);

                     Assert.IsTrue(bb.Intersects(BoundingBox.ByGeometry(transformedInitialSurfaceToFinal)));
                     Console.WriteLine("This label was in the right spot at the end of the unfold");
                 }

             }

             public void AssertLabelsGoodStartingLocationAndOrientation(List<PlanarUnfolder.UnfoldableFaceLabel
                 <GeneratePlanarUnfold.EdgeLikeEntity,GeneratePlanarUnfold.FaceLikeEntity>> labels)
             {
                 // get aligned geometry
                 var alignedGeo = labels.Select(x => x.AlignedLabelGeometry).ToList();
                 // assert that the bounding box of the label at least intersects the face it represents
                 foreach (var curveList in alignedGeo)
                 {
                     Random rnd = new Random();
                     
                     var index = alignedGeo.IndexOf(curveList);
                     var bb = BoundingBox.ByGeometry(curveList);
                     var surfacebb = BoundingBox.ByGeometry(labels[index].UnfoldableFace.SurfaceEntity);

                     Surface triangleOfLabel = null;
                     // horrific code - randomly finding 3 points from the label and trying to gen a triangle...
                     // really need bounding box not to failing going to polysurface when flat.
                     bool tryAgain = true;
                     while (tryAgain)
                     {
                         try
                         {
                             var curvePoints = curveList.Select(x => x.StartPoint);
                             var threepoints = curvePoints.OrderBy(x => rnd.Next()).Take(3).ToList();

                             triangleOfLabel = Surface.ByPerimeterPoints(threepoints);
                             tryAgain = false;
                         }
                         catch (Exception e)
                         {
                             // Or maybe set tryAgain = false; here, depending upon the exception, or saved details from within the try.
                         }
                     }

                     
                     

                     Console.WriteLine("index = " + index.ToString());
                     // assert that the box intersects with the bounding box of the surface
                     var bbcenter = bb.MinPoint.Add((bb.MaxPoint.Subtract(bb.MinPoint.AsVector()).AsVector().Scale(.5)));
                     var surfacebbcenter = surfacebb.MinPoint.Add((surfacebb.MaxPoint.Subtract(surfacebb.MinPoint.AsVector()).AsVector().Scale(.5)));

                     var distance = bbcenter.DistanceTo(surfacebbcenter);

                     Assert.IsTrue(distance < Vector.ByTwoPoints(surfacebb.MaxPoint, surfacebb.MinPoint).Length);
                     Console.WriteLine("This label was in the right spot at the start of the unfold");
                     //also assert that the face normal is parallel with the normal of the boundingbox plane

                     var face = labels[index].UnfoldableFace.SurfaceEntity;

                     UnfoldTestUtils.AssertSurfacesAreCoplanar(triangleOfLabel, face);
                     Console.WriteLine("This label was in the right orientation at the start of the unfold");

                 }

                 


             }


             [Test]
             public void UnfoldAndLabelCubeFromFaces()
             {

                 // unfold cube
                 Solid testcube = UnfoldTestUtils.SetupCube();
                 List<Face> faces = testcube.Faces.ToList();

                 var unfoldObject = PlanarUnfolder.DSPLanarUnfold(faces);

                 var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                 Console.WriteLine("generating labels");
                 
                 // generate labels
                 var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
                new PlanarUnfolder.UnfoldableFaceLabel<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(x)).ToList();

                 AssertLabelsGoodStartingLocationAndOrientation(labels);
                 
                 // next check the positions of the translated labels,

                 var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                 AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

             }

             [Test]
             public void UnfoldAndLabelCubeFromSurfacs()
             {

                 // unfold cube
                 Solid testcube = UnfoldTestUtils.SetupCube();
                 List<Face> faces = testcube.Faces.ToList();
                 var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                 var unfoldObject = PlanarUnfolder.DSPLanarUnfold(surfaces);

                 var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                 Console.WriteLine("generating labels");

                 // generate labels
                 var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
                new PlanarUnfolder.UnfoldableFaceLabel<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(x)).ToList();

                 AssertLabelsGoodStartingLocationAndOrientation(labels);

                 // next check the positions of the translated labels

                 var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                 AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

             }
             [Test]
             public void UnfoldAndLabelExtrudedLFromSurfacs()
             {
                 throw new NotImplementedException();
                 // unfold cube
                 Solid testcube = UnfoldTestUtils.SetupCube();
                 List<Face> faces = testcube.Faces.ToList();

                 var unfoldObject = PlanarUnfolder.DSPLanarUnfold(faces);

                 var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                 Console.WriteLine("generating labels");

                 // generate labels
                 var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
                new PlanarUnfolder.UnfoldableFaceLabel<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(x)).ToList();

                 AssertLabelsGoodStartingLocationAndOrientation(labels);

                 // next check the positions of the translated labels

                 var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                 AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);

             }
             [Test]
             public void UnfoldAndLabelTallCone()
             {

                 // unfold cube
                 Solid testcone = UnfoldTestUtils.SetupTallCone();
                 List<Face> faces = testcone.Faces.ToList();
                 var surfaces =  faces.Select(x=>x.SurfaceGeometry()).ToList();
                 //handle tesselation here
                 var pointtuples = Tessellate.Tesselate(surfaces);
                 //convert triangles to surfaces
                 List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                 var unfoldObject = PlanarUnfolder.DSPLanarUnfold(trisurfaces);

                 var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                 Console.WriteLine("generating labels");

                 // generate labels
                 var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
                new PlanarUnfolder.UnfoldableFaceLabel<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(x)).ToList();

                 AssertLabelsGoodStartingLocationAndOrientation(labels);

                 // next check the positions of the translated labels

                 var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                 AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);


             }
             [Test]
             public void UnfoldAndLabelWideCone()
             {

                 // unfold cube
                 Solid testcube = UnfoldTestUtils.SetupLargeCone();
                 List<Face> faces = testcube.Faces.ToList();
                 var surfaces = faces.Select(x => x.SurfaceGeometry()).ToList();
                 //handle tesselation here
                 var pointtuples = Tessellate.Tesselate(surfaces);
                 //convert triangles to surfaces
                 List<Surface> trisurfaces = pointtuples.Select(x => Surface.ByPerimeterPoints(new List<Point>() { x[0], x[1], x[2] })).ToList();


                 var unfoldObject = PlanarUnfolder.DSPLanarUnfold(trisurfaces);
               
                 var unfoldsurfaces = unfoldObject.UnfoldedSurfaceSet;

                 Console.WriteLine("generating labels");

                 // generate labels
                 var labels = unfoldObject.StartingUnfoldableFaces.Select(x =>
                new PlanarUnfolder.UnfoldableFaceLabel<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(x)).ToList();

                 AssertLabelsGoodStartingLocationAndOrientation(labels);

                 // next check the positions of the translated labels

                 var transformedGeo = labels.Select(x => PlanarUnfolder.MapGeometryToUnfoldingByID(unfoldObject, x.AlignedLabelGeometry, x.ID)).ToList();

                 AssertLabelsGoodFinalLocationAndOrientation(labels, transformedGeo, unfoldObject);


             }



        }

    }
}
