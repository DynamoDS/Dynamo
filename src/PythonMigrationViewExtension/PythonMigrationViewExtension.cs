using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Models.Migration.Python;
using Dynamo.PythonMigration.Controls;
using Dynamo.PythonMigration.MigrationAssistant;
using Dynamo.PythonMigration.Properties;
using Dynamo.PythonServices;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PythonNodeModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Dynamo.PythonMigration
{
    internal class PythonMigrationViewExtension : IViewExtension
    {
        private const string EXTENSION_NAME = "Python Migration";
        private const string EXTENSION_GUID = "1f8146d0-58b1-4b3c-82b7-34a3fab5ac5d";
        private bool hasCPython3Engine;
        private bool hasPythonNet3Engine;
        private bool enginesSubscribed;
        private bool autoRunTemporarilyDisabled = false;
        private readonly HashSet<Guid> tempMigratedCustomDefs = new HashSet<Guid>();
        private readonly HashSet<Guid> permMigratedCustomDefs = new HashSet<Guid>();
        private readonly HashSet<WorkspaceModel> touchedCustomWorkspaces = new HashSet<WorkspaceModel>();
        private readonly HashSet<Guid> customToastShownDef = new HashSet<Guid>();                           // track which custom node have already been shown the toast in this session  // DO WE NEED THIS??
        private Guid lastWorkspaceGuid = Guid.Empty;
        private PythonEngineUpgradeService upgradeSvc;
        private bool saveBackup;


        internal ViewLoadedParams LoadedParams { get; set; }
        internal DynamoViewModel DynamoViewModel { get; set; }
        internal WorkspaceModel CurrentWorkspace { get; set; }
        internal GraphPythonDependencies PythonDependencies { get; set; }
        internal CustomNodeManager CustomNodeManager { get; set; }
        internal static Uri Python3HelpLink = new Uri(PythonNodeModels.Properties.Resources.PythonMigrationWarningUriString, UriKind.Relative);
        private Dispatcher Dispatcher { get; set; }

        internal Dictionary<Guid, NotificationMessage> NotificationTracker = new Dictionary<Guid, NotificationMessage>();

        /// <summary>
        /// Extension GUID
        /// </summary>
        public string UniqueId { get { return EXTENSION_GUID; } }

        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name { get { return EXTENSION_NAME; } }

        public void Shutdown()
        {
            // Do nothing for now
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now 
        }

        public void Dispose()
        {
            UnsubscribeEvents();
        }

        public void Loaded(ViewLoadedParams p)
        {
            LoadedParams = p;

            var ironPythonVersion = new Version(3, 2, 0);
            if(LoadedParams.StartupParams.Preferences is PreferenceSettings prefs)
            {
                 Version.TryParse(prefs.IronPythonResolveTargetVersion,out ironPythonVersion);
            }

            PythonDependencies = new GraphPythonDependencies(LoadedParams.CurrentWorkspaceModel, LoadedParams.StartupParams.CustomNodeManager,ironPythonVersion );
            DynamoViewModel = LoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            CurrentWorkspace = LoadedParams.CurrentWorkspaceModel as WorkspaceModel;
            CustomNodeManager = (CustomNodeManager)LoadedParams.StartupParams.CustomNodeManager;
            CurrentWorkspace.RequestPythonEngineMapping += PythonDependencies.GetPythonEngineMapping;
            Dispatcher = Dispatcher.CurrentDispatcher;
            upgradeSvc = new PythonEngineUpgradeService(DynamoViewModel.Model, LoadedParams.StartupParams.PathManager);

            SubscribeToDynamoEvents();
            InitEngineFlagAndSubscribe();
        }

        private void LogCPython3Notification()
        {
            if (NotificationTracker.ContainsKey(CurrentWorkspace.Guid))
                return;

            DynamoViewModel.Model.Logger.LogNotification(
                GetType().Name,
                EXTENSION_NAME,
                Resources.CPython3NotificationShortMessage,
                Resources.CPython3NotificationDetailedMessage);
        }

        private void LogIronPythonNotification()
        {
            if (NotificationTracker.ContainsKey(CurrentWorkspace.Guid))
                return;

            DynamoViewModel.Model.Logger.LogNotification(
                GetType().Name,
                EXTENSION_NAME,
                Resources.IronPythonNotificationShortMessage,
                Resources.IronPythonNotificationDetailedMessage);
        }

        internal void OpenPythonMigrationWarningDocumentation()
        {
            LoadedParams.ViewModelCommandExecutive.OpenDocumentationLinkCommand(Python3HelpLink);
        }

        #region Events

        private void OnMigrationAssistantRequested(object sender, EventArgs e)
        {
            var routedEventArgs = e as RoutedEventArgs;
            if (routedEventArgs == null)
                throw new NullReferenceException(nameof(e));

            var btn = routedEventArgs.OriginalSource as System.Windows.Controls.Button;
            var parentWindow = Window.GetWindow(btn);

            var node = sender as PythonNode;
            var viewModel = new PythonMigrationAssistantViewModel(node, LoadedParams.CurrentWorkspaceModel as WorkspaceModel, LoadedParams.StartupParams.PathManager, LoadedParams.ViewStartupParams.DynamoVersion);
            var assistantWindow = new BaseDiffViewer(viewModel)
            {
                Owner = parentWindow
            };

            // show modal window so user cant interact with dynamo while migration assistant is open
            // if running in test mode, show modeless window show the test doesn't hang when opening the assistant window.
            if (Models.DynamoModel.IsTestMode)
            {
                assistantWindow.Show();
                return;
            }
                
            assistantWindow.ShowDialog();
        }

        private void OnNotificationLogged(NotificationMessage obj)
        {
            if (obj.Title == EXTENSION_NAME)
            {
                NotificationTracker[CurrentWorkspace.Guid] = obj;
            }
        }

        private void OnNodeAdded(Graph.Nodes.NodeModel obj)
        {
            if (!NotificationTracker.ContainsKey(CurrentWorkspace.Guid))
            {
                if (GraphPythonDependencies.IsCPythonNode(obj))
                {
                    LogCPython3Notification();
                }
                if (GraphPythonDependencies.IsIronPythonNode(obj))
                {
                    LogIronPythonNotification();
                }
            }

            // Custom Node definition added
            if (obj is Dynamo.Graph.Nodes.CustomNodes.Function customNode)
            {
                var cnm = DynamoViewModel.Model.CustomNodeManager;
                if (cnm != null)
                {
                    var defId = customNode.Definition?.FunctionId ?? Guid.Empty;
                    if (defId != Guid.Empty
                        && cnm.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out ICustomNodeWorkspaceModel defWsModel) == true
                        && defWsModel is WorkspaceModel defWs)
                    {
                        if (upgradeSvc != null)
                        {
                            var usageInside = upgradeSvc.DetectPythonUsage(defWs, IsPythonNode);

                            if (usageInside.DirectPythonNodes.Count() > 0)
                            {
                                // Track this def as temp-migrated for the session
                                tempMigratedCustomDefs.Add(defId);
                                touchedCustomWorkspaces.Add(defWs);

                                var count = upgradeSvc.UpgradeNodesInMemory(
                                    usageInside.DirectPythonNodes,
                                    defWs,
                                    SetEngine);

                                TryShowPythonEngineUpgradeToast(0, count);
                            }
                        }
                    }
                }
            }

            // Cpython Node added directly to the graph
            if (obj is PythonNodeBase pyNode)
            {
                SubscribeToPythonNodeEvents(pyNode);

                if (upgradeSvc != null)
                {
                    if (GraphPythonDependencies.IsCPythonNode(pyNode))
                    {
                        var count = upgradeSvc.UpgradeNodesInMemory(
                            new List<NodeModel> { pyNode },
                            CurrentWorkspace,
                            SetEngine);

                        TryShowPythonEngineUpgradeToast(count, 0);
                    }
                }
            }
        }

        private void OnNodeRemoved(Graph.Nodes.NodeModel obj)
        {
            if (obj is PythonNodeBase pythonNode)
            {
                UnSubscribePythonNodeEvents(pythonNode);
            }
            else if (obj is Dynamo.Graph.Nodes.CustomNodes.Function customNode)
            {
                var cnm = DynamoViewModel?.Model?.CustomNodeManager;
                var defId = customNode.Definition?.FunctionId ?? Guid.Empty;
                if (defId == Guid.Empty || cnm == null) return;

                // Remove upgrade tracking only If this was the last instance of that custom node
                var anyRemaining = CurrentWorkspace?.Nodes
                    .OfType<Dynamo.Graph.Nodes.CustomNodes.Function>()
                    .Any(f => (f.Definition?.FunctionId ?? Guid.Empty) == defId) == true;

                if (!anyRemaining
                    && cnm.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out ICustomNodeWorkspaceModel defWsModel) == true
                    && defWsModel is WorkspaceModel defWs)
                {
                    touchedCustomWorkspaces.Remove(defWs);
                }
            }
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            UnSubscribeWorkspaceEvents();

            // Reset autorun toggle used to block first eval while upgrading
            autoRunTemporarilyDisabled = false;

            var previous = CurrentWorkspace;
            CurrentWorkspace = workspace as WorkspaceModel;
            PythonDependencies.UpdateWorkspace(CurrentWorkspace);

            SubscribeToWorkspaceEvents();

            // Fresh per-workspace notification state
            CurrentWorkspace.HasShownCPythonNotification = false;

            // Close any upgrade toast when switching tabs/workspaces.
            DynamoViewModel.MainGuideManager?.CloseRealTimeInfoWindow();

            if (previous != null)
            {
                NotificationTracker.Remove(previous.Guid);
            }
            GraphPythonDependencies.CustomNodePythonDependencyMap.Clear();

            if (CurrentWorkspace is HomeWorkspaceModel hws)
            {
                if (lastWorkspaceGuid != hws.Guid)
                {
                    // Track last home workspace to avoid duplicate work on the same one
                    lastWorkspaceGuid = hws.Guid;

                    // If opening in Automatic, switch to Manual before upgrading to avoid mutating during eval
                    if (hws.RunSettings.RunType == RunType.Automatic)
                    {
                        hws.RunSettings.RunType = RunType.Manual;
                        autoRunTemporarilyDisabled = true;
                    }

                    //RecomputeCPython3NotificationForWorkspace();

                    // Restore Automatic and trigger initial run after upgrades
                    if (autoRunTemporarilyDisabled)
                    {
                        hws.RunSettings.RunType = RunType.Automatic;

                        //hws.RequestRun();
                        var nodes = hws.Nodes.ToList();
                        hws.MarkNodesAsModifiedAndRequestRun(nodes, forceExecute: true);

                        autoRunTemporarilyDisabled = false;
                    }
                }
            }
            else if (CurrentWorkspace is ICustomNodeWorkspaceModel cws)
            {
                var defId = cws.CustomNodeDefinition?.FunctionId ?? Guid.Empty;
                if (defId != Guid.Empty)
                {
                    // Custom node tab opened. Handle three cases:
                    // A) Already permanently migrated in this session → no notification
                    // B) Temp-migrated earlier via a host graph → show one-time toast in this tab
                    // C) Opened directly from file and not processed yet → recompute/upgrade now
                    if (permMigratedCustomDefs.Contains(defId))
                    {
                        // no notificarions needed
                    }
                    else if (tempMigratedCustomDefs.Contains(defId))
                    {
                        var preferenceSettings = DynamoViewModel?.Model?.PreferenceSettings;
                        if (!customToastShownDef.Contains(defId) || !preferenceSettings.HideCPython3Notifications)
                        {
                            var pyNodes = CurrentWorkspace.Nodes.OfType<PythonNodeBase>().ToList();
                            TryShowPythonEngineUpgradeToast(pyNodes.Count, 0);
                        }
                    }
                    else
                    {
                        RecomputeCPython3NotificationForWorkspace();
                    }
                }
            }

            CurrentWorkspace.Nodes
                    .Where(x => x is PythonNodeBase)
                    .ToList()
                    .ForEach(x => SubscribeToPythonNodeEvents(x as PythonNodeBase));

            //if (saveBackup)
            //{
            //    SavePythonMigrationBackup();

            //}
        }

        private void OnCurrentWorkspaceCleared(IWorkspaceModel workspace)
        {
            // Close the CPython toast notification when workspace is cleared/closed
            DynamoViewModel.MainGuideManager?.CloseRealTimeInfoWindow();
            lastWorkspaceGuid = Guid.Empty;
        }

        private void OnCurrentWorkspaceSaved()
        {
            // If we are in a Custom Node workspace, remove this definition from tracking
            if (CurrentWorkspace is CustomNodeWorkspaceModel cws)
            {
                touchedCustomWorkspaces.Remove(cws);
                tempMigratedCustomDefs.Remove(cws.CustomNodeId);
                return;
            }

            CommitCustomNodeMigrationsOnSave();

            // Show the notification only once
            // CurrentWorkspace.HasShownCPythonNotification = true;
            touchedCustomWorkspaces.Clear();
        }

        private void SubscribeToDynamoEvents()
        {
            LoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            LoadedParams.CurrentWorkspaceCleared += OnCurrentWorkspaceCleared;
            DynamoViewModel.Model.Logger.NotificationLogged += OnNotificationLogged;
            SubscribeToWorkspaceEvents();
        }

        private void SubscribeToWorkspaceEvents()
        {
            CurrentWorkspace.NodeAdded += OnNodeAdded;
            CurrentWorkspace.NodeRemoved += OnNodeRemoved;
            CurrentWorkspace.Saved += OnCurrentWorkspaceSaved;
            CurrentWorkspace.RequestPythonEngineMapping += PythonDependencies.GetPythonEngineMapping;
        }

        private void SubscribeToPythonNodeEvents(PythonNodeBase node)
        {
            node.MigrationAssistantRequested += OnMigrationAssistantRequested;
        }

        private void UnSubscribePythonNodeEvents(PythonNodeBase node)
        {
            node.MigrationAssistantRequested -= OnMigrationAssistantRequested;
        }

        private void UnSubscribeWorkspaceEvents()
        {
            if (CurrentWorkspace != null)
            {
                CurrentWorkspace.RequestPythonEngineMapping -= PythonDependencies.GetPythonEngineMapping;
                CurrentWorkspace.NodeAdded -= OnNodeAdded;
                CurrentWorkspace.NodeRemoved -= OnNodeRemoved;
                CurrentWorkspace.Saved -= OnCurrentWorkspaceSaved;
                CurrentWorkspace.Nodes
                    .Where(n => n is PythonNode)
                    .Cast<PythonNode>()
                    .ToList()
                    .ForEach(n => UnSubscribePythonNodeEvents(n));
            }
        }

        private void UnsubscribeEvents()
        {
            if (LoadedParams != null)
            {
                LoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
                LoadedParams.CurrentWorkspaceCleared -= OnCurrentWorkspaceCleared;
                DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded -= OnNodeAdded;
                DynamoViewModel.CurrentSpaceViewModel.Model.NodeRemoved -= OnNodeRemoved;
                DynamoViewModel.Model.Logger.NotificationLogged -= OnNotificationLogged;
            }
            
            if (CurrentWorkspace  != null)
            {
                CurrentWorkspace.RequestPythonEngineMapping -= PythonDependencies.GetPythonEngineMapping;
            }

            if (enginesSubscribed)
            {
                PythonEngineManager.Instance.AvailableEngines.CollectionChanged -= AvailableEngines_CollectionChanged;
                enginesSubscribed = false;
            }

            UnSubscribeWorkspaceEvents();
        }

        #endregion

        #region Recompute Notifications        

        /// <summary>
        /// Scans the current workspace for CPython usage (direct and in one-layer custom nodes),
        /// shows a toast if needed, and performs temp upgrades.
        /// </summary>
        private void RecomputeCPython3NotificationForWorkspace()
        {
            if (CurrentWorkspace == null) return;

            // Exit early if CPython engine is available
            var preferenceSettings = DynamoViewModel?.Model?.PreferenceSettings;
            if (preferenceSettings == null || hasCPython3Engine)
            {
                CurrentWorkspace.ShowCPythonNotifications = false;
                return;
            }

            // If we are inside a custom node and it is already PythonNet3,                             // THAT NEEDS TO MAKE IT RO THE NEW CUSTOM CLASS
            // clear any stale auto-upgrade banners left from a cached definition
            if (CurrentWorkspace is CustomNodeWorkspaceModel)
            {
                foreach (var py in CurrentWorkspace.Nodes.OfType<PythonNode>()
                             .Where(n => n.EngineName == PythonEngineManager.PythonNet3EngineName))
                {
                    py.ShowAutoUpgradedBar = false;
                }
            }

            var cnManager = DynamoViewModel?.Model?.CustomNodeManager;

            // Detect any direct CPythonNodes and custom node definitions with CPython nodes
            var usage = upgradeSvc.DetectPythonUsage(CurrentWorkspace, IsPythonNode);

            // Migrate the direct CPython nodes in memory
            if (usage.DirectPythonNodes.Count() > 0)
            {
                upgradeSvc.UpgradeNodesInMemory(
                    usage.DirectPythonNodes,
                    CurrentWorkspace,
                    SetEngine);
            }

            // Prepare custom node definitions that contain CPython nodes
            foreach (var defId in usage.CustomNodeDefIdsWithPython)
            {
                

                WorkspaceModel defWs = null;

                // Open the custom node worskspace to perform in-memory upgrade and log it for later save
                if (cnManager != null
                    && cnManager.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out CustomNodeWorkspaceModel defWsModel)
                    && defWsModel is WorkspaceModel workspace)
                {
                    
                    var inner = upgradeSvc.DetectPythonUsage(workspace, IsPythonNode);

                    if (inner.DirectPythonNodes.Count() > 0)
                    {
                        tempMigratedCustomDefs.Add(defId);
                        touchedCustomWorkspaces.Add(workspace);

                        upgradeSvc.UpgradeNodesInMemory(
                        inner.DirectPythonNodes,
                        workspace,
                        SetEngine);
                    }
                }
            }

            // Show notification if any upgrades were made
            if (usage.DirectPythonNodes.Count() > 0 || usage.CustomNodeDefIdsWithPython.Count() > 0)
            {
                var backupPath = GetPythonMigrationBackupFilePath(CurrentWorkspace);

                TryShowPythonEngineUpgradeToast(
                    usage.DirectPythonNodes.Count(),
                    usage.CustomNodeDefIdsWithPython.Count(),
                    backupPath);
            }
        }

        private static bool IsPythonNode(NodeModel n) => n is PythonNodeBase;

        private static void SetEngine(NodeModel node, WorkspaceModel workspace)
        {
            if (node is PythonNodeBase pyNode && pyNode.EngineName == PythonEngineManager.CPython3EngineName)
            {
                pyNode.EngineName = PythonEngineManager.PythonNet3EngineName;
                pyNode.OnNodeModified();

                // If we are inside a custom node and it is already PythonNet3,
                // clear any stale auto-upgrade banners left from a cached definition
                if (workspace is CustomNodeWorkspaceModel && pyNode is PythonNode p)
                {
                    p.ShowAutoUpgradedBar = false;                              // NOT SURE ABUTE THAT !?! WOULD IT WORK IF WE OPEN CUSTOM NODE DIRECTLY??
                }
            }
        }

        #endregion

        #region Notification Toast

        private void TryShowPythonEngineUpgradeToast(int cpythonNodeCount, int customDefCount, string backupPath = "")
        {
            var prefs = DynamoViewModel?.Model?.PreferenceSettings;
            if (prefs != null && prefs.HideCPython3Notifications) return;

            CurrentWorkspace.ShowCPythonNotifications = true;
            ShowPythonEngineUpgradeToast(cpythonNodeCount, customDefCount, backupPath);
        }

        /// <summary>
        /// Shows a single canvas toast covering:
        ///  - direct CPython nodes converted to PythonNet3, and
        ///  - custom node definitions temporarily converted (save to persist)
        /// Uses your existing resource strings for each part and concatenates them.
        /// </summary>
        private void ShowPythonEngineUpgradeToast(int cpythonNodeCount, int customDefCount, string backupPath = "")
        {
            if (cpythonNodeCount < 1 && customDefCount < 1) return;

            string combined = BuildCombinedUpgradeLine(cpythonNodeCount, customDefCount);

            string bachuptext = string.Empty;
            if (backupPath != "")
            {
                bachuptext = string.Format(Resources.CPythonMigrationBackupFileCreatedMessage, MakeWrapFriendlyPath(backupPath));
            }

            var parts = new[] { combined, bachuptext }.Where(s => !string.IsNullOrWhiteSpace(s));
            var msg = string.Join("\n\n", parts);

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => DynamoViewModel.ShowPythonEngineUpgradeCanvasToast(msg, stayOpen: true)));
        }

        /// <summary>
        /// Inserts a zero-width space U+200B after common separators so WPF can wrap
        /// </summary>
        private static string MakeWrapFriendlyPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            
            return path
                .Replace("\\", "\\\u200B")
                .Replace("/", "/\u200B")
                .Replace(".", ".\u200B")
                .Replace("_", "_\u200B")
                .Replace("-", "-\u200B");
        }

        private static string BuildCombinedUpgradeLine(int cpythonNodeCount, int customDefCount)
        {
            // nothing to say
            if (cpythonNodeCount < 1 && customDefCount < 1) return string.Empty;

            // piece 1: "N CPython node(s)"
            string cpyLabel = (cpythonNodeCount == 1)
                ? Dynamo.PythonMigration.Properties.Resources.CPythonNodesLabelSingular
                : Dynamo.PythonMigration.Properties.Resources.CPythonNodesLabelPlural;

            string part1 = (cpythonNodeCount > 0)
                ? $"{cpythonNodeCount} {cpyLabel}"
                : string.Empty;

            // piece 2: "N custom node definition(s)"
            string cnLabel = (customDefCount == 1)
                ? Dynamo.PythonMigration.Properties.Resources.CustomNodeDefsLabelSingular
                : Dynamo.PythonMigration.Properties.Resources.CustomNodeDefsLabelPlural;

            string part2 = (customDefCount > 0)
                ? $"{customDefCount} {cnLabel}"
                : string.Empty;

            string subject = (part1.Length > 0 && part2.Length > 0)
                ? $"{part1} {Dynamo.PythonMigration.Properties.Resources.AndConjunction} {part2}"
                : (part1.Length > 0 ? part1 : part2);

            // finish with a single, reusable suffix
            return $"{subject} {Dynamo.PythonMigration.Properties.Resources.CombinedUpgradeToastSuffix}";
        }

        #endregion

        #region Permanent Custom Node Upgrade
        /// <summary>
        /// Commit custom node migrations on save: overwrite .dyf files for any definitions
        /// we temporarily upgraded this session, and mark them as permanent.
        /// </summary>
        private void CommitCustomNodeMigrationsOnSave()
        {
            foreach (var workspace in touchedCustomWorkspaces.ToList())
            {
                try
                {
                    if (!TryGetCustomIdAndPath(workspace, out var defId, out var dyfPath) || string.IsNullOrEmpty(dyfPath)) continue;

                    var path = GetWorkspaceFilePath(workspace);
                    if (string.IsNullOrEmpty(path)) continue;

                    if (workspace is Dynamo.Graph.Workspaces.CustomNodeWorkspaceModel customWorkspace)
                    {
                        customWorkspace.IsVisibleInDynamoLibrary = true;
                    }

                    // Overwrite the .dyf files
                    workspace.Save(path, false, DynamoViewModel?.Model?.EngineController);
                    EnsureDyfHasLibraryViewFlag(dyfPath);

                    permMigratedCustomDefs.Add(defId);
                    tempMigratedCustomDefs.Remove(defId);
                }
                catch { }
            }
        }

        /// <summary>
        /// Returns the on-disk file path for the given workspace
        /// </summary>
        private string GetWorkspaceFilePath(WorkspaceModel ws)
        {
            if (ws == null) return null;

            var path = ws.FileName;
            if (!string.IsNullOrEmpty(path)) return path;

            return null;
        }

        /// <summary>
        /// Outputs the custom node definition ID and its .dyf path for a custom workspace, returning true if the workspace is a custom node
        /// </summary>
        private bool TryGetCustomIdAndPath(WorkspaceModel ws, out Guid defId, out string dyfPath)
        {
            defId = Guid.Empty;
            dyfPath = null;

            if (ws is Dynamo.Graph.Workspaces.CustomNodeWorkspaceModel cws)
            {
                defId = cws.CustomNodeId;

                var cnm = DynamoViewModel?.Model?.CustomNodeManager;
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

        /// <summary>
        /// Ensures the .dyf JSON sets View.Dynamo.IsVisibleInDynamoLibrary = true and writes the file back if needed
        /// </summary>
        private void EnsureDyfHasLibraryViewFlag(string dyfPath)    //
        {
            if (string.IsNullOrEmpty(dyfPath) || !File.Exists(dyfPath)) return;

            var json = File.ReadAllText(dyfPath);
            var root = JObject.Parse(json);

            var view = (JObject?)root["View"] ?? new JObject();
            var dyn = (JObject?)view["Dynamo"] ?? new JObject();

            dyn["IsVisibleInDynamoLibrary"] = true;
            view["Dynamo"] = dyn;
            root["View"] = view;

            File.WriteAllText(dyfPath, root.ToString(Formatting.Indented));
        }
        #endregion

        /// <summary>
        /// Creates a one-time backup of the current workspace and returns the path.
        /// </summary>
        private void SavePythonMigrationBackup()
        {
            var workspace = CurrentWorkspace;
            var path = GetPythonMigrationBackupFilePath(workspace);

            workspace.Save(path, true);
        }

        private string GetPythonMigrationBackupFilePath(WorkspaceModel workspace)
        {
            var backupDir = LoadedParams?.StartupParams?.PathManager?.BackupDirectory;
            var backupExtensionToken = Properties.Resources.CPythonMigrationBackupExtension;

            if (workspace == null || backupDir == null) return null;
            if (Models.DynamoModel.IsTestMode) return null;

            var extension = workspace is CustomNodeWorkspaceModel ? ".dyf" : ".dyn";
            var timeStamp = DateTime.Now.ToString("yyyyMMdd'T'HHmmss");
            var fileName = string.Concat(workspace.Name, ".", backupExtensionToken, ".", timeStamp, extension);
            var path = Path.Combine(backupDir, fileName);

            return path;
        }

        private void InitEngineFlagAndSubscribe()
        {
            RecomputeEngineFlags();

            if (!enginesSubscribed)
            {
                PythonEngineManager.Instance.AvailableEngines.CollectionChanged += AvailableEngines_CollectionChanged;
                enginesSubscribed = true;
            }
        }

        private void AvailableEngines_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RecomputeEngineFlags();
            RecomputeCPython3NotificationForWorkspace();
        }

        /// <summary>
        /// Checks if we have CPython3 and PythonNet3 engines installed
        /// </summary>
        private void RecomputeEngineFlags()
        {
            hasCPython3Engine = PythonEngineManager.Instance.AvailableEngines
                .Any(e => e.Name == PythonEngineManager.CPython3EngineName);

            hasPythonNet3Engine = PythonEngineManager.Instance.AvailableEngines
                .Any(e => e.Name == PythonEngineManager.PythonNet3EngineName);
        }
    }  
}
