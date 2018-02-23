using System;
using System.Collections.Generic;


namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Handles loading extensions given an extension definition files path
    /// </summary>
    public interface IViewExtensionLoader
    {
        IViewExtension Load(string extensionPath);
        IEnumerable<IViewExtension> LoadDirectory(string extensionsPath);

        /// <summary>
        /// An event that is raised when an extension starts loading.
        /// </summary>
        event Action<IViewExtension> ExtensionLoading;
    }
}
