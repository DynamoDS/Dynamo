﻿using System;
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
        /// Extension method for loading assembly from the path. Returns <see cref="IExtension"/>.
        /// </summary>
        IExtension Load(string extensionPath);

        /// <summary>
        /// Extension method for loading assembly from a directory. Returns <see cref="IExtension"/>.
        /// </summary>
        IEnumerable<IExtension> LoadDirectory(string extensionsPath);
    }
}
