using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Dynamo.Utilities;
using Dynamo.Models;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Tests
{
    class DynamoSamples : DSEvaluationViewModelUnitTest
    {
        [Test, Category("SmokeTests")]
        public void Basics_Basic03()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Basics\Basics_Basic03.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            AssertPreviewValue("980b45cf-716b-4c8e-b7c2-47a23abf85ed", 9);

            AssertPreviewCount("42d739c7-8e21-4275-9a12-cbcbfd3cf569", 4);

        }

        [Test, Category("SmokeTests")]
        public void Core_AttractorPoint()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_AttractorPoint.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count());

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
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(89, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(68, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from First Section of DYN files (there are total 6 sections)

            // Checking decimal points in CBN: 3.142
            AssertPreviewValue("27c2f333-f51f-4a0c-9f71-70dc64f2ecbe", 3.142);

            // Checking String in CBN: "Less is more."
            AssertPreviewValue("4c8ddee8-e2b1-4472-9470-a4142f56ac97", "Less is more.");

            // Checking Multiplication in CBN: 3*5
            AssertPreviewValue("d4ae3f27-c68c-41dd-830d-36ee0f8f51cc", 15.0);

        }

        [Test, Category("SmokeTests")]
        public void Core_CodeBlocks_02()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(89, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(68, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from Second Section of DYN files (there are total 6 sections)

            var pointNodeID = "5ba4c6a0-4641-4624-86f7-d26506f554b0";
            // output will be Point @ 0,-10
            var pt1 = GetPreviewValue(pointNodeID) as Point;
            Assert.AreEqual(0, pt1.X);
            Assert.AreEqual(-10, pt1.Y);

            var cbnNodeID = "4ea1d0d8-4882-4f6f-b659-2dcb297db34e";
            // output will be Point @ 0,0,0
            var pt = GetPreviewValue(cbnNodeID) as Point;
            Assert.AreEqual(0, pt.X);
            Assert.AreEqual(0, pt.Y);
            Assert.AreEqual(0, pt.Z);

            var pointVecAdd = "9aa85384-3767-46da-b524-e0b969d9420a";
            // Add Point and Vector in CBN
            var pt2 = GetPreviewValue(pointVecAdd) as Point;
            Assert.AreEqual(0, pt2.X);
            Assert.AreEqual(0, pt2.Y);
            Assert.AreEqual(1, pt2.Z);

            var nodeID = "613c6d53-0eab-414f-9aeb-fc7c56de6900";
            // Getting Property in CBN
            AssertPreviewValue(nodeID, 10);

            var cbnNodeID1 = "34a4b2fc-a1c5-4c18-a0fe-48b1c3b8f711";
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
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(89, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(68, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from Third Section of DYN files (there are total 6 sections)
            // This section is for creating and accessing List in CBN.

            // Multiline CBN
            string nodeID = "d71e5f04-9928-437a-863c-5bf34666370b";
            var cbn = model.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(6, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);
            AssertPreviewValue(nodeID, new string[] { "alpha", "beta", "charlie", "delta", "echo" });

            // Indexing in CBN using Range expression
            AssertPreviewValue("0c192d74-5c94-41d1-a433-fb79536d5d73",
                new string[] { "charlie", "delta" });

            // Indexing in CBN for 2D Array
            AssertPreviewValue("72665884-ac4e-488f-84cd-6ad8b26b3c56", "dirty clothes");
            //=============================================================================

        }

        [Test, Category("SmokeTests")]
        public void Core_CodeBlocks_04()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(89, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(68, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from Fifth Section of DYN files (there are total 6 sections)
            // This section is for Defining Functions in CBN

            // Writing Function CBN
            string nodeID = "79bff081-608e-4b02-9ae0-e5197ff4a3a6";
            var cbn = model.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(0, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            // Using Function in CBN. Fucntion was writting in another CBN.
            AssertPreviewValue("cba93904-d0da-4452-9864-0d1c02706e95", 3);

            AssertPreviewValue("9256e209-fd59-492c-81c8-65cb84552ef5", "catdog");
            //=============================================================================

        }

        [Test, Category("SmokeTests")]
        public void Core_CodeBlocks_05()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_CodeBlocks.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(89, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(68, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            //============================================================================
            // Examples from Last Section of DYN files (there are total 6 sections)
            // This section is for Writing entire code in CBN
            // Calling function with different argument in CBN

            // Writing entire code in CBN to create some Geometry object.
            string nodeID = "e4a7b57e-420d-41d1-bdef-cea7956fbd3b";
            var cbn = model.CurrentWorkspace.NodeFromWorkspace(nodeID);
            Assert.AreNotEqual(ElementState.Error, cbn.State);
            Assert.AreEqual(11, cbn.OutPorts.Count);
            Assert.AreEqual(0, cbn.InPorts.Count);

            var nurbsCurve = GetPreviewValue(nodeID) as NurbsCurve;

            var cbnNodeID = "8dce8e89-1d9d-4ffe-b6ea-e7251ab1368b";
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
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_ListLacing.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(19, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count());

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
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_Math.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(199, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(201, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Operators Test
            // Arithmetic Operators in CBN and Formula Node.
            AssertPreviewValue("4151d6e3-7dff-4800-a066-d1b0fccfd423", 1); // CBN
            AssertPreviewValue("a8155c26-edb0-4a47-8dee-7fabc7e7ec6d", 1); //Formula Node

            // Boolean Operators in CBN and Formula Node.
            AssertPreviewValue("2df0798e-3eae-4425-b2ed-5c263f0c9042", false); // CBN 
            AssertPreviewValue("960221c2-8b6b-4478-a7d1-af11ae0e7a4b", false); //Formula Node

            // Rounding Operators in CBN and Formula Node.
            AssertPreviewValue("0a71dd72-43b7-4370-a2e9-a05b61256827", 4); // CBN 
            AssertPreviewValue("33784fd9-2846-408e-a58e-292504f0e5c8", 4); //Formula Node

        }

        [Test, Category("SmokeTests")]
        public void Core_Math_02()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_Math.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(199, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(201, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Testing from the section "Angles in nodes or Code Block are in radians."
            var cbnNodeID1 = "f87d0ef6-1cd4-46cc-a5eb-0a518296d27b";
            var line = GetPreviewValue(cbnNodeID1) as Line;
            Assert.IsNotNull(line);
            Assert.AreEqual(20, line.StartPoint.X);
            Assert.AreEqual(35, line.StartPoint.Y);
            Assert.AreEqual(0, line.StartPoint.Z);

            var cbnNodeID2 = "ccf76cd8-73c3-486a-a4e2-9493c9bc1f3f";
            var line1 = GetPreviewValue(cbnNodeID2) as Line;
            Assert.IsNotNull(line1);
            Assert.AreEqual(0, line1.EndPoint.X);
            Assert.AreEqual(35, line1.EndPoint.Y);
            Assert.AreEqual(0, line1.EndPoint.Z);

        }

        [Test, Category("SmokeTests")]
        public void Core_Math_03()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_Math.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(199, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(201, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Testing from the section "Angles in nodes or Code Block are in radians."
            var cbnNodeID1 = "288d67b1-9d89-48bd-9dcc-15f7f443b09c";
            var nurbsCurve = GetPreviewValue(cbnNodeID1) as NurbsCurve;
            Assert.IsNotNull(nurbsCurve);

            var cbnNodeID2 = "ceec2673-d097-4245-938d-6a3a7abb24f4";
            var nurbsCurve1 = GetPreviewValue(cbnNodeID2) as NurbsCurve;
            Assert.IsNotNull(nurbsCurve1);

            var cbnNodeID3 = "445edc7a-5bf1-4056-88aa-e503cda048a0";
            var nurbsCurve2 = GetPreviewValue(cbnNodeID3) as NurbsCurve;
            Assert.IsNotNull(nurbsCurve2);

        }

        [Test, Category("SmokeTests")]
        public void Core_Math_04()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_Math.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(199, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(201, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Testing from the section "Different ways to approach dealing with Math functions."
            var cbnNodeID1 = "de9ea606-0507-4a81-ad3a-8bfb0fc19e6f";
            var surface = GetPreviewValue(cbnNodeID1) as Surface;
            Assert.IsNotNull(surface);
            Assert.IsFalse(surface.Closed);
            Assert.IsFalse(surface.ClosedInU);
            Assert.IsFalse(surface.ClosedInV);

            var cbnNodeID2 = "354a6f9e-05d0-4370-a72b-a14a2c831362";
            var surface1 = GetPreviewValue(cbnNodeID2) as Surface;
            Assert.IsNotNull(surface1);
            Assert.IsFalse(surface1.Closed);
            Assert.IsFalse(surface1.ClosedInU);
            Assert.IsFalse(surface1.ClosedInV);

            var cbnNodeID3 = "4941b1c0-0045-4223-9aee-1b7a936c7d4a";
            var surface2 = GetPreviewValue(cbnNodeID3) as Surface;
            Assert.IsNotNull(surface2);
            Assert.IsFalse(surface2.Closed);
            Assert.IsFalse(surface2.ClosedInU);
            Assert.IsFalse(surface2.ClosedInV);

            var cbnNodeID4 = "019f2710-869d-4f26-ac77-3e8332b78bb6";
            var surface3 = GetPreviewValue(cbnNodeID4) as Surface;
            Assert.IsNotNull(surface3);
            Assert.IsFalse(surface3.Closed);
            Assert.IsFalse(surface3.ClosedInU);
            Assert.IsFalse(surface3.ClosedInV);

            // All surfaces are creating in a differnet ways and involving many Math operations
            // that is the reason I have put verification all 4 surfaces.
        }

        [Test, Category("SmokeTests")]
        public void Core_PassingFunctions()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_PassingFunctions.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count());

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
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_Python.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count());

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
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_RangeSyntax.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(35, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(24, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // CBN 0..#howMany..3;
            AssertPreviewValue("99552b5c-caf6-4ae4-85ab-9e8660643726", new int[] { 0, 3, 6, 9, 12 });

            // CBN 0..#10..2;
            AssertPreviewValue("b00ccf43-2f2e-49fe-aa81-d072a3598834",
                new int[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18 });

            // Range Sequence Node;
            AssertPreviewValue("8b6a80c2-5218-459a-8bf7-5a0cb5b558d6",
                new int[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, 18 });

        }

        [Test, Category("SmokeTests")]
        public void Core_Strings()
        {
            DynamoModel model = ViewModel.Model;
            OpenSampleModel(@"en-US\Core\Core_Strings.dyn");

            // check all the nodes and connectors are loaded
            Assert.AreEqual(32, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(37, model.CurrentWorkspace.Connectors.Count());

            RunCurrentModel();

            // Output from String.CountOccurrences, which is using so many String Operations.;
            AssertPreviewValue("a93dc4ad-9f2a-4158-b994-363ce737a070", 2);

            // String.Contains;
            Dictionary<int, object> validationData = new Dictionary<int, object>()
			{
				{4, true},
				{12, false},
                {16, true},
                {20, true},
			};
            SelectivelyAssertPreviewValues("d78216bf-d28d-4b15-b605-0b6a66f603c9", validationData);
        }

        [Test, Category("SmokeTests")]
        public void ImportExport_CSV_to_Stuff()
        {
            OpenSampleModel(@"en-US\ImportExport\ImportExport_CSV to Stuff.dyn");

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            string resultPath = GetSampleDirectory() + "Data\\helix.csv";
            // Although old path is a hard coded but that is not going to change 
            // because it is saved in DYN which we have added in Samples folder.
            filename.Value = filename.Value.Replace
                ("C:\\ProgramData\\Dynamo\\0.8\\samples\\Data\\helix.csv", resultPath);

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
            OpenSampleModel(@"en-US\ImportExport\ImportExport_Data To Excel.dyn");

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            string resultPath = GetSampleDirectory() + "Data\\icosohedron_points.csv";
            // Although old path is a hard coded but that is not going to change 
            // because it is saved in DYN which we have added in Samples folder.
            filename.Value = filename.Value.Replace
                ("C:\\ProgramData\\Dynamo\\0.8\\samples\\Data\\icosohedron_points.csv", resultPath);

            //RunCurrentModel();

            const string lineNodeID = "48175079-300b-4b1d-9953-e23d570dce12";
            AssertPreviewCount(lineNodeID, 65);

            // Killing excel process if there is any after running the graph.
            Process[] procs = Process.GetProcessesByName("excel");
            foreach (Process proc in procs)
                proc.Kill();

        }

        [Test]
        [Category("Failure")]
            //Todo Ritesh: Locally passing but failing on CI.
            //After fixing issue with this test case add Smoke Test Category.
        public void ImportExport_Excel_to_Dynamo()
        {
            OpenSampleModel(@"en-US\ImportExport\ImportExport_Excel to Dynamo.dyn");

            var filename = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSCore.File.Filename>();

            string resultPath = GetSampleDirectory() + "Data\\helix.xlsx";
            // Although old path is a hard coded but that is not going to change 
            // because it is saved in DYN which we have added in Samples folder.
            filename.Value = filename.Value.Replace
                ("C:\\ProgramData\\Dynamo\\0.8\\samples\\Data\\helix.xlsx", resultPath);

            //RunCurrentModel();

            const string lineNodeID = "d538c147-b79f-4f11-9c00-1efd7f9b3c09";
            AssertPreviewCount(lineNodeID, 201);

            // There should be 201 Points.
            for (int i = 0; i <= 200; i++)
            {
                var point = GetPreviewValueAtIndex(lineNodeID, i) as Point;
                Assert.IsNotNull(point);
            }

            // Killing excel process if there is any after running the graph.
            Process[] procs = Process.GetProcessesByName("excel");
            foreach (Process proc in procs)
                proc.Kill();
        }
    }
}
