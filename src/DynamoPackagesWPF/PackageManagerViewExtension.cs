using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Wpf.Extensions;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// The View layer of the packageManagerExtension.
    /// Currently its only responsibility is to request the loading of ViewExtensions which it finds in packages.
    /// In the future packageManager functionality should be moved from DynamoCoreWPF to this ViewExtension.
    /// </summary>
    public class PackageManagerViewExtension : IViewExtension, IViewExtensionSource
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

        public event Action<IViewExtension> RequestAddExtension;
        public event Func<string, IViewExtension> RequestLoadExtension;

        public void Dispose()
        {
            packageManager.PackageLoader.PackgeLoaded -= packageLoadedHandler;
        }

        public void Loaded(ViewLoadedParams p)
        {
           
        }

        public void Shutdown()
        {
            Dispose();
        }

        public void Startup(ViewStartupParams p)
        {
            var packageManager = p.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            this.packageManager = packageManager;

            //when this extension is started up we should look for all packages,
            //and find the viewExtension manifest files in those packages.
            //Then request that these extensions be loaded.
            if (packageManager != null)
            {
                //attach event which we can use to watch when new packages are fully loaded.
                packageManager.PackageLoader.PackgeLoaded += packageLoadedHandler;
                var packagesToCheck = packageManager.PackageLoader.LocalPackages;
                requestLoadViewExtensionsForLoadedPackages(packagesToCheck);
            }
        }

        private void requestLoadViewExtensionsForLoadedPackages(IEnumerable<Package> packages)
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
                        }
                        this.requestedExtensions.Add(viewExtension);
                    }
                }
            }
        }

        private void packageLoadedHandler(Package package)
        {
            //when a package is loaded with packageManager, this extension should inspect it for viewExtensions.
            this.requestLoadViewExtensionsForLoadedPackages(new List<Package>() { package });
        }
    }
}
