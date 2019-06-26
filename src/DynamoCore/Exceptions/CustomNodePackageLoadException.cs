using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Exceptions
{
    public class CustomNodePackageLoadException : Exception
    {
        /// <summary>
        /// File path of failing custom node package.
        /// </summary>
        public string Path;

        /// <summary>
        /// Failure reason.
        /// </summary>
        public string Reason;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="reason"></param>
        public CustomNodePackageLoadException(string path, string reason)
            : base(String.Format(Properties.Resources.FailedToLoad, path, reason))
        {
            Path = path;
            Reason = reason;
        }
    }
}
