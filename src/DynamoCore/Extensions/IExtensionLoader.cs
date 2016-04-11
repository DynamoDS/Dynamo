using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Handles loading extensions given an extension definition files path
    /// </summary>
    public interface IExtensionLoader
    {
        /// <summary>
        /// Loads assembly by passed extension path and return it as IExtension
        /// </summary>
        IExtension Load(string extensionPath);

        /// <summary>
        /// Returns an enum of IExtension specified by passed extension path
        /// </summary>
        IEnumerable<IExtension> LoadDirectory(string extensionsPath);
    }
}
