using System;
using System.Linq;
using System.Windows.Controls;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.WorkspaceDependency.Properties;
using Dynamo.Wpf.Extensions;
using Dynamo.PythonMigration;

namespace Dynamo.WorkspaceDependency
{
    /// <summary>
    /// This sample view extension demonstrates a sample IViewExtension 
    /// which tracks graph dependencies (currently only packages) on the Dynamo right panel.
    /// It reacts to workspace modified/ cleared events to refresh.
    /// </summary>
    public class WorkspaceDependencyViewExtension : IViewExtension, ILogSource
    {
        internal MenuItem workspaceReferencesMenuItem;
        private const string extensionName = "Workspace References";
        internal readonly string pythonPackage = "DSIronPython_Test";
        internal readonly Version pythonPackageVersion = new Version(1, 0, 7);

        private ICustomNodeManager customNodeManager;

        internal WorkspaceDependencyView DependencyView
        {
            get;
            set;
        }

        internal PackageManagerExtension pmExtension;

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
                return "A6706BF5-11C2-458F-B7C8-B745A77EF7FD";
            }
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {
            DependencyView.OnExtensionTabClosed -= OnCloseExtension;
        }


        [Obsolete("This method is not implemented and will be removed.")]
        public void Ready(ReadyParams readyParams)
        {
        }

        public void Shutdown()
        {
            DependencyView.Dispose();
            this.Dispose();
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            pmExtension = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
        }

        public event Action<ILogMessage> MessageLogged;

        internal void OnMessageLogged(ILogMessage msg)
        {
            this.MessageLogged?.Invoke(msg);
        }

        internal void OnCloseExtension(String extensionTabName)
        {
            if (extensionTabName.Equals(extensionName))
            {
                this.workspaceReferencesMenuItem.IsChecked = false;
            }  
        }

        internal INodeLibraryDependencyInfo AddPythonPackageDependency(WorkspaceModel workspace)
        {
            if (!GraphPythonDependencies.ContainsIronPythonDependencyInCurrentWS(workspace, customNodeManager))
                return null;

            var packageInfo = new PackageInfo(pythonPackage, pythonPackageVersion);
            var packageDependencyInfo = new PackageDependencyInfo(packageInfo);

            //var installedPackages = pmExtension.PackageLoader.LocalPackages;
            //var isPackageLoaded = installedPackages.Any(x =>
            //    x.Name == pythonPackage && x.VersionName == pythonPackageVersion.ToString());
            packageDependencyInfo.State = GraphPythonDependencies.IsIronPythonPackageLoaded ? 
                PackageDependencyState.Loaded : PackageDependencyState.Missing;

            return packageDependencyInfo;
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            customNodeManager = viewLoadedParams.StartupParams.CustomNodeManager;
            
            DependencyView = new WorkspaceDependencyView(this, viewLoadedParams);
            // when a package is loaded update the DependencyView 
            // as we may have installed a missing package.

            pmExtension.PackageLoader.PackgeLoaded += (package) =>
            {
                DependencyView.DependencyRegen(viewLoadedParams.CurrentWorkspaceModel as WorkspaceModel);
            };

            DependencyView.OnExtensionTabClosed += OnCloseExtension;

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

    }
}
