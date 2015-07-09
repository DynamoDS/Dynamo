using System;
using System.Reflection;

namespace Dynamo.Interfaces
{
    public interface IDynamoModel : IDisposable
    {
        ICustomNodeManager CustomNodeManager { get; }
        void LoadNodeLibraryFromAssembly(Assembly library);
    }
}
