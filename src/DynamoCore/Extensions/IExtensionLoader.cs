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
        IExtension Load(string extensionPath);
        IEnumerable<IExtension> LoadDirectory(string extensionsPath);
    }
}
