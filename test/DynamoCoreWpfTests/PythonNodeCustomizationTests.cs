using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CoreNodeModels;
using Dynamo.Controls;
using Dynamo.DocumentationBrowser;
using Dynamo.Graph.Workspaces;
using Dynamo.PythonServices;
using Dynamo.Tests;
using Dynamo.Utilities;
using DynamoCoreWpfTests.Utility;
using ICSharpCode.AvalonEdit.Folding;
using NUnit.Framework;
using PythonNodeModels;
using PythonNodeModelsWpf;

namespace DynamoCoreWpfTests
{
    public class PythonNodeCustomizationTests : DynamoTestUIBase
    {
        bool bTextEnteringEventRaised = false;

        private readonly List<string> expectedEngineMenuItems = new List<string>()
        {
            PythonEngineManager.CPython3EngineName,
            PythonEngineManager.IronPython2EngineName
        };

        private static DependencyObject GetInfoBubble(DependencyObject parent)
        {
            if (parent.GetType().Name == nameof(InfoBubbleView))
            {
                return parent;
            }

            foreach (var child in parent.Children())
            {
                var output = GetInfoBubble(child);
                if (output != null)
                {
                    return output;
                }
            }
            return null;
        }

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("DSCPython.dll");
            libraries.Add("VMDataBridge.dll");
            libraries.Add("DSCoreNodes.dll");
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
            var expectedAvailableEngines = new List<string>()
            {
                PythonEngineManager.CPython3EngineName,
                PythonEngineManager.IronPython2EngineName,
                
            };
            var expectedDefaultEngine = PythonEngineManager.IronPython2EngineName;
            var engineChange = PythonEngineManager.CPython3EngineName;

