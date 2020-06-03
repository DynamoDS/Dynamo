using System;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Wpf;
using PythonNodeModels;
using PythonNodeModelsWpf.Controls;

namespace PythonNodeModelsWpf
{
    class PythonStringNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<PythonStringNode>
    {
        private PythonStringNode pythonStringNodeModel;
        private NodeView pythonStringNodeView;
        private MenuItem pythonEngine2Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionTwo, IsCheckable = true };
        private MenuItem pythonEngine3Item = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineVersionThree, IsCheckable = true };

        public void CustomizeView(PythonStringNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            pythonStringNodeModel = nodeModel;
            pythonStringNodeView = nodeView;

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
                if (pythonStringNodeModel.Engine == PythonEngineVersion.IronPython2)
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

            nodeModel.Disposed += NodeModel_Disposed;

            nodeView.PresentationGrid.Visibility = Visibility.Visible;
            nodeView.PresentationGrid.Children.Add(new EngineLabel(pythonStringNodeModel));
        }

        private void NodeModel_Disposed(Dynamo.Graph.ModelBase obj)
        {
            pythonEngine2Item.Click -= UpdateToPython2Engine;
            pythonEngine3Item.Click -= UpdateToPython3Engine;
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython2Engine(object sender, EventArgs e)
        {
            pythonStringNodeModel.Engine = PythonEngineVersion.IronPython2;
            pythonEngine2Item.IsChecked = true;
            pythonEngine3Item.IsChecked = false;
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython3Engine(object sender, EventArgs e)
        {
            pythonStringNodeModel.Engine = PythonEngineVersion.CPython3;
            pythonEngine2Item.IsChecked = false;
            pythonEngine3Item.IsChecked = true;
        }
    }
}
