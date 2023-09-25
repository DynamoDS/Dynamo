using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Logging;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.Properties;
using Greg.Responses;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// The View layer of the packageManagerExtension.
    /// Currently its only responsibility is to request the loading of ViewExtensions which it finds in packages.
    /// In the future packageManager functionality should be moved from DynamoCoreWPF to this ViewExtension.
    /// </summary>
    public class PackageManagerViewExtension : IViewExtension, IViewExtensionSource, ILayoutSpecSource, INotificationSource
    {
        private readonly List<IViewExtension> requestedExtensions = new List<IViewExtension>();
        private PackageManagerExtension packageManager;
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
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
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
                if (package.Loaded)
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


    }
}
