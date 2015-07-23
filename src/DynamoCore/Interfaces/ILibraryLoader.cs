using System;
using System.Reflection;

namespace Dynamo.Interfaces
{
    public interface ILibraryLoader : IDisposable
    {
        void LoadNodeLibrary(Assembly library);
    }
}
