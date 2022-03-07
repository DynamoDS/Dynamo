using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.PackageManager;
using NUnit.Framework;
using SystemTestServices;

namespace Dynamo.Tests.Nodes
{
    [TestFixture]
    public class CustomDropDownTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        [Test]
        /// This test makes sure all the saved menu items are retrieved correctly
        public void OpenJsonDYNWithCorrectMenuItems()
        {
            // Define package loading reference path
            var dir = SystemTestBase.GetTestDirectory(ExecutingDirectory);
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.LoadPackages(new List<Package> {pkg});
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "CustomDropdownMenuNodeSample.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();

            object itemsAsObject = node.GetType().GetProperty(nameof(CustomSelectionNodeModel.Items), typeof(ObservableCollection<CustomSelectionItemViewModel>)).GetValue(node);
            Assert.NotNull(itemsAsObject);
            var items = (ObservableCollection<CustomSelectionItemViewModel>)(itemsAsObject);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("One", items[0].Name);
        }


        [Test]
        /// This test makes sure all the saved menu items are retrieved correctly
        public void OpenJsonDYNWithCorrectSelectedItem()
        {
            // Define package loading reference path
            var dir = SystemTestBase.GetTestDirectory(ExecutingDirectory);
            var pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            var pkgMan = this.CurrentDynamoModel.GetPackageManagerExtension();
            var loader = pkgMan.PackageLoader;
            var pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.LoadPackages(new List<Package> { pkg });
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");

            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "CustomDropdownMenuNodeSample.dyn");
            RunModel(path);

            var node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();

            object selectedItemAsObject = node.GetType().GetProperty(nameof(CustomSelectionNodeModel.SelectedItem)).GetValue(node);
            Assert.NotNull(selectedItemAsObject);
            var selectedItem = (CustomSelectionItemViewModel)selectedItemAsObject;
            Assert.AreEqual("2", selectedItem.Value);
            Assert.AreEqual("Two", selectedItem.Name);
        }



    }
}
