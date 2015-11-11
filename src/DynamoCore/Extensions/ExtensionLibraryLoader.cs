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
    public class ExtensionLibraryLoader : ILibraryLoader
    {
        private readonly DynamoModel model;

        internal ExtensionLibraryLoader(DynamoModel model)
        {
            this.model = model;
        }

        public void LoadNodeLibrary(Assembly library)
        {
            model.LoadNodeLibrary(library);
        }
    }
}
