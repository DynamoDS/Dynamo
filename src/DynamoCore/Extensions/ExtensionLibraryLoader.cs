using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamo.Engine;
using Dynamo.Library;
using Dynamo.Models;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Provides functionality for loading node's DLLs
    /// </summary>
    public class ExtensionLibraryLoader : ILibraryLoader 
    {
        private readonly DynamoModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionLibraryLoader"/> class.
        /// </summary>
        /// <param name="model">Dynamo model.</param>
        internal ExtensionLibraryLoader(DynamoModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Loads a ZeroTouch or NodeModel based node into the VM and search.
        /// To guarantee the node is correctly added to the LibraryUI this method should not
        /// be called while LibraryExtension is loading.
        /// </summary>
        /// <param name="library">The library.</param>
        public void LoadNodeLibrary(Assembly library)
        {
            model.LoadNodeLibrary(library,false);
        }

        //TODO add to ILibraryLoader in 3.0 OR refactor package/zeroTouch import code path.
        /// <summary>
        /// Loads a zeroTouch or explicit NodeModel based node into the VM.
        /// Does not add zeroTouch libraries to Search. Currently only used by package manager extension.
        /// </summary>
        /// <param name="library">The library.</param>
        internal void LoadLibraryAndSuppressZTSearchImport(Assembly library)
        {
            model.LoadNodeLibrary(library, true);
        }

        //TODO add to ILibraryLoader in 3.0
        /// <summary>
        /// Loads packages for import into VM and for node search.
        /// </summary>
        /// <param name="assemblies"></param>
        public void LoadPackages(IEnumerable<Assembly> assemblies)
        {
            var libraryPaths = assemblies.Select(x => x.Location);
            model.LibraryServices.OnLibrariesImported(new LibraryServices.LibraryLoadedEventArgs(libraryPaths));
        }
    }
}
