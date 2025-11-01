using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dynamo.Models.Migration.Python
{
    /// <summary>
    /// Headless service that detects Python engine usage in a workspace,
    /// performs temporary engine upgrades on nodes, and (optionally) commits
    /// permanent upgrades for custom node workspaces.
    /// </summary>
    public sealed class PythonEngineUpgradeService
    {
        private readonly DynamoModel model;
        private readonly IPathManager pathManager;

        public PythonEngineUpgradeService( DynamoModel model, IPathManager pathManager)
        {
            this.model = model;
            this.pathManager = pathManager;
        }

        /// <summary>
        /// Custom node definition IDs temporarily migrated
        /// </summary>
        public readonly HashSet<Guid> tempMigratedCustomDefs = new HashSet<Guid>();

        /// <summary>
        /// Custom node definition IDs permanently migrated
        /// </summary>
        public readonly HashSet<Guid> permMigratedCustomDefs = new HashSet<Guid>();

        /// <summary>
        /// Custom node workspaces touched during this session
        /// </summary>
        public readonly HashSet<WorkspaceModel> touchedCustomWorkspaces = new HashSet<WorkspaceModel>();

        /// <summary>
        /// Custom node definition IDs for which a toast/notice has already been shown
        /// </summary>
        public readonly HashSet<Guid> customToastShownDef = new HashSet<Guid>();

        public Guid LastWorkspaceId = Guid.Empty;

        /// <summary>
        /// Lightweight pair that binds a NodeModel to the WorkspaceModel it lives in.
        /// Keep this identical in usage to your original PyNodeWithWorkspace.
        /// </summary>
        public sealed class NodeWithWorkspace
        {
            public WorkspaceModel Workspace { get; }
            public NodeModel Node { get; }

            public NodeWithWorkspace(WorkspaceModel ws, NodeModel node)
            {
                Workspace = ws;
                Node = node;
            }
        }

        /// <summary>
        /// Result of a simple scan: direct python nodes + custom node definitions that contain python.
        /// </summary>
        public sealed class Usage
        {
            public WorkspaceModel Workspace { get; }
            public IEnumerable<NodeModel> DirectPythonNodes { get; }
            public IEnumerable<Guid> CustomNodeDefIdsWithPython { get; }
            public int TotalCount => (DirectPythonNodes?.Count() ?? 0) + (CustomNodeDefIdsWithPython?.Count() ?? 0);

            internal Usage(WorkspaceModel workspace, IReadOnlyList<NodeModel> pyNodes, IReadOnlyList<Guid> customDefs)
            {
                Workspace = workspace;
                DirectPythonNodes = pyNodes;
                CustomNodeDefIdsWithPython = customDefs;
            }
        }

        /// <summary>
        /// Scan the workspace for python usage (direct) and one level of custom nodes.
        /// </summary>
        public Usage DetectPythonUsage(WorkspaceModel workspace, Func<NodeModel, bool> isPythonNode)
        {
            if (workspace == null) throw new ArgumentNullException(nameof(workspace));
            if (isPythonNode == null) throw new ArgumentNullException(nameof(isPythonNode));

            // Direct python nodes
            var directNodes = workspace.Nodes.Where(isPythonNode).ToList();

            // Custom nodes that contain python
            var customDefIds = new HashSet<Guid>();
            foreach (var func in workspace.Nodes.OfType<Dynamo.Graph.Nodes.CustomNodes.Function>())
            {
                var defId = func.Definition?.FunctionId ?? Guid.Empty;
                if (defId == Guid.Empty) continue;

                if (CustomNodeHasPython(defId, isPythonNode))
                {
                    customDefIds.Add(defId);
                }
            }

            return new Usage( workspace, directNodes, customDefIds.ToList());
        }

        /// <summary>
        /// Upgrade the engine for a set of python nodes in memory and rerurn the count changed.
        /// </summary>
        public int UpgradeNodesInMemory(
            IEnumerable<NodeModel> pyNodes,
            WorkspaceModel workspace,
            Action<NodeModel, WorkspaceModel> setEngine)
        {
            if (pyNodes is null) throw new ArgumentNullException(nameof(pyNodes));
            if (workspace is null) throw new ArgumentNullException(nameof(workspace));
            if (setEngine is null) throw new ArgumentNullException(nameof(setEngine));

            int changed = 0;

            foreach (var node in pyNodes)
            {
                setEngine(node, workspace);
                changed++;
            }
            return changed;
        }

        //#region Commit Custom Node Migration
        ///// <summary>
        ///// Commit custom node migrations on save: overwrite .dyf files for any definitions
        ///// we temporarily upgraded this session, and mark them as permanent.
        ///// </summary>
        //public void CommitCustomNodeMigrationsOnSave()
        //{
        //    foreach (var workspace in touchedCustomWorkspaces.ToList())
        //    {
        //        try
        //        {
        //            if (!TryGetCustomIdAndPath(workspace, out var defId, out var dyfPath) ||
        //                string.IsNullOrEmpty(dyfPath))
        //                continue;

        //            var path = GetWorkspaceFilePath(workspace);
        //            if (string.IsNullOrEmpty(path))
        //            {
        //                path = dyfPath;
        //                if (string.IsNullOrEmpty(path)) continue;
        //            }

        //            if (workspace is CustomNodeWorkspaceModel customWorkspace)
        //            {
        //                customWorkspace.IsVisibleInDynamoLibrary = true;
        //            }

        //            // Overwrite the .dyf file
        //            workspace.Save(path, false, model.EngineController);
        //            EnsureDyfHasLibraryViewFlag(dyfPath);

        //            permMigratedCustomDefs.Add(defId);
        //            tempMigratedCustomDefs.Remove(defId);
        //        }
        //        catch { }
        //    }
        //}

        ///// <summary>
        ///// Returns the on-disk file path for the given workspace
        ///// </summary>
        //private string GetWorkspaceFilePath(WorkspaceModel ws)
        //{
        //    if (ws == null) return null;

        //    var path = ws.FileName;
        //    if (!string.IsNullOrEmpty(path)) return path;

        //    return null;
        //}

        ///// <summary>
        ///// Outputs the custom node definition ID and its .dyf path for a custom workspace, returning true if the workspace is a custom node
        ///// </summary>
        //private bool TryGetCustomIdAndPath(WorkspaceModel ws, out Guid defId, out string dyfPath)
        //{
        //    defId = Guid.Empty;
        //    dyfPath = null;

        //    if (ws is Dynamo.Graph.Workspaces.CustomNodeWorkspaceModel cws)
        //    {
        //        defId = cws.CustomNodeId;

        //        var cnm = model?.CustomNodeManager;
        //        if (cnm != null &&
        //            cnm.NodeInfos.TryGetValue(defId, out var info) &&
        //            !string.IsNullOrEmpty(info.Path))
        //        {
        //            dyfPath = info.Path;
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// Ensures the .dyf JSON sets View.Dynamo.IsVisibleInDynamoLibrary = true and writes the file back if needed
        ///// </summary>
        //private void EnsureDyfHasLibraryViewFlag(string dyfPath)
        //{
        //    if (string.IsNullOrEmpty(dyfPath) || !File.Exists(dyfPath)) return;

        //    var json = File.ReadAllText(dyfPath);
        //    var root = JObject.Parse(json);

        //    var view = (JObject?)root["View"] ?? new JObject();
        //    var dyn = (JObject?)view["Dynamo"] ?? new JObject();

        //    dyn["IsVisibleInDynamoLibrary"] = true;
        //    view["Dynamo"] = dyn;
        //    root["View"] = view;

        //    File.WriteAllText(dyfPath, root.ToString(Formatting.Indented));
        //}
        //#endregion


        

        private bool CustomNodeHasPython(Guid defId, Func<NodeModel, bool> isPythonNode)
        {
            if (model?.CustomNodeManager == null) return false;

            CustomNodeWorkspaceModel ws;
            if (model.CustomNodeManager.TryGetFunctionWorkspace(defId, DynamoModel.IsTestMode, out ws) && ws != null)
            {
                return ws.Nodes?.Any(isPythonNode) == true;
            }

            return false;
        }

    }
}
