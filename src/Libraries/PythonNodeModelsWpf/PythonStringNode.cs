﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.PythonServices;
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
        private MenuItem pythonEngineVersionMenu;
        private readonly MenuItem learnMoreItem = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuLearnMoreButton };

        public void CustomizeView(PythonStringNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            pythonStringNodeModel = nodeModel;
            pythonStringNodeView = nodeView;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            pythonEngineVersionMenu = new MenuItem { Header = PythonNodeModels.Properties.Resources.PythonNodeContextMenuEngineSwitcher, IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(pythonEngineVersionMenu);

            var availableEngineNames = PythonEngineManager.Instance.AvailableEngines.Select(x => x.Name).ToList();
            // Add the serialized Python Engine even if it is missing (so that the user does not see an empty slot)
            if (!availableEngineNames.Contains(nodeModel.EngineName))
            {
                availableEngineNames.Add(nodeModel.EngineName);
            }
            availableEngineNames.ForEach(x => AddPythonEngineToMenuItems(x));

            PythonEngineManager.Instance.AvailableEngines.CollectionChanged += PythonEnginesChanged;

            learnMoreItem.Click += OpenPythonLearningMaterial;

            nodeView.MainContextMenu.Items.Add(learnMoreItem);

            nodeModel.Disposed += NodeModel_Disposed;

            nodeView.PresentationGrid.Visibility = Visibility.Visible;
            nodeView.PresentationGrid.Children.Add(new EngineLabel(pythonStringNodeModel));
        }

        private void NodeModel_Disposed(Dynamo.Graph.ModelBase obj)
        {
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
        private void UpdateEngine(object sender, EventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                dynamoViewModel.ExecuteCommand(
                   new DynamoModel.UpdateModelValueCommand(
                       Guid.Empty, pythonStringNodeModel.GUID, nameof(pythonStringNodeModel.EngineName), (string)menuItem.Header));
                pythonStringNodeModel.OnNodeModified();
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
            pythonEngineItem.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(pythonStringNodeModel.EngineName))
            {
                Source = pythonStringNodeModel,
                Converter = new CompareToParameterConverter(),
                ConverterParameter = engineName
            });
            pythonEngineVersionMenu.Items.Add(pythonEngineItem);
        }
    }
}
