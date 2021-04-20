using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.Graph;
using Dynamo.Linting;
using Dynamo.LintingViewExtension.Properties;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.LintingViewExtension
{
    public class LintingViewExtension : ViewExtensionBase, IExtensionStorageAccess
    {
        private const string EXTENSION_NAME = "Dynamo Linter";
        private const string EXTENSION_GUID = "3467481b-d20d-4918-a454-bf19fc5c25d7";
        private const string GRAPH_USAGES_NAME_PROP = "GraphUsages";
        private const string GRAPH_USAGES_ID_PROP = "GraphUsagesId";
        private const string NODE_ISSUES_COUNT_PROP = "Unresolved Node Issues";
        private const string GRAPH_ISSUES_COUNT_PROP = "Unresolved Graph Issues";

        private LinterManager linterManager;
        private ViewLoadedParams viewLoadedParamsReference;
        private MenuItem linterMenuItem;
        private LinterViewModel linterViewModel;
        private LinterView linterView;


        public override string UniqueId { get { return EXTENSION_GUID; } }

        public override string Name { get { return EXTENSION_NAME; } }

        public override void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            this.linterManager = (viewLoadedParams.DynamoWindow.DataContext as DynamoViewModel).Model.LinterManager;
            this.viewLoadedParamsReference = viewLoadedParams;
            this.linterViewModel = new LinterViewModel(linterManager, viewLoadedParamsReference);
            this.linterView = new LinterView() { DataContext = linterViewModel };

            // Add a button to Dynamo View menu to manually show the window
            this.linterMenuItem = new MenuItem { Header = Resources.MenuItemText, IsCheckable = true };
            this.linterMenuItem.Checked += MenuItemCheckHandler;
            this.linterMenuItem.Unchecked += MenuItemUnCheckedHandler;
            this.viewLoadedParamsReference.AddExtensionMenuItem(this.linterMenuItem);
        }

        private void MenuItemUnCheckedHandler(object sender, RoutedEventArgs e)
        {
            viewLoadedParamsReference.CloseExtensioninInSideBar(this);
        }

        private void MenuItemCheckHandler(object sender, RoutedEventArgs e)
        {
            this.viewLoadedParamsReference?.AddToExtensionsSideBar(this, this.linterView);
        }

        public override void Shutdown()
        {
            // Do nothing for now 
        }
        public override void Dispose()
        {
            this.linterMenuItem.Checked -= MenuItemCheckHandler;
            this.linterMenuItem.Unchecked -= MenuItemUnCheckedHandler;
        }

        public override void Closed()
        {
            if (this.linterMenuItem is null)
                return;
            
            this.linterMenuItem.IsChecked = false;
        }

        public void OnWorkspaceOpen(Dictionary<string, string> extensionData)
        {
            if (!extensionData.TryGetValue("GraphUsagesId", out string activeLinterId))
                return;

            var activeLitner = this.linterManager.AvailableLinters.
                Where(x => x.Id == activeLinterId).
                FirstOrDefault();

            if (activeLitner is null)
                return;

            this.linterViewModel.ActiveLinter = activeLitner;

            if (!this.linterMenuItem.IsChecked)
            {
                this.linterMenuItem.IsChecked = true;
            }
        }

        public void OnWorkspaceSaving(Dictionary<string, string> extensionData, SaveContext saveContext)
        {
            extensionData[GRAPH_USAGES_NAME_PROP] = this.linterViewModel.ActiveLinter.Name;
            extensionData[GRAPH_USAGES_ID_PROP] = this.linterViewModel.ActiveLinter.Id;
            extensionData[NODE_ISSUES_COUNT_PROP] = this.linterViewModel.NodeIssues.Count.ToString();
            extensionData[GRAPH_ISSUES_COUNT_PROP] = this.linterViewModel.GraphIssues.Count.ToString();
        }
    }
}
