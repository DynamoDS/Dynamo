using Dynamo.Models;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using PythonNodeModelsWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DynamoCoreWpfTests
{
    public class PythonNodeCustomizationTests : DynamoTestUIBase
    {
        private readonly List<string> expectedEngineMenuItems = Enum.GetNames(typeof(PythonNodeModels.PythonEngineVersion)).ToList();

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// This test checks if its possible to change the Python nodemodels Engine property
        /// from the dropdown selector inside the script editor.
        /// </summary>
        [Test]
        public void CanChangeEngineFromScriptEditorDropDown()
        {
            // Arrange
            var expectedAvailableEnignes = Enum.GetValues(typeof(PythonNodeModels.PythonEngineVersion)).Cast<PythonNodeModels.PythonEngineVersion>();
            var expectedDefaultEngine = PythonNodeModels.PythonEngineVersion.IronPython2;
            var engineChange = PythonNodeModels.PythonEngineVersion.CPython3;

            Open(@"core\python\python.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeModels.PythonNodeBase;
            Assert.NotNull(nodeModel);

            // get the `Edit...` menu item from the nodes context menu so we can simulate the clik event.
            var editMenuItem = nodeView.MainContextMenu
                .Items
                .Cast<MenuItem>()
                .Where(x => x.Header.ToString() == "Edit...")
                .Select(x => x)
                .First();

            editMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            // after simulating the click event get the opend Script editor window
            // and fetch the EngineSelector dropdown
            var scriptEditorWindow = this.View.GetChildrenWindowsOfType<ScriptEditorWindow>().First();
            var windowGrid = scriptEditorWindow.Content as Grid;
            var engineSelectorComboBox = windowGrid
                .ChildrenOfType<ComboBox>()
                .Where(x=>x.Name == "EngineSelectorComboBox")
                .Select(x=>x)
                .First();

            // Act
            var engineBeforeChange = engineSelectorComboBox.SelectedItem;
            var comboBoxEngines = engineSelectorComboBox.Items.SourceCollection;
            var engineAfterChange = engineSelectorComboBox.SelectedItem = engineChange;

            // Assert
            CollectionAssert.AreEqual(expectedAvailableEnignes, comboBoxEngines);
            Assert.AreEqual(expectedDefaultEngine, engineBeforeChange);
            Assert.AreEqual(engineSelectorComboBox.SelectedItem, PythonNodeModels.PythonEngineVersion.CPython3);
            Assert.AreEqual(nodeModel.Engine, engineAfterChange);
        }

        /// <summary>
        /// This test checks if its possible to change the Python nodemodels Engine property
        /// from the context menu on the Python From String node.
        /// </summary>
        [Test]
        public void CanChangePythonEngineFromContextMenuOnPythonFromStringNode()
        {
            // Arrange
            var expectedEngineVersionOnOpen = PythonNodeModels.PythonEngineVersion.CPython3;
            var expectedEngineVersionAfterChange = PythonNodeModels.PythonEngineVersion.IronPython2;

            Open(@"core\python\pythonFromString.dyn");

            var nodeView = NodeViewWithGuid(new Guid("bad59bc89b4947b699ee34fa8dca91ae").ToString("D"));
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeModels.PythonStringNode;
            Assert.NotNull(nodeModel);

            var engineVersionOnOpen = nodeModel.Engine;

            var editMenuItem = nodeView.MainContextMenu
                .Items
                .Cast<MenuItem>()
                .Where(x => x.Header.ToString() == "Python Engine Version")
                .Select(x => x)
                .First();

            var engineMenuItems = editMenuItem.Items;
            var ironPython2MenuItem = engineMenuItems
                .Cast<MenuItem>()
                .Where(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo)
                .Select(x => x)
                .First();

            // Act
            ironPython2MenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            var engineVersionAfterChange = nodeModel.Engine;

            // Assert
            Assert.AreEqual(expectedEngineVersionOnOpen, engineVersionOnOpen);
            CollectionAssert.AreEqual(expectedEngineMenuItems, engineMenuItems.Cast<MenuItem>().Select(x => x.Header));
            Assert.AreEqual(expectedEngineVersionAfterChange, engineVersionAfterChange);
        }

        /// <summary>
        /// This test checks if the Python Node displays an EngineLabel showing which Engine the node is set to use,
        /// and if this label updates as expected when changing the Engine property on the nodemodel.
        /// </summary>
        [Test]
        public void PythonNodeHasLabelDisplayingCurrentEngine()
        {
            // Arrange
            var expectedDefaultEngineLabelText = PythonNodeModels.PythonEngineVersion.IronPython2.ToString();
            var engineChange = PythonNodeModels.PythonEngineVersion.CPython3;

            Open(@"core\python\python.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeModels.PythonNodeBase;
            Assert.NotNull(nodeModel);

            var engineLabel = nodeView.PresentationGrid
                .Children
                .Cast<UIElement>()
                .Where(x=>x.GetType() == typeof(PythonNodeModelsWpf.Controls.EngineLabel))
                .Select(x=>x as PythonNodeModelsWpf.Controls.EngineLabel).First();
            Assert.NotNull(engineLabel);

            var currentEngineTextBlock = (engineLabel.Content as Grid).Children
                .Cast<UIElement>()
                .Where(x=>x.GetType() == typeof(TextBlock))
                .Select(x => x as TextBlock)
                .First();
            var defaultEngineLabelText = currentEngineTextBlock.Text;

            // Act
            nodeModel.Engine = engineChange;
            var engineLabelTextAfterChange = currentEngineTextBlock.Text;

            // Assert
            Assert.IsTrue(nodeView.PresentationGrid.IsVisible);
            Assert.AreEqual(expectedDefaultEngineLabelText, defaultEngineLabelText);
            Assert.AreEqual(engineChange.ToString(), engineLabelTextAfterChange);

        }
    }
}
