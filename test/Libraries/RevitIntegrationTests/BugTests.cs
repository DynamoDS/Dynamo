using System.IO;

using Autodesk.DesignScript.Geometry;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;

using NUnit.Framework;

using Revit.Elements;

using RevitNodesTests;

using RTF.Framework;

namespace RevitSystemTests
{
    [TestFixture]
    class BugTests : SystemTest
    {
        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\MAGN_66.rfa")]
        public void MAGN_66()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-66

            string samplePath = Path.Combine(workingDirectory, @".\\Bugs\MAGN_66.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test, Category("Failure")]
        [Category("RegressionTests")]
        [TestModel(@".\empty.rfa")]
        public void MAGN_102()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-102

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\\Bugs\MAGN_102_projectPointsToFace_selfContained.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(15, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\MAGN-122_wallsAndFloorsAndLevels.rvt")]
        public void MAGN_122()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-122
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\\Bugs\MAGN_122_wallsAndFloorsAndLevels.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(20, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(23, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            // Check for Walls Creation
            var walls = "b7392d1d-6333-4bed-b10d-7b83520d2c3e";
            AssertPreviewCount(walls, 6);

            // I will un-comment this code once Ian fix the issue with Document.IsFamilyDocument 
            //var wallinst = GetPreviewValueAtIndex(walls, 3) as Floor;
            //Assert.IsNotNull(wallinst);
            //Assert.IsNotNullOrEmpty(wallinst.Name);

            // Check for Floor Creation
            var floors = "25392912-b625-4020-8dd1-81923c5e4823";
            AssertPreviewCount(floors, 6);

            // I will un-comment this code once Ian fix the issue with Document.IsFamilyDocument 
            //var floorInst = GetPreviewValueAtIndex(floors, 3) as Floor;
            //Assert.IsNotNull(floorInst);
            //Assert.IsNotNullOrEmpty(floorInst.Name);

        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\MAGN-438_structuralFraming_simple.rvt")]
        public void MAGN_438()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-438
            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\\Bugs\MAGN-438_structuralFraming_simple.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(17, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(17, model.CurrentWorkspace.Connectors.Count);

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
        }

        [Test, Category("Failure")]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\MAGN_2576_DataImport.rvt")]
        public void MAGN_2576()
        {
            // Details are available in defect http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2576
            var model = ViewModel.Model;
            var workspace = ViewModel.Model.CurrentWorkspace;

            string samplePath = Path.Combine(workingDirectory, @".\\Bugs\Defect_MAGN_2576.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(12, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(14, model.CurrentWorkspace.Connectors.Count);

            ViewModel.Model.RunExpression();

            // there should not be any crash on running this graph.
            // below node should have an error because there is no selection for Floor Type.
            NodeModel nodeModel = workspace.NodeFromWorkspace("cc38d11d-cda2-4294-81dc-119776af7338");
            Assert.AreEqual(ElementState.Warning, nodeModel.State);

        }
        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\MAGN-3620_topo.rvt")]
        public void MAGN_3620()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3620

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\\Bugs\MAGN-3620_Elementgeometry.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(2, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(1, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Check for all Elements Creation
            var allElements = "c3d4e57e-2292-4d18-a603-30467df92d3f";
            AssertPreviewCount(allElements, 15);

            // Verification of Curve creation.
            var polyCurve = GetPreviewValueAtIndex(allElements, 1) as PolyCurve;
            Assert.IsNotNull(polyCurve);
            Assert.IsTrue(polyCurve.IsClosed);

            // Verify last geometry is Mesh
            var mesh = GetPreviewValueAtIndex(allElements, 14) as Mesh;
            Assert.IsNotNull(mesh);


        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\empty.rfa")]
        public void MAGN_3784()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3784

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\\Bugs\MAGN_3784.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(5, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // evaluate graph
            var refPtNodeId = "92774673-e265-4378-b8ba-aef86c1a616e";
            var refPt = GetPreviewValue(refPtNodeId) as ReferencePoint;
            Assert.IsNotNull(refPt);
            Assert.AreEqual(0, refPt.X);

            // change slider value and re-evaluate graph
            IntegerSlider slider = model.CurrentWorkspace.NodeFromWorkspace
                ("55a992c9-8f16-4c07-a049-b0627d78c93c") as IntegerSlider;
            slider.Value = 10;

            RunCurrentModel();

            refPt = GetPreviewValue(refPtNodeId) as ReferencePoint;
            Assert.IsNotNull(refPt);
            (10.0).ShouldBeApproximately(refPt.X);

            RunCurrentModel();

            // Cross check from Revit side.
            var selectElementType = "4a99826a-eb73-4831-857c-909579c7eb12";
            var refPt1 = GetPreviewValueAtIndex(selectElementType, 0) as ReferencePoint;
            AssertPreviewCount(selectElementType, 1);

            Assert.IsNotNull(refPt1);
            (10.0).ShouldBeApproximately(refPt1.X, 1.0e-6);

        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\empty.rfa")]
        public void MAGN_4511()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-34511

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, 
                                    @".\Bugs\MAGN_4511_NullInputToForm.ByLoftCrossSections.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            // If this test reaches here, it means there is no hang in system.
            Assert.Pass();

        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\MAGN_2589.rvt")]
        public void MAGN_2589()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-2589

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Bugs\MAGN_2589.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(20, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(29, model.CurrentWorkspace.Connectors.Count);

            // Validation for Geometry Instance import.
            var geometryInstance = "d017525b-2b02-44c4-88cf-2ed887c14a17";
            var solid = GetPreviewValue(geometryInstance) as ImportInstance;
            Assert.IsNotNull(solid);

            // change slider (Resolution) value and re-evaluate graph
            DoubleSlider slider = model.CurrentWorkspace.NodeFromWorkspace
                ("f6c91ebf-7eac-426c-81a9-97a0d5121fa5") as DoubleSlider;
            slider.Value = 35;

            RunCurrentModel();

            var solid1 = GetPreviewValue(geometryInstance) as ImportInstance;
            Assert.IsNotNull(solid1);

        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\MAGN_3408.rvt")]
        public void MAGN_3408()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3408

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Bugs\MAGN_3408.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(14, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(12, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Check for Wall
            var wall = "8f649c1c-d2c2-42dd-8b87-8dbe49afff4f";
            AssertPreviewCount(wall, 6);

            // get all Walls.
            for (int i = 0; i <= 5; i++)
            {
                var allwalls = GetPreviewValueAtIndex(wall, i) as Wall;
                Assert.IsNotNull(allwalls);
            }

            // Verification for first point from List
            var firsPoint = GetPreviewValue("f6d11b16-8f5e-45a1-9dce-cb8e8528baf2") as Point;
            Assert.IsNotNull(firsPoint);
        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\MAGN_529_GetFamilyInstanceLocation.rfa")]
        public void MAGN_529()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-529

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory,
                                            @".\Bugs\MAGN_529_GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(7, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(6, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Verification for final Surface created using Points captured from Family Location.
            var surface = GetPreviewValue("b28838eb-5f65-4f29-ae33-913becb3036e") as Surface;
            Assert.IsNotNull(surface);
        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\Bugs\SurfaceNew.rfa")]
        public void SurfaceCreatedUsingCurvesFromRevit()
        {
            /* This test is not dependent on any defect, this is for validating Curves from
               Revit can be used to Create Surface and Solids.*/

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Bugs\SurfaceNew.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(28, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(32, model.CurrentWorkspace.Connectors.Count);

            RunCurrentModel();

            // Check for Surface
            var surface = "e7c8c8ba-2150-4e78-9628-04cd46f59400";
            AssertPreviewCount(surface, 20);

            // get all Surfaces.
            for (int i = 0; i <= 19; i++)
            {
                var allSurface = GetPreviewValueAtIndex(surface, i) as Surface;
                Assert.IsNotNull(allSurface);
            }

            // Check for Solid
            var solid = "cbd9b0db-c803-4319-b059-1d2c149be8a4";
            AssertPreviewCount(solid, 20);

            // get all Solids.
            for (int i = 0; i <= 19; i++)
            {
                var allSolids = GetPreviewValueAtIndex(solid, i) as Autodesk.DesignScript.Geometry.Solid;
                Assert.IsNotNull(allSolids);
            }
        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\empty.rfa")]
        public void MAGN_4566()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4566
            // Passing empty list was resulting in Crash.

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory, @".\Bugs\MAGN_4566.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(10, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(9, model.CurrentWorkspace.Connectors.Count);

            /* As Nodes output is Null because of Empty List, this doesn’t need any validation on 
             any node. Test reaches here means there is no Crash on running the graph. */
        }

        [Test]
        [Category("RegressionTests")]
        [TestModel(@".\empty.rfa")]
        public void MAGN_4737()
        {
            // Details are available in defect 
            // http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-4566
            // Passing ReferencePlane to watch node was crashng Dynamo.

            var model = ViewModel.Model;

            string samplePath = Path.Combine(workingDirectory,
                                        @".\Bugs\MAGN_4737_ReferencePlaneInWatchNode.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            RunCurrentModel();

            // check all the nodes and connectors are loaded
            Assert.AreEqual(4, model.CurrentWorkspace.Nodes.Count);
            Assert.AreEqual(3, model.CurrentWorkspace.Connectors.Count);

            var refPlane = GetPreviewValue("85c1f8c5-00da-4a7e-94c7-655140e39f6a") as Plane;
            Assert.IsNotNull(refPlane);
        }

    }
}
