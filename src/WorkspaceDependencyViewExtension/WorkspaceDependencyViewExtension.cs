using System;
using System.Linq;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.WorkspaceDependency.Properties;
using Dynamo.Wpf.Extensions;

namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// This sample view extension demonstrates a sample IViewExtension 
    /// which tracks graph dependencies (currently only packages) on the Dynamo right panel.
    /// It reacts to workspace modified/ cleared events to refresh.
    /// </summary>
    public class WorkspaceDependencyViewExtension : ViewExtensionBase, IViewExtension, ILogSource
    {
        internal MenuItem workspaceReferencesMenuItem;
        private readonly String extensionName = Properties.Resources.ExtensionName;

        internal WorkspaceDependencyView DependencyView
        {
            get;
            set;
        }

        internal PackageManagerExtension pmExtension;

        /// <summary>
        /// Extension Name
        /// </summary>
        public override string Name
        {
            get
            {
                return extensionName;
            }
        }

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public override string UniqueId
        {
            get
            {
                return "A6706BF5-11C2-458F-B7C8-B745A77EF7FD";
            }
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public override void Dispose()
        {
            DependencyView.Dispose();
        }


        [Obsolete("This method is not implemented and will be removed.")]
        public void Ready(ReadyParams readyParams)
        {
        }

        public override void Startup(ViewStartupParams viewStartupParams)
        {
            pmExtension = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
        }

        public event Action<ILogMessage> MessageLogged;

        internal void OnMessageLogged(ILogMessage msg)
        {
            this.MessageLogged?.Invoke(msg);
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            DependencyView = new WorkspaceDependencyView(this, viewLoadedParams);
            // when a package is loaded update the DependencyView 
            // as we may have installed a missing package.

            pmExtension.PackageLoader.PackgeLoaded += (package) =>
            {
                DependencyView.DependencyRegen(viewLoadedParams.CurrentWorkspaceModel as WorkspaceModel);
            };

            // Adding a button in view menu to refresh and show manually
            workspaceReferencesMenuItem = new MenuItem { Header = Resources.MenuItemString, IsCheckable = true, IsChecked = false };
            workspaceReferencesMenuItem.Click += (sender, args) =>
            {
                if (workspaceReferencesMenuItem.IsChecked)
                {
                    // Refresh dependency data
                    DependencyView.DependencyRegen(viewLoadedParams.CurrentWorkspaceModel as WorkspaceModel);
                    viewLoadedParams.AddToExtensionsSideBar(this, DependencyView);
                    workspaceReferencesMenuItem.IsChecked = true;
                }
                else
                {
                    viewLoadedParams.CloseExtensioninInSideBar(this);
                    workspaceReferencesMenuItem.IsChecked = false;
                }
            };
            viewLoadedParams.AddMenuItem(MenuBarType.View, workspaceReferencesMenuItem);
        }

        public override void Closed()
        {
            if (this.workspaceReferencesMenuItem != null) 
            { 
                this.workspaceReferencesMenuItem.IsChecked = false;
            }
        }
    }
}
