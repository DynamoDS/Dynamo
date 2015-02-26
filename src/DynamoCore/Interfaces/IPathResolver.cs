using System.Collections.Generic;

namespace Dynamo.Interfaces
{
    public interface IPathResolver
    {
    }

    public interface IPathManager
    {
        /// <summary>
        /// The local directory that contains custom nodes created by the user.
        /// </summary>
        string UserDefinitions { get; }

        /// <summary>
        /// The local directory that contains custom nodes created by all users.
        /// </summary>
        string CommonDefinitions { get; }

        /// <summary>
        /// The local directory where log files are generated.
        /// </summary>
        string LogDirectory { get; }

        IEnumerable<string> NodeDirectories { get; }
    }
}