            Open(@"core\python\python.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
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
            Assert.AreEqual(engineSelectorComboBox.SelectedItem, PythonEngineManager.CPython3EngineName);
            
            //Assert that selecting an engine from drop-down without saving won't update the engine.
            Assert.AreEqual(nodeModel.EngineName, engineBeforeChange);
            Assert.AreEqual(scriptWindow.CachedEngine, engineAfterChange);

            //Clicking save button to actually update the engine.
            var saveButton = scriptWindow.FindName("SaveScriptChangesButton") as Button;
            saveButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(nodeModel.EngineName, engineAfterChange);
            var engineMenuItem = nodeView.MainContextMenu
                .Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == "Python Engine Version");
            var ironPython2MenuItem = engineMenuItem.Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo);
            var cPython3MenuItem = engineMenuItem.Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree);
            Assert.AreEqual(false, ironPython2MenuItem.IsChecked);
            Assert.AreEqual(true, cPython3MenuItem.IsChecked);
            DispatcherUtil.DoEvents();
        }

        /// <summary>
        /// This test will validate the OnTextAreaTextEntering event that is generated by the ScriptEdirtorWindows when doing auto-completion
        /// </summary>
        [Test]
        public void OnTextAreaTextEnteringEventTest()
        {
            // Arrange
            string pythonCode = "from System.Collections import ArrayList";
            Open(@"core\python\python.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
            Assert.NotNull(nodeModel);

            var scriptWindow = EditPythonCode(nodeView, View);
            var codeEditor = FindCodeEditor(scriptWindow);
            scriptWindow.editText.TextArea.TextEntering += TextArea_TextEntering;
            codeEditor.SelectionStart = 0;
            codeEditor.Text = pythonCode;
            codeEditor.Focus();

            //This will generate the event of going to the end of the line
            var textArea = Keyboard.FocusedElement;
            textArea.RaiseEvent(new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(codeEditor),
                0,
                Key.End)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            }
            );

            //This will pop up the automcompletion list info
            textArea.RaiseEvent(
                new TextCompositionEventArgs(
                    Keyboard.PrimaryDevice,
                    new TextComposition(InputManager.Current, textArea, "."))
                    {
                        RoutedEvent = TextCompositionManager.TextInputEvent
                    }
            );

            //This will indicate to the python script to import everything from the ArrayList module and will raise the OnTextAreaTextEntering entering to a specific code section
            textArea.RaiseEvent(
               new TextCompositionEventArgs(
                   Keyboard.PrimaryDevice,
                   new TextComposition(InputManager.Current, textArea, "*"))
               {
                   RoutedEvent = TextCompositionManager.TextInputEvent
               }
           );
     
            //Act
            DispatcherUtil.DoEvents();

            scriptWindow.editText.TextArea.TextEntering -= TextArea_TextEntering;
            //Assert
            //Validates that OnTextAreaTextEntering event was executed.
            Assert.IsNotNull(codeEditor.Text);
            Assert.That(codeEditor.Text, Is.EqualTo(pythonCode+".*"));
            Assert.IsTrue(bTextEnteringEventRaised);
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            bTextEnteringEventRaised = true;
        }

        /// <summary>
        /// The next test will execute the events:
        /// OnSaveClicked(object sender, RoutedEventArgs e)
        /// OnRevertClicked(object sender, RoutedEventArgs e)
        /// OnRunClicked(object sender, RoutedEventArgs e)
        /// </summary>
        [Test]
        public void OnSaveRunRevertEventsTest()
        {
            Open(@"core\python\python_check_output.dyn");
            ViewModel.HomeSpace.Run();

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
            Assert.NotNull(nodeModel);

            //Get the Watch node so we can check the content later
            var watchOUT = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("714838b43a114f5f93cfe2f1d26092cf");

            var scriptWindow = EditPythonCode(nodeView, View);
            var codeEditor = FindCodeEditor(scriptWindow);

            //Replace dictionary values so we can use the Revert and Run buttons
            codeEditor.Text = codeEditor.Text.Replace("Autodesk", "Softdesk").Replace("1982", "1997");
            codeEditor.Focus();

            //This will get the buttons from the WPF xaml code
            var saveButton = scriptWindow.FindName("SaveScriptChangesButton") as Button;
            var runButton = scriptWindow.FindName("RunPythonScriptButton") as Button;
            var revertButton = scriptWindow.FindName("RevertScriptChangesButton") as Button;

            //Press the run button
            runButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //Check that the changes are reflected in the Watch
            Assert.That(watchOUT.CachedValue,Is.EqualTo("Softdesk,1997"));

            //Pressing the Revert button
            revertButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();
            ViewModel.HomeSpace.Run();
            
            //After pressing the Revert button we need to have the previous result in Watch node
            Assert.That(watchOUT.CachedValue, Is.EqualTo("Autodesk,1982"));

            codeEditor.Text = codeEditor.Text.Replace("Softdesk","Autodesk").Replace("1997", "1982");
            saveButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            ViewModel.HomeSpace.Run();

            Assert.That(watchOUT.CachedValue, Is.EqualTo("Autodesk,1982"));
        }

        /// <summary>
        /// This test method will validate that the OnMoreInfoEvent was executed correctly
        /// </summary>
        [Test]
        public void OnMoreInfoEventTest()
        {
            Open(@"core\python\python_check_output.dyn");
            ViewModel.HomeSpace.Run();

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
            Assert.NotNull(nodeModel);

            //Get the Watch node so we can check the content later
            var watchOUT = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("714838b43a114f5f93cfe2f1d26092cf");

            var scriptWindow = EditPythonCode(nodeView, View);
            var codeEditor = FindCodeEditor(scriptWindow);

            //This line is replacing the python dictionary values so later after re-running we will see the changes in the Watch node
            codeEditor.Text = codeEditor.Text.Replace("Autodesk", "Softdesk").Replace("1982", "1997");
            codeEditor.Focus();

            //Check that we don't have any extension tabs shown right now
            Assert.That(this.View.ExtensionTabItems.Count, Is.EqualTo(0));

            var moreInfoButton = scriptWindow.FindName("MoreInfoButton") as Button;

            //Pressing the MoreInfo button, here the OnMoreInfoEvent is raised 
            moreInfoButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEvents();

            //Check that now we are showing a extension tab (DocumentationBrowser)
            Assert.That(this.View.ExtensionTabItems.Count, Is.EqualTo(1));

            var docBrowser = this.View.ExtensionTabItems
                            .Where(x => x.Content.GetType().Equals(typeof(DocumentationBrowserView)))
                            .FirstOrDefault();

            //Validate that the DocmentationBrowser extension tab is valid
            Assert.IsNotNull(docBrowser);
            Assert.IsTrue(docBrowser.IsVisible);
        }

        /// <summary>
        /// PythonNode
        /// This test method will click in the "Learn more about Python" button and the OpenPythonLearningMaterial method from PythonNode.cs will be executed
        /// </summary>
        [Test]
        public void OpenPythonLearningMaterialValidationTest()
        {
            Open(@"core\python\python_check_output.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
            Assert.NotNull(nodeModel);

            //Open the python script editor
            var scriptWindow = EditPythonCode(nodeView, View);
               
            var learnMoreMenuItem = nodeView.MainContextMenu
                .Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == "Learn more about Python");

            Assert.IsNotNull(learnMoreMenuItem);

            //Click the button and internally the  OpenPythonLearningMaterial method is executed
            learnMoreMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            DispatcherUtil.DoEvents();

            var learnMoreTab = this.View.ExtensionTabItems
                                .Where(x => x.Content.GetType().Equals(typeof(DocumentationBrowserView)))
                                .FirstOrDefault();

            //Validate tha the documentation browser tab is opened and visible once we clicked the button.
            Assert.IsNotNull(learnMoreTab);
            Assert.IsTrue(learnMoreTab.IsVisible);
        }

        /// <summary>
        /// PythonStringNode
        /// This test method will click in the "Learn more about Python" button and the OpenPythonLearningMaterial method from PythonStringNode.cs will be executed
        /// </summary>
        [Test]
        public void OpenPythonLearningMaterial_PythonNodeFromStringValidationTest()
        {
            Open(@"core\python\pyFromString_UnsavedEngine.dyn");

            var nodeView = NodeViewWithGuid("bad59bc8-9b49-47b6-99ee-34fa8dca91ae");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
            Assert.NotNull(nodeModel);

            var learnMoreMenuItem = nodeView.MainContextMenu
                .Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == "Learn more about Python");

            Assert.IsNotNull(learnMoreMenuItem);

            //Click the button and internally the  OpenPythonLearningMaterial method is executed
            learnMoreMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            DispatcherUtil.DoEvents();

            var learnMoreTab = this.View.ExtensionTabItems
                                .Where(x => x.Content.GetType().Equals(typeof(DocumentationBrowserView)))
                                .FirstOrDefault();

            //Validate tha the documentation browser tab is opened and visible once we clicked the button.
            Assert.IsNotNull(learnMoreTab);
            Assert.IsTrue(learnMoreTab.IsVisible);
        }

        /// <summary>
        /// PythonStringNode
        /// This test method will click in the "Python Engine Version" menu option and the UpdateToPython3Engine() method from PythonStringNode.cs will be executed
        /// </summary>
        [Test]
        public void UpdateToPython3Engine_PythonStringNodeTest()
        {
            Open(@"core\python\pythonFromString2to3Test.dyn");

            var nodeView = NodeViewWithGuid("bad59bc8-9b49-47b6-99ee-34fa8dca91ae");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;

            //Get the Watch node so we can check the content later
            var watchOUT = Model.CurrentWorkspace.NodeFromWorkspace<Watch>("971220846c1b4d54aa11cca81f417c2b");
            Assert.NotNull(nodeModel);

            //Get the python engine menu 
            var engineMenuItem = nodeView.MainContextMenu
                .Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == "Python Engine Version");

            Assert.IsNotNull(engineMenuItem);

            //Get the python engine menu option "CPython3"
            var cPython3MenuItem = engineMenuItem.Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree);

            Assert.IsNotNull(cPython3MenuItem);

            //Click the CPython3 option (previously was IronPython2)
            cPython3MenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            DispatcherUtil.DoEvents();

            //After running the graph if the python code is not valid for CPython3 we will get a null value in the Watch node
            ViewModel.HomeSpace.Run();

            //Validate that the content of the Watch node is what we expected after running the Graph
            Assert.That(watchOUT.CachedValue, Is.EqualTo("Hello World 2020"));

        }

        /// <summary>
        /// This test method will execute the EditScriptContent method from the PythonNode.cs file (when there is already a Python Editor opened).
        /// </summary>
        [Test]
        public void EditScriptContent_ReActivateTest()
        {
            Open(@"core\python\python_check_output.dyn");
            ViewModel.HomeSpace.Run();

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
            Assert.NotNull(nodeModel);

            //This line will show the script editor window the first time
            var scriptWindow = EditPythonCode(nodeView, View);
            //Hide the script editor window
            scriptWindow.Hide();
            //Just validates that the script editor window is hidden
            Assert.That(scriptWindow.Visibility, Is.EqualTo(Visibility.Hidden));

            //Because previously the Edit Script window was hidden we will check that after calling the EditScriptContent method again will show the windows
            //The Window.Show() method is Async so we need to do the Visibility verification in a function subscribed to the event
            RoutedEventHandler tmpDelegate = (object s, RoutedEventArgs ev) => Assert.That(scriptWindow.Visibility, Is.EqualTo(Visibility.Visible));
            scriptWindow.Loaded += tmpDelegate;

            //This call will reactivate the Edit python code window and a specific section of the EditScriptContent method is executed.
            scriptWindow = EditPythonCode(nodeView, View);

            //Unsubscribing the previous delegate assigned
            scriptWindow.Loaded -= tmpDelegate;
      
        }


        /// <summary>
        /// This test checks if its changing the engine via 
        /// dropdown selector inside the script editor executes the most up to date code.
        /// </summary>
        [Test]
        public void ChangingDropdownEngineDoesNotSavesCodeOrRun()
        {
            // Arrange
            var engineChange = PythonEngineManager.CPython3EngineName;

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
            //theres still one executions from modifying the engine, as changing engines does not trigger a run.
            Assert.AreEqual(1, (Model.CurrentWorkspace as HomeWorkspaceModel).EvaluationCount);

            //assert model code is updated.
            Assert.AreEqual("ok", (nodeModel as PythonNode).Script);

            DispatcherUtil.DoEvents();
        }

        private static ICSharpCode.AvalonEdit.TextEditor FindCodeEditor(ScriptEditorWindow view)
        {
            DispatcherUtil.DoEvents();
            var windowGrid = view.Content as Grid;
            var codeEditor = windowGrid
                .ChildrenOfType<ICSharpCode.AvalonEdit.TextEditor>()
                .First();
            return codeEditor;
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
                .OfType<MenuItem>()
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

        private static void SetEngineViaContextMenu(NodeView nodeView, string engine)
        {
            var engineSelection = nodeView.MainContextMenu.Items
                      .OfType<MenuItem>()
                      .Where(item => (item.Header as string) == PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher).FirstOrDefault();
            switch (engine)
            {
                case "IronPython2":
                    (engineSelection.Items[0] as MenuItem).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    break;
                case "CPython3":
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
            var expectedEngineVersionOnOpen = PythonEngineManager.IronPython2EngineName;
            var expectedEngineVersionAfterChange = PythonEngineManager.CPython3EngineName;

            Open(@"core\python\pyFromString_UnsavedEngine.dyn");

            var nodeView = NodeViewWithGuid(new Guid("bad59bc89b4947b699ee34fa8dca91ae").ToString("D"));
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeModels.PythonStringNode;
            Assert.NotNull(nodeModel);

            var engineVersionOnOpen = nodeModel.EngineName;

            var editMenuItem = nodeView.MainContextMenu
                .Items
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == "Python Engine Version");

            var engineMenuItems = editMenuItem.Items;
            var ironPython2MenuItem = engineMenuItems
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == PythonEngineManager.IronPython2EngineName);
            var cPython3MenuItem = engineMenuItems
                .OfType<MenuItem>()
                .First(x => x.Header.ToString() == PythonEngineManager.CPython3EngineName);

            // Act
            cPython3MenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            var engineVersionAfterChange = nodeModel.EngineName;

            // Assert
            Assert.AreEqual(expectedEngineVersionOnOpen, engineVersionOnOpen);
            CollectionAssert.AreEqual(expectedEngineMenuItems, engineMenuItems.Cast<MenuItem>().Select(x => x.Header));
            Assert.AreEqual(expectedEngineVersionAfterChange, engineVersionAfterChange);
            Assert.AreEqual(false, ironPython2MenuItem.IsChecked);
            Assert.AreEqual(true, cPython3MenuItem.IsChecked);

            // Act
            nodeModel.EngineName = PythonEngineManager.IronPython2EngineName;

            // Assert
            Assert.AreEqual(true, ironPython2MenuItem.IsChecked);
            Assert.AreEqual(false, cPython3MenuItem.IsChecked);
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
            var expectedDefaultEngineLabelText = PythonEngineManager.IronPython2EngineName;
            var engineChange = PythonEngineManager.CPython3EngineName;

            Open(@"core\python\python.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeModels.PythonNodeBase;
            Assert.NotNull(nodeModel);

            var engineLabel = nodeView.grid
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
            nodeModel.EngineName = engineChange;
            var engineLabelTextAfterChange = currentEngineTextBlock.Text;

            // Assert
            Assert.IsTrue(nodeView.grid.IsVisible);
            Assert.AreEqual(expectedDefaultEngineLabelText, defaultEngineLabelText);
            Assert.AreEqual(engineChange.ToString(), engineLabelTextAfterChange);
            DispatcherUtil.DoEvents();
        }

        [Test]
        public void TabWithSpacesMatchesEngine()
        {
            // Arrange
            Open(@"core\python\python.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
            Assert.NotNull(nodeModel);

            var scriptWindow = EditPythonCode(nodeView, View);
            var codeEditor = FindCodeEditor(scriptWindow);
            var engineSelectorComboBox = FindEditorDropDown(scriptWindow);

            Assert.AreEqual(PythonEngineManager.IronPython2EngineName, engineSelectorComboBox.SelectedItem);

            // Act
            codeEditor.Focus();
            codeEditor.SelectionStart = 0;
            var textArea = Keyboard.FocusedElement;
            textArea.RaiseEvent(new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(codeEditor),
                0,
                Key.Tab)
                {
                    RoutedEvent = Keyboard.KeyDownEvent
                }
            );
            DispatcherUtil.DoEvents();

            engineSelectorComboBox.SelectedItem = PythonEngineManager.CPython3EngineName;

            codeEditor.SelectionStart = 0;
            textArea.RaiseEvent(new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(codeEditor),
                0,
                Key.Tab)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            }
            );
            DispatcherUtil.DoEvents();

            // Assert
            StringAssert.StartsWith("    \t", codeEditor.Text);
        }

        [Test]
        public void PythonNodeErrorBubblePersists()
        {
            // open file
            var model = ViewModel.Model;
            Open(@"core\python\python.dyn");
            Run();

            // get the python node and check Engine property
            var workspace = model.CurrentWorkspace;
            var nodeModel = workspace.NodeFromWorkspace("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var pynode = nodeModel as PythonNode;
            Assert.AreEqual(pynode.EngineName, PythonEngineManager.IronPython2EngineName);

            Assert.AreEqual(pynode.State, Dynamo.Graph.Nodes.ElementState.Warning);
            DispatcherUtil.DoEvents();
            var nodeView = NodeViewWithGuid(nodeModel.GUID.ToString());
            
            Assert.IsNotNull(nodeView);
            Assert.IsNotNull(nodeView.ViewModel.ErrorBubble);

            nodeView.UpdateLayout();
       
            var errorBubble = GetInfoBubble(View);
            Assert.IsNotNull(errorBubble);
            Assert.AreEqual(Visibility.Visible, (errorBubble as UIElement).Visibility);
        }

        /// <summary>
        /// This test evaluates the functionality of the Tab Folding Strategy
        /// Tab consists of 4 spaces and every tab yields a new folding
        /// A folding closes when reaching a line with the same number of tabs it was opened with
        /// </summary>
        [Test]
        public void AvalonEditTabFoldingStrategyTest()
        {
            // Arrange
            string pythonCode = "from System.Collections import ArrayList";
            Open(@"core\python\python.dyn");

            var nodeView = NodeViewWithGuid("3bcad14e-d086-4278-9e08-ed2759ef92f3");
            var nodeModel = nodeView.ViewModel.NodeModel as PythonNodeBase;
            Assert.NotNull(nodeModel);

            var scriptWindow = EditPythonCode(nodeView, View);
            var codeEditor = FindCodeEditor(scriptWindow);
            var foldings = scriptWindow.foldingManager.AllFoldings.Count();
            DispatcherUtil.DoEvents();

            Assert.IsTrue(foldings == 0);

            var textWithFourFoldings =
                @"def TestDef(self):
    # Every tab will cause a new foldnig to be created
    self.data = 'reloaded'    

try:
    data = true
    if data:
        pass
except:
    data = false

# Inadequate number of spaces different than 4 should not form a folding
# Therefore leaving any amount of spaces not divisible by 4 should not yield further foldings

class NoTabs():
   value = true
       pass

";
            SetTextEditorText(scriptWindow, textWithFourFoldings);
            foldings = scriptWindow.foldingManager.AllFoldings.Count();
            DispatcherUtil.DoEvents();

            Assert.IsTrue(foldings == 4);
        }
    }
}
