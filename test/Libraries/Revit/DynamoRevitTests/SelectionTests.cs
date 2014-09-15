using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSRevitNodesUI;

using Dynamo.Interfaces;
using Dynamo.Nodes;
using NUnit.Framework;

using RevitServices.Persistence;
using RTF.Framework;

using Element = Revit.Elements.Element;
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
        public void AllSelectionNodes()
        {
            var model = ViewModel.Model;
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\Selection.dyn"));

            Assert.DoesNotThrow(() => ViewModel.Model.RunExpression());

            var selNodes = model.AllNodes.Where(x => x is DSModelElementsSelection);
            Assert.IsFalse(selNodes.Any(x => x.CachedValue == null));
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectModelElement()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath,@".\Selection\SelectModelElement.dyn"));
            TestSelection<Autodesk.Revit.DB.Element, Element>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectModelElements()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectModelElements.dyn"));
            TestSelection<Autodesk.Revit.DB.Element, Element>(SelectionType.Many);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectFace()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectFace.dyn"));
            TestSelection<Reference,Surface>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectEdge()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectEdge.dyn"));
            TestSelection<Reference,Autodesk.DesignScript.Geometry.Curve>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectPointOnFace()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectPointOnFace.dyn"));
            TestSelection<Reference,Autodesk.DesignScript.Geometry.Point>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectUVOnFace()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectUVOnFace.dyn"));
            TestSelection<Reference,Autodesk.DesignScript.Geometry.UV>(SelectionType.One);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectDividedSurfaceFamilies()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectDividedSurfaceFamilies.dyn"));
            TestSelection<Autodesk.Revit.DB.DividedSurface,Revit.Elements.DividedSurface>(SelectionType.Many);
        }

        [Test]
        [TestModel(@".\Selection\Selection.rfa")]
        public void SelectFaces()
        {
            OpenAndAssertNoDummyNodes(Path.Combine(_testPath, @".\Selection\SelectFaces.dyn"));
            TestSelection<Reference,Surface>(SelectionType.Many);
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
            var selectNode = (RevitSelection<T1>)(ViewModel.Model.Nodes.FirstOrDefault(x => x is RevitSelection<T1>));
            Assert.NotNull(selectNode);

            switch (selectionType)
            {
                case SelectionType.One:
                    var element = (T2)GetPreviewValueAtIndex(
                        selectNode.GUID.ToString(), 0);
                    Assert.NotNull(element);
                    selectNode.Selection = new List<T1>();
                    RunCurrentModel();
                    element = (T2)GetPreviewValueAtIndex(
                        selectNode.GUID.ToString(), 0);
                    Assert.Null(element);
                    break;
                case SelectionType.Many:
                    var elements = GetPreviewCollection<T2>(
                        selectNode.GUID.ToString());
                    Assert.NotNull(elements);
                    Assert.Greater(elements.Count(),0);
                    selectNode.Selection = new List<T1>();
                    RunCurrentModel();
                    elements = GetPreviewCollection<T2>(
                        selectNode.GUID.ToString());
                    Assert.Null(elements);
                    break;
            }
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
