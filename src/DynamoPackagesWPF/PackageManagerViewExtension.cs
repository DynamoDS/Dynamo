using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Dynamo.Logging;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.Properties;
using Greg.Responses;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// The View layer of the packageManagerExtension.
    /// Manages PackageManager UI including PackageManagerView and PackageManagerSearchView windows.
    /// </summary>
    public class PackageManagerViewExtension : IViewExtension, IViewExtensionSource, ILayoutSpecSource, INotificationSource
    {
        private readonly List<IViewExtension> requestedExtensions = new List<IViewExtension>();
        private PackageManagerExtension packageManager;
        private ViewLoadedParams viewLoadedParams;
        private DynamoViewModel dynamoViewModel;
        
        // Package Manager UI state
        private PackageManagerView packageManagerWindow;
        private PackageManagerSearchView searchPackagesView;
        private PackageManagerSearchViewModel pkgSearchVM;
        private PackageManagerViewModel pkgVM;
        public string Name
        {
            get
            {
                return "PackageManagerViewExtension";
            }
        }
        /// <summary>
        /// ViewExtensions that were requested to be loaded.
        /// </summary>
        public IEnumerable<IViewExtension> RequestedExtensions
        {
            get
            {
                return this.requestedExtensions;
            }
        }

        public string UniqueId
        {
            get
            {
                return "100f5ec3-fde7-4205-80a7-c968b3a5a27b";
            }
        }
        /// <summary>
        /// Collection of layout spec paths that were requested for merging into the library layout.
        /// </summary>
        internal IEnumerable<string> RequestedLayoutSpecPaths { get;private set;} = new List<string>();

        public event Action<IViewExtension> RequestAddExtension;
        public event Func<string, IViewExtension> RequestLoadExtension;

        private Action<string> layouthandler;
        //explicit interface implementation lets us keep the event internal for now.
        event Action<string> ILayoutSpecSource.RequestApplyLayoutSpec
        {
            add { layouthandler += value; }
            remove { layouthandler -= value; }
        }

        public event Func<LayoutSpecification> RequestLayoutSpec;


        Action<NotificationMessage> notificationLogged;
        event Action<NotificationMessage> INotificationSource.NotificationLogged
        {
            add
            {
             notificationLogged+=value;
            }

            remove
            {
                notificationLogged -= value;
            }
        }

        public void Dispose()
        {
            if (packageManager != null)
            {
                packageManager.PackageLoader.PackgeLoaded -= packageLoadedHandler;
            }
            
            // Unsubscribe from DynamoViewModel events
            if (dynamoViewModel != null)
            {
                dynamoViewModel.RequestPackageManagerDialog -= OnRequestPackageManagerDialog;
                dynamoViewModel.RequestPackageManagerSearchDialog -= OnRequestPackageManagerSearchDialog;
                dynamoViewModel.RequestPackagePublishDialog -= OnRequestPackagePublishDialog;
            }
            
            // Clean up view models
            pkgSearchVM?.Dispose();
            pkgVM?.Dispose();
            
            // Close windows if open
            packageManagerWindow?.Close();
            searchPackagesView?.Close();
        }

        public void Loaded(ViewLoadedParams p)
        {
            viewLoadedParams = p;
            dynamoViewModel = p.DynamoWindow.DataContext as DynamoViewModel;
            
            // Subscribe to PackageManager dialog events from DynamoViewModel
            if (dynamoViewModel != null)
            {
                dynamoViewModel.RequestPackageManagerDialog += OnRequestPackageManagerDialog;
                dynamoViewModel.RequestPackageManagerSearchDialog += OnRequestPackageManagerSearchDialog;
                dynamoViewModel.RequestPackagePublishDialog += OnRequestPackagePublishDialog;
            }
            
            RequestLoadLayoutSpecs(packageManager?.PackageLoader.LocalPackages);
            var packagesToCheck = packageManager?.PackageLoader.LocalPackages;
            if(packagesToCheck != null)
            {
                RaisePackageHostNotifications(packagesToCheck);
            }

        }

        public void Shutdown()
        {
            // Do nothing for now
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            var packageManager = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            this.packageManager = packageManager;
            //when this extension is started up we should look for all packages,
            //and find the viewExtension manifest files in those packages.
            //Then request that these extensions be loaded.
            if (packageManager != null)
            {
                //attach event which we can use to watch when new packages are fully loaded.
                packageManager.PackageLoader.PackgeLoaded += packageLoadedHandler;
                var packagesToCheck = packageManager.PackageLoader.LocalPackages;
                RequestLoadViewExtensionsForLoadedPackages(packagesToCheck);
            }
            
        }

        private void RequestLoadViewExtensionsForLoadedPackages(IEnumerable<Package> packages)
        {
            foreach (var package in packages)
            {
                //if package was previously loaded then additional files are already cached.
                if (package.LoadState.State == PackageLoadState.StateTypes.Loaded)
                {
                    var vieweExtensionManifests = package.AdditionalFiles.Where(file => file.Model.Name.Contains("ViewExtensionDefinition.xml")).ToList();
                    foreach (var extPath in vieweExtensionManifests)
                    {
                        var viewExtension = RequestLoadExtension?.Invoke(extPath.Model.FullName);
                        if (viewExtension != null)
                        {
                            RequestAddExtension?.Invoke(viewExtension);
                            this.requestedExtensions.Add(viewExtension);
                        }
                    }
                }
            }
        }

        private void RaisePackageHostNotifications(IEnumerable<Package> packages)
        {
            foreach (var pkg in packages)
            {
                //check that the package does not target another host, if it does raise a warning.
                var pkgVersion = new PackageVersion() { host_dependencies = pkg.HostDependencies };
                var containsPackagesThatTargetOtherHosts = packageManager.CheckIfPackagesTargetOtherHosts(new List<PackageVersion>() { pkgVersion });

                // if any do, notify user of the potential conflict with notification.
                if (containsPackagesThatTargetOtherHosts)
                {
                    notificationLogged?.Invoke(
                        new NotificationMessage(Name,
                        $"{Resources.MessagePackageTargetOtherHostShort}: {pkg.Name}",
                        $"{Resources.MessagePackageTargetOtherHosts2}: {pkg.Name}",
                        Resources.TitlePackageTargetOtherHost));
                }
            }
        }

        private void RequestLoadLayoutSpecs(IEnumerable<Package> packages)
        {
            if (packages == null) return;
            foreach(var package in packages)
            {
                //only load layout specs for built in packages.
                if (!package.BuiltInPackage)
                {
                    continue;
                }
                //if package is builtin and has a layout spec, try to apply it.
                var packageLayoutSpecs = package.AdditionalFiles.Where(
                    file => file.Model.Name.ToLowerInvariant().Contains("layoutspecs.json")).ToList();
                //if an extension capable of handling layout specs is found, request
                //they apply the spec.
                foreach (var specPath in packageLayoutSpecs)
                {
                    (RequestedLayoutSpecPaths as IList<string>)?.Add(specPath.Model.FullName);
                    layouthandler?.Invoke(File.ReadAllText(specPath.Model.FullName));
                }
            }
        }


        private void packageLoadedHandler(Package package)
        {
            //when a package is loaded with packageManager, this extension should inspect it for viewExtensions.
            var pkgs = new List<Package>() { package };
            RequestLoadViewExtensionsForLoadedPackages(pkgs);
            //also check host target of the package and raise notifications.
            RaisePackageHostNotifications(pkgs);
            
        }

        #region PackageManager UI Event Handlers

        private void OnRequestPackageManagerDialog(object sender, EventArgs e)
        {
            if (!DisplayTermsOfUseForAcceptance())
                return; // Terms of use not accepted.

            InitializePackageManagerViewModels();

            if (packageManagerWindow == null)
            {
                if (e is PackageManagerSizeEventArgs sizeArgs)
                {
                    pkgVM.Width = sizeArgs.Width;
                    pkgVM.Height = sizeArgs.Height;
                }

                packageManagerWindow = new PackageManagerView(viewLoadedParams.DynamoWindow, pkgVM)
                {
                    Owner = viewLoadedParams.DynamoWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                dynamoViewModel.Owner = packageManagerWindow;
                packageManagerWindow.Closed += HandlePackageManagerWindowClosed;
                packageManagerWindow.Show();

                if (packageManagerWindow.IsLoaded && viewLoadedParams.DynamoWindow.IsLoaded)
                    packageManagerWindow.Owner = viewLoadedParams.DynamoWindow;
            }

            packageManagerWindow.Focus();
            if (e is OpenPackageManagerEventArgs openArgs)
            {
                packageManagerWindow.Navigate(openArgs.Tab);
            }

            pkgSearchVM.RefreshAndSearchAsync();
        }

        private void OnRequestPackageManagerSearchDialog(object sender, EventArgs e)
        {
            if (!DisplayTermsOfUseForAcceptance())
                return; // Terms of use not accepted.

            var cmd = Analytics.TrackTaskCommandEvent("SearchPackage");

            if (pkgSearchVM == null)
            {
                pkgSearchVM = new PackageManagerSearchViewModel(dynamoViewModel.PackageManagerClientViewModel);
            }
            else
            {
                pkgSearchVM.InitializeLuceneForPackageManager();
            }

            if (searchPackagesView == null)
            {
                searchPackagesView = new PackageManagerSearchView(pkgSearchVM)
                {
                    Owner = viewLoadedParams.DynamoWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                searchPackagesView.Closed += (s, args) =>
                {
                    searchPackagesView = null;
                    Analytics.EndTaskCommandEvent(cmd);
                };
                searchPackagesView.Show();

                if (searchPackagesView.IsLoaded && viewLoadedParams.DynamoWindow.IsLoaded)
                    searchPackagesView.Owner = viewLoadedParams.DynamoWindow;
            }

            searchPackagesView.Focus();
            pkgSearchVM.RefreshAndSearchAsync();
        }

        private void OnRequestPackagePublishDialog(PublishPackageViewModel model)
        {
            InitializePackageManagerViewModels();

            if (packageManagerWindow == null)
            {
                packageManagerWindow = new PackageManagerView(viewLoadedParams.DynamoWindow, pkgVM)
                {
                    Owner = viewLoadedParams.DynamoWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                dynamoViewModel.Owner = packageManagerWindow;
                packageManagerWindow.Closed += HandlePackageManagerWindowClosed;
                packageManagerWindow.Show();

                if (packageManagerWindow.IsLoaded && viewLoadedParams.DynamoWindow.IsLoaded)
                    packageManagerWindow.Owner = viewLoadedParams.DynamoWindow;
            }

            if (pkgVM != null)
            {
                pkgVM.PublishPackageViewModel = model;
            }

            packageManagerWindow.Focus();
            packageManagerWindow.Navigate(Resources.PackageManagerPublishTab);
        }

        private void HandlePackageManagerWindowClosed(object sender, EventArgs e)
        {
            packageManagerWindow.Closed -= HandlePackageManagerWindowClosed;
            packageManagerWindow = null;

            var cmd = Analytics.TrackCommandEvent("PackageManager");
            cmd.Dispose();

            viewLoadedParams?.DynamoWindow?.Activate();
        }

        private void InitializePackageManagerViewModels()
        {
            if (pkgSearchVM == null)
            {
                pkgSearchVM = new PackageManagerSearchViewModel(dynamoViewModel.PackageManagerClientViewModel);
            }

            if (pkgVM == null)
            {
                pkgVM = new PackageManagerViewModel(dynamoViewModel, pkgSearchVM);
            }
        }

        private bool DisplayTermsOfUseForAcceptance()
        {
            var prefSettings = dynamoViewModel.Model.PreferenceSettings;
            if (prefSettings.PackageDownloadTouAccepted)
                return true;

            prefSettings.PackageDownloadTouAccepted = TermsOfUseHelper.ShowTermsOfUseDialog(false, null, viewLoadedParams.DynamoWindow);
            return prefSettings.PackageDownloadTouAccepted;
        }

        #endregion

    }
}
