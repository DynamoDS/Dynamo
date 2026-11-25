using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CoreNodeModels;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace CoreNodeModelsWpf.Nodes
{
    /// <summary>
    /// Customizes the WPF view for the <see cref="Receive"/> node with a dropdown of workspace channel names.
    /// </summary>
    public class ReceiveNodeViewCustomization : INodeViewCustomization<Receive>
    {
        private Receive model;
        private ComboBox comboBox;
        private DynamoViewModel dynamoViewModel;
        private Guid workspaceGuid;
        private bool isSyncingSelection;

        /// <inheritdoc />
        public void CustomizeView(Receive model, NodeView nodeView)
        {
            this.model = model;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            workspaceGuid = nodeView.ViewModel.WorkspaceViewModel.Model.Guid;

            comboBox = new ComboBox
            {
                Width = double.NaN,
                MinWidth = 150,
                Height = Configurations.PortHeightInPixels,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)SharedDictionaryManager.DynamoModernDictionary["RefreshComboBox"]
            };

            nodeView.inputGrid.Children.Add(comboBox);
            Grid.SetColumn(comboBox, 0);
            Grid.SetRow(comboBox, 0);

            comboBox.DataContext = model;
            comboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Receive.DisplayChannels))
            {
                Mode = BindingMode.OneWay,
                Source = model
            });

            SyncComboSelectionFromModel();

            comboBox.SelectionChanged += OnComboSelectionChanged;
            model.PropertyChanged += OnModelPropertyChanged;
            model.DisplayChannels.CollectionChanged += OnDisplayChannelsChanged;
        }

        private void OnComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isSyncingSelection || comboBox.SelectedItem == null)
            {
                return;
            }

            var selected = comboBox.SelectedItem as string ?? string.Empty;
            if (string.Equals(selected, model.ChannelName?.Trim(), StringComparison.Ordinal))
            {
                return;
            }

            dynamoViewModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    workspaceGuid,
                    model.GUID,
                    nameof(Receive.ChannelName),
                    selected));
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Receive.ChannelName))
            {
                SyncComboSelectionFromModel();
            }
        }

        private void OnDisplayChannelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncComboSelectionFromModel();
        }

        private void SyncComboSelectionFromModel()
        {
            if (comboBox == null || model == null)
            {
                return;
            }

            isSyncingSelection = true;
            try
            {
                var trimmed = model.ChannelName?.Trim();
                comboBox.SelectedItem = string.IsNullOrEmpty(trimmed) ? null : trimmed;
            }
            finally
            {
                isSyncingSelection = false;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (comboBox != null)
            {
                comboBox.SelectionChanged -= OnComboSelectionChanged;
            }

            if (model != null)
            {
                model.PropertyChanged -= OnModelPropertyChanged;
                model.DisplayChannels.CollectionChanged -= OnDisplayChannelsChanged;
            }
        }
    }
}
