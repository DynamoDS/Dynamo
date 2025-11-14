using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dynamo.Models.Migration.Python
{
    /// <summary>
    /// Headless service that detects Python engine usage in a workspace,
    /// performs temporary engine upgrades on nodes, and (optionally) commits
    /// permanent upgrades for custom node workspaces.
    /// </summary>
    public sealed class PythonEngineUpgradeService
    {
        private readonly DynamoModel dynamoModel;
        private readonly IPathManager pathManager;

        public PythonEngineUpgradeService(DynamoModel dynamoModel, IPathManager pathManager)
        {
            this.dynamoModel = dynamoModel;
            this.pathManager = pathManager;
        }

        /// <summary>
        /// Custom node definition IDs temporarily migrated
        /// </summary>
        public readonly HashSet<Guid> TempMigratedCustomDefs = new HashSet<Guid>();

        /// <summary>
        /// Custom node definition IDs permanently migrated
        /// </summary>
        public readonly HashSet<Guid> PermMigratedCustomDefs = new HashSet<Guid>();

        /// <summary>
        /// Custom node workspaces touched during this session
        /// </summary>
        public readonly HashSet<WorkspaceModel> TouchedCustomWorkspaces = new HashSet<WorkspaceModel>();

        /// <summary>
        /// Custom node definition IDs for which a toast/notice has already been shown
        /// </summary>
        public readonly HashSet<Guid> CustomToastShownDef = new HashSet<Guid>();

        /// <summary>
        /// Result of a simple scan: direct python nodes + custom node definitions that contain python.
        /// </summary>
        public sealed class Usage
        {
            public WorkspaceModel Workspace { get; }
            public IEnumerable<NodeModel> DirectPythonNodes { get; }
            public IEnumerable<Guid> CustomNodeDefIdsWithPython { get; }

            internal Usage(WorkspaceModel workspace, IReadOnlyList<NodeModel> pyNodes, IReadOnlyList<Guid> customDefs)
            {
                Workspace = workspace;
                DirectPythonNodes = pyNodes ?? Enumerable.Empty<NodeModel>();
                CustomNodeDefIdsWithPython = customDefs ?? Enumerable.Empty<Guid>();
            }
        }

        /// <summary>
        /// Attempts to retrieve the custom node workspace associated with the specified function identifier from the
        /// given Dynamo model.
        /// </summary>
        internal ICustomNodeWorkspaceModel TryGetFunctionWorkspace(DynamoModel dynamoModel, Guid guid)
        {
            ICustomNodeWorkspaceModel ws;
            var cnm = dynamoModel?.CustomNodeManager;

            if (cnm != null && cnm.TryGetFunctionWorkspace(guid, DynamoModel.IsTestMode, out ws))
            {
                return ws;
            }

            return null;
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

                if (CustomNodeHasPython(defId, isPythonNode) || TempMigratedCustomDefs.Contains(defId))
                {
                    customDefIds.Add(defId);
                }
            }

            return new Usage(workspace, directNodes, customDefIds.ToList());
        }

        /// <summary>
        /// Upgrade the engine for a set of python nodes in memory and return the count changed.
        /// </summary>
        public int UpgradeNodesInMemory(
            IEnumerable<NodeModel> pyNodes,
            WorkspaceModel workspace,
            Action<NodeModel, WorkspaceModel> setEngine)
        {
            if (pyNodes is null) throw new ArgumentNullException(nameof(pyNodes));
            if (workspace is null) throw new ArgumentNullException(nameof(workspace));
            if (setEngine is null) throw new ArgumentNullException(nameof(setEngine));

            var nodes = pyNodes.ToList();
            nodes.ForEach(node => setEngine(node, workspace));
            return nodes.Count;
        }

        /// <summary>
        /// Commits migration changes for custom-node workspaces by editing the .dyf JSON in place:
        /// switches Python nodes from CPython3 to PythonNet3 and saves a backup file
        /// </summary>
        public void CommitCustomNodeMigrationsOnSave()
        {
            foreach (var workspace in TouchedCustomWorkspaces.ToList())
            {
                try
                {
                    if (!TryGetCustomIdAndPath(workspace, out var defId, out var dyfPath) || string.IsNullOrEmpty(dyfPath)) continue;

                    SaveCustomNodeBackup(workspace, dyfPath, PythonServices.PythonEngineManager.CPython3EngineName);

                    var upgraded = SwitchDyfPythonEngineInPlace(
                        dyfPath,
                        PythonServices.PythonEngineManager.CPython3EngineName,
                        PythonServices.PythonEngineManager.PythonNet3EngineName);

                    if (upgraded)
                    {
                        PermMigratedCustomDefs.Add(defId);
                        TempMigratedCustomDefs.Remove(defId);
                    }
                }
                catch (Exception ex)
                {
                    this.dynamoModel?.Logger?.Log(ex);
                }
            }
        }

        /// <summary>
        /// Build a backup file path for a .dyn or .dyf backup
        /// </summary>
        public string BuildDynBackupFilePath(WorkspaceModel workspace, string token)
        {
            if (workspace == null || pathManager == null) return null;
            if (DynamoModel.IsTestMode) return null;

            var backupDir = pathManager.BackupDirectory;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string baseName = Path.GetFileNameWithoutExtension(workspace.FileName);
            string ext = Path.GetExtension(workspace.FileName);

            baseName = SanitizeFileName(baseName);
            var fileName = $"{baseName}.{token}.{timestamp}{ext}";

            return Path.Combine(backupDir, fileName);
        }

        /// <summary>
        /// Save a .dyn backup of the given workspace with the given token.
        /// </summary>
        public string SaveGraphBackup(WorkspaceModel workspace, string token)
        {
            var path = BuildDynBackupFilePath(workspace, token);
            if (string.IsNullOrEmpty(path)) return null;

            try
            {
                workspace.Save(path, true);
                return path;
            }
            catch (Exception ex)
            {
                this.dynamoModel?.Logger?.Log(ex);
                return null;
            }
        }

        /// <summary>
        /// Save a .dyf backup of the given custom node workspace before engine upgrade.
        /// </summary>
        private string SaveCustomNodeBackup(WorkspaceModel workspace, string sourcePath, string token) 
        {
            var backupPath = BuildDynBackupFilePath(workspace, token);
            if (string.IsNullOrEmpty(backupPath)) return null;

            try
            {
                File.Copy(sourcePath, backupPath);
                return backupPath;
            }
            catch (Exception ex)
            {
                this.dynamoModel?.Logger?.Log(ex);
                return null;
            }
        }

        private bool SwitchDyfPythonEngineInPlace(string dyfPath, string oldEngName, string newEngName)
        {
            if (string.IsNullOrEmpty(dyfPath) || !File.Exists(dyfPath)) return false;
                
            var root = JObject.Parse(File.ReadAllText(dyfPath));
            var nodes = root["Nodes"] as JArray;
            if (nodes == null || nodes.Count == 0) return false;

            try
            {
                foreach (var n in nodes.OfType<JObject>())
                {
                    var concrete = n.Value<string>("ConcreteType");
                    if (string.IsNullOrEmpty(concrete) || !concrete.StartsWith("PythonNodeModels", StringComparison.Ordinal)) continue;

                    var engine = n.Property("Engine", StringComparison.OrdinalIgnoreCase);
                    if (engine != null && string.Equals((string)engine.Value, oldEngName, StringComparison.Ordinal))
                    {
                        engine.Value = newEngName;
                    }
                }

                File.WriteAllText(dyfPath, root.ToString(Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                this.dynamoModel?.Logger?.Log(ex);
                return false;
            }
        }

        private bool TryGetCustomIdAndPath(WorkspaceModel workspace, out Guid defId, out string dyfPath)
        {
            defId = Guid.Empty;
            dyfPath = null;

            if (workspace is CustomNodeWorkspaceModel cws)
            {
                defId = cws.CustomNodeId;
                var cnm = dynamoModel?.CustomNodeManager;
                if (cnm != null &&
                    cnm.NodeInfos.TryGetValue(defId, out var info) &&
                    !string.IsNullOrEmpty(info.Path))
                {
                    dyfPath = info.Path;
                }
                return true;
            }
            return false;
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        }

        private bool CustomNodeHasPython(Guid defId, Func<NodeModel, bool> isPythonNode)
        {
            if (dynamoModel?.CustomNodeManager == null) return false;

            var cws = this.TryGetFunctionWorkspace(dynamoModel, defId) as CustomNodeWorkspaceModel;
            return cws?.Nodes != null && cws.Nodes.Any(isPythonNode) == true;
        }
    }
}
