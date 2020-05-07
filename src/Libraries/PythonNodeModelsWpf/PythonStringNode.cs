using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using PythonNodeModels;
using PythonNodeModelsWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

            var pythonEngineVersionMenu = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher, IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(pythonEngineVersionMenu);
            pythonEngine2Item.Click += delegate { UpdateToPython2Engine(); };
            pythonEngine3Item.Click += delegate { UpdateToPython3Engine(); };
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

            nodeView.PresentationGrid.Visibility = Visibility.Visible;
            nodeView.PresentationGrid.DataContext = this.pythonStringNodeModel;
            nodeView.PresentationGrid.Children.Add(new EngineLabel());
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython2Engine()
        {
            pythonStringNodeModel.Engine = PythonEngineVersion.IronPython2;
            pythonEngine2Item.IsChecked = true;
            pythonEngine3Item.IsChecked = false;
        }

        /// <summary>
        /// MenuItem click handler
        /// </summary>
        private void UpdateToPython3Engine()
        {
            pythonStringNodeModel.Engine = PythonEngineVersion.CPython3;
            pythonEngine2Item.IsChecked = false;
            pythonEngine3Item.IsChecked = true;
        }
    }
}
