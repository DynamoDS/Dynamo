using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.PluginManager.ViewModel;
using System.ComponentModel;
using PluginManager;
using System.Windows.Controls;
using Dynamo.PluginManager.Model;

namespace Dynamo.PluginManager.View
{
    public partial class PluginManagerView : Window
    {
        private PluginManagerViewModel ViewModel
        {
            get { return this.DataContext as PluginManagerViewModel; }
        }
        private PluginManagerExtension pluginManagerContext;

        public PluginManagerView(PluginManagerViewModel viewModel,PluginManagerExtension pluginManagerContext)
        {
            InitializeComponent();
            this.pluginManagerContext = pluginManagerContext;
            this.DataContext = viewModel;
            viewModel.PropertyChanged += OnPropertyChanged;
            UpdateVisualToReflectSelectionState();
        }
        private void UpdateVisualToReflectSelectionState()
        {
            var newIndex = ViewModel.SelectedIndex;
            if (PathListBox.SelectedIndex != newIndex)
                PathListBox.SelectedIndex = newIndex;
        }
        private void OnEditScriptClicked(object sender, RoutedEventArgs e)
        {
            PluginScriptEditor pluginScriptEditor = new PluginScriptEditor(ViewModel.PluginModelList.ElementAt(ViewModel.SelectedIndex).FilePath,pluginManagerContext);
            pluginScriptEditor.Initialize();
            this.WindowState = System.Windows.WindowState.Minimized;
            pluginScriptEditor.Show();
        }
        void OnPluginSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
                return;
            var selected = e.AddedItems[0] as PluginModel;
            ViewModel.SelectedIndex = ViewModel.PluginModelList.IndexOf(selected);
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("SelectedIndex"))
            {
                // Repositioning the selection should retain its visual state.
                UpdateVisualToReflectSelectionState();
            }
        }
    }
}
