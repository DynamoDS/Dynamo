using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.PythonServices;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.Windows;
using PythonNodeModels;
using PythonNodeModelsWpf.Controls;

namespace PythonNodeModelsWpf
{
    internal static class PythonNodeUtils
    {
        /// <summary>
        /// Returns unique set of names from loaded and deserialized engines.
        /// </summary>
        /// <param name="nodeModel"></param>
        /// <returns></returns>
        internal static List<string> GetEngineNames(NodeModel nodeModel)
        {
            var engineName = string.Empty;
            switch (nodeModel)
            {
                case PythonNode pyNode:
                    engineName = pyNode.EngineName;
                    break;
                case PythonStringNode pyNode:
                    engineName = pyNode.EngineName;
                    break;
            }

            var availableEngineNames = PythonEngineManager.Instance.AvailableEngines.Select(x => x.Name).ToList();
            // Add the serialized Python Engine even if it is missing (so that the user does not see an empty slot)
            if (!availableEngineNames.Contains(engineName))
            {
                availableEngineNames.Add(engineName);
            }
            return availableEngineNames.Distinct().ToList();
        }
    }

    public class PythonNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<PythonNode>
    {       
        private DynamoViewModel dynamoViewModel;
        private PythonNode pythonNodeModel;
        private NodeView pythonNodeView;
        private WorkspaceModel workspaceModel;
        private ScriptEditorWindow editWindow;
        private DynamoView dynamoView;
        private ModelessChildWindow.WindowRect editorWindowRect;
        private readonly MenuItem editWindowItem = new MenuItem { Header = PythonNodeModels.Properties.Resources.EditHeader, IsCheckable = false };
        private MenuItem pythonEngineVersionMenu;
        private readonly MenuItem learnMoreItem = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuLearnMoreButton };

        public void CustomizeView(PythonNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            pythonNodeModel = nodeModel;
            pythonNodeView = nodeView;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            workspaceModel = nodeView.ViewModel.WorkspaceViewModel.Model;

            nodeView.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += EditScriptContent;

            var previousdelegates = pythonNodeModel.GetInvocationListForEditAction();

            // Unsubscibe any previous listeners to the EditNode action when switching views.
            foreach (Delegate d in previousdelegates)
            {
                pythonNodeModel.EditNode -= (Action<string>)d;
            }

            pythonNodeModel.EditNode += EditScriptContent;
            pythonNodeModel.PropertyChanged += NodeModel_PropertyChanged;

            pythonEngineVersionMenu = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher, IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(pythonEngineVersionMenu);

            learnMoreItem.Click += OpenPythonLearningMaterial;

            PythonNodeUtils.GetEngineNames(nodeModel).ForEach(engineName => dynamoViewModel.AddPythonEngineToMenuItems(
                new List<PythonNodeBase>() { pythonNodeModel }, pythonEngineVersionMenu, UpdateEngine, engineName));

            PythonEngineManager.Instance.AvailableEngines.CollectionChanged += PythonEnginesChanged;

            nodeView.MainContextMenu.Items.Add(learnMoreItem);
            

            nodeView.MouseDown += View_MouseDown;
            nodeModel.DeletionStarted += NodeModel_DeletionStarted;
            nodeModel.Disposed += NodeModel_Disposed;

            EngineLabel engineLabel = new EngineLabel(nodeModel);
            Canvas.SetZIndex(engineLabel, 5);
            engineLabel.HorizontalAlignment = HorizontalAlignment.Left;
            engineLabel.VerticalAlignment = VerticalAlignment.Bottom;
            engineLabel.Margin = new Thickness(14, -4, -10, 4);
            nodeView.grid.Children.Add(engineLabel);
            Grid.SetColumn(engineLabel, 0);
            Grid.SetRow(engineLabel, 3);
        }

       

        /// <summary>
        /// Check if the script editor is saved.
        /// </summary>
        /// <returns></returns>
        internal bool IsEditorSaved()
        {
            if (editWindow != null)
            {
                return editWindow.IsSaved;
            }
            return false;
        }

        /// <summary>
        /// Learn More button handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenPythonLearningMaterial(object sender, RoutedEventArgs e)
        {
            dynamoViewModel.OpenDocumentationLinkCommand.Execute(new OpenDocumentationLinkEventArgs(
                new Uri(PythonNodeModels.Properties.Resources.PythonMigrationWarningUriString, UriKind.Relative)));
        }

