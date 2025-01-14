using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using NUnit.Framework;

namespace Dynamo.Tests
{
    class DynamoSamples : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("VMDataBridge.dll");  // Required for Watch node.
            libraries.Add("ProtoGeometry.dll"); // Required for Surface.
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");   // Required for built-in nodes.
            libraries.Add("DSCPython.dll");  // Required for Python tests.
            libraries.Add("FunctionObject.ds"); // Required for partially applied nodes.
            libraries.Add("DSOffice.dll"); // Required for Excel testing.
            base.GetLibrariesToPreload(libraries);
        }

        [Test, Category("SmokeTests")]
        public void Basics_Basic03()
        {
            OpenSampleModel(@"en-US\Basics\Basics_Basic03.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(9, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            AssertPreviewValue("980b45cf-716b-4c8e-b7c2-47a23abf85ed", 9);

            AssertPreviewCount("42d739c7-8e21-4275-9a12-cbcbfd3cf569", 4);

        }

        [Test, Category("SmokeTests")]
        public void Core_AttractorPoint()
        {
            OpenSampleModel(@"en-US\Core\Core_AttractorPoint.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            var solidNodeId = "ef3eaed0-7a8e-47a9-b06e-416bb30ec72f";
            AssertPreviewCount(solidNodeId, 100);

            // output will be 99 Cylinders, so putting verification for all Cylinder creation
            for (int i = 0; i <= 99; i++)
            {
                var cylinder = GetPreviewValueAtIndex(solidNodeId, i) as Solid;
                Assert.IsNotNull(cylinder);
            }
        }

        [Test, Category("SmokeTests")]
        public void Core_CodeBlocks_01()
        {
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(104, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(80, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from First Section of DYN files (there are total 6 sections)

            // Checking decimal points in CBN: 3.142
            AssertPreviewValue("c184fd253755407ba6086b80996df549", 3.14);

            // Checking String in CBN: "Less is more."
            AssertPreviewValue("0aea97cdb971438c92165575e5d693df", "Less is more.");

            // Checking Multiplication in CBN: 3*5
            AssertPreviewValue("30848488006e4a92a23fbdd17e1a85d8", 15.0);
        }

        [Test, Category("SmokeTests")]
        public void Core_CodeBlocks_02()
        {
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(104, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(80, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from Second Section of DYN files (there are total 6 sections)

            var pointNodeID = "238c40dbe78b41238b7c21625a9ba06a";
            // output will be Point @ 0,-10
            var pt1 = GetPreviewValue(pointNodeID) as Point;
            Assert.AreEqual(0, pt1.X);
            Assert.AreEqual(-10, pt1.Y);

            var cbnNodeID = "26d3385a14f84aeca233a3d7bb0248f4";
            // output will be Point @ 0,0,0
            var pt = GetPreviewValue(cbnNodeID) as Point;
            Assert.AreEqual(0, pt.X);
            Assert.AreEqual(0, pt.Y);
            Assert.AreEqual(0, pt.Z);

            var pointVecAdd = "830475257e894a3ba744a4640def0e57";
            // Add Point and Vector in CBN
            var pt2 = GetPreviewValue(pointVecAdd) as Point;
            Assert.AreEqual(0, pt2.X);
            Assert.AreEqual(0, pt2.Y);
            Assert.AreEqual(1, pt2.Z);

            var nodeID = "3639dc028a7741e086e1cc34307bcb42";
            // Getting Property in CBN
            AssertPreviewValue(nodeID, 10);

            var cbnNodeID1 = "d55dadaea1f04a4ea23a7a897db02c85";
            // CBN with different input
            var pt3 = GetPreviewValue(cbnNodeID1) as Point;
            Assert.AreEqual(10, pt3.X);
            Assert.AreEqual(-10, pt3.Y);
            Assert.AreEqual(0, pt3.Z);
            //=============================================================================
        }

        [Test, Category("SmokeTests")]
        public void Core_CodeBlocks_03()
        {
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(104, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(80, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from Third Section of DYN files (there are total 6 sections)
            // This section is for creating and accessing List in CBN.

            // Multiline CBN
            string nodeID = "e211542ff30140eea2c43c5f87ae479b";
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(6, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);
            AssertPreviewValue(nodeID, new string[] { "alpha", "beta", "charlie", "delta", "echo" });

            // Indexing in CBN using Range expression
            AssertPreviewValue("1f6946f3b2ea490fb505737d8a9c22a0",
                new string[] { "charlie", "delta" });

            // Indexing in CBN for 2D Array
            AssertPreviewValue("8abf1aba83174c3d872cbd5cdb9c4437", "dirty clothes");
            //=============================================================================

        }

        [Test, Category("SmokeTests")]
        public void Core_CodeBlocks_04()
        {
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(104, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(80, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from Fifth Section of DYN files (there are total 6 sections)
            // This section is for Defining Functions in CBN

            // Writing Function CBN
            string nodeID = "c31fab7f5cd042c2a21b7644c748b29a";
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(0, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            // Using Function in CBN. Fucntion was writting in another CBN.
            AssertPreviewValue("c25ac1d9c87444778c5287b87496b940", 3);

            AssertPreviewValue("95710e5082324b8cb5e52a6f8a19e326", "catdog");
            //=============================================================================

        }

        [Test, Category("SmokeTests")]
        public void Core_CodeBlocks_05()
        {
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(104, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(80, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from Last Section of DYN files (there are total 6 sections)
            // This section is for Writing entire code in CBN
            // Calling function with different argument in CBN

            // Writing entire code in CBN to create some Geometry object.
            string nodeID = "15af5d1edb7347b5b849a541fc5ec868";
            var cbn = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(11, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            var nurbsCurve = GetPreviewValue(nodeID) as NurbsCurve;

            var cbnNodeID = "de7376216b3c49f28a28736d69b2ded3";
            AssertPreviewCount(cbnNodeID, 24);

            for (int i = 0; i <= 23; i++)
            {
                var nurbsCurve1 = GetPreviewValueAtIndex(cbnNodeID, i) as NurbsCurve;
                Assert.IsNotNull(nurbsCurve1);
            }
            //=============================================================================
        }

        [Test, Category("SmokeTests")]
        public void Core_ListLacing()
        {
            OpenSampleModel(@"en-US\Core\Core_ListLacing.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(19, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(20, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Shortest Lacing
            string lineNodeID = "e93fee37-1901-4162-8f73-6b5e98c1167f";
            AssertPreviewCount(lineNodeID, 6);

            // There should be 6 line created with Shortest lacing.
            for (int i = 0; i <= 5; i++)
            {
                var line = GetPreviewValueAtIndex(lineNodeID, i) as Line;
                Assert.IsNotNull(line);
            }

            // Longest Lacing
            string lineNodeID1 = "de2b1391-95e0-4b3d-b7f0-43d03d5c5b5a";
            AssertPreviewCount(lineNodeID1, 10);

            // There should be 10 lines created with Shortest lacing.
            for (int i = 0; i <= 9; i++)
            {
                var line = GetPreviewValueAtIndex(lineNodeID1, i) as Line;
                Assert.IsNotNull(line);
            }

            // CrossProduct Lacing
            string lineNodeID2 = "c16658b3-42dc-4a45-b58a-10e3f6bb2f67";
            AssertPreviewCount(lineNodeID2, 10);


        }

        [Test, Category("SmokeTests")]
        public void Core_Math_01()
        {
            OpenSampleModel(@"en-US\Core\Core_Math.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(151, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(157, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Operators Test
            // Arithmetic Operators in CBN and Formula Node.
            AssertPreviewValue("5ae3c3563e8343a9a12695b82cdcacc1", 1); // CBN
            AssertPreviewValue("4151d6e37dff4800a066d1b0fccfd423", 1); //Formula Node

            // Boolean Operators in CBN and Formula Node.
            AssertPreviewValue("cb92c120c2d74277b8b1213ba6800e91", false); // CBN 
            AssertPreviewValue("cb966af8f0d84965aeaa1ee0c4cbc373", false); //Formula Node

            // Rounding Operators in CBN and Formula Node.
            AssertPreviewValue("6583492a29c94ac4923f5b0a23f0838a", 4); // CBN 
            AssertPreviewValue("0a71dd7243b74370a2e9a05b61256827", 4); //Formula Node

        }

        [Test, Category("SmokeTests")]
        public void Core_Math_02()
        {
            OpenSampleModel(@"en-US\Core\Core_Math.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(151, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(157, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Testing from the section "Angles in nodes or Code Block are in radians."
            var cbnNodeID1 = "8e406f77898f4e2cb2091fcfc3104214";
            var arc = GetPreviewValue(cbnNodeID1) as Arc;
            Assert.IsNotNull(arc);
            Assert.AreEqual(20, arc.StartPoint.X);
            Assert.AreEqual(35, arc.StartPoint.Y);
            Assert.AreEqual(0, arc.StartPoint.Z);

            var cbnNodeID2 = "89f252d833e74575acbacb127c2fd303";
            var arc1 = GetPreviewValue(cbnNodeID2) as Arc;
            Assert.IsNotNull(arc1);
            Assert.AreEqual(-1.8875070607700219, arc1.EndPoint.X);
            Assert.AreEqual(30.369954957503836, arc1.EndPoint.Y);
            Assert.AreEqual(0, arc1.EndPoint.Z);

        }

        [Test, Category("SmokeTests")]
        public void Core_Math_03()
        {
            OpenSampleModel(@"en-US\Core\Core_Math.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(151, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(157, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Testing from the section "Angles in nodes or Code Block are in radians."
            var cbnNodeID1 = "ceec2673d0974245938d6a3a7abb24f4";
            var nurbsCurve = GetPreviewValue(cbnNodeID1) as NurbsCurve;
            Assert.IsNotNull(nurbsCurve);

            var cbnNodeID2 = "288d67b19d8948bd9dcc15f7f443b09c";
            var nurbsCurve1 = GetPreviewValue(cbnNodeID2) as NurbsCurve;
            Assert.IsNotNull(nurbsCurve1);

        }

        [Test, Category("SmokeTests")]
        public void Core_Math_04()
        {
            OpenSampleModel(@"en-US\Core\Core_Math.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(151, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(157, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Testing from the section "Different ways to approach dealing with Math functions."
            var cbnNodeID1 = "354a6f9e05d04370a72ba14a2c831362";
            var surface = GetPreviewValue(cbnNodeID1) as Surface;
            Assert.IsNotNull(surface);
            Assert.IsFalse(surface.Closed);
            Assert.IsFalse(surface.ClosedInU);
            Assert.IsFalse(surface.ClosedInV);

            var cbnNodeID2 = "019f2710869d4f26ac773e8332b78bb6";
            var surface1 = GetPreviewValue(cbnNodeID2) as Surface;
            Assert.IsNotNull(surface1);
            Assert.IsFalse(surface1.Closed);
            Assert.IsFalse(surface1.ClosedInU);
            Assert.IsFalse(surface1.ClosedInV);

            var cbnNodeID3 = "4941b1c0004542239aee1b7a936c7d4a";
            var surface2 = GetPreviewValue(cbnNodeID3) as Surface;
            Assert.IsNotNull(surface2);
            Assert.IsFalse(surface2.Closed);
            Assert.IsFalse(surface2.ClosedInU);
            Assert.IsFalse(surface2.ClosedInV);

            // All surfaces are creating in a differnet ways and involving many Math operations
            // that is the reason I have put verification all 3 surfaces.
        }

        [Test, Category("SmokeTests")]
        public void Core_PassingFunctions()
        {
            OpenSampleModel(@"en-US\Core\Core_PassingFunctions.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(16, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Creating Surface using Curve.Extrude
            string curveNodeID = "2abb7d97-6b23-4b26-91af-c11407503a66";
            AssertPreviewCount(curveNodeID, 2);

            // Creating Surface using Curve.Extrude
            string curveNodeID1 = "44b197d9-25e2-4afe-96d2-badc82b0e37d";
            AssertPreviewCount(curveNodeID1, 8);
        }

        [Test, Category("SmokeTests")]
        public void Core_Python()
        {
            OpenSampleModel(@"en-US\Core\Core_Python.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(13, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            AssertPreviewValue("b1be50ed-f93c-4d9c-b7c4-55d97e820672", 55);
            AssertPreviewValue("e14a4953-9115-4344-ac65-e7243e4975e3", 
                new int[] { 0, 1, 3, 6, 10, 15, 21, 28, 36, 45, 55 });

            // Checking first putput of CBN
            var watch1 = "516af144-3b9d-45b3-9f0f-4864055cf9e0";
            var line = GetPreviewValue(watch1) as Line;
            Assert.IsNotNull(line);
            Assert.AreEqual(-10, line.EndPoint.X);

            // Checking second putput of CBN
            var watch2 = "6d21b7b3-7073-42ed-8927-34d225a90f1a";
            var nurbsCurve = GetPreviewValue(watch2) as NurbsCurve;
            Assert.IsNotNull(nurbsCurve);
            Assert.AreEqual(3, nurbsCurve.Degree);

            // Checking third putput of CBN
            var watch3 = "df5d88d2-dfc5-42d4-b5dd-2ded5c79980c";
            var surface = GetPreviewValue(watch3) as Surface;
            Assert.IsNotNull(surface);
            Assert.IsFalse(surface.Closed);
            Assert.IsFalse(surface.ClosedInU);
            Assert.IsFalse(surface.ClosedInV);

        }

        [Test, Category("SmokeTests")]
        public void Core_RangeSyntax()
        {
            OpenSampleModel(@"en-US\Core\Core_RangeSyntax.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(31, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(22, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // CBN 0..#howMany..3;
            AssertPreviewValue("99552b5ccaf64ae485ab9e8660643726", new int[] { 0, 3, 6, 9, 12, 15, 18 });

            // CBN 0..10..2;
            AssertPreviewValue("2750ebb54e0b487fb2a122494fd159d8",
                new int[] { 0, 2, 4, 6, 8, 10 });

            // Range Sequence Node;
            AssertPreviewValue("8b6a80c25218459a8bf75a0cb5b558d6",
                new int[] { 0, 2, 4, 6, 8, 10, 12, 14 });

        }

        [Test, Category("SmokeTests")]
        public void Core_Strings()
        {
            OpenSampleModel(@"en-US\Core\Core_Strings.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(53, CurrentDynamoModel.CurrentWorkspace.Nodes.Count());
            Assert.AreEqual(56, CurrentDynamoModel.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Output from String.CountOccurrences, which is using so many String Operations.;
            AssertPreviewValue("a93dc4ad9f2a4158b994363ce737a070", 2);

            // String.Contains;
            Dictionary<int, object> validationData = new Dictionary<int, object>()
			{
				{4, true},
				{12, false},
                {16, true},
                {20, true},
			};
            SelectivelyAssertPreviewValues("d78216bfd28d4b15b6050b6a66f603c9", validationData);
        }

        [Test, Category("SmokeTests")]
        public void ImportExport_CSV_to_Stuff()
        {
            OpenSampleModel(@"en-US\ImportExport\ImportExport_CSV to Stuff.dyn");

            var filename = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            string resultPath = SampleDirectory + "Data\\helix.csv";
          
            //we cannot count on this path never changing as the samples path
            //must be updated to match dynamo version number
            filename.Value = resultPath;

            RunCurrentModel();

            const string lineNodeID = "0cde47c6-106f-4a0a-9566-872fd23a0a20";
            AssertPreviewCount(lineNodeID, 201);

            // There should be 201 Points.
            for (int i = 0; i <= 200; i++)
            {
                var point = GetPreviewValueAtIndex(lineNodeID, i) as Point;
                Assert.IsNotNull(point);
            }
        }

        [Test, Category("SmokeTests")]
        public void ImportExport_Data_To_Excel()
        {
            OpenSampleModel(@"en-US\ImportExport\ImportExport_Data To Excel.dyn", true);

            var filename = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            string resultPath = Path.Combine(TempFolder, "icosohedron_points.csv");

            //we cannot count on this path never changing as the samples path
            //must be updated to match dynamo version number
            filename.Value = resultPath;

            RunCurrentModel();

            const string lineNodeID = "48175079-300b-4b1d-9953-e23d570dce12";
            AssertPreviewCount(lineNodeID, 65);

            // Killing excel process if there is any after running the graph.
            foreach (var process in Process.GetProcessesByName("EXCEL"))
            {
                if (process.MainWindowTitle.Equals("icosohedron_points - Excel"))
                {
                    process.Kill();
                    break;
                }
            }
        }

        [Test, Category("ExcelTest"), Category("SmokeTests")]
        public void ImportExport_Excel_to_Dynamo()
        {
            OpenSampleModel(@"en-US\ImportExport\ImportExport_Excel to Dynamo.dyn", true);

            var filename = CurrentDynamoModel.CurrentWorkspace.FirstNodeFromWorkspace<Filename>();

            string inputFile = Path.Combine(SampleDirectory, "Data\\helix.xlsx");

            //we cannot count on this path never changing as the samples path
            //must be updated to match dynamo version number
            filename.Value = inputFile;

            RunCurrentModel();

            const string lineNodeID = "d538c147-b79f-4f11-9c00-1efd7f9b3c09";
            AssertPreviewCount(lineNodeID, 201);

            // There should be 201 Points.
            for (int i = 0; i <= 200; i++)
            {
                var point = GetPreviewValueAtIndex(lineNodeID, i) as Point;
                Assert.IsNotNull(point);
            }

            // Killing excel process if there is any after running the graph.
            foreach (var process in Process.GetProcessesByName("EXCEL"))
            {
                if (process.MainWindowTitle.Equals("helix - Excel"))
                {
                    process.Kill();
                    break;
                }
            }
        }
    }
}
