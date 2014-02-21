using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using NUnit.Framework;

namespace Dynamo.Tests
{
    [TestFixture]
    class FamilyTests:DynamoRevitUnitTestBase
    {   
        [Test]
        public void GetCurvesFromFamilyInstance()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetCurvesFromFamilyInstance.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            var node =
                (CurvesFromFamilyInstance)dynSettings.Controller.DynamoModel.Nodes.First(x => x is CurvesFromFamilyInstance);
            Assert.IsTrue(node.OldValue.IsList);

            // check that we get the correct number of curves back from the instance
            var list = ((FScheme.Value.List)node.OldValue).Item;
            Assert.AreEqual(4, list.Count());

            // get the filter node from dynamo and  make sure it has an empty list as output
            //we've filtered the list of distances by 0 to make sure distances between orginal points
            // and end points of new curves are 0.
            var filterNode = (FilterOut)dynSettings.Controller.DynamoModel.Nodes.First(x => x is FilterOut);
            Assert.IsTrue(filterNode.OldValue.IsList);
            
            // ensure it is empty
            var filterList = ((FScheme.Value.List)filterNode.OldValue).Item;
            Assert.AreEqual(0.0,filterList.Length);

        }
        
        [Test]
        public void GetFamilyInstancesByType()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstancesByType.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            var node =
                (GetFamilyInstancesByType)dynSettings.Controller.DynamoModel.Nodes.First(x => x is GetFamilyInstancesByType);
            Assert.IsTrue(node.OldValue.IsList);

            var list = ((FScheme.Value.List)node.OldValue).Item;
            Assert.AreEqual(100, list.Count());
        }

        [Test]
        public void GetFamilyInstanceLocation()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\GetFamilyInstanceLocation.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));
        }

        [Test]
        public void CanLocateAdaptiveComponent()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\AC_locationStandAlone.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //ensure that the collection of points
            //returned has the 4 corner points
            var locNode = model.AllNodes.FirstOrDefault(x => x is GetFamilyInstanceLocation);
            Assert.IsNotNull(locNode);
            var locs = locNode.OldValue.GetListFromFSchemeValue();
            Assert.AreEqual(4, locs.Count());

            //asert that the list is full of XYZs
            var xyzs = locs.Select(x=>x.GetObjectFromFSchemeValue<XYZ>());
        }

        [Test]
        public void CanLocateAdaptiveComponentInDividedSurface()
        {
            var model = dynSettings.Controller.DynamoModel;

            string samplePath = Path.Combine(_testPath, @".\Family\AC_locationInDividedSurface.dyn");
            string testPath = Path.GetFullPath(samplePath);

            model.Open(testPath);
            Assert.DoesNotThrow(() => dynSettings.Controller.RunExpression(true));

            //ensure that you get a list of lists
            //with 5 lists each with 5 lists of 4 points
            var locNode = model.AllNodes.FirstOrDefault(x => x is GetFamilyInstanceLocation);
            Assert.IsNotNull(locNode);
            var rows = locNode.OldValue.GetListFromFSchemeValue();
            Assert.AreEqual(5, rows.Count());

            var column = rows.First().GetListFromFSchemeValue();
            Assert.AreEqual(5, column.Count());

            var cell = column.First().GetListFromFSchemeValue();
            Assert.AreEqual(4, cell.Count());
        }
    }
}
