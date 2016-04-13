using System.Reflection;

namespace Dynamo.Library
{
    /// <summary>
    /// An interface which provides functionality for loading node's library
    /// </summary>
    public interface ILibraryLoader
    {
        /// <summary>
        /// Loads node's library
        /// </summary>
        void LoadNodeLibrary(Assembly library);
    }
}
