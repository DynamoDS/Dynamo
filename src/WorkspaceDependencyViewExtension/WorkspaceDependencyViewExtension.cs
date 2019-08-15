using System.Collections.Generic;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.WorkspaceDependency.Properties;
using Dynamo.Wpf.Extensions;

namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// This sample view extension demonstrates a sample IViewExtension 
    /// which tracks graph dependencies (currently only packages) on the Dynamo right panel.
    /// It reacts to workspace modified/ cleared events to refresh.
    /// </summary>
    public class WorkspaceDependencyViewExtension : IViewExtension
    {
        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name
        {
            get
            {
                return "Workspace Dependency ViewExtension";
            }
        }

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public string UniqueId
        {
            get
            {
                return "A6706BF5-11C2-458F-B7C8-B745A77EF7FD";
            }
        }

        internal WorkspaceDependencyView DependencyView
        {
            get;
            set;
        }

        internal ViewStartupParams ViewLoadedParams;

        private ReadyParams ReadyParams;

        internal DynamoLogger logger;

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {

        }

       
        public void Ready(ReadyParams readyParams)
        {
            ReadyParams = readyParams;
        }

        public void Shutdown()
        {
            ReadyParams.CurrentWorkspaceChanged -= DependencyView.OnWorkspaceChanged;
            ReadyParams.CurrentWorkspaceCleared -= DependencyView.OnWorkspaceCleared;
            this.Dispose();
        }

        public void Startup(ViewStartupParams viewLoadedParams)
        {
            ViewLoadedParams = viewLoadedParams;
        }

        private MenuItem packageDependencyMenuItem;

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            DependencyView = new WorkspaceDependencyView(this, viewLoadedParams);
            logger = viewModel.Model.Logger;

            // Adding a button in view menu to refresh and show manually
            packageDependencyMenuItem = new MenuItem { Header = Resources.MenuItemString };
            packageDependencyMenuItem.Click += (sender, args) =>
            {
                // Refresh dependency data
                DependencyView.DependencyRegen(viewLoadedParams.CurrentWorkspaceModel as WorkspaceModel);
                viewLoadedParams.AddToExtensionsSideBar(this, DependencyView);
            };
            viewLoadedParams.AddMenuItem(MenuBarType.View, packageDependencyMenuItem);
        }
    }
}
