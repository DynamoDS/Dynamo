using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Extensions;
using Dynamo.Graph;
using Dynamo.GraphMetadata.Properties;
using Dynamo.Wpf.Extensions;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.Graph.Workspaces;

namespace Dynamo.GraphMetadata
{
    public class GraphMetadataViewExtension : ViewExtensionBase, IExtensionStorageAccess
    {
        internal static string extensionName = "Properties";
        internal GraphMetadataViewModel viewModel;
        private GraphMetadataView graphMetadataView;
        private ViewLoadedParams viewLoadedParamsReference;
        private MenuItem graphMetadataMenuItem;

        /// <summary>
        /// A reference the PreferenceSettings as set by the Dynamo > Preferences window
        /// </summary>
        public PreferenceSettings PreferenceSettings { get; set; }

        public override string UniqueId => "28992e1d-abb9-417f-8b1b-05e053bee670";

        public override string Name => extensionName;

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            if (viewLoadedParams == null) throw new ArgumentNullException(nameof(viewLoadedParams));
            
            this.viewLoadedParamsReference = viewLoadedParams;
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
            // Don't allow the extension to show in anything that isnt a HomeWorkspaceModel
            if (!(this.viewLoadedParamsReference.CurrentWorkspaceModel is HomeWorkspaceModel))
            {
                this.Closed();
                return;
            }

            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.graphMetadataView);
        }

        #region Storage Access implementation

        /// <summary>
        /// Adds required property values and instantiates custom properties based on data saved in the .dyn (JSON) file.
        /// </summary>
        /// <param name="extensionData"></param>
        public void OnWorkspaceOpen(Dictionary<string, string> extensionData)
        {
            // There are multiple places where RequiredProperties' values may be set
            // If the value is defined globally, this is saved in the DynamoSettings.xml file and is loaded
            // in the constructor of the GraphMetadataViewModel
            // However, RequiredProperty values may also be graph-specific, in which case they live in the 
            // JSON data of the .dyn file format. In this case, they are loaded in here.
            
            List<string> xmlRequiredPropertyKeys = this.PreferenceSettings.RequiredProperties
                .Select(x => x.Key)
                .ToList();
            
            Dictionary<string,string> dynRequiredProperties = extensionData
                .Where(x => xmlRequiredPropertyKeys.Contains(x.Key) && !string.IsNullOrWhiteSpace(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);

            Dictionary<string, string> dynCustomProperties = extensionData
                .Where(x => !xmlRequiredPropertyKeys.Contains(x.Key) && !string.IsNullOrWhiteSpace(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);

            // The keys of RequiredProperties whose values are set globally and therefore have already been fully resolved.
            List<string> resolvedKeys = viewModel.RequiredProperties
                .Where(x => x.ValueIsGlobal)
                .Select(x => x.Key)
                .ToList();

            // Instantiating any RequiredProperties whose values are not set globally from the extensionData.
            // RequiredProperties whose values are set globally are instantiated in the GraphMetadataViewModel.
            foreach (RequiredProperty requiredProperty in this.PreferenceSettings.RequiredProperties)
            {
                // If this property has already been resolved we may skip over any information stored locally in the .dyn file.
                if (resolvedKeys.Contains(requiredProperty.Key)) continue;
                
                if (dynRequiredProperties.ContainsKey(requiredProperty.Key))
                {
                    RequiredProperty requiredPropertyToUpdate = viewModel.RequiredProperties
                            .FirstOrDefault(x => x.Key == requiredProperty.Key);

                    // The RequiredProperty's GraphValue is set to its locally stored value.
                    requiredPropertyToUpdate.GraphValue = extensionData[requiredProperty.Key];
                }
                else
                {
                    viewModel.AddRequiredProperty
                    (
                        requiredProperty.UniqueId,
                        requiredProperty.Key,
                        extensionData[requiredProperty.Key],
                        requiredProperty.ValueIsGlobal,
                        false
                    );
                }
            }

            // The instantiation of CustomProperties comes last. If there are values in the .dyn/JSON data which aren't 
            // either globally-set or locally-set RequiredProperties, they'll be loaded in as CustomProperties.
            foreach (KeyValuePair<string, string> keyValuePair in dynCustomProperties)
            {
                this.viewModel.AddCustomProperty(keyValuePair.Key, keyValuePair.Value, false);
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

            foreach (RequiredProperty requiredProperty in viewModel.RequiredProperties)
            {
                extensionData[requiredProperty.Key] = requiredProperty.GraphValue;
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
