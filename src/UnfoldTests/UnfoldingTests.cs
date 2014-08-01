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

                var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(faces).UnfoldedSurfaceSet;

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
                var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(surfaces).UnfoldedSurfaceSet;

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


                var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(trisurfaces).UnfoldedSurfaceSet;

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


                var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(trisurfaces).UnfoldedSurfaceSet;

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


                var unfoldsurfaces = GeneratePlanarUnfold.PlanarUnfolder.DSPLanarUnfold(trisurfaces).UnfoldedSurfaceSet;

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

    }
}
