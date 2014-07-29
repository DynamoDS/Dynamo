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


    public class HostSetupTest
    {
        [Test]
        public void HostSimple()
        {
            Assert.DoesNotThrow(() => HostFactory.Instance.StartUp());

            Assert.DoesNotThrow(() => HostFactory.Instance.ShutDown());
        }

    }



    public class TopologyTests : HostFactorySetup
    {
        


        [TestFixture]
        public class InitialGraphTests
        {

            [Test]
            public void GraphCanBeGeneratedFromCubeFaces()
            {
                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                Assert.AreEqual(faces.Count, 6);

                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> face_objs = faces.Select(x => x as Object).ToList();

                UnfoldTestUtils.GraphHasVertForEachFace(graph, face_objs);

                UnfoldTestUtils.GraphHasCorrectNumberOfEdges(24, graph);

                var sccs = GraphUtilities.tarjansAlgo<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>.CycleDetect(graph);

                UnfoldTestUtils.IsOneStronglyConnectedGraph(sccs);
                //
            }


            [Test]
            public void GraphCanBeGeneratedFromCubeSurfaces()
            {


                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Surface> surfaces = testcube.Faces.Select(x => x.SurfaceGeometry()).ToList();

                Assert.AreEqual(surfaces.Count, 6);

                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromSurfaces(surfaces);

                List<Object> face_objs = surfaces.Select(x => x as Object).ToList();

                UnfoldTestUtils.GraphHasVertForEachFace(graph, face_objs);

                UnfoldTestUtils.GraphHasCorrectNumberOfEdges(24, graph);


                //
            }

        }
        [TestFixture]
        public class BFSTreeTests
        {


            [Test]
            public void GenBFSTreeFromCubeFaces()
            {

                Solid testcube = UnfoldTestUtils.SetupCube();
                List<Face> faces = testcube.Faces.ToList();

                var graph = GeneratePlanarUnfold.ModelTopology.GenerateTopologyFromFaces(faces);

                List<Object> face_objs = faces.Select(x => x as Object).ToList();


                UnfoldTestUtils.GraphHasVertForEachFace(graph, face_objs);

                UnfoldTestUtils.GraphHasCorrectNumberOfEdges(24, graph);

                var nodereturn = GeneratePlanarUnfold.ModelGraph.BFS<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(graph);
                object tree = nodereturn["BFS finished"];

                var casttree = tree as List<GeneratePlanarUnfold.GraphVertex<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>>;

                UnfoldTestUtils.GraphHasVertForEachFace(casttree, face_objs);
                UnfoldTestUtils.GraphHasCorrectNumberOfEdges(5, casttree);
                UnfoldTestUtils.AssertAllFinishingTimesSet(graph);

                var sccs = GraphUtilities.tarjansAlgo<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>.CycleDetect(casttree);

                UnfoldTestUtils.IsAcylic<GeneratePlanarUnfold.EdgeLikeEntity, GeneratePlanarUnfold.FaceLikeEntity>(sccs, casttree);

            }

           
        }


       



    }
}