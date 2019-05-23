using System.Collections.Generic;
using System.Reflection;

namespace Dynamo.Library
{
    /// <summary>
    /// Exposes (use cref) LoadNodeLibrary method. Specify the usage of LoadNodeLibrary
    /// </summary>
    public interface ILibraryLoader
    {
        /// <summary>
        /// Loads node's library
        /// </summary>
        void LoadNodeLibrary(Assembly library);

        void LoadPackages(IEnumerable<Assembly> assemblies);
    }
}
