using System;

namespace Dynamo.Interfaces
{
    public interface ICustomNodeWorkspaceModel : IWorkspaceModel, IDisposable
    {        
        Guid CustomNodeId { get; }

        CustomNodeDefinition CustomNodeDefinition { get; }
        CustomNodeInfo CustomNodeInfo { get; }

        event Action InfoChanged;
        event Action DefinitionUpdated;
        event Action<Guid> FunctionIdChanged;
    }
}
