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
        /// The local directory where log files are generated. This directory is 
        /// specific to the current user.
        /// </summary>
        string LogDirectory { get; }

        /// <summary>
        /// The packages directory, which contains pacakages downloaded through
        /// the package manager. This directory is specific to the current user.
        /// </summary>
        string PackagesDirectory { get; }

        IEnumerable<string> NodeDirectories { get; }
    }
}
