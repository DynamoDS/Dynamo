using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

using DSRevitNodesUI;

using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Tests;

using NUnit.Framework;

using RevitServices.Persistence;
using RevitServices.Transactions;

using RTF.Framework;

using ReferencePoint = Revit.Elements.ReferencePoint;
using Surface = Autodesk.DesignScript.Geometry.Surface;

namespace RevitSystemTests
{
    [TestFixture]
    class SelectionTests : SystemTest
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void FamilyTypeSelectorNode()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Selection\SelectFamily.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //open the test file
            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            //first assert that we have only one node
            var nodeCount = ViewModel.Model.Nodes.Count;
            Assert.AreEqual(1, nodeCount);

            //assert that we have the right number of family symbols
            //in the node's items source
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            fec.OfClass(typeof(Family));
            var families = fec.ToElements().Cast<Family>();
            var symbolIds = families.SelectMany(f => f.GetFamilySymbolIds());
            var count = symbolIds.Count();

            var typeSelNode = (FamilyTypes)ViewModel.Model.Nodes.First();
            Assert.AreEqual(typeSelNode.Items.Count, count);

            //assert that the selected index is correct
            Assert.AreEqual(typeSelNode.SelectedIndex, 3);

            //now try and set the selected index to something
            //greater than what is possible
            typeSelNode.SelectedIndex = count + 5;
            Assert.AreEqual(typeSelNode.SelectedIndex, -1);
        }
        
        [Test, Category("SmokeTests"), TestModel(@".\empty.rfa")]
        public void SelectionDocModificationSync()
        {
            string samplePath = Path.Combine(workingDirectory, @".\Selection\SelectAndUpdate.dyn");
            string testPath = Path.GetFullPath(samplePath);

            ViewModel.OpenCommand.Execute(testPath);
            AssertNoDummyNodes();

            var selectNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<DSModelElementSelection>();
            var watchNode = ViewModel.Model.CurrentWorkspace.FirstNodeFromWorkspace<Watch>();

            var refPt = ReferencePoint.ByCoordinates(0, 0, 0);
            selectNode.UpdateSelection(new[] { refPt.InternalElement });

            ViewModel.Model.RunExpression();

            Assert.AreEqual(0, watchNode.CachedValue);

            refPt.X = 10;

            TransactionManager.Instance.ForceCloseTransaction();

            Assert.AreEqual(true, selectNode.ForceReExecuteOfNode);
            ViewModel.Model.RunExpression();

            Assert.AreNotEqual(0, watchNode.CachedValue); //Actual value depends on units
        }

        [Test, Category("SmokeTests"), TestModel(@".\Selection\Selection.rfa")]
        public void EmptySingleSelectionReturnsNull()
        {
            // Verify that an empty single-selection returns null
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectAndMultiSelect.dyn"));

            var guid = "938e1543-c1d5-4c92-83a7-3abcae2b8264";
            var selectionNode = ViewModel.Model.Nodes.FirstOrDefault(n => n.GUID.ToString() == guid) as ElementSelection<Element>;
            Assert.NotNull(selectionNode, "The requested node could not be found");
            
            selectionNode.ClearSelections();

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
            var element = GetPreviewCollection(guid);
            Assert.Null(element);
        }

        [Test, Category("SmokeTests"), TestModel(@".\Selection\Selection.rfa")]
        public void EmptyMultiSelectionReturnsNull()
        {
            // Verify that an empty multi-selection returns null
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectAndMultiSelect.dyn"));

            var guid = "34f4f2cc-63c3-41ec-91fa-68db7820cee5";
            var selectionNode = ViewModel.Model.Nodes.FirstOrDefault(n => n.GUID.ToString() == guid) as ElementSelection<Element>;
            Assert.NotNull(selectionNode, "The requested node could not be found");

            selectionNode.ClearSelections();

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
            var element = GetPreviewCollection(guid);
            Assert.Null(element);
        }

        [Test, Category("SmokeTests"), TestModel(@".\Selection\Selection.rfa")]
        public void SingleSelectionReturnsSingleObject()
        {
            // Verify that the selection of a single element
            // returns only one object NOT a list of objects
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectAndMultiSelect.dyn"));

            Assert.DoesNotThrow(()=>ViewModel.Model.RunExpression());

            var guid = "938e1543-c1d5-4c92-83a7-3abcae2b8264";
            var element = GetPreviewValue(guid);
            Assert.IsInstanceOf<Revit.Elements.ReferencePoint>(element);
        }

        [Test, Category("SmokeTests"), TestModel(@".\Selection\Selection.rfa")]
        public void MultiSelectionReturnsMultipleObjects()
        {
            // Verify that the selection of many elements
            // returns a list of objects
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectAndMultiSelect.dyn"));

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            var guid = "34f4f2cc-63c3-41ec-91fa-68db7820cee5";
            var element = GetPreviewCollection(guid);
            Assert.IsInstanceOf<List<object>>(element);
        }

        [Test]
        [Category("SmokeTests")]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectModelElement()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory,@".\Selection\SelectModelElement.dyn"));
            TestSelection<Element, Element>(SelectionType.One);
        }

        [Test]
        [Category("SmokeTests")]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectModelElements()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectModelElements.dyn"));
            TestSelection<Element, Element>(SelectionType.Many);
        }

        [Test]
        [Category("SmokeTests")]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectFace()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectFace.dyn"));

            RunCurrentModel();

            // Get the selection node
            var selectNode = (ReferenceSelection)(ViewModel.Model.Nodes.FirstOrDefault(x => x is ReferenceSelection));
            Assert.NotNull(selectNode);

            // The select faces node returns a list of lists
            var list = GetFlattenedPreviewValues(selectNode.GUID.ToString());
            Assert.AreEqual(1, list.Count());
            Assert.IsInstanceOf<Surface>(list[0]);

            // Clear the selection
            selectNode.ClearSelections();

            RunCurrentModel();

            list = GetFlattenedPreviewValues(selectNode.GUID.ToString());
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        [Category("SmokeTests")]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectEdge()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectEdge.dyn"));
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            var selectionNode = ViewModel.Model.Nodes.FirstOrDefault(n => n is ReferenceSelection) as ReferenceSelection;
            Assert.NotNull(selectionNode);
            var element = GetPreviewValue(selectionNode.GUID.ToString());
            Assert.IsInstanceOf<NurbsCurve>(element);

            selectionNode.ClearSelections();
            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());
            element = GetPreviewValue(selectionNode.GUID.ToString());
            Assert.Null(element);
        }

        //[Test]
        //[TestModel(@".\Selection\Selection.rfa")]
        //public void SelectPointOnFace()
        //{
        //    OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectPointOnFace.dyn"));
        //    TestSelection<Reference,Reference>(SelectionType.One);
        //}

        //[Test]
        //[TestModel(@".\Selection\Selection.rfa")]
        //public void SelectUVOnFace()
        //{
        //    OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectUVOnFace.dyn"));
        //    TestSelection<Reference,Reference>(SelectionType.One);
        //}

        [Test]
        [Category("SmokeTests")]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectDividedSurfaceFamilies()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectDividedSurfaceFamilies.dyn"));

            // Get the selection node
            var selectNode = (ElementSelection<DividedSurface>)(ViewModel.Model.Nodes.FirstOrDefault(x => x is ElementSelection<DividedSurface>));
            Assert.NotNull(selectNode);

            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(DividedSurface));

            var ds = fec.ToElements().FirstOrDefault() as DividedSurface;
            Assert.NotNull(ds);

            RunCurrentModel();

            var elements = GetPreviewCollection(selectNode.GUID.ToString());
            Assert.AreEqual(25, elements.Count);

            // Reset the selection
            selectNode.UpdateSelection(new[] { ds });

            using (var trans = new Transaction(DocumentManager.Instance.CurrentDBDocument))
            {
                try
                {
                    trans.Start("SelectDividedSurfaceFamilies_Test");

                    // Flex the divided surface division and ensure the 
                    // SelectionResults is updated.
                    ds.USpacingRule.Number = 3;
                    ds.VSpacingRule.Number = 3;

                    trans.Commit();
                }
                catch(Exception ex)
                {
                    if (trans.HasStarted())
                    {
                        trans.RollBack();
                    }

                    Assert.Fail(ex.Message);
                }
            }

            RunCurrentModel();

            elements = GetPreviewCollection(selectNode.GUID.ToString());
            Assert.AreEqual(9, elements.Count);
        }

        [Test]
        [Category("SmokeTests")]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectFaces()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectFaces.dyn"));

            RunCurrentModel();

            // Get the selection node
            var selectNode = (ReferenceSelection)(ViewModel.Model.Nodes.FirstOrDefault(x => x is ReferenceSelection));
            Assert.NotNull(selectNode);

            // The select faces node returns a list of lists
            var list = GetFlattenedPreviewValues(selectNode.GUID.ToString());
            Assert.AreEqual(3, list.Count);
            Assert.IsInstanceOf<Surface>(list[0]);

            // Clear the selection
            selectNode.ClearSelections();

            RunCurrentModel();

            list = GetFlattenedPreviewValues(selectNode.GUID.ToString());
            Assert.AreEqual(0, list.Count);
        }

        [Test, Category("SmokeTests"), TestModel(@".\Selection\SelectionSync.rvt")]
        public void SelectionInSyncWithDocumentOperations_Elements()
        {
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(Wall));

            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectionSyncElements.dyn"));

            const string selectNodeGuid = "3dbe16b8-e855-4229-a1cf-4643e69ba7b4";

            var walls = fec.ToElements();
            int remainingWallCount = walls.Count;
            while (remainingWallCount > 1)
            {
                remainingWallCount = DeleteWallAndRun<Revit.Elements.Wall>(selectNodeGuid);
            }
        }

        [Test, Category("SmokeTests"), TestModel(@".\Selection\SelectionSync.rvt")]
        public void SelectionInSyncWithDocumentOperations_References()
        {
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(Wall));

            OpenAndAssertNoDummyNodes(Path.Combine(workingDirectory, @".\Selection\SelectionSyncReferences.dyn"));

            const string selectNodeGuid = "91fd4f06-dde2-449f-aff5-f6203e4777ed";
            var walls = fec.ToElements();
            int remainingWallCount = walls.Count;
            while (remainingWallCount > 1)
            {
                remainingWallCount = DeleteWallAndRun<Surface>(selectNodeGuid);
            }

        }

        private int DeleteWallAndRun<T>(string testGuid)
        {
            var walls = GetWalls();
   
            var wall = walls.FirstOrDefault();
            if (walls.Count == 0)
            {
                Assert.Fail("No more walls could be found in the model.");
            }
            using (var t = new Transaction(DocumentManager.Instance.CurrentDBDocument))
            {
                t.Start("Delete wall test.");
                DocumentManager.Instance.CurrentDBDocument.Delete(wall.Id);
                t.Commit();
            }

            walls = GetWalls();

            RunCurrentModel();

            var values = GetFlattenedPreviewValues(testGuid);
            Assert.AreEqual(values.Count, walls.Count);
            Assert.IsInstanceOf<T>(values.First());

            return walls.Count;
        }

        private List<Wall> GetWalls()
        {
            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(Wall));

            return fec.ToElements().Cast<Wall>().ToList();
        }

        


        /// <summary>
        /// Find the first selection node in a graph, run the graph
        /// and assert that the returned object is valid. Then
        /// clear the selection and re-run, ensuring that the result
        /// is null.
        /// </summary>
        /// <typeparam name="T1">The type parameter for the selector method.</typeparam>
        /// <typeparam name="T2">The expected return type for elements in the selection.</typeparam>
        /// <param name="selectionType"></param>
        private void TestSelection<T1,T2>(SelectionType selectionType)
        {
            RunCurrentModel();

            // Find the first node of the specified selection type
            var selectNode =
                ViewModel.Model.HomeSpace.FirstNodeFromWorkspace<RevitSelection<T1, T2>>();
            Assert.NotNull(selectNode);

            switch (selectionType)
            {
                case SelectionType.One:
                    TestSingleSelection(selectNode);
                    break;
                case SelectionType.Many:
                    TestMultipleSelection(selectNode);
                    break;
            }
        }

        /// <summary>
        /// Test running the node, then clearing the 
        /// selection and running again.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="selectNode"></param>
        private void TestSingleSelection<T1, T2>(SelectionBase<T1, T2> selectNode)
        {
            var element = GetPreviewValue(selectNode.GUID.ToString());
            Assert.NotNull(element);
            Assert.IsTrue(selectNode.State != ElementState.Warning);
            selectNode.ClearSelections();
            RunCurrentModel();
            element = GetPreviewValue(selectNode.GUID.ToString());
            Assert.Null(element);
            Assert.IsTrue(selectNode.State == ElementState.Warning);
        }

        /// <summary>
        /// Test running the node, then clearing the 
        /// selection and running again.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="selectNode"></param>
        private void TestMultipleSelection<T1, T2>(SelectionBase<T1, T2> selectNode)
        {
            var elements = GetPreviewCollection(selectNode.GUID.ToString());
            Assert.NotNull(elements);
            Assert.IsTrue(selectNode.State != ElementState.Warning);
            Assert.Greater(elements.Count(), 0);
            selectNode.ClearSelections();
            RunCurrentModel();
            elements = GetPreviewCollection(selectNode.GUID.ToString());
            Assert.Null(elements);
            Assert.IsTrue(selectNode.State == ElementState.Warning);
        }

        private void OpenAndAssertNoDummyNodes(string samplePath)
        {
            var testPath = Path.GetFullPath(samplePath);

            //open the test file
            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();
        }
    }
}
