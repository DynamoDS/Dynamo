using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
    public class PythonNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<PythonNode>
    {       
        private DynamoViewModel dynamoViewModel;
        private PythonNode pythonNodeModel;
        private NodeView pythonNodeView;
        private WorkspaceModel workspaceModel;
        private ScriptEditorWindow editWindow;
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
            pythonNodeModel.EditNode += EditScriptContent;

            pythonEngineVersionMenu = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher, IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(pythonEngineVersionMenu);

            learnMoreItem.Click += OpenPythonLearningMaterial;

            var availableEngineNames = PythonEngineManager.Instance.AvailableEngines.Select(x => x.Name).ToList();
            // Add the serialized Python Engine even if it is missing (so that the user does not see an empty slot)
            if (!availableEngineNames.Contains(nodeModel.EngineName))
            {
                availableEngineNames.Add(nodeModel.EngineName);
            }
            availableEngineNames.ForEach(x => AddPythonEngineToMenuItems(x));

            PythonEngineManager.Instance.AvailableEngines.CollectionChanged += PythonEnginesChanged;

            nodeView.MainContextMenu.Items.Add(learnMoreItem);

            nodeView.UpdateLayout();

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

            editWindowItem.Click -= EditScriptContent;
            pythonNodeModel.EditNode -= EditScriptContent;

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
            if (editWindow != null)
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
            using (var cmd = Dynamo.Logging.Analytics.TrackCommandEvent("PythonEdit"))
            {
                if (editWindow != null)
                {
                    editWindow.Activate();
                }
                else
                {
                    editWindow = new ScriptEditorWindow(dynamoViewModel, pythonNodeModel, pythonNodeView, ref editorWindowRect);
                    if (pythonNodeModel.ScriptContentSaved)
                    {
                        editWindow.Initialize(workspaceModel.Guid, pythonNodeModel.GUID, "ScriptContent", pythonNodeModel.Script);
                    }
                    else
                    {
                        editWindow.Initialize(workspaceModel.Guid, pythonNodeModel.GUID, "ScriptContent", sender.ToString());
                        editWindow.IsSaved = false;
                    }
                    editWindow.Closed += editWindow_Closed;

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

        private bool IsEditorInDockedState()
        {
            var nodemodel = pythonNodeModel as NodeModel;
            dynamoViewModel.NodeWindowsState.TryGetValue(nodemodel.GUID.ToString(), out ViewExtensionDisplayMode viewExtensionDisplayMode);

            return viewExtensionDisplayMode.Equals(ViewExtensionDisplayMode.DockRight);
        }

        private void EditScriptContent(string text)
        {
            EditScriptContent(text, null);
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateEngine(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                dynamoViewModel.ExecuteCommand(
                   new DynamoModel.UpdateModelValueCommand(
                       Guid.Empty, pythonNodeModel.GUID, nameof(pythonNodeModel.EngineName), (string)menuItem.Header));
                pythonNodeModel.OnNodeModified();
            }
        }

        private void PythonEnginesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    AddPythonEngineToMenuItems((item as PythonEngine).Name);
                }   
            }
        }

        /// <summary>
        /// Adds python engine to MenuItems
        /// </summary>
        private void AddPythonEngineToMenuItems(string engineName)
        {
            var pythonEngineItem = new MenuItem { Header = engineName, IsCheckable = false };
            pythonEngineItem.Click += UpdateEngine;
            pythonEngineItem.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(pythonNodeModel.EngineName))
            {
                Source = pythonNodeModel,
                Converter = new CompareToParameterConverter(),
                ConverterParameter = engineName
            });
            pythonEngineVersionMenu.Items.Add(pythonEngineItem);
        }
    }
}
