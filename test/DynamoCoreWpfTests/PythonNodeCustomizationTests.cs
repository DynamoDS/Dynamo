using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;
using PythonNodeModels;
using PythonNodeModelsWpf;

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

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCPython.dll");
            libraries.Add("DSIronPython.dll");
            base.GetLibrariesToPreload(libraries);
        }

        /// <summary>
        /// This test checks if its possible to change the Python nodemodels Engine property
        /// from the dropdown selector inside the script editor.
        /// </summary>
        [Test]
        public void CanChangeEngineFromScriptEditorDropDown()
        {
            // Arrange
            var expectedAvailableEngines = Enum.GetValues(typeof(PythonNodeModels.PythonEngineVersion)).Cast<PythonNodeModels.PythonEngineVersion>();
            var expectedDefaultEngine = PythonNodeModels.PythonEngineVersion.IronPython2;
            var engineChange = PythonNodeModels.PythonEngineVersion.CPython3;

            Open(@"core\python\python.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeModels.PythonNodeBase;
            Assert.NotNull(nodeModel);

            var scriptWindow = EditPythonCode(nodeView,View);

            var engineSelectorComboBox = FindEditorDropDown(scriptWindow);

            // Act
            var engineBeforeChange = engineSelectorComboBox.SelectedItem;
            var comboBoxEngines = engineSelectorComboBox.Items.SourceCollection;
            var engineAfterChange = engineSelectorComboBox.SelectedItem = engineChange;

            // Assert
            Assert.AreEqual(engineSelectorComboBox.Visibility, Visibility.Visible);
            CollectionAssert.AreEqual(expectedAvailableEngines, comboBoxEngines);
            Assert.AreEqual(expectedDefaultEngine, engineBeforeChange);
            Assert.AreEqual(engineSelectorComboBox.SelectedItem, PythonNodeModels.PythonEngineVersion.CPython3);
            Assert.AreEqual(nodeModel.Engine, engineAfterChange);
            var engineMenuItem = nodeView.MainContextMenu
                .Items
                .Cast<MenuItem>()
                .First(x => x.Header.ToString() == "Python Engine Version");
            var ironPython2MenuItem = engineMenuItem.Items
                .Cast<MenuItem>()
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo);
            var cPython3MenuItem = engineMenuItem.Items
                .Cast<MenuItem>()
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree);
            Assert.AreEqual(false, ironPython2MenuItem.IsChecked);
            Assert.AreEqual(true, cPython3MenuItem.IsChecked);
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// This test checks if its changing the engine via 
        /// dropdown selector inside the script editor executes the most up to date code.
        /// </summary>
        [Test]
        public void ChangingDropdownEngineSavesCodeBeforeRunning()
        {
            // Arrange
            var engineChange = PythonNodeModels.PythonEngineVersion.CPython3;

            Open(@"core\python\python.dyn");
            (Model.CurrentWorkspace as HomeWorkspaceModel).RunSettings.RunType = Dynamo.Models.RunType.Automatic;
            Assert.AreEqual(1, (Model.CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);
            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeModels.PythonNodeBase;
            Assert.NotNull(nodeModel);
            var scriptWindow = EditPythonCode(nodeView,View);

            var engineSelectorComboBox = FindEditorDropDown(scriptWindow);

            // Act

            //modify code in editor
            Assert.AreEqual("ok",(nodeModel as PythonNode).Script);
            SetTextEditorText(scriptWindow, "OUT = 100");
            //theres one execution from opening the graph.
            Assert.AreEqual(1, (Model.CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);
            //modify engine
            engineSelectorComboBox.SelectedItem = engineChange;
            //theres two executions from modifying the engine.
            Assert.AreEqual(2, (Model.CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);

            //assert model code is updated.
            Assert.AreEqual("OUT = 100", (nodeModel as PythonNode).Script);
            DispatcherUtil.DoEvents();
            Assert.AreEqual(100, nodeModel.CachedValue.Data);
            //still only 2 executions.
            Assert.AreEqual(2,(Model.CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);
            DispatcherUtil.DoEvents();
        }

        private static ComboBox FindEditorDropDown(ScriptEditorWindow view)
        {
            DispatcherUtil.DoEvents();
            // after simulating the click event get the opened Script editor window
            // and fetch the EngineSelector dropdown
            var windowGrid = view.Content as Grid;
            var engineSelectorComboBox = windowGrid
                .ChildrenOfType<ComboBox>()
                .First(x => x.Name == "EngineSelectorComboBox");
            return engineSelectorComboBox;
        }

        private static ScriptEditorWindow EditPythonCode(NodeView nodeView, DynamoView window)
        {
            DispatcherUtil.DoEvents();
            // get the `Edit...` menu item from the nodes context menu so we can simulate the click event.
            var editMenuItem = nodeView.MainContextMenu
                .Items
                .Cast<MenuItem>()
                .First(x => x.Header.ToString() == "Edit...");

            editMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            DispatcherUtil.DoEvents();
            return window.GetChildrenWindowsOfType<ScriptEditorWindow>().First();
        }

        private static void SetTextEditorText(ScriptEditorWindow view,string code)
        {
            var editor = view.ChildrenOfType<ICSharpCode.AvalonEdit.TextEditor>();
            editor.FirstOrDefault().Text = code;
            DispatcherUtil.DoEvents();
        }

        private static void SetEngineViaContextMenu(NodeView nodeView, PythonEngineVersion engine)
        {
            var engineSelection = nodeView.MainContextMenu.Items
                      .Cast<MenuItem>()
                      .Where(item => (item.Header as string) == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher).FirstOrDefault();
            switch (engine)
            {
                case PythonEngineVersion.IronPython2:
                    (engineSelection.Items[0] as MenuItem).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    break;
                case PythonEngineVersion.CPython3:
                    (engineSelection.Items[1] as MenuItem).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    break;
            }
            DispatcherUtil.DoEvents();
        } 


        /// <summary>
        /// This test checks if its possible to change the Python nodemodels Engine property
        /// from the context menu on the Python From String node.
        /// </summary>
        [Test]
        public void CanChangePythonEngineFromContextMenuOnPythonFromStringNode()
        {
            // Arrange
            // Setup the python3 debug mode, otherwise we wont be able to get the engine version selector 
            // from the nodes context menu
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
                .First(x => x.Header.ToString() == "Python Engine Version");

            var engineMenuItems = editMenuItem.Items;
            var ironPython2MenuItem = engineMenuItems
                .Cast<MenuItem>()
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo);
            var cPython3MenuItem = engineMenuItems
                .Cast<MenuItem>()
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree);

            // Act
            ironPython2MenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            var engineVersionAfterChange = nodeModel.Engine;

            // Assert
            Assert.AreEqual(expectedEngineVersionOnOpen, engineVersionOnOpen);
            CollectionAssert.AreEqual(expectedEngineMenuItems, engineMenuItems.Cast<MenuItem>().Select(x => x.Header));
            Assert.AreEqual(expectedEngineVersionAfterChange, engineVersionAfterChange);
            Assert.AreEqual(true, ironPython2MenuItem.IsChecked);
            Assert.AreEqual(false, cPython3MenuItem.IsChecked);

            // Act
            nodeModel.Engine = PythonNodeModels.PythonEngineVersion.CPython3;

            // Assert
            Assert.AreEqual(false, ironPython2MenuItem.IsChecked);
            Assert.AreEqual(true, cPython3MenuItem.IsChecked);
            DispatcherUtil.DoEvents();
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
            DispatcherUtil.DoEvents();
        }

        [Test]
        public void WorkspaceWithMultiplePythonEnginesUpdatesCorrectlyViaContextHandler()
        {
            // open test graph
            Open(@"core\python\WorkspaceWithMultiplePythonEngines.dyn");

            var nodeModels = ViewModel.Model.CurrentWorkspace.Nodes.Where(n => n.NodeType == "PythonScriptNode");
            List<PythonNode> pythonNodes = nodeModels.Cast<PythonNode>().ToList();
            var pynode1 = pythonNodes.ElementAt(0);
            var pynode2 = pythonNodes.ElementAt(1);
            var pynode1view = NodeViewWithGuid("d060e68f-510f-43fe-8990-c2c1ba7e0f80");
            var pynode2view = NodeViewWithGuid("4050d23e-529c-43e9-b614-0506d8adb06b");


            Assert.AreEqual(new List<string> { "2.7.9", "2.7.9" }, pynode2.CachedValue.GetElements().Select(x=>x.Data));

            SetEngineViaContextMenu(pynode1view, PythonEngineVersion.CPython3);

            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(new List<string> { "3.8.3", "2.7.9" }, pynode2.CachedValue.GetElements().Select(x => x.Data));

            SetEngineViaContextMenu(pynode2view, PythonEngineVersion.CPython3);

            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(new List<string> { "3.8.3", "3.8.3" }, pynode2.CachedValue.GetElements().Select(x => x.Data));

            SetEngineViaContextMenu(pynode1view, PythonEngineVersion.IronPython2);
            SetEngineViaContextMenu(pynode2view, PythonEngineVersion.IronPython2);

            Assert.IsTrue(ViewModel.Model.CurrentWorkspace.HasUnsavedChanges);
            Assert.AreEqual(new List<string> { "2.7.9", "2.7.9" }, pynode2.CachedValue.GetElements().Select(x => x.Data));
            DispatcherUtil.DoEvents();
        }
    }
}
