using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels;
using Dynamo.PackageManager;
using NUnit.Framework;
using SystemTestServices;

namespace Dynamo.Tests.Nodes
{
    [TestFixture]
    public class DropDownTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        public class myDropDown: DSDropDownBase
        {

            protected override SelectionState PopulateItemsCore(string currentSelection)
            {
                Items.Clear();
                // Fake a new type of DropDown which populate several dates
                Items.Add(new DynamoDropDownItem("Monday", new DateTime(2019, 1, 7, 9,0, 0 )));
                Items.Add(new DynamoDropDownItem("Tuesday", new DateTime(2019, 1, 8, 9, 0, 0)));
                Items.Add(new DynamoDropDownItem("Wednesday", new DateTime(2019, 1, 9, 9, 0, 0)));
                Items.Add(new DynamoDropDownItem("Thursday", new DateTime(2019, 1, 10, 9, 0, 0)));
                return SelectionState.Restore;
            }
        }

        [Test]
        public void OpenJsonDYNwithSelectionNode()
        {
            // Define package loading reference path
            var dir = SystemTestBase.GetTestDirectory(ExecutingDirectory);
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.Load(pkg);
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // Second selection selected
            Assert.AreEqual(1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual("1", node.GetType().GetProperty("SelectedString").GetValue(node));
        }


        [Test]
        public void OpenJsonDYNwithSelectionNodeAndWrongSelectionIndexSerialized()
        {
            // Define package loading reference path
            var dir = SystemTestBase.GetTestDirectory(ExecutingDirectory);
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.Load(pkg);
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with incorrect SelectedIndex serialized, 
            // this could be across host versions, index changed in newer version of host
            // node should still deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSampleWithWrongIndex.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // Second selection selected
            Assert.AreEqual(1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual("1", node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        [Test]
        public void OpenXmlDYNwithSelectionNode()
        {

            // Define package loading reference path
            var dir = SystemTestBase.GetTestDirectory(ExecutingDirectory);
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.Load(pkg);
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample_1Dot3.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // Second selection selected
            Assert.AreEqual(1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual("1", node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        //[Test]
        //public void GetSelectedStringFromItemShouldReturnString()
        //{
        //    // Initialize the node
        //    var node = new myDropDown();
        //    node.PopulateItems();
        //    Assert.AreEqual("Monday", node.GetType().GetMethod("GetSelectedStringFromItem").Invoke(node, new object[] { node.Items.FirstOrDefault() }));
        //}

        [Test]
        public void PopulateItemsShouldNotChangeSelectedIndex()
        {
            // Initialize the node
            var node = new myDropDown();
            node.PopulateItems();
            node.SelectedIndex = 3;

            // Populate the items again without modifying the items
            node.PopulateItems();

            // Index should stay the same
            Assert.AreEqual(3, node.SelectedIndex);
        }

        [Test]
        public void Save_NothingSelected()
        {
            Assert.AreEqual(
                "-1",
                DSDropDownBase.SaveSelectedIndexImpl(-1, TestList()));
        }

        [Test]
        public void Save_NothingInList()
        {
            Assert.AreEqual("-1", DSDropDownBase.SaveSelectedIndexImpl(5, new List<DynamoDropDownItem>()));
        }

        [Test]
        public void Save_SelectedIndex()
        {
            Assert.AreEqual(
                "2:banana:blueberry",
                DSDropDownBase.SaveSelectedIndexImpl(2, TestList()));
        }

        [Test]
        public void Load_NothingSelected()
        {
            Assert.AreEqual(-1, DSDropDownBase.ParseSelectedIndexImpl("-1", TestList()));
        }

        [Test]
        public void Load_Selection()
        {
            Assert.AreEqual(2, DSDropDownBase.ParseSelectedIndexImpl("2:banana:blueberry", TestList()));
        }

        [Test]
        public void Load_SelectionIndexOnly()
        {
            Assert.AreEqual(2, DSDropDownBase.ParseSelectedIndexImpl("2", TestList()));
        }

        [Test]
        public void Load_SelectionIndexOutOfRange()
        {
            Assert.AreEqual(-1, DSDropDownBase.ParseSelectedIndexImpl("12", TestList()));
        }

        [Test]
        public void Load_SelectionIndexNoNameMatch()
        {
            Assert.AreEqual(-1, DSDropDownBase.ParseSelectedIndexImpl("2:foo", TestList()));
        }

        private static List<DynamoDropDownItem> TestList()
        {
            var items = new List<DynamoDropDownItem>
            {
                new DynamoDropDownItem("cat", "cat"),
                new DynamoDropDownItem("dog", "dog"),
                new DynamoDropDownItem("banana:blueberry", "stuff"),
                new DynamoDropDownItem("!@#$%%%^&*()", "craziness")
            };

            return items;
        }

    }
}
