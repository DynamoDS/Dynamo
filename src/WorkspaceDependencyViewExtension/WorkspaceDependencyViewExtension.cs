using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.WorkspaceDependency.Properties;
using Dynamo.Wpf.Extensions;
using DynamoUtilities;

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
        private readonly String extensionName = Resources.ExtensionName;
        private readonly string customNodeExtension = ".dyf";

        internal WorkspaceDependencyView DependencyView
        {
            get;
            set;
        }

        /// <summary>
        /// Internal cache of the data displayed in data grid, useful in unit testing.
        /// You are not expected to modify this but rather inspection.
        /// </summary>
        internal IEnumerable<PackageDependencyRow> dataRows;
        internal IEnumerable<DependencyRow> localDefinitionDataRows;
        internal IEnumerable<DependencyRow> externalFilesDataRows;

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
            DependencyView?.Dispose();
            dataRows = null;
            localDefinitionDataRows = null;
            externalFilesDataRows = null;
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

        private Boolean hasDependencyIssue = false;
        /// <summary>
        /// Property to raise to author's attention, if the Dynamo active workspace has any missing dependencies.
        /// This determines if the package dep viewer will be injected into right panel.
        /// </summary>
        private Boolean HasDependencyIssue
        {
            get { return hasDependencyIssue; }
            set
            {
                hasDependencyIssue = value;
                if (hasDependencyIssue)
                {
                    DependencyView.loadedParams.AddToExtensionsSideBar(this, DependencyView);
                    if (workspaceReferencesMenuItem != null && !workspaceReferencesMenuItem.IsChecked)
                    {
                        workspaceReferencesMenuItem.IsChecked = true;
                    }
                }
            }
        }

        public override void Loaded(ViewLoadedParams viewLoadedParams)
        {
            DependencyView = new WorkspaceDependencyView(this, viewLoadedParams);
            DependencyRegen(DependencyView.currentWorkspace);
            // when a package is loaded update the DependencyView 
            // as we may have installed a missing package.

            DependencyView.CustomNodeManager = (CustomNodeManager)viewLoadedParams.StartupParams.CustomNodeManager;
            if (pmExtension != null)
            {
                pmExtension.PackageLoader.PackgeLoaded += (package) =>
                {
                    DependencyRegen(viewLoadedParams.CurrentWorkspaceModel as WorkspaceModel, true);
                };
            }

            // Adding a button in view menu to refresh and show manually
            workspaceReferencesMenuItem = new MenuItem { Header = Resources.MenuItemString, IsCheckable = true, IsChecked = false };
            workspaceReferencesMenuItem.Click += (sender, args) =>
            {
                if (workspaceReferencesMenuItem.IsChecked)
                {
                    // Refresh dependency data
                    DependencyRegen(viewLoadedParams.CurrentWorkspaceModel as WorkspaceModel, true);
                    viewLoadedParams.AddToExtensionsSideBar(this, DependencyView);
                    workspaceReferencesMenuItem.IsChecked = true;
                }
                else
                {
                    viewLoadedParams.CloseExtensioninInSideBar(this);
                    workspaceReferencesMenuItem.IsChecked = false;
                }
            };
            viewLoadedParams.AddExtensionMenuItem(workspaceReferencesMenuItem);
        }

        /// <summary>
        /// Regenerate dependency table
        /// </summary>
        /// <param name="ws">workspace model</param>
        /// <param name="forceCompute">flag indicating if the workspace references should be computed</param>
        internal void DependencyRegen(WorkspaceModel ws, bool forceCompute = false)
        {
            DependencyView.RestartBanner.Visibility = Visibility.Hidden;
            ws.ForceComputeWorkspaceReferences = forceCompute;
            // Dependency infos for each category
            var packageDependencies = ws.NodeLibraryDependencies?.Where(d => d is PackageDependencyInfo).ToList();
            var localDefinitions = ws.NodeLocalDefinitions?.Where(d => d is DependencyInfo).ToList();
            var externalFiles = ws.ExternalFiles?.Where(d => d is DependencyInfo).ToList();

            foreach (DependencyInfo info in localDefinitions)
            {
                try
                {
                    if (info.ReferenceType == ReferenceType.DYFFile)
                    {
                        // Try to get the Custom node information if possible.
                        string customNodeName = info.Name.Replace(customNodeExtension, "");
                        DependencyView.CustomNodeManager.TryGetNodeInfo(customNodeName, out CustomNodeInfo customNodeInfo);

                        if (customNodeInfo != null)
                        {
                            info.Path = customNodeInfo.Path;
                        }
                    }
                    info.Size = PathHelper.GetFileSize(info.Path);
                }
                catch (Exception ex)
                {
                    OnMessageLogged(LogMessage.Info(string.Format(Resources.DependencyViewExtensionErrorTemplate, ex.ToString())));
                }
                HasDependencyIssue = string.IsNullOrEmpty(info.Path);
            }

            foreach (DependencyInfo info in externalFiles)
            {
                HasDependencyIssue = string.IsNullOrEmpty(info.Path);
            }

            if (packageDependencies.Any(d => d.State != PackageDependencyState.Loaded))
            {
                HasDependencyIssue = true;
            }

            if (packageDependencies.Count != 0)
            {
                Boolean hasPackageMarkedForUninstall = false;
                // If package is set to uninstall state, update the package info
                foreach (var package in pmExtension.PackageLoader.LocalPackages.Where(x =>
                x.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForDeletion || x.LoadState.ScheduledState == PackageLoadState.ScheduledTypes.ScheduledForUnload))
                {
                    try
                    {
                        (packageDependencies.FirstOrDefault(x => x.Name == package.Name) as PackageDependencyInfo).State =
                        PackageDependencyState.RequiresRestart;
                        hasPackageMarkedForUninstall = true;
                    }
                    catch (Exception ex)
                    {
                        OnMessageLogged(LogMessage.Info(string.Format(Resources.DependencyViewExtensionErrorTemplate, $"failure to set package uninstall state |{ex.ToString()}")));
                    }
                }

                DependencyView.RestartBanner.Visibility = hasPackageMarkedForUninstall ? Visibility.Visible : Visibility.Hidden;
            }

            if (pmExtension != null)
            {
                foreach (PackageDependencyInfo packageDependencyInfo in packageDependencies)
                {
                    try
                    {
                        var targetInfo = pmExtension.PackageLoader.LocalPackages.Where(x => x.Name == packageDependencyInfo.Name).FirstOrDefault();
                        if (targetInfo != null)
                        {
                            packageDependencyInfo.Path = targetInfo.RootDirectory;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnMessageLogged(LogMessage.Info(string.Format(Resources.DependencyViewExtensionErrorTemplate, ex.ToString())));
                    }
                }
            }
            try
            {
                dataRows = packageDependencies?.Select(d => new PackageDependencyRow(d as PackageDependencyInfo));
                localDefinitionDataRows = localDefinitions?.Select(d => new DependencyRow(d as DependencyInfo));
                externalFilesDataRows = externalFiles?.Select(d => new DependencyRow(d as DependencyInfo));

                DependencyView.Packages.IsExpanded = dataRows.Any();
                DependencyView.LocalDefinitions.IsExpanded = localDefinitionDataRows.Any();
                DependencyView.ExternalFiles.IsExpanded = externalFilesDataRows.Any();

                ws.ForceComputeWorkspaceReferences = false;

                DependencyView.PackageDependencyTable.ItemsSource = dataRows;
                DependencyView.LocalDefinitionsTable.ItemsSource = localDefinitionDataRows;
                DependencyView.ExternalFilesTable.ItemsSource = externalFilesDataRows;
            }
            catch(Exception ex)
            {
                OnMessageLogged(LogMessage.Info(string.Format(Resources.DependencyViewExtensionErrorTemplate, ex.ToString())));
            }
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
