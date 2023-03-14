using System;
using System.Windows.Controls;
using System.Collections.Generic;
using Dynamo.Extensions;
using Dynamo.Graph;
using Dynamo.GraphMetadata.Properties;
using Dynamo.Wpf.Extensions;
using System.Windows;
using Dynamo.Graph.Workspaces;

namespace Dynamo.GraphMetadata
{
    public class GraphMetadataViewExtension : ViewExtensionBase, IExtensionStorageAccess
    {
        internal GraphMetadataViewModel viewModel;
        private GraphMetadataView graphMetadataView;
        private ViewLoadedParams viewLoadedParamsReference;
        private MenuItem graphMetadataMenuItem;

        public override string UniqueId => "28992e1d-abb9-417f-8b1b-05e053bee670";

        public override string Name => Resources.ExtensionName;

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            this.viewLoadedParamsReference = viewLoadedParams ?? throw new ArgumentNullException(nameof(viewLoadedParams));
            this.viewModel = new GraphMetadataViewModel(viewLoadedParams, this);
            this.graphMetadataView = new GraphMetadataView();
            graphMetadataView.DataContext = viewModel;

            // Add a button to Dynamo View menu to manually show the window
            this.graphMetadataMenuItem = new MenuItem { Header = Resources.MenuItemText, IsCheckable = true };
            this.graphMetadataMenuItem.Checked += MenuItemCheckHandler;
            this.graphMetadataMenuItem.Unchecked += MenuItemUnCheckedHandler;
            this.viewLoadedParamsReference.AddExtensionMenuItem(this.graphMetadataMenuItem);
        }

        private void MenuItemUnCheckedHandler(object sender, RoutedEventArgs e)
        {
            viewLoadedParamsReference.CloseExtensioninInSideBar(this);
        }

        private void MenuItemCheckHandler(object sender, RoutedEventArgs e)
        {
            AddToSidebar();
        }

        private void AddToSidebar()
        {
            // Dont allow the extension to show in anything that isnt a HomeWorkspaceModel
            if (!(this.viewLoadedParamsReference.CurrentWorkspaceModel is HomeWorkspaceModel))
            {
                this.Closed();
                return;
            }

            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.graphMetadataView);
        }

        public override void ReOpen()
        {
            AddToSidebar();
            this.graphMetadataMenuItem.IsChecked = true;
        }

        #region Storage Access implementation

        /// <summary>
        /// Adds custom properties serialized in the graph to the viewModels CustomProperty collection
        /// </summary>
        /// <param name="extensionData"></param>
        public void OnWorkspaceOpen(Dictionary<string, string> extensionData)
        {
            foreach (var kv in extensionData)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;

                var valueModified = kv.Value == null ? string.Empty : kv.Value;

                this.viewModel.AddCustomProperty(kv.Key, valueModified, false);
            }
        }

        /// <summary>
        /// Adds all CustomProperties to this extensions extensionData
        /// </summary>
        /// <param name="extensionData"></param>
        /// <param name="saveContext"></param>
        public void OnWorkspaceSaving(Dictionary<string, string> extensionData, SaveContext saveContext)
        {
            // Clearing the extensionData dictionary before adding new values
            // as the GraphMetadataViewModel.CustomProperties is the true source of the custom properties
            extensionData.Clear();
            foreach (var p in this.viewModel.CustomProperties)
            {
                extensionData[p.PropertyName] = p.PropertyValue;
            }
        }


        #endregion

        public override void Closed()
        {
            if (this.graphMetadataMenuItem != null)
            {
                this.graphMetadataMenuItem.IsChecked = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            viewModel.Dispose();

            this.graphMetadataMenuItem.Checked -= MenuItemCheckHandler;
            this.graphMetadataMenuItem.Unchecked -= MenuItemUnCheckedHandler;
        }

        public override void Dispose()
        {
            Dispose(true);
        }
    }
}
