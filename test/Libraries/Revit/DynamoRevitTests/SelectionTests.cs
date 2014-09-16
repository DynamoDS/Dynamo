using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;
using DSRevitNodesUI;

using Dynamo.Interfaces;
using Dynamo.Nodes;
using NUnit.Framework;

using ProtoCore.Mirror;

using RevitServices.Persistence;
using RTF.Framework;

using Family = Autodesk.Revit.DB.Family;
using FamilySymbol = Autodesk.Revit.DB.FamilySymbol;

namespace Dynamo.Tests
{
    [TestFixture]
    class SelectionTests: DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@".\empty.rfa")]
        public void FamilyTypeSelectorNode()
        {
            string samplePath = Path.Combine(_testPath, @".\Selection\SelectFamily.dyn");
            string testPath = Path.GetFullPath(samplePath);

            //open the test file
            ViewModel.OpenCommand.Execute(testPath);

            AssertNoDummyNodes();

            //first assert that we have only one node
            var nodeCount = ViewModel.Model.Nodes.Count;
            Assert.AreEqual(1, nodeCount);

            //assert that we have the right number of family symbols
            //in the node's items source
            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            fec.OfClass(typeof(Family));
            int count = 0;
            foreach (Family f in fec.ToElements())
            {
                foreach (FamilySymbol fs in f.Symbols)
                {
                    count++;
                }
            }

            FamilyTypes typeSelNode = (FamilyTypes)ViewModel.Model.Nodes.First();
            Assert.AreEqual(typeSelNode.Items.Count, count);

            //assert that the selected index is correct
            Assert.AreEqual(typeSelNode.SelectedIndex, 3);

            //now try and set the selected index to something
            //greater than what is possible
            typeSelNode.SelectedIndex = count + 5;
            Assert.AreEqual(typeSelNode.SelectedIndex, -1);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectModelElement()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath,@".\Selection\SelectModelElement.dyn"));
            TestSelection<Autodesk.Revit.DB.Element, Autodesk.Revit.DB.Element>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectModelElements()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectModelElements.dyn"));
            TestSelection<Autodesk.Revit.DB.Element, Autodesk.Revit.DB.Element>(SelectionType.Many);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectFace()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectFace.dyn"));

            RunCurrentModel();

            // Get the selection node
            var selectNode = (ReferenceSelection)(ViewModel.Model.Nodes.FirstOrDefault(x => x is ReferenceSelection));
            Assert.NotNull(selectNode);

            // The select faces node returns a list of lists
            var list = GetFlattenedPreviewValues(selectNode.GUID.ToString());
            Assert.AreEqual(1, list.Count);

            // Clear the selection
            selectNode.ClearSelections();

            RunCurrentModel();

            list = GetFlattenedPreviewValues(selectNode.GUID.ToString());
            Assert.Null(list);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectEdge()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectEdge.dyn"));
            TestSelection<Reference,Reference>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectPointOnFace()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectPointOnFace.dyn"));
            TestSelection<Reference,Reference>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectUVOnFace()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectUVOnFace.dyn"));
            TestSelection<Reference,Reference>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectDividedSurfaceFamilies()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectDividedSurfaceFamilies.dyn"));

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
            selectNode.Selection = new List<DividedSurface>() { ds };

            using (var trans = new Transaction(DocumentManager.Instance.CurrentDBDocument))
            {
                try
                {
                    trans.Start("SelectDividedSurfaceFamilies_Test");

                    // Flex the divided surface division and ensure the 
                    // Selection is updated.
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
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectFaces()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectFaces.dyn"));

            RunCurrentModel();

            // Get the selection node
            var selectNode = (ReferenceSelection)(ViewModel.Model.Nodes.FirstOrDefault(x => x is ReferenceSelection));
            Assert.NotNull(selectNode);

            // The select faces node returns a list of lists
            var list = GetFlattenedPreviewValues(selectNode.GUID.ToString());
            Assert.AreEqual(3, list.Count);

            // Clear the selection
            selectNode.ClearSelections();

            RunCurrentModel();

            list = GetFlattenedPreviewValues(selectNode.GUID.ToString());
            Assert.Null(list);
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
        private List<T1> TestSelection<T1,T2>(SelectionType selectionType)
        {
            RunCurrentModel();

            // Find the first node of the specified selection type
            var selectNode = (RevitSelection<T1,T2>)(ViewModel.Model.Nodes.FirstOrDefault(x => x is RevitSelection<T1,T2>));
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

            return selectNode.Selection;
        }

        private void TestSingleSelection<T1, T2>(SelectionBase<T1, T2> selectNode)
        {
            var element = GetPreviewValueAtIndex(
                selectNode.GUID.ToString(),
                0);
            Assert.NotNull(element);
            selectNode.Selection = new List<T1>();
            RunCurrentModel();
            element = GetPreviewValueAtIndex(
                selectNode.GUID.ToString(),
                0);
            Assert.Null(element);
        }

        private void TestMultipleSelection<T1, T2>(SelectionBase<T1, T2> selectNode)
        {
            var elements = GetPreviewCollection(
                selectNode.GUID.ToString());
            Assert.NotNull(elements);
            Assert.Greater(elements.Count(), 0);
            selectNode.Selection = new List<T1>();
            RunCurrentModel();
            elements = GetPreviewCollection(
                selectNode.GUID.ToString());
            Assert.Null(elements);
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
