﻿using System.Collections.Generic;
using System.Linq;
using Dynamo.Configuration;
using NUnit.Framework;
using Path = System.IO.Path;

namespace Dynamo.Tests
{
    public class PackageValidationTest : DynamoModelTestBase
    {
        public PackageValidationTest()
        {
            dynamoSettings = new PreferenceSettings()
            {
                CustomPackageFolders = new List<string>()
                {
                    Path.Combine(TestDirectory, @"core\userdata\0.8")
                }
            };
        }

        protected override string GetUserUserDataRootFolder()
        {
            string userDataDirectory = Path.Combine(TestDirectory, @"core\userdata");
            return userDataDirectory;
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
            libraries.Add("DSIronPython.dll");
            libraries.Add("FunctionObject.ds");
            base.GetLibrariesToPreload(libraries);
        }
        
        [Test]
        public void TestLoadPackageFromOtherLocation()
        {
            var testFilePath = Path.Combine(TestDirectory, @"core\userdata\packageTest.dyn");
            RunModel(testFilePath);
            AssertPreviewValue("5e44d891-d2bb-4188-9f20-6ced9b430b4f", new object[] { 1,2,3});
        }

        #region Dynamo Text Package Tests

        [Test]
        public void TestTextFromDynamoText()
        {
            var testFilePath = Path.Combine(TestDirectory, @"core\userdata\BasicTextTest.dyn");
            RunModel(testFilePath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(7, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());


            var textNodeID = "343723bb-fdac-49c7-ace8-c2cc91a2fae2";
            AssertPreviewCount(textNodeID, 268);

            // output will be 268 Lines, so putting verification for all Lines creation
            for (int i = 0; i < 267; i++)
            {
                var linesFromText = GetPreviewValueAtIndex
                                        (textNodeID, i) as Autodesk.DesignScript.Geometry.Line;
                Assert.IsNotNull(linesFromText);
            }

            var thickenNodeID = "c44bf1a4-2b18-480d-af18-1640ec5cc4fe";
            AssertPreviewCount(thickenNodeID, 268);

             // output will be 268 Solids, so putting verification for all Solid creation
            for (int i = 0; i < 267; i++)
            {
                var solids = GetPreviewValueAtIndex
                                        (thickenNodeID, i) as Autodesk.DesignScript.Geometry.Solid;
                Assert.IsNotNull(solids);
            }
        }

        #endregion

        #region Mesh Toolkit Tests

        [Test]
        public void ConvertMeshIntoLinePointsSurfaces()
        {
            var testFilePath = Path.Combine(TestDirectory,
    @"core\userdata\0.8\packages\MeshToolkit.0.8.2\extra\ConvertMeshIntoLinePointsSurfaces.dyn");

            RunModel(testFilePath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var meshTrianglesNodeID = "1afb1a1c-44f7-414f-8d54-567c52f79652";
            AssertPreviewCount(meshTrianglesNodeID, 8);

            // output will be 6 Surface from Mesh.
            for (int i = 0; i < 7; i++)
            {
                var surfacesFromMesh = GetPreviewValueAtIndex
                            (meshTrianglesNodeID, i) as Autodesk.DesignScript.Geometry.Surface;
                Assert.IsNotNull(surfacesFromMesh);
            }

            var meshVerticesNodeID = "13ef5d4b-04dd-499f-bf2a-5e565cdc0859";
            AssertPreviewCount(meshVerticesNodeID, 6);

            // output will be 6 points from vertices of Mesh
            for (int i = 0; i < 5; i++)
            {
                var points = GetPreviewValueAtIndex
                                (meshVerticesNodeID, i) as Autodesk.DesignScript.Geometry.Point;
                Assert.IsNotNull(points);
            }
        }

        [Test]
        [Category("Failure")]
        public void CreateMeshFromSolids()
        {
            var testFilePath = Path.Combine(TestDirectory,
@"core\userdata\0.8\packages\MeshToolkit.0.8.2\extra\CreateMeshUsingGeometryAndQueryAllProperties.dyn");

            RunModel(testFilePath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(14, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            // Vertex Count for Mesh
            AssertPreviewValue("4fbf3cd0-e1ba-4013-a3df-b39883864f87", 521);

            //Triangle count for Mesh
            AssertPreviewValue("5acdacbc-91a7-45f3-a943-8e201f123474", 1038);

            //Edge Count for Mesh
            AssertPreviewValue("6ea19ed2-2f31-4ed4-a506-a819e27abdcb", 1557); 

        }

        [Test]
        public void CreateMeshUsingVerticesAndFaceIndices()
        {
            var testFilePath = Path.Combine(TestDirectory,
@"core\userdata\0.8\packages\MeshToolkit.0.8.2\extra\CreateMeshUsingVerticesAndFaceIndices.dyn");

            RunModel(testFilePath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(14, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var meshTrianglesNodeID = "1afb1a1c-44f7-414f-8d54-567c52f79652";
            AssertPreviewCount(meshTrianglesNodeID, 8);

            // output will be 6 Surface from Mesh.
            for (int i = 0; i < 7; i++)
            {
                var surfacesFromMesh = GetPreviewValueAtIndex
                            (meshTrianglesNodeID, i) as Autodesk.DesignScript.Geometry.Surface;
                Assert.IsNotNull(surfacesFromMesh);
            }

            var meshVerticesNodeID = "13ef5d4b-04dd-499f-bf2a-5e565cdc0859";
            AssertPreviewCount(meshVerticesNodeID, 6);

            // output will be 6 points from vertices of Mesh
            for (int i = 0; i < 5; i++)
            {
                var points = GetPreviewValueAtIndex
                                (meshVerticesNodeID, i) as Autodesk.DesignScript.Geometry.Point;
                Assert.IsNotNull(points);
            }


        }

        [Test]
        public void SliceMesh()
        {
            var testFilePath = Path.Combine(TestDirectory,
                        @"core\userdata\0.8\packages\MeshToolkit.0.8.2\extra\SliceMesh.dyn");

            RunModel(testFilePath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            var meshTrianglesNodeID = "499ca665-0173-45f5-b580-f8b7c5a7cd73";
            AssertPreviewCount(meshTrianglesNodeID, 4);

            // output will be 4 Polycurves from Slice Operation.
            for (int i = 0; i < 3; i++)
            {
                var surfacesFromMesh = GetPreviewValueAtIndex
                            (meshTrianglesNodeID, i) as Autodesk.DesignScript.Geometry.PolyCurve;
                Assert.IsNotNull(surfacesFromMesh);
            }
        }

        #endregion

    }
}
