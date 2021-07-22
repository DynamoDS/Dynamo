using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Extensions;
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
        private ExtensionLibraryLoader librayLoader;
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

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            InspectPackageForNodeViewCustomizationBinaries(packageManager?.PackageLoader.LocalPackages);
        }

        public void Shutdown()
        {
            // Do nothing for now
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            var packageManager = viewStartupParams.ExtensionManager.Extensions.OfType<PackageManagerExtension>().FirstOrDefault();
            this.packageManager = packageManager;
            this.librayLoader = viewStartupParams.LibraryLoader as ExtensionLibraryLoader;

            //when this extension is started up we should look for all packages,
            //and find the viewExtension manifest files in those packages.
            //Then request that these extensions be loaded.
            if (packageManager != null)
            {
                //attach event which we can use to watch when new packages are fully loaded.
                packageManager.PackageLoader.PackgeLoaded += packageLoadedHandler;
                var packagesToCheck = packageManager.PackageLoader.LocalPackages;
                RequestLoadViewExtensionsForLoadedPackages(packagesToCheck);
                // also look for assemblies that only contains view customization (no nodeModels) as they won't have
                // been imported into the customizationLibrary previously.
                InspectPackageForNodeViewCustomizationBinaries(packagesToCheck);
            }
        }

        private void InspectPackageForNodeViewCustomizationBinaries(IEnumerable<Package> packages)
        {
            foreach (var package in packages)
            {
                // these packages are already loaded, these assemblies which contain only INodeViewCustomizatons
                // should already be loaded as well - so we need to check packageAssemblies that are marked as NodeLibraies
                // check if they contain INodeViewCustomization and if so inject them into the CustmizationLibrary
                if (package.Loaded)
                {
                    foreach(var nodeLibAssem in package.LoadedAssemblies.Where(x => x.IsNodeLibrary))
                    {
                        // if this assembly is a nodeLibrary(was in nodelibraries list) and it contains customizations
                        // TODO be careful of loading customizations twice.
                        if (Wpf.NodeViewCustomizationLoader.ContainsNodeViewCustomizationType(nodeLibAssem.Assembly))
                        {
                            librayLoader.LoadNodeViewCustomizationAssembly(nodeLibAssem.Assembly);
                        }
                    }
                }
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
                        }
                        this.requestedExtensions.Add(viewExtension);
                    }
                }
            }
        }

        private void packageLoadedHandler(Package package)
        {
            //when a package is loaded with packageManager, this extension should inspect it for viewExtensions.
            this.RequestLoadViewExtensionsForLoadedPackages(new List<Package>() { package });
        }
    }
}
