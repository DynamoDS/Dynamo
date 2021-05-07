using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Extensions;
using Dynamo.Graph;
using Dynamo.GraphMetadata.Properties;
using Dynamo.Wpf.Extensions;
using System.Windows;

namespace Dynamo.GraphMetadata
{
    public class GraphMetadataViewExtension : IViewExtension, IExtensionStorageAccess
    {
        public GraphMetadataViewModel viewModel;
        private GraphMetadataView graphMetadataView;
        private ViewLoadedParams viewLoadedParamsReference;
        private MenuItem graphMetadataMenuItem;

        public string UniqueId => "28992e1d-abb9-417f-8b1b-05e053bee670";

        public string Name => "Properties";


        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (viewLoadedParams == null) throw new ArgumentNullException(nameof(viewLoadedParams));

            this.viewLoadedParamsReference = viewLoadedParams;
            this.viewModel = new GraphMetadataViewModel(viewLoadedParams);
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
            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.graphMetadataView);
        }

        #region Storage Access implementation
        public void OnWorkspaceOpen(Dictionary<string, string> extensionData)
        {
            foreach (var p in this.viewModel.CustomProperties)
            {
                extensionData[p.PropertyName] = p.PropertyValue;
            }

            ////throw new NotImplementedException();
        }

        public void OnWorkspaceSaving(Dictionary<string, string> extensionData, SaveContext saveContext)
        {
            foreach (var p in this.viewModel.CustomProperties)
            {
                extensionData[p.PropertyName] = p.PropertyValue;
            }
        }
        #endregion

        public void Shutdown() { }

        public void Startup(ViewStartupParams viewStartupParams) { }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
