﻿using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitTestFramework;

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
        [TestModel(@".\Family\GetFamilyInstancesByType.rv")]
        public void GetFamilyInstancesByType()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstancesByType.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //var node =
            //    (GetFamilyInstancesByType)dynSettings.Controller.DynamoModel.Nodes.First(x => x is GetFamilyInstancesByType);
            //Assert.IsTrue(node.OldValue.IsCollection);

            //var list = node.OldValue.GetElements();
            //Assert.AreEqual(100, list.Count());

            Assert.Inconclusive("Porting : GetFamilyInstancesByType");
        }

        [Test]
        [TestModel(@".\Family\GetFamilyInstanceLocation.rvt")]
        public void GetFamilyInstanceLocation()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression());
        }

        [Test]
        [TestModel(@".\Family\AC_locationStandAlone.rfa")]
        public void CanLocateAdaptiveComponent()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_testPath, @".\Family\AC_locationStandAlone.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            ////ensure that the collection of points
            ////returned has the 4 corner points
            //var locNode = model.AllNodes.FirstOrDefault(x => x is GetFamilyInstanceLocation);
            //Assert.IsNotNull(locNode);
            //var locs = locNode.OldValue.GetElements();
            //Assert.AreEqual(4, locs.Count());

            ////asert that the list is full of XYZs
            //Assert.IsTrue(locs.All(x => x.Data is XYZ));

            Assert.Inconclusive("Porting : GetFamilyInstancesByType");
        }

        [Test]
        [TestModel(@".\Family\AC_locationInDividedSurface.rfa")]
        public void CanLocateAdaptiveComponentInDividedSurface()
        {
            //var model = dynSettings.Controller.DynamoModel;

            //string samplePath = Path.Combine(_testPath, @".\Family\AC_locationInDividedSurface.dyn");
            //string testPath = Path.GetFullPath(samplePath);

            //model.Open(testPath);
            //Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            ////ensure that you get a list of lists
            ////with 5 lists each with 5 lists of 4 points
            //var locNode = model.AllNodes.FirstOrDefault(x => x is GetFamilyInstanceLocation);
            //Assert.IsNotNull(locNode);
            //var rows = locNode.OldValue.GetElements();
            //Assert.AreEqual(5, rows.Count());

            //var column = rows.First().GetElements();
            //Assert.AreEqual(5, column.Count());

            //var cell = column.First().GetElements();
            //Assert.AreEqual(4, cell.Count());

            Assert.Inconclusive("Porting : GetFamilyInstancesByType");
        }
    }
}
