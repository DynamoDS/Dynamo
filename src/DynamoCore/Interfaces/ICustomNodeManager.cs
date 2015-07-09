using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Dynamo.Interfaces
{
    public interface ICustomNodeManager : ICustomNodeSource
    {
        event Action<Guid> CustomNodeRemoved;
        event Action<CustomNodeInfo> InfoUpdated;
        event Action<CustomNodeDefinition> DefinitionUpdated;
        event Action<ILogMessage> MessageLogged;

        IEnumerable<CustomNodeDefinition> LoadedDefinitions { get; }
        IEnumerable<CustomNodeWorkspaceModel> LoadedWorkspaces { get; }
        Dictionary<Guid, CustomNodeInfo> NodeInfos { get; }
        
        WorkspaceModel CreateCustomNode(
            string name, string category, string description, Guid? functionId = null);

        IEnumerable<CustomNodeInfo> AddUninitializedCustomNodesInPath(
            string path, bool isTestMode, bool isPackageMember = false);

        bool OpenCustomNodeWorkspace(XmlDocument xmlDoc, WorkspaceInfo workspaceInfo, 
            bool isTestMode, out WorkspaceModel workspace);

        CustomNodeWorkspaceModel Collapse(
            IEnumerable<NodeModel> selectedNodes, WorkspaceModel currentWorkspace,
            bool isTestMode, FunctionNamePromptEventArgs args);

        bool Contains(Guid guid);
        bool Contains(string name);
        bool IsInitialized(Guid guid);
        bool AddUninitializedCustomNode(string file, bool isTestMode, out CustomNodeInfo info);
        bool TryGetFunctionDefinition(Guid id, bool isTestMode, out CustomNodeDefinition definition);
        bool TryGetFunctionWorkspace(Guid id, bool isTestMode, out CustomNodeWorkspaceModel ws);
        bool TryGetNodeInfo(Guid id, out CustomNodeInfo info);
        void Remove(Guid guid);
        CustomNodeWorkspaceModel GetWorkspaceById(Guid customNodeId);
        IEnumerable<Guid> GetAllDependenciesGuids(CustomNodeDefinition def);
        Guid GuidFromPath(string path);
    }
}
