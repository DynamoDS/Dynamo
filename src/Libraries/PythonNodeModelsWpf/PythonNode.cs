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
        private MenuItem pythonEngine2Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo, IsCheckable = true };
        private MenuItem pythonEngine3Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree, IsCheckable = true };

        public void CustomizeView(PythonNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            pythonNodeModel = nodeModel;
            pythonNodeView = nodeView;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            workspaceModel = nodeView.ViewModel.WorkspaceViewModel.Model;

            var editWindowItem = new MenuItem { Header = PythonNodeModels.Properties.Resources.EditHeader, IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += delegate { EditScriptContent(); };
            // If it is a Debug build, display a python engine switcher
            if (dynamoViewModel.IsDebugBuild)
            {
                var pythonEngineVersionMenu = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher, IsCheckable = false };
                nodeView.MainContextMenu.Items.Add(pythonEngineVersionMenu);
                pythonEngine2Item.Click += delegate { UpdateToPython2Engine(); };
                pythonEngine3Item.Click += delegate { UpdateToPython3Engine(); };
                pythonEngineVersionMenu.Items.Add(pythonEngine2Item);
                pythonEngineVersionMenu.Items.Add(pythonEngine3Item);

                // Check the correct item based on NodeModel engine version
                if (pythonNodeModel.Engine == PythonNodeBase.DefaultPythonEngine)
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

            nodeView.MouseDown += view_MouseDown;
            nodeModel.DeletionStarted += NodeModel_DeletionStarted;
            nodeModel.Disposed += NodeModel_Disposed;
        }

        private void NodeModel_Disposed(Dynamo.Graph.ModelBase obj)
        {
            if (editWindow != null)
            {
                editWindow.Close();
            }
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

        private void view_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                EditScriptContent();
                e.Handled = true;
            }
        }

        public void editWindow_Closed(object sender, EventArgs e)
        {
            editWindow = null;
        }

        private void EditScriptContent()
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
        private void UpdateToPython2Engine()
        {
            // If PythonNodeBase.DefaultPythonEngine is updated to python 3, we should update this piece of code
            pythonNodeModel.Engine = PythonNodeBase.DefaultPythonEngine;
            pythonEngine2Item.IsChecked = true;
            pythonEngine3Item.IsChecked = false;
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython3Engine()
        {
            pythonNodeModel.Engine = PythonNodeBase.PythonNet3Engine;
            pythonEngine2Item.IsChecked = false;
            pythonEngine3Item.IsChecked = true;
        }
    }
}
