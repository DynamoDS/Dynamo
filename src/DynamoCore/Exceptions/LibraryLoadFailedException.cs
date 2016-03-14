using System;

namespace Dynamo.Exceptions
{
    /// <summary>
    /// Customized exception thrown when library load failed.
    /// </summary>
    public class LibraryLoadFailedException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        public LibraryLoadFailedException(string message)
            : base(message)
        {

        }
    }
}
