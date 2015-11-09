using System.Reflection;

namespace Dynamo.Library
{
    public interface ILibraryLoader
    {
        void LoadNodeLibrary(Assembly library);
    }
}
