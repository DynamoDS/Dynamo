﻿using System;
using System.IO;
using Dynamo.Nodes;
using Dynamo.Selection;
using System.Linq;
using Dynamo.Utilities;
using NUnit.Framework;
using DSCoreNodesUI;
using DSCore.File;
using Revit.Elements;
using RTF.Framework;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Tests
{
    [TestFixture]
    class SampleTests : DynamoRevitUnitTestBase
    {
        #region OLD Sample Tests

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CreatePointSequenceSample()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(_testPath, @".\Samples\createpoint_sequence.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(8, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            // evaluate  graph
            RunCurrentModel();

            var refPtNodeId = "d615cc73-d32d-4b1f-b519-0b8f9b903ebf";
            AssertPreviewCount(refPtNodeId, 9);

            // get 8th reference point
            var refPt = GetPreviewValueAtIndex(refPtNodeId, 8) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(80, refPt.Z, 0.00000001);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CreatePointEndSample()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\createpointend.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            RunCurrentModel();

            // test copying and pasting the workflow
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.AddRange(ViewModel.Model.Nodes);
            model.Copy(null);
            model.Paste(null);

            // evaluate graph
            var refPtNodeId = "16d1ceb2-c780-45d1-9dfb-d9c49836a931";
            var refPt = GetPreviewValue(refPtNodeId) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(63.275, refPt.Z, 0.0000001);

            // change slider value and re-evaluate graph
            DoubleSlider slider = model.CurrentWorkspace.NodeFromWorkspace
                ("2eb70bdb-773d-4cf4-a10e-828dd39a0cca") as DoubleSlider;
            slider.Value = 56.78;

            RunCurrentModel();

            refPt = GetPreviewValue(refPtNodeId) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(56.78, refPt.Z);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void RefGridSlidersSample()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(_testPath, @".\Samples\refgridsliders.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.GreaterOrEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.GreaterOrEqual(10, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            // evaluate graph
            RunCurrentModel();

            var refPtNodeId = "69dcdcdc-941f-46f9-8e8b-242b61e74e80";
            AssertPreviewCount(refPtNodeId, 36);

            var refPt = GetPreviewValueAtIndex(refPtNodeId, 23) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(57, refPt.Y, 0.000001);

            // change slider value and re-evaluate graph
            DoubleSlider slider = model.CurrentWorkspace.NodeFromWorkspace("5adff29b-3cac-4387-8d1d-b75ceb9c6dec") as DoubleSlider;
            slider.Value = 3.5;

            RunCurrentModel();
            AssertPreviewCount(refPtNodeId, 16);
        }

        [Test]
        [TestModel(@".\Samples\DivideSelectedCurve.rfa")]
        public void DivideSelectedCurveSample()
        {
            var model = ViewModel.Model;
            string samplePath = Path.Combine(_testPath, @".\Samples\divideselectedcurve.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.GreaterOrEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.GreaterOrEqual(5, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            // evaluate graph
            RunCurrentModel();

            var refPtNodeId = "7e23ea22-600f-4263-89af-defa541e90f2";
            AssertPreviewCount(refPtNodeId, 33);

            var refPt = GetPreviewValueAtIndex(refPtNodeId, 3) as ReferencePoint;
            Assert.IsNotNull(refPt);
            //Assert.AreEqual(57, refPt.Y, 0.000001);

            // change slider value and re-evaluate graph
            DoubleSlider slider = model.CurrentWorkspace.NodeFromWorkspace
                ("a1844c0d-99bd-4a32-84f8-2e94685f3229") as DoubleSlider;
            slider.Value = 15.0;

            RunCurrentModel();
            AssertPreviewCount(refPtNodeId, 15);
            var refPt1 = GetPreviewValueAtIndex(refPtNodeId, 3) as ReferencePoint;
            Assert.IsNotNull(refPt1);

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void FormFromCurveSelectionSample()
        {

            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\FormFromCurveSelection.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);
            AssertNoDummyNodes();

            // evaluate  graph
            RunCurrentModel();

            var form = GetPreviewValue("380e6666-c37d-477b-860f-da5c01f3e32e") as Form;
            Assert.IsNotNull(form);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void ScalableGraphFunctionSample()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\scalablegraphfunction.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var modelCurve = "6b38f5da-3373-4226-bdd6-4ff60f275b23";
            AssertPreviewCount(modelCurve, 32);

            // get all Model Curves.
            for (int i = 0; i <= 31; i++)
            {
                var curves = GetPreviewValueAtIndex(modelCurve, i) as ModelCurve;
                Assert.IsNotNull(curves);
            }
        }

        [Test]
        [TestModel(@".\Samples\instparam.rvt")]
        public void InstParamSample()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\instparam.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(4, model.CurrentWorkspace.Connectors.Count);

            AssertNoDummyNodes();

            // evaluate  graph
            RunCurrentModel();

            var value = GetPreviewValue("e66d5203-378b-4dfe-9aea-4415176caa52");
            Assert.AreEqual(value, 0.6);

        }

        [Test]
        [TestModel(@".\Samples\instparammassfamilies.rvt")]
        public void InstParam2MassesSample()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\instparam2masses.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            // check all the nodes and connectors are loaded
            Assert.AreEqual(8, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(10, model.CurrentWorkspace.Connectors.Count);

            AssertNoDummyNodes();

            // evaluate  graph
            RunCurrentModel();

            var value = GetPreviewValue("795cc658-d64e-4808-af66-a83f655a75e2");
            Assert.IsNotNull(value);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Attractor_1()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\AttractorLogic_End.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var modelCurve = "6b38f5da-3373-4226-bdd6-4ff60f275b23";
            AssertPreviewCount(modelCurve, 225);

            // get all Model Curves.
            for (int i = 0; i <= 224; i++)
            {
                var curves = GetPreviewValueAtIndex(modelCurve, i) as ModelCurve;
                Assert.IsNotNull(curves);
            }

        }

        [Test]
        [TestModel(@".\Samples\IndexedFamilyInstances.rfa")]
        public void IndexedFamilyInstances()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\IndexedFamilyInstances.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(13, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();
            var family = "277dec13-7918-47c2-b33e-2346058dc5c2";
            AssertPreviewCount(family, 20);

            // get all Family Instances.
            for (int i = 0; i <= 19; i++)
            {
                var familyInstance = GetPreviewValueAtIndex(family, i) as FamilyInstance;
                Assert.IsNotNull(familyInstance);
            }

        }

        [Test]
        [TestModel(@".\Samples\tesselation.rfa")]
        public void Tesselation_1()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\2dDomain.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(23, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(32, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();
            var refPoint = "b0684654-367e-4cbf-bfbc-ff28df9afef9";
            AssertPreviewCount(refPoint, 746);

            // get all Reference Points.
            for (int i = 0; i <= 745; i++)
            {
                var point = GetPreviewValueAtIndex(refPoint, i) as ReferencePoint;
                Assert.IsNotNull(point);
            }

        }

        [Test]
        [TestModel(@".\Samples\tesselation.rfa")]
        public void Tesselation_2()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\tesselationwithcoincidentgrids.dyn");
            string testPath = Path.GetFullPath(samplePath);
            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(19, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(25, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();
            var refPointNodeID = "a80c323f-7443-42fd-a38c-4a84623fdeb5";
            AssertPreviewCount(refPointNodeID, 122);

            // get all Reference Points.
            for (int i = 0; i <= 120; i++)
            {
                var point = GetPreviewValueAtIndex(refPointNodeID, i) as Point;
                Assert.IsNotNull(point);
            }
        }

        [Test]
        [TestModel(@".\Samples\tesselation.rfa")]
        public void Tesselation_3()
        {
            //TODO:[Ritesh] Some random behgavior in output, need to check with Ian.
            // Will enable verification after fixing test case.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4041

            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\tesselation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            //var curve = "ca608a4e-0430-4cee-a0bb-61e81f198e8b";
            //AssertPreviewCount(curve, 479);

            //// get all Lines created using Voronoi on Face
            //for (int i = 0; i <= 478; i++)
            //{
            //    var lines = GetPreviewValueAtIndex(curve, i) as Line;
            //    Assert.IsNotNull(lines);
            //}
        }

        [Test]
        [TestModel(@".\Samples\tesselation.rfa")]
        public void Tesselation_4()
        {
            //TODO:[Ritesh] Some random behgavior in output, need to check with Ian.
            // Will enable verification after fixing test case.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4041

            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\tesselation_types.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(19, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            //var curve = "3a3c0d74-e4d1-47f6-82e1-ec32f28b8d78";
            //AssertPreviewCount(curve, 359);

            //// get all Lines
            //for (int i = 0; i <= 354; i++)
            //{
            //    var line = GetPreviewValueAtIndex(curve, i) as Line;
            //    Assert.IsNotNull(line);
            //}
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Transforms_TranslateAndRotatesequence()
        {
            // TODO:[Ritesh] Need to add more verification.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4041

            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\TranslateandRotatesequence.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(19, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Transforms_TranslateAndRotate()
        {
            // TODO:[Ritesh] Need to add more verification.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4041

            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\TranslateandRotate.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Formulas_FormulaCurve()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\FormulaCurve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation for Model Curve
            var modelCurveNodeID = "981b8d59-5d7d-4fc5-869c-0b7ca88fc4eb";
            var curve = GetPreviewValue(modelCurveNodeID) as ModelCurve;
            Assert.IsNotNull(curve);

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Formulas_ScalableCircle()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\ScalableCircle.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var refPointNodeID = "f3ea3259-dffc-4917-b561-ee1552700200";
            AssertPreviewCount(refPointNodeID, 10);
            // get all Ref Points
            for (int i = 0; i <= 9; i++)
            {
                var point = GetPreviewValueAtIndex(refPointNodeID, i) as ReferencePoint;
                Assert.IsNotNull(point);
            }
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Spreadsheets_ExcelToStuff()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\ExceltoStuff.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(22, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            var workspace = model.CurrentWorkspace;
            var filePickerNode = workspace.FirstNodeFromWorkspace<Filename>();

            // remap the file name as Excel requires an absolute path
            var excelFilePath = Path.Combine(_testPath, @".\Samples\");
            //excelFilePath = Path.Combine(excelFilePath, excelFileName);
            excelFilePath = Path.Combine(excelFilePath, "helix.xlsx");
            filePickerNode.Value = excelFilePath;

            Assert.IsFalse(string.IsNullOrEmpty(excelFilePath));
            Assert.IsTrue(File.Exists(excelFilePath));

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void Spreadsheets_CSVToStuff()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\CSVtoStuff.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(11, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(7, model.CurrentWorkspace.Connectors.Count);

            var workspace = model.CurrentWorkspace;
            var filePickerNode = workspace.FirstNodeFromWorkspace<Filename>();

            // remap the file name as CSV requires an absolute path
            var excelFilePath = Path.Combine(_testPath, @".\Samples\");
            excelFilePath = Path.Combine(excelFilePath, "helix_smaller.csv");

            filePickerNode.Value = excelFilePath;

            Assert.IsFalse(string.IsNullOrEmpty(excelFilePath));
            Assert.IsTrue(File.Exists(excelFilePath));
        }

        [Ignore]
        [TestModel(@".\empty.rfa")]
        public void Rendering_hill_climbing_simple()
        {
            // referencing the samples directly from the samples folder
            // and the custom nodes from the distrib folder

            var model = ViewModel.Model;
            // look at the sample folder and one directory up to get the distrib folder and combine with defs folder
            string customNodePath = Path.Combine(Path.Combine(_samplesPath, @"..\\"), @".\
\Dynamo Sample Custom Nodes\dyf\");
            // get the full path to the distrib folder and def folder
            string fullCustomNodePath = Path.GetFullPath(customNodePath);

            string samplePath = Path.Combine(_samplesPath, @".\25 Rendering\hill_climbing_simple.dyn");
            string testPath = Path.GetFullPath(samplePath);

            // make sure that the two custom nodes we need exist
            string customDefPath1 = Path.Combine(fullCustomNodePath, "ProduceChild.dyf");
            string customDefPath2 = Path.Combine(fullCustomNodePath, "DecideNewParent.dyf");

            Assert.IsTrue(File.Exists(customDefPath1), "Cannot find specified custom definition to load for testing at." + customDefPath1);
            Assert.IsTrue(File.Exists(customDefPath2), "Cannot find specified custom definition to load for testing." + customDefPath2);

            Assert.DoesNotThrow(() =>
                         ViewModel.Model.CustomNodeManager.AddFileToPath(customDefPath2));
            Assert.DoesNotThrow(() =>
                          ViewModel.Model.CustomNodeManager.AddFileToPath(customDefPath1));



            ViewModel.OpenCommand.Execute(testPath);

            var nodes = ViewModel.Model.Nodes.OfType<DummyNode>();

            double noOfNdoes = nodes.Count();

            if (noOfNdoes >= 1)
            {
                Assert.Fail("Number of Dummy Node found in Sample: " + noOfNdoes);
            }

            Assert.AreEqual(2, ViewModel.Model.CustomNodeManager.LoadedCustomNodes.Count);
            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());


            var workspace = model.CurrentWorkspace;

            Assert.Fail("Mike to update for CB2B1");
        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void AllCurveTest()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\all curve test.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(40, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(42, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var listNodeID = "41d7967e-b10f-42cd-84e1-2321053dfa87";
            AssertPreviewCount(listNodeID, 5);

            var arc = GetPreviewValueAtIndex(listNodeID, 0) as Arc;
            Assert.IsNotNull(arc);

            var circle = GetPreviewValueAtIndex(listNodeID, 2) as Circle;
            Assert.IsNotNull(circle);

            var ellipseArc = GetPreviewValueAtIndex(listNodeID, 3) as EllipseArc;
            Assert.IsNotNull(ellipseArc);

            var ellipse = GetPreviewValueAtIndex(listNodeID, 4) as Ellipse;
            Assert.IsNotNull(ellipse);

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void ArcAndLine()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\Arc and Line.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            
            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(19, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(18, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var nodeID = "6de77be2-fa0f-41ec-a494-151d47ad8274";

            var arc = GetPreviewValueAtIndex(nodeID, 0) as Arc;
            Assert.IsNotNull(arc);

            var circle = GetPreviewValueAtIndex(nodeID, 1) as Line;
            Assert.IsNotNull(circle);

            var modelCurve = GetPreviewValue("a91af17e-111e-4945-9f74-9ac09d168ad4") as ModelCurve;
            Assert.IsNotNull(modelCurve);
        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void Circle()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\circle.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var circle = GetPreviewValue("e478e251-3144-4aac-99fc-c92e520f518e") as Circle;
            Assert.IsNotNull(circle);

            var modelCurve = GetPreviewValue("e2f3ee81-e1f8-4fc0-85d2-0d35ab675b2b") as ModelCurve;
            Assert.IsNotNull(modelCurve);

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void Ellipse()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\ellipse.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(16, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Ellipse passed to ModelCurve to create similar Ellipse in Revit.
            var modelCurve = GetPreviewValue("e2f3ee81-e1f8-4fc0-85d2-0d35ab675b2b") as ModelCurve;
            Assert.IsNotNull(modelCurve);

        }

        [Test]
        [TestModel(@".\Samples\AllCurves.rfa")]
        public void ConnectTwoPointArraysWithoutPython()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\connect two point arrays without python.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(16, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(16, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var nodeID = "0947eea4-3129-488d-bc7b-88e2eda01995";
            AssertPreviewCount(nodeID, 4);

            // get all Curves
            for (int i = 0; i <= 3; i++)
            {
                var curve = GetPreviewValueAtIndex(nodeID, i) as CurveByPoints;
                Assert.IsNotNull(curve);
            }
        }

        [Test]
        public void CreateSineWaveFromSelectedCurve()
        {
            // TODO:[Ritesh] Need to add more verification.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4041

            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\create sine wave from selected curve.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(3, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(2, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

        }

        [Test]
        public void CreateSineWaveFromSelectedPoints()
        {
            // TODO:[Ritesh] Need to add more verification.
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4041

            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\create sine wave from selected points.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();
            // check all the nodes and connectors are loaded
            Assert.AreEqual(6, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(5, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

        }

        #endregion

        #region New Sample Tests

        [Test, Category("SmokeTests")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_Adaptive_Component_Placement()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\Revit_Adaptive Component Placement.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();


            var refPtNodeId = "357e7a53-361c-4c1e-81ae-83e16213a39a";
            AssertPreviewCount(refPtNodeId, 9);

            // get all AdaptiveComponent.
            for (int i = 0; i <= 8; i++)
            {
                var refPt = GetPreviewValueAtIndex(refPtNodeId, i) as AdaptiveComponent;
                Assert.IsNotNull(refPt);
            }

            // change slider value and re-evaluate graph
            IntegerSlider slider = model.CurrentWorkspace.NodeFromWorkspace
                ("cc3ba87a-cc1f-4db6-99f2-769f3020e0df") as IntegerSlider;
            slider.Value = 6;

            RunCurrentModel();

            AssertPreviewCount(refPtNodeId, 6);

            // Now there should be only 6 AdaptiveComponent in Revit.
            for (int i = 0; i <= 5; i++)
            {
                var refPt = GetPreviewValueAtIndex(refPtNodeId, i) as AdaptiveComponent;
                Assert.IsNotNull(refPt);
            }

        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_Color()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\Revit_Color.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(16, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(21, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var refPtNodeId = "ecb2936d-6ee9-4b99-9ab1-24269ffedfc5";
            AssertPreviewCount(refPtNodeId, 10);

            // get all Walls.
            for (int i = 0; i <= 9; i++)
            {
                var refPt = GetPreviewValueAtIndex(refPtNodeId, i) as Wall;
                Assert.IsNotNull(refPt);
            }
        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_Floors_and_Framing()
        {
            // this test marked as Ignore because on running it is throwing error from Revit side.
            // if I run it manually there is no error. Will discuss this with Ian

            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\Revit_Floors and Framing.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(31, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(35, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var floorByTypeAndLevel = "4074e4e4-c6ee-4413-8cbb-cc9af5b6127f";
            AssertPreviewCount(floorByTypeAndLevel, 5);

            // get all Floors.
            for (int i = 0; i <= 4; i++)
            {
                var floors = GetPreviewValueAtIndex(floorByTypeAndLevel, i) as Floor;
                Assert.IsNotNull(floors);
            }

            var structuralFraming = "205a479f-3b9e-4f4f-866e-6901fde3d9ca";
            AssertPreviewCount(structuralFraming, 20);

            // get all Structural Framing elements
            for (int i = 0; i <= 19; i++)
            {
                var framing = GetPreviewValueAtIndex(structuralFraming, i) as StructuralFraming;
                Assert.IsNotNull(framing);
            }
        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_ImportSolid()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\Revit_ImportSolid.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(18, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(20, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var NodeId = "e3fedc00-247a-4971-901c-7fcb063344c6";

            // get imported geometry instance.
            var geometryInstance = GetPreviewValue(NodeId) as ImportInstance;
            Assert.IsNotNull(geometryInstance);

        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_PlaceFamiliesByLevel_Set_Parameters()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine
                (_testPath, @".\Samples\Revit_PlaceFamiliesByLevel_Set Parameters.dyn");

            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var familyInstance = "026aadc9-644e-4e6c-b35c-bf1aec67045c";
            AssertPreviewCount(familyInstance, 27);

            // get all Family instances.
            for (int i = 0; i <= 26; i++)
            {
                var family = GetPreviewValueAtIndex(familyInstance, i) as FamilyInstance;
                Assert.IsNotNull(family);
            }
        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\Samples\DynamoSample.rvt")]
        public void Revit_StructuralFraming()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine
                (_testPath, @".\Samples\Revit_StructuralFraming.dyn");

            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(15, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            var familyInstance = "e98dab6a-e6bc-4da8-84b6-d756caee48fe";
            AssertPreviewCount(familyInstance, 9);

            // get all Families.
            for (int i = 0; i <= 8; i++)
            {
                var family = GetPreviewValueAtIndex(familyInstance, i) as StructuralFraming;
                Assert.IsNotNull(family);
            }
        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\empty.rfa")]
        public void Geometry_Curves()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\Geometry_Curves.dyn");

            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(20, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(22, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation for Model Curves.
            var modelCurve = "e8fc9b8f-03ce-4351-8333-6ae47a62ac07";
            AssertPreviewCount(modelCurve, 4);

            for (int i = 0; i <= 3; i++)
            {
                var curve = GetPreviewValueAtIndex(modelCurve, i) as ModelCurve;
                Assert.IsNotNull(curve);
            }
        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\empty.rfa")]
        public void Geometry_Points()
        {
            var model = ViewModel.Model;

            string samplePath = Path.Combine(_testPath, @".\Samples\Geometry_Points.dyn");

            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(13, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation for Reference Points.
            var refPoints = "6e4226cf-674b-4c94-9c78-7818af53464e";
            AssertPreviewCount(refPoints, 11);

            for (int i = 0; i <= 10; i++)
            {
                var points = GetPreviewValueAtIndex(refPoints, i) as ReferencePoint;
                Assert.IsNotNull(points);
            }
        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\empty.rfa")]
        public void Geometry_Solids()
        {
            var model = ViewModel.Model;
            OpenModel(@".\Geometry\Geometry_Solids.dyn");

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(43, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(51, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation of Form creation
            var formByLoft = "d8b7c8f4-2f70-4e3b-a913-91316e009759";
            var surface = GetPreviewValue(formByLoft) as Form;
            Assert.IsNotNull(surface);

            // Validation for Geometry Instance import.
            var geometryInstance = "e0e441ca-08a4-48b7-8f64-96e65e320376";
            var solid = GetPreviewValue(geometryInstance) as ImportInstance;
            Assert.IsNotNull(solid);

            // Validation for Solids.
            var trimmedSolid = "66746ec0-c2bd-4a5c-a365-63fe4ca239ba";
            var solid1 = GetPreviewValueAtIndex(trimmedSolid, 0) as Solid;
            Assert.IsNotNull(solid1);

        }

        [Test, Category("SmokeTests")]
        [TestModel(@".\empty.rfa")]
        public void Geometry_Surfaces()
        {
            var model = ViewModel.Model;
            OpenModel(@".\Geometry\Geometry_Surfaces.dyn");

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(42, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(49, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Validation of Form creation
            var formByLoft = "ae4bfdfe-524a-4a5c-bd64-75fd56be89af";
            var surface = GetPreviewValue(formByLoft) as Form;
            Assert.IsNotNull(surface);

            // Validation for Geometry Instance import.
            var geometryInstance = "c1e10fd1-974e-4374-b6ec-0766615d3df4";
            var solid = GetPreviewValue(geometryInstance) as ImportInstance;
            Assert.IsNotNull(solid);

            // Validation for NurbsSurface
            var nurbsSurface = "4859e415-f137-4590-b107-c528167769c0";
            var surface1 = GetPreviewValue(nurbsSurface) as NurbsSurface;
            Assert.IsNotNull(surface1);

            // Validation for Surface
            var extrudedSurface = "660a5c8e-c828-45ff-849e-2d65e04c7206";
            var surface2 = GetPreviewValue(extrudedSurface) as Surface;
            Assert.IsNotNull(surface2);

        }
        #endregion
    }

}
