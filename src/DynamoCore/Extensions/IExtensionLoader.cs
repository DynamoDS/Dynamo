using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.PackageManager;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Handles loading extensions given an extension definition file
    /// </summary>
    internal interface IExtensionLoader
    {
        IExtension Load(ExtensionDefinition extension);
    }
}
