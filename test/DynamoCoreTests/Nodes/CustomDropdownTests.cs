using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using CoreNodeModels;
using CoreNodeModels.Input;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
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

        /// <summary>
        /// Deleting a customized Custom Selection node and undoing the delete should
        /// restore the user's items and selection rather than the constructor defaults.
        /// Regression test for https://github.com/DynamoDS/Dynamo/issues/17305
        /// </summary>
        [Test]
        public void DeleteAndUndoKeepsCustomItemsAndSelection()
        {
            var node = new CustomSelection();

            // Replace the default items with custom ones and pick a non-default selection.
            node.Items.Clear();
            node.Items.Add(new DynamoDropDownItem("Alpha", "10"));
            node.Items.Add(new DynamoDropDownItem("Beta", "20"));
            node.Items.Add(new DynamoDropDownItem("Gamma", "30"));
            node.SelectedIndex = 2;

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(node, 0, 0, true, false));

            var guid = node.GUID;

            // Delete the node, then undo the delete.
            CurrentDynamoModel.ExecuteCommand(new DynamoModel.DeleteModelCommand(guid));
            Assert.IsNull(CurrentDynamoModel.CurrentWorkspace.Nodes.FirstOrDefault(n => n.GUID == guid));

            CurrentDynamoModel.ExecuteCommand(new DynamoModel.UndoRedoCommand(DynamoModel.UndoRedoCommand.Operation.Undo));

            var restored = CurrentDynamoModel.CurrentWorkspace.Nodes
                .OfType<CustomSelection>()
                .FirstOrDefault(n => n.GUID == guid);
            Assert.NotNull(restored, "The Custom Selection node should be restored after undo.");

            Assert.AreEqual(3, restored.Items.Count);
            Assert.AreEqual("Alpha", restored.Items[0].Name);
            Assert.AreEqual("10", restored.Items[0].Item);
            Assert.AreEqual("Beta", restored.Items[1].Name);
            Assert.AreEqual("20", restored.Items[1].Item);
            Assert.AreEqual("Gamma", restored.Items[2].Name);
            Assert.AreEqual("30", restored.Items[2].Item);
            Assert.AreEqual(2, restored.SelectedIndex);
        }
    }
}
