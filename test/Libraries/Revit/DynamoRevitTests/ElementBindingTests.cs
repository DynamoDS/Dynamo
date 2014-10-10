﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using RTF.Framework;
using Autodesk.Revit.DB;

using Dynamo.Nodes;

using RevitServices.Persistence;

using Transaction = Autodesk.Revit.DB.Transaction;

namespace Dynamo.Tests
{
    [TestFixture]
    internal class ElementBindingTests : DynamoRevitUnitTestBase
    {
        /// <summary>
        /// This function gets all the reference points in the current Revit document
        /// </summary>
        /// <param name="startNewTransaction">whether do the filtering in a new transaction</param>
        /// <returns>the reference points</returns>
        private static IList<Element> GetAllReferencePointElements(bool startNewTransaction)
        {
            if (startNewTransaction)
            {
                using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "FilteringElements"))
                {
                    trans.Start();

                    ElementClassFilter ef = new ElementClassFilter(typeof(ReferencePoint));
                    FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                    fec.WherePasses(ef);

                    trans.Commit();
                    return fec.ToElements();
                }
            }
            else
            {
                ElementClassFilter ef = new ElementClassFilter(typeof(ReferencePoint));
                FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                fec.WherePasses(ef);
                return fec.ToElements();
            }
        }

        /// <summary>
        /// This function gets all the walls in the current Revit document
        /// </summary>
        /// <param name="startNewTransaction">whether do the filtering in a new transaction</param>
        /// <returns>the walls</returns>
        private static IList<Element> GetAllWallElements(bool startNewTransaction)
        {
            if (startNewTransaction)
            {
                using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "FilteringElements"))
                {
                    trans.Start();

                    ElementClassFilter ef = new ElementClassFilter(typeof(Wall));
                    FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                    fec.WherePasses(ef);

                    trans.Commit();
                    return fec.ToElements();
                }
            }
            else
            {
                ElementClassFilter ef = new ElementClassFilter(typeof(Wall));
                FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
                fec.WherePasses(ef);
                return fec.ToElements();
            }
        }

        /// <summary>
        /// Given a node guid, this function will return the ElementId of the binding element.
        /// This function will work if only one element is created by the node.
        /// </summary>
        /// <param name="guid">the node guid</param>
        /// <returns>the element id</returns>
        private ElementId GetBindingElementIdForNode(Guid guid)
        {
            ProtoCore.Core core = ViewModel.Model.EngineController.LiveRunnerCore;
            var guidToCallSites = core.GetCallsitesForNodes(new []{guid});
            var callSites = guidToCallSites[guid];
            if (!callSites.Any())
                return null;
            var callSite = callSites[0];
            var traceDataList = callSite.TraceData;
            if (!traceDataList.Any())
                return null;
            var data = traceDataList[0].GetLeftMostData();
            var id = data as SerializableId;
            return new ElementId(id.IntID);
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CreateInDynamoDeleteInRevit()
        {
            //This test case is to test that elements can be created via Dynamo.
            //After they are deleted in Revit, we can still create them via Dynamo.

            //Create a reference point in Dynamo
            string dynFilePath = Path.Combine(_testPath, @".\ElementBinding\CreateInDynamo.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());
            var model = ViewModel.Model;
            var selNodes = model.AllNodes.Where(x => string.Equals(x.GUID.ToString(), "6a79717b-7438-458a-a725-587be0ba84fd"));
            Assert.IsTrue(selNodes.Any());
            var node = selNodes.First();
            var id1 = GetBindingElementIdForNode(node.GUID);

            //Delete all reference points in Revit
            using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "DeleteInRevit"))
            {
                trans.Start();

                IList<Element> rps = GetAllReferencePointElements(false);
                var rpIDs = rps.Select(x => x.Id);
                DocumentManager.Instance.CurrentDBDocument.Delete(rpIDs.ToList());

                trans.Commit();
            }

            //Run the graph again
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());
            var id2 = GetBindingElementIdForNode(node.GUID);

            //Check the number of reference points
            //This also verifies MAGN-2317
            IList<Element> newRps = GetAllReferencePointElements(true);
            Assert.AreEqual(1, newRps.Count());

            //Ensure the binding elements are different
            Assert.IsTrue(!id1.Equals(id2));
        }

        [Test, Ignore]
        [TestModel(@".\empty.rfa")]
        public void CreateInDynamoUndoInRevit()
        {
            //Create a reference point in Dynamo
            string dynFilePath = Path.Combine(_testPath, @".\ElementBinding\CreateInDynamo.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());

            //Undo the creation of a reference point in Revit
            Assert.Inconclusive("TO DO");
        }

        [Test, Ignore]
        [TestModel(@".\empty.rfa")]
        public void CreateInDynamoModifyInRevit()
        {
            //Create a wall in Dynamo
            string dynFilePath = Path.Combine(_testPath, @".\ElementBinding\CreateWallInDynamo.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());

            //Modify the wall in Revit
            using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "DeleteInRevit"))
            {
                trans.Start();

                IList<Element> rps = GetAllWallElements(false);
                Assert.AreEqual(1, rps.Count);
                Wall wall = rps.First() as Wall;
                //Modify the wall to cause a failure
                Assert.Inconclusive("TO DO");
                wall.Flip();
                DocumentManager.Instance.CurrentDBDocument.Delete(wall);

                trans.Commit();
            }
        }

        [Test, Ignore]
        [TestModel(@".\ElementBinding\BindingCloseReopen.rvt")]
        public void SelectElementCloseReopenDocument()
        {
            string dynFilePath = Path.Combine(_testPath, @".\ElementBinding\BindingCloseReopen.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());

            //Select one of the walls
            var model = ViewModel.Model;
            var selNodes = model.AllNodes.Where(x => x is ElementSelection<Autodesk.Revit.DB.Element>);
            var selNode = selNodes.First() as ElementSelection<Autodesk.Revit.DB.Element>;

            var elId = new ElementId(184273);
            var el = DocumentManager.Instance.CurrentDBDocument.GetElement(elId);
            //selNode.SelectionResults.Add(el);

            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());

            //Ensure the running result is correct
            Assert.AreEqual(true, GetPreviewValue("6e4abc3b-83fd-44fe-821b-447f1ec0a56c"));

            //Close and reopen the document
            Assert.Inconclusive("TO DO");

            //Run the graph again and ensure the result is correct
            Assert.AreEqual(true, GetPreviewValue("6e4abc3b-83fd-44fe-821b-447f1ec0a56c"));
        }

        [Test, Ignore]
        [TestModel(@".\ElementBinding\BindingCloseReopen.rvt")]
        public void SelectElementFromFamilyDocumentSwitchToProjectDocument()
        {            
            Assert.Inconclusive("TO DO");
        }

        [Test, Ignore]
        [TestModel(@".\empty.rfa")]
        public void CreateInRevitSelectInDynamoUndoInRevit()
        {
            //Create a reference point in Revit
            string rpID;
            ReferencePoint rp;
            using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "CreateInRevit"))
            {
                trans.Start();

                rp = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ());
                rpID = rp.UniqueId;

                trans.Commit();
            }

            //Select the reference point in Dynamo
            string dynFilePath = Path.Combine(_testPath, @".\ElementBinding\SelectInDynamo.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);

            var model = ViewModel.Model;
            var selNodes = model.AllNodes.Where(x => x is ElementSelection<Autodesk.Revit.DB.Element>);
            var selNode = selNodes.First() as ElementSelection<Autodesk.Revit.DB.Element>;
            //selNode.SelectionResults.Add(rp);

            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());

            //Undo the creation of a reference point in Revit
            Assert.Inconclusive("TO DO");
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CreateInRevitSelectInDynamoSelectDifferentElement()
        {
            //This is to test that when a node is binding with a new element, the information related to binding
            //has actually changed.

            //Create two reference points in Revit
            string rpID1, rpID2;
            ReferencePoint rp1, rp2;
            using (var trans = new Transaction(DocumentManager.Instance.CurrentUIDocument.Document, "CreateInRevit"))
            {
                trans.Start();

                rp1 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ());
                rpID1 = rp1.UniqueId;
                rp2 = DocumentManager.Instance.CurrentUIDocument.Document.FamilyCreate.NewReferencePoint(new XYZ(10, 0, 0));
                rpID2 = rp2.UniqueId;

                trans.Commit();
            }

            //Select the first reference point in Dynamo
            string dynFilePath = Path.Combine(_testPath, @".\ElementBinding\SelectInDynamo.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);

            var model = ViewModel.Model;
            var selNodes = model.AllNodes.Where(x => x is ElementSelection<Autodesk.Revit.DB.Element>);
            var selNode = selNodes.First() as ElementSelection<Autodesk.Revit.DB.Element>;
            IEnumerable<Element> selection1 = new[] { rp1 };
            selNode.UpdateSelection(selection1);
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());
            var id1 = selNode.SelectionResults.First();

            //Select the second reference point in Dynamo
            IEnumerable<Element> selection2 = new[] { rp2 };
            selNode.UpdateSelection(selection2);
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());
            var id2 = selNode.SelectionResults.First();

            //Ensure the element binding is not the same
            Assert.IsTrue(!id1.Equals(id2));
        }

        [Test]
        [TestModel(@".\empty.rfa")]
        public void CreateDifferentNumberOfElementsInDynamo()
        {
            //This is to test that the same node can bind with different number of elements correctly

            //Create 4x2 reference points
            string dynFilePath = Path.Combine(_testPath, @".\ElementBinding\CreateDifferentNumberOfPoints.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());

            //Check the number of the refrence points
            var points = GetAllReferencePointElements(true);
            Assert.AreEqual(8, points.Count);

            var model = ViewModel.Model;
            var selNodes = model.AllNodes.Where(x => string.Equals(x.GUID.ToString(), "a52bee11-4382-4c42-a676-443f9d7eedf2"));
            Assert.IsTrue(selNodes.Any());
            var node = selNodes.First();
            var slider = node as IntegerSlider;

            //Change the slider value from 4 to 3
            slider.Value = 3;

            //Run the graph again
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());

            //Check the number of the refrence points
            points = GetAllReferencePointElements(true);
            Assert.AreEqual(6, points.Count);
        }

        [Test, Ignore]
        [TestModel(@".\empty.rfa")]
        public void CreateDifferentNumberOfElementsInDynamoWithDifferentLacingStrategies()
        {
            //This is to test that the same node can bind correctly with different number of elements
            //when the lacing strategies for the node change

            Assert.Inconclusive("TO DO");

            //Create 4x2 reference points
            string dynFilePath = Path.Combine(_testPath, @".\ElementBinding\CreateDifferentNumberOfPoints.dyn");
            string testPath = Path.GetFullPath(dynFilePath);

            ViewModel.OpenCommand.Execute(testPath);
            Assert.DoesNotThrow(() =>ViewModel.Model.RunExpression());

            //Check the number of the refrence points
            var points = GetAllReferencePointElements(true);
            Assert.AreEqual(8, points.Count);

            var model = ViewModel.Model;
            var selNodes = model.AllNodes.Where(x => string.Equals(x.NickName, "ReferencePoint.ByCoordinates"));
            Assert.IsTrue(selNodes.Any());
            var node = selNodes.First() as DSFunction;

            //As the unit test will hang, so make it fail
            Assert.Fail("Reference points will be created at the same location!");

            //Change the slider value from 4 to 3
            node.ArgumentLacing = Models.LacingStrategy.Longest;

            //Run the graph again
           ViewModel.Model.RunExpression();

            //Check the number of the refrence points
            points = GetAllReferencePointElements(true);
            Assert.AreEqual(4, points.Count);
        }
    }
}
