using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Dynamo.Wpf.Windows;
using PythonNodeModels;

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
        private readonly MenuItem pythonEngine2Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo, IsCheckable = true };
        private readonly MenuItem pythonEngine3Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree, IsCheckable = true };

        public void CustomizeView(PythonNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            pythonNodeModel = nodeModel;
            pythonNodeView = nodeView;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            workspaceModel = nodeView.ViewModel.WorkspaceViewModel.Model;

            nodeView.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += EditScriptContent;
            // If it is a Debug build, display a python engine switcher
            if (Dynamo.Configuration.DebugModes.IsEnabled("Python3DebugMode"))
            {
                var pythonEngineVersionMenu = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher, IsCheckable = false };
                nodeView.MainContextMenu.Items.Add(pythonEngineVersionMenu);
                pythonEngine2Item.Click += UpdateToPython2Engine;
                pythonEngine3Item.Click += UpdateToPython3Engine;
                pythonEngineVersionMenu.Items.Add(pythonEngine2Item);
                pythonEngineVersionMenu.Items.Add(pythonEngine3Item);

                // Check the correct item based on NodeModel engine version
                if (pythonNodeModel.Engine == PythonEngineVersion.IronPython2)
                {
                    pythonEngine2Item.IsChecked = true;
                    pythonEngine3Item.IsChecked = false;
                }
                else
                {
                    pythonEngine2Item.IsChecked = false;
                    pythonEngine3Item.IsChecked = true;
                }      
            }

            nodeView.UpdateLayout();

            nodeView.MouseDown += View_MouseDown;
            nodeModel.DeletionStarted += NodeModel_DeletionStarted;
            nodeModel.Disposed += NodeModel_Disposed;
        }

        private void NodeModel_Disposed(Dynamo.Graph.ModelBase obj)
        {
            if (editWindow != null)
            {
                editWindow.Close();
            }
            editWindowItem.Click -= EditScriptContent;
            pythonEngine2Item.Click -= UpdateToPython2Engine;
            pythonEngine3Item.Click -= UpdateToPython3Engine;
        }

        private void NodeModel_DeletionStarted(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (editWindow != null)
            {
                var res = MessageBox.Show(
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
                    editWindow.Initialize(workspaceModel.Guid, pythonNodeModel.GUID, "ScriptContent", pythonNodeModel.Script);
                    editWindow.Closed += editWindow_Closed;
                    editWindow.Show();
                }
            }
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython2Engine(object sender, EventArgs e)
        {
            pythonNodeModel.Engine = PythonEngineVersion.IronPython2;
            pythonEngine2Item.IsChecked = true;
            pythonEngine3Item.IsChecked = false;
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython3Engine(object sender, EventArgs e)
        {
            pythonNodeModel.Engine = PythonEngineVersion.CPython3;
            pythonEngine2Item.IsChecked = false;
            pythonEngine3Item.IsChecked = true;
        }
    }
}
