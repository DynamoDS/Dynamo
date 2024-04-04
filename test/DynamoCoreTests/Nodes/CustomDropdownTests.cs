using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.PackageManager;

using NUnit.Framework;

namespace Dynamo.Tests.Nodes
{
    /// <summary>
    /// Test the creation of a custom selection node.
    /// </summary>
    [TestFixture]
    public class CustomDropDownTests : DynamoModelTestBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DesignScriptBuiltin.dll");
            libraries.Add("DSCoreNodes.dll");
            base.GetLibrariesToPreload(libraries);
        }

        private void LoadSamplesPackage()
        {
            // Define package loading reference path
            string dir = TestDirectory;
            string pkgDir = Path.Combine(dir, "pkgs\\Dynamo Samples");
            PackageManagerExtension pkgMan = CurrentDynamoModel.GetPackageManagerExtension();
            PackageLoader loader = pkgMan.PackageLoader;
            Package pkg = loader.ScanPackageDirectory(pkgDir);

            // Load the sample package
            loader.LoadPackages(new List<Package> { pkg });
            // Assert expected package was loaded
            Assert.AreEqual(pkg.Name, "Dynamo Samples");
        }

        private void RunDropdownGraph()
        {
            // Run the graph with correct info serialized, node should deserialize to correct selection
            string path = Path.Combine(TestDirectory, "pkgs", "Dynamo Samples", "extra", "CustomDropdownMenuNodeSample.dyn");
            RunModel(path);
        }

        [Test]
        public void OpenJsonDYNWithCorrectMenuItems()
        {
            LoadSamplesPackage();
            RunDropdownGraph();

            NodeModel node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();

            object itemsAsObject = node.GetType().GetProperty(nameof(DSDropDownBase.Items), typeof(ObservableCollection<DynamoDropDownItem>)).GetValue(node);
            Assert.NotNull(itemsAsObject);
            var items = (ObservableCollection<DynamoDropDownItem>)itemsAsObject;
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("One", items[0].Name);
            Assert.AreEqual("1", items[0].Item);
        }

        [Test]
        public void OpenJsonDYNWithCorrectSelectedItem()
        {
            LoadSamplesPackage();
            RunDropdownGraph();

            NodeModel node = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();

            object selectedItemAsObject = node.GetType().GetProperty(nameof(CustomSelection.SelectedString)).GetValue(node);
            Assert.NotNull(selectedItemAsObject);
            string selectedItem = (string)selectedItemAsObject;
            Assert.AreEqual("Two", selectedItem);
        }

        [Test]
        public void UpdateDropdownValue()
        {
            LoadSamplesPackage();
            RunDropdownGraph();

            NodeModel dropdownNode = CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault();
            CurrentDynamoModel.CurrentWorkspace.UpdateModelValue(new[] { dropdownNode.GUID }, "Value", "2");

            RunCurrentModel();

            NodeModel watchNode = CurrentDynamoModel.CurrentWorkspace.Nodes.Skip(1).FirstOrDefault();
            Assert.NotNull(watchNode);
            string watchValue = watchNode.CachedValue.StringData;
            Assert.AreEqual("3", watchValue);
        }
    }
}
