using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.PackageDependency.Properties;
using Dynamo.Wpf.Extensions;
using System.Windows.Controls;

namespace Dynamo.PackageDependency
{
    /// <summary>
    /// This sample view extension demonstrates a sample IViewExtension 
    /// which tracks graph package dependencies on the Dynamo right panel.
    /// It reacts to workspace modified/ cleared events to refresh.
    /// </summary>
    public class PackageDependencyViewExtension : IViewExtension
    {
        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name
        {
            get
            {
                return "Package Dependency ViewExtension";
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

        private PackageDependencyView DependencyView
        {
            get;
            set;
        }

        private ReadyParams ReadyParams;

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Ready is called when the DynamoModel is finished being built, or when the extension is installed
        /// sometime after the DynamoModel is already built. ReadyParams provide access to references like the
        /// CurrentWorkspace.
        /// </summary>
        /// <param name="sp"></param>
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

        }

        private MenuItem packageDependencyMenuItem;

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            DependencyView = new PackageDependencyView(this, viewLoadedParams);

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
