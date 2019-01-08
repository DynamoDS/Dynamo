using System;
using System.Collections.Generic;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Handles loading extensions given an extension definition files path
    /// </summary>
    public interface IExtensionLoader
    {
        /// <summary>
        /// Extension method for loading assembly from the path. Returns <see cref="IExtension"/>.
        /// </summary>
        IExtension Load(string extensionPath);

        /// <summary>
        /// Extension method for loading assembly from a directory. Returns <see cref="IExtension"/>.
        /// </summary>
        IEnumerable<IExtension> LoadDirectory(string extensionsPath);

        /// <summary>
        /// An event that is raised when an extension starts loading.
        /// </summary>
        event Action<IExtension> ExtensionLoading;
    }
}
