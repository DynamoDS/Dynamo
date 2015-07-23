using System;
using System.Reflection;

namespace Dynamo.Interfaces
{
    public interface ILibraryLoader
    {
        void LoadNodeLibrary(Assembly library);
    }
}
