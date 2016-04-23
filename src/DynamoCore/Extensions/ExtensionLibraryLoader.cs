using Dynamo.Interfaces;
using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dynamo.Library;

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
        /// Loads the node library.
        /// </summary>
        /// <param name="library">The library.</param>
        public void LoadNodeLibrary(Assembly library)
        {
            model.LoadNodeLibrary(library);
        }
    }
}
