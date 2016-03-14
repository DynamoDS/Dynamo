using System;

namespace Dynamo.Exceptions
{
    /// <summary>
    /// Customized exception thrown when library load failed.
    /// </summary>
    public class LibraryLoadFailedException : Exception
    {
        public string Path;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        public LibraryLoadFailedException(string path, string message)
            : base(message)
        {
            Path = path;
        }
    }
}
