using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using DSRevitNodesUI;
using Dynamo.Utilities;
using NUnit.Framework;
using RTF.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FamilyTests:DynamoRevitUnitTestBase
    {   
        [Test]
        [TestModel(@".\Family\GetCurvesFromFamilyInstance.rfa")]
        public void GetCurvesFromFamilyInstance()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_testPath, @".\Family\GetCurvesFromFamilyInstance.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //var node = (CurvesFromFamilyInstance)dynSettings.Controller.DynamoModel.Nodes.First(x => x is CurvesFromFamilyInstance);
            //Assert.IsTrue(node.OldValue.IsCollection);

            //// check that we get the correct number of curves back from the instance
            //var list = node.OldValue.GetElements();
            //Assert.AreEqual(4, list.Count());

            //// get the filter node from dynamo and  make sure it has an empty list as output
            ////we've filtered the list of distances by 0 to make sure distances between orginal points
            //// and end points of new curves are 0.
            //var filterNode = (FilterOut)dynSettings.Controller.DynamoModel.Nodes.First(x => x is FilterOut);
            //Assert.IsTrue(filterNode.OldValue.IsCollection);
            
            //// ensure it is empty
            //var filterList = filterNode.OldValue.GetElements();
            //Assert.AreEqual(0.0,filterList.Count);

            Assert.Inconclusive("Porting : FilterOut");

        }
        
        [Test]
        [TestModel(@".\Family\GetFamilyInstancesByType.rvt")]
        public void GetFamilyInstancesByType()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstancesByType.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            Assert.AreEqual(100, GetPreviewValue("5eac6ab9-e736-49a9-a90a-8b6d93676813"));
        }

        [Test]
        [TestModel(@".\Family\GetFamilyInstanceLocation.rvt")]
        public void GetFamilyInstanceLocation()
        {
            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());

            AssertPreviewCount("4274fd18-23b8-4c5c-9006-14d927fa3ff3", 100);
        }

        [Test]
        [TestModel(@".\Family\AC_locationStandAlone.rfa")]
        public void CanLocateAdaptiveComponent()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\AC_locationStandAlone.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            var pnt = GetPreviewValue("79dde258-ddce-49b7-9700-da21b2d5a9ae") as Autodesk.DesignScript.Geometry.Point;
            Assert.IsNotNull(pnt);

            //Check the value is (-9.586931, -35.209237, 8.170673) in the metric system
            Assert.IsTrue(IsFuzzyEqual(pnt.X, -9.586931, 1.0e-6));
            Assert.IsTrue(IsFuzzyEqual(pnt.Y, -35.209237, 1.0e-6));
            Assert.IsTrue(IsFuzzyEqual(pnt.Z, 8.170673, 1.0e-6));
        }

        [Test]
        [TestModel(@".\Family\AC_locationInDividedSurface.rfa")]
        public void CanLocateAdaptiveComponentInDividedSurface()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\AC_locationInDividedSurface.dyn");
            string testPath = Path.GetFullPath(samplePath);

            Controller.DynamoViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            AssertPreviewCount("76076507-d16e-4480-802c-14ba87d88f81", 25);
        }
    }
}
