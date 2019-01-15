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
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));
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
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));
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
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        /// <summary>
        /// This test will make sure when opening an XML DYN containing dropdown
        /// with invalid SelectedIndex but valid SelectedString can still be opened correctly
        /// </summary>
        [Test]
        public void OpenXmlDYNwithInvalidSelectedIndex()
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
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample_1Dot3_InvalidSelectedIndex.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // Second selection selected
            Assert.AreEqual(1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        /// <summary>
        /// This test will make sure when opening an XML DYN containing dropdown
        /// with negative SelectedIndex can still be opened correctly
        /// </summary>
        [Test]
        public void OpenXmlDYNwithInvalidSelectedIndex2()
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
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample_1Dot3_InvalidSelectedIndex2.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // No selection selected
            Assert.AreEqual(-1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual(String.Empty, node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        /// <summary>
        /// This test will make sure when opening an XML DYN containing dropdown
        /// with SelectedIndex larger than item count can still be opened correctly
        /// </summary>
        [Test]
        public void OpenXmlDYNwithInvalidSelectedIndex3()
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
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "DropDownSample_1Dot3_InvalidSelectedIndex3.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            // No selection selected
            Assert.AreEqual(-1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreEqual(String.Empty, node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        [Test]
        public void PopulateItemsShouldNotChangeSelectedIndex()
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
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));

            // Call PopulateItems() on node should not change index
            // TODO fix the selection state code in PopulateItemsCore() in DynamoSamples repo 
            // and update the test to be the correct behavior
            node.GetType().GetMethod("PopulateItems").Invoke(node, new object[] { });
            Assert.AreNotEqual(1, node.GetType().GetProperty("SelectedIndex").GetValue(node));
            Assert.AreNotEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));
        }

        [Test]
        public void GetSelectedStringFromItemShouldReturnString()
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
            Assert.AreEqual("Cersei", node.GetType().GetProperty("SelectedString").GetValue(node));

            // Call GetSelectedStringFromItem() on DropDownItem should give back SelectedString
            Assert.AreNotEqual("Cersei", node.GetType().GetMethod("GetSelectedStringFromItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(node, new object[] { new DynamoDropDownItem("Tywin", 0) }));
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
