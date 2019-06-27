using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Exceptions
{
    public class CustomNodePackageLoadException : LibraryLoadFailedException
    {
        /// <summary>
        /// File path of existing custom node package.
        /// </summary>
        public string InstalledPath { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">File path of failing custom node package.</param>
        /// <param name="installedPath">File path of existing package.</param>
        /// <param name="reason">Failure reason.</param>
        public CustomNodePackageLoadException(string path, string installedPath, string reason)
            : base(path, reason)
        {
            InstalledPath = installedPath;
        }
    }
}
