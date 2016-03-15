using System;

namespace Dynamo.Exceptions
{
    /// <summary>
    /// Customized exception thrown when library load failed.
    /// </summary>
    public class LibraryLoadFailedException : Exception
    {
        /// <summary>
        /// File path of failing dll.
        /// </summary>
        public string Path;

        /// <summary>
        /// Failure reason.
        /// </summary>
        public string Reason;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        public LibraryLoadFailedException(string path, string reason)
            : base(String.Format("Failed to load {0}", path))
        {
            Path = path;
            Reason = reason;
        }
    }
}
