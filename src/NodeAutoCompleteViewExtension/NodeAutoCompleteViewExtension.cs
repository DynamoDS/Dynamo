using System;
using Dynamo.Core;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.Logging;
using Dynamo.Wpf.Extensions;
using System.Collections.Generic;
using Dynamo.ViewModels;
using Dynamo.Search.SearchElements;
using System.Data;
using System.Linq;

namespace Dynamo.NodeAutoComplete
{
    /// <summary>
    /// This view extension tracks current clicked node port in Dynamo and 
    /// tries to suggest the next best node to connect that port.
    /// </summary>
    public class NodeAutoCompleteViewExtension : IViewExtension, ILogSource
    {
        private const String extensionName = "Node Auto Complete";

        internal MenuItem nodeAutocompleteMenuItem;

        /// <summary>
        /// Internal cache of the data displayed in data grid, useful in unit testing.
        /// You are not expected to modify this but rather inspection.
        /// </summary>
        internal IEnumerable<NodeAutocompleteCluster> NodeAutocompleteClusters;

        internal NodeAutoCompleteViewModel nodeAutoCompleteViewModel { get; set; }

        internal NodeAutoCompleteView DependencyView
        {
            get;
            set;
        }

        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name
        {
            get
            {
                return extensionName;
            }
        }

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public string UniqueId
        {
            get
            {
                return "64F28473-0DCB-4E41-BB5B-A409FF6C90AD";
            }
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {
            // Do nothing for now
        }

        public void Ready(ReadyParams readyParams)
        {
            // Do nothing for now
        }

        public void Shutdown()
        {
            // Do nothing for now
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now

        }

        public event Action<ILogMessage> MessageLogged;

        internal void OnMessageLogged(ILogMessage msg)
        {
            this.MessageLogged?.Invoke(msg);
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            var dynamoViewModel = viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel;

            DependencyView = new NodeAutoCompleteView(this, viewLoadedParams);
            nodeAutoCompleteViewModel = new NodeAutoCompleteViewModel(viewLoadedParams.DynamoWindow, dynamoViewModel);

            // Adding a button in view menu to refresh and show manually
            nodeAutocompleteMenuItem = new MenuItem { Header = "Show NodeAutocomplete view extension", IsCheckable = true, IsChecked = false };
            nodeAutocompleteMenuItem.Click += (sender, args) =>
            {
                if (nodeAutocompleteMenuItem.IsChecked)
                {
                    viewLoadedParams.AddToExtensionsSideBar(this, DependencyView);
                    nodeAutocompleteMenuItem.IsChecked = true;
                }
                else
                {
                    viewLoadedParams.CloseExtensioninInSideBar(this);
                    nodeAutocompleteMenuItem.IsChecked = false;
                }
            };
            viewLoadedParams.AddExtensionMenuItem(nodeAutocompleteMenuItem);
        }

        internal void ShowClusterNodeAutocompleteResults(MLNodeClusterAutoCompletionResponse results)
        {
            var nodeAutocompleteClusters = results.Results.ToList().Select(r => new NodeAutocompleteCluster(r));

            DependencyView.MainItems.ItemsSource = nodeAutocompleteClusters;
        }
    }
}
