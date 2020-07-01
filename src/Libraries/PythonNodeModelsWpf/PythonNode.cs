using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Wpf;
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
        private readonly MenuItem pythonEngine2Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo, IsCheckable = false };
        private readonly MenuItem pythonEngine3Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree, IsCheckable = false };
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
            // If it is a Debug build, display a python engine switcher
            if (Dynamo.Configuration.DebugModes.IsEnabled("PythonEngineSelectionUIDebugMode"))
            {
                var pythonEngineVersionMenu = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher, IsCheckable = false };
                nodeView.MainContextMenu.Items.Add(pythonEngineVersionMenu);
                pythonEngine2Item.Click += UpdateToPython2Engine;
                // Bind menu item check state to the Engine property in the ViewModel.
                // By doing this, we make sure the check status is in sync with the ViewModel,
                // no matter if we update it through the context menu or other means.
                // Setting the IsChecked property, on the other hand, is error prone and redundant
                // once data binding has been set up.
                pythonEngine2Item.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(pythonNodeModel.Engine))
                {
                    Source = pythonNodeModel,
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = PythonEngineVersion.IronPython2.ToString()
                });
                pythonEngine3Item.Click += UpdateToPython3Engine;
                pythonEngine3Item.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(pythonNodeModel.Engine))
                {
                    Source = pythonNodeModel,
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = PythonEngineVersion.CPython3.ToString()
                });
                learnMoreItem.Click += OpenPythonLearningMaterial;
                pythonEngineVersionMenu.Items.Add(pythonEngine2Item);
                pythonEngineVersionMenu.Items.Add(pythonEngine3Item);
                nodeView.MainContextMenu.Items.Add(learnMoreItem);
            }

            nodeView.UpdateLayout();

            nodeView.MouseDown += View_MouseDown;
            nodeModel.DeletionStarted += NodeModel_DeletionStarted;
            nodeModel.Disposed += NodeModel_Disposed;

            nodeView.PresentationGrid.Visibility = Visibility.Visible;
            nodeView.PresentationGrid.Children.Add(new EngineLabel(nodeModel));
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
            pythonEngine2Item.Click -= UpdateToPython2Engine;
            pythonEngine3Item.Click -= UpdateToPython3Engine;
            learnMoreItem.Click -= OpenPythonLearningMaterial;
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
            pythonNodeModel.OnNodeModified();
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython3Engine(object sender, EventArgs e)
        {
            pythonNodeModel.Engine = PythonEngineVersion.CPython3;
            pythonNodeModel.OnNodeModified();
        }
    }
}
