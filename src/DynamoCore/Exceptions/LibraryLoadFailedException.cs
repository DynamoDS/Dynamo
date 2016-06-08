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
        /// <param name="path"></param>
        /// <param name="reason"></param>
        public LibraryLoadFailedException(string path, string reason)
            : base(String.Format(Properties.Resources.FailedToLoadLibrary, path))
        {
            Path = path;
            Reason = reason;
        }
    }
}