        private void NodeModel_Disposed(Dynamo.Graph.ModelBase obj)
        {
            if (editWindow != null)
            {
                editWindow.Close();
            }

            // When the node is disposed, close the script editor if it is docked in the right SideBar
            if (dynamoView != null && IsEditorCurrentlyDocked())
            {
                var tabItem = dynamoViewModel.SideBarTabItems.FirstOrDefault(t => t.Uid == pythonNodeModel.GUID.ToString());
                dynamoView.CloseRightSideBarTab(tabItem);
            }

            editWindowItem.Click -= EditScriptContent;
            pythonNodeModel.EditNode -= EditScriptContent;
            pythonNodeModel.PropertyChanged -= NodeModel_PropertyChanged;

            if (pythonEngineVersionMenu != null)
            {
                foreach (var item in pythonEngineVersionMenu.Items)
                {
                    if (item is MenuItem menuItem)
                    {
                        menuItem.Click -= UpdateEngine;
                    }
                }
            }

            PythonEngineManager.Instance.AvailableEngines.CollectionChanged -= PythonEnginesChanged;
            learnMoreItem.Click -= OpenPythonLearningMaterial;
        }

        private void NodeModel_DeletionStarted(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (editWindow != null || IsEditorCurrentlyDocked())
            {
                var res = MessageBoxService.Show(
                    String.Format(
                        PythonNodeModels.Properties.Resources.DeletingPythonNodeWithOpenEditorMessage, 
                        this.pythonNodeModel.Name),
                    PythonNodeModels.Properties.Resources.DeletingPythonNodeWithOpenEditorTitle,
                    MessageBoxButton.OKCancel, 
                    MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void View_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                EditScriptContent(null, null);
                e.Handled = true;
            }
        }

        public void editWindow_Closed(object sender, EventArgs e)
        {
            editWindow = null;
        }

        private void EditScriptContent(object sender, EventArgs e)
        {
            try
            {
                using (var cmd = Dynamo.Logging.Analytics.TrackTaskCommandEvent("PythonEdit"))
                {
                    if (editWindow != null)
                    {
                        editWindow.Activate();
                        dynamoView = editWindow.Owner as DynamoView;
                    }
                    else
                    {
                        editWindow = new ScriptEditorWindow(dynamoViewModel, pythonNodeModel, pythonNodeView, ref editorWindowRect);

                        if (pythonNodeModel.ScriptContentSaved || sender == null)
                        {
                            editWindow.Initialize(workspaceModel.Guid, pythonNodeModel.GUID, "ScriptContent", pythonNodeModel.Script);
                        }
                        else
                        {
                            editWindow.Initialize(workspaceModel.Guid, pythonNodeModel.GUID, "ScriptContent", sender.ToString());
                            editWindow.IsSaved = false;
                        }

                        editWindow.Closed += editWindow_Closed;
                        dynamoView = editWindow.Owner as DynamoView;

                        if (IsEditorInDockedState())
                        {
                            editWindow.DockWindow();
                        }
                        else
                        {
                            editWindow.Show();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dynamoViewModel.Model.Logger.Log("Failed to open script editor window.");
                dynamoViewModel.Model.Logger.Log(ex.Message);
                dynamoViewModel.Model.Logger.Log(ex.StackTrace);
            }
        }

        // Checks the state of this node script editor, either DockRight or FloatingWindow
        private bool IsEditorInDockedState()
        {
            var nodemodel = pythonNodeModel as NodeModel;
            dynamoViewModel.NodeWindowsState.TryGetValue(nodemodel.GUID.ToString(), out ViewExtensionDisplayMode viewExtensionDisplayMode);

            return viewExtensionDisplayMode.Equals(ViewExtensionDisplayMode.DockRight);
        }

        // Checks if this node editor is currrently docked(and opened) in the right side-bar
        private bool IsEditorCurrentlyDocked()
        {
            var nodemodel = pythonNodeModel as NodeModel;
            return dynamoViewModel.DockedNodeWindows.Contains(nodemodel.GUID.ToString());
        }

        private void EditScriptContent(string text)
        {
            EditScriptContent(text, null);
        }

        private void NodeModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // If this node is renamed, update the tab header in the right SideBar
                case nameof(NodeModel.Name):
                    if (dynamoView != null && IsEditorCurrentlyDocked())
                    {
                        TabItem tabItem = dynamoViewModel.SideBarTabItems.OfType<TabItem>().SingleOrDefault(n => n.Uid.ToString() == pythonNodeModel.GUID.ToString());
                        if (tabItem != null)
                        {
                            tabItem.Header = pythonNodeModel.Name;
                        }
                    }
                    break;
            }
        }


        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateEngine(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                dynamoViewModel.UpdatePythonNodeEngine(pythonNodeModel, (string)menuItem.Header);
            }
        }

        private void PythonEnginesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if(item is PythonEngine newEngine)
                    {
                        dynamoViewModel.AddPythonEngineToMenuItems(new List<PythonNodeBase>() { pythonNodeModel }, pythonEngineVersionMenu, UpdateEngine, newEngine.Name);
                    }
                }   
            }
        }
    }
}
