using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using PythonNodeModels;
using PythonNodeModelsWpf.Controls;

namespace PythonNodeModelsWpf
{
    class PythonStringNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<PythonStringNode>
    {
        private DynamoViewModel dynamoViewModel;
        private PythonStringNode pythonStringNodeModel;
        private NodeView pythonStringNodeView;
        private MenuItem pythonEngine2Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo, IsCheckable = false };
        private MenuItem pythonEngine3Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree, IsCheckable = false };
        private readonly MenuItem learnMoreItem = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuLearnMoreButton };

        public void CustomizeView(PythonStringNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            pythonStringNodeModel = nodeModel;
            pythonStringNodeView = nodeView;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            // If it is a Debug build, display a python engine switcher
            if (Dynamo.Configuration.DebugModes.IsEnabled("PythonEngineSelectionUIDebugMode"))
            {
                var pythonEngineVersionMenu = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher, IsCheckable = false };
                nodeView.MainContextMenu.Items.Add(pythonEngineVersionMenu);
                pythonEngine2Item.Click += UpdateToPython2Engine;
                pythonEngine2Item.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(pythonStringNodeModel.Engine))
                {
                    Source = pythonStringNodeModel,
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = PythonEngineVersion.IronPython2.ToString()
                });
                pythonEngine3Item.Click += UpdateToPython3Engine;
                pythonEngine3Item.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(pythonStringNodeModel.Engine))
                {
                    Source = pythonStringNodeModel,
                    Converter = new EnumToBooleanConverter(),
                    ConverterParameter = PythonEngineVersion.CPython3.ToString()
                });
                learnMoreItem.Click += OpenPythonLearningMaterial;
                pythonEngineVersionMenu.Items.Add(pythonEngine2Item);
                pythonEngineVersionMenu.Items.Add(pythonEngine3Item);
                nodeView.MainContextMenu.Items.Add(learnMoreItem);
            }

            nodeModel.Disposed += NodeModel_Disposed;

            nodeView.PresentationGrid.Visibility = Visibility.Visible;
            nodeView.PresentationGrid.Children.Add(new EngineLabel(pythonStringNodeModel));
        }

        private void NodeModel_Disposed(Dynamo.Graph.ModelBase obj)
        {
            pythonEngine2Item.Click -= UpdateToPython2Engine;
            pythonEngine3Item.Click -= UpdateToPython3Engine;
            learnMoreItem.Click -= OpenPythonLearningMaterial;
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython2Engine(object sender, EventArgs e)
        {
            pythonStringNodeModel.Engine = PythonEngineVersion.IronPython2;
            pythonStringNodeModel.OnNodeModified();
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

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython3Engine(object sender, EventArgs e)
        {
            pythonStringNodeModel.Engine = PythonEngineVersion.CPython3;
            pythonStringNodeModel.OnNodeModified();
        }
    }
}
