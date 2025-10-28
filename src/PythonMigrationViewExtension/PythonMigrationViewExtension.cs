using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Graph;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PythonMigration.Controls;
using Dynamo.PythonMigration.MigrationAssistant;
using Dynamo.PythonMigration.Properties;
using Dynamo.PythonServices;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PythonNodeModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
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


        // NEW STRATEGY
        private readonly HashSet<Guid> tempMigratedCustomDefs = new HashSet<Guid>();                        // defId -> initial CP count seen
        private readonly HashSet<Guid> permMigratedCustomDefs = new HashSet<Guid>();                        // permanently migrated defs this session
        private readonly HashSet<WorkspaceModel> touchedCustomWorkspaces = new HashSet<WorkspaceModel>();   // custom workspaces to save
        private readonly HashSet<Guid> customToastShownDef = new HashSet<Guid>();                           // track which custom node have already been shown the toast in this session  // DO WE NEED THIS??

        private Guid lastWorkspaceGuid = Guid.Empty;


        // SUSPEND AUTOMATIC EXECUTION UTIL THE NODES ARE PROCESSED
        private bool autoRunTemporarilyDisabled = false;


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

            // ------------------------------------------------------------------------
            // CUSTOM NODE INSTANCE ADDED
            // ------------------------------------------------------------------------
            if (obj is Dynamo.Graph.Nodes.CustomNodes.Function funcNode)
            {
                var cnm = DynamoViewModel.Model.CustomNodeManager;
                var defId = funcNode.Definition?.FunctionId ?? Guid.Empty;

                if (defId != Guid.Empty
                    && cnm.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out ICustomNodeWorkspaceModel defWsModel) == true
                    && defWsModel is WorkspaceModel defWs)
                {
                    var nestedCP = defWs.Nodes
                        .OfType<PythonNodeBase>()
                        .Where(n => n.EngineName == PythonEngineManager.CPython3EngineName)
                        .ToList();

                    if (nestedCP.Count > 0)
                    {
                        // Track this def as temp-migrated for the session
                        tempMigratedCustomDefs.Add(defId);
                        touchedCustomWorkspaces.Add(defWs);

                        TempUpgradeCustomNodeDefinitions(new HashSet<Guid> { defId });
                    }
                }
                
                if (tempMigratedCustomDefs.Contains(defId))
                {
                    // still show toast and mark the workspace to show notification on close
                    ShowPythonEngineUpgradeToast(0, 1);
                    CurrentWorkspace.ShowCPythonNotifications = true;
                }
            }

            // ------------------------------------------------------------------------
            // DIRECT PYTHON NODE ADDED
            // ------------------------------------------------------------------------
            if (obj is PythonNodeBase pyNode)
            {
                SubscribeToPythonNodeEvents(pyNode);
                RecomputeCPython3NotificationForNode(pyNode);
            }
        }

        private void OnNodeRemoved(Graph.Nodes.NodeModel obj) // REMOVE THE NODE FROM THE NEW DICTIONARY !!??
        {
            if (!(obj is PythonNodeBase pythonNode))
                return;

            UnSubscribePythonNodeEvents(pythonNode);

            // REMOVE THE NODE FROM THE LIST TO UPGRADE
            if (obj is Dynamo.Graph.Nodes.CustomNodes.Function funcNode)
            {
                var cnm = DynamoViewModel.Model.CustomNodeManager;
                var defId = funcNode.Definition?.FunctionId ?? Guid.Empty;
                if (defId != Guid.Empty
                    && cnm.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out ICustomNodeWorkspaceModel defWsModel) == true
                    && defWsModel is WorkspaceModel defWs)
                {
                    if (touchedCustomWorkspaces.Contains(defWs))
                    {
                        touchedCustomWorkspaces.Remove(defWs);
                    }
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

                    RecomputeCPython3NotificationForWorkspace();

                    // Restore Automatic and trigger initial run after upgrades
                    if (autoRunTemporarilyDisabled)
                    {
                        hws.RunSettings.RunType = RunType.Automatic;
                        hws.RequestRun();
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
                        if (!customToastShownDef.Contains(defId))
                        {
                            var pyNodes = CurrentWorkspace.Nodes.OfType<PythonNodeBase>().ToList();
                            ShowPythonEngineUpgradeToast(pyNodes.Count, 0);
                            CurrentWorkspace.ShowCPythonNotifications = true;
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
        }

        private void OnCurrentWorkspaceCleared(IWorkspaceModel workspace)
        {
            // Close the CPython toast notification when workspace is cleared/closed
            DynamoViewModel.MainGuideManager?.CloseRealTimeInfoWindow();
            lastWorkspaceGuid = Guid.Empty;
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
            CurrentWorkspace.WorkspaceSaving += OnCurrentWorkspaceSaving;
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
                CurrentWorkspace.WorkspaceSaving -= OnCurrentWorkspaceSaving;
                CurrentWorkspace.Saved -= OnCurrentWorkspaceSaved;
                CurrentWorkspace.Nodes
                    .Where(n => n is PythonNode)
                    .Cast<PythonNode>()
                    .ToList()
                    .ForEach(n => UnSubscribePythonNodeEvents(n));

                //if (CurrentWorkspace is HomeWorkspaceModel hws)
                //{
                //    hws.EvaluationCompleted -= OnFirstEvaluationCompleted;
                //}
            }
        }

        private void UnsubscribeEvents()
        {
            if (LoadedParams != null)
            {
                LoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
                LoadedParams.CurrentWorkspaceCleared -= OnCurrentWorkspaceCleared;
                DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded -= OnNodeAdded;
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

        /// <summary>
        /// When a single Python node is CPython and CPython3 engine is unavailable,
        /// temp-upgrade it and let the workspace recompute handle the toast.
        /// </summary>
        private void RecomputeCPython3NotificationForNode(PythonNodeBase pyNode)
        {
            if (pyNode == null || CurrentWorkspace == null) return;

            // If this is a CPython node and the legacy CPython3 engine is not installed
            if (GraphPythonDependencies.IsCPythonNode(pyNode) && !hasCPython3Engine)
            {
                TempUpgradeDirectCPythonNodes(new List<PythonNodeBase> { pyNode });

                var preferenceSettings = DynamoViewModel?.Model?.PreferenceSettings;
                if (preferenceSettings != null && !preferenceSettings.HideCPython3Notifications)
                {
                    // Mark the workspace and let the workspace-level recompute show a toast
                    CurrentWorkspace.ShowCPythonNotifications = true;
                    ShowPythonEngineUpgradeToast(1,0);                    
                }
            }
        }

        /// <summary>
        /// Scans the current workspace for CPython usage (direct and in one-layer custom nodes),
        /// shows a toast if needed, and performs temp upgrades.
        /// </summary>
        private void RecomputeCPython3NotificationForWorkspace()
        {
            if (CurrentWorkspace == null) return;

            var preferenceSettings = DynamoViewModel?.Model?.PreferenceSettings;
            if (preferenceSettings == null || hasCPython3Engine)
            {
                CurrentWorkspace.ShowCPythonNotifications = false;
                return;
            }

            // Log direct CPython nodes on this graph
            var directCPythonNodes = CurrentWorkspace
                .Nodes
                .OfType<PythonNodeBase>()
                .Where(n => n.EngineName == PythonEngineManager.CPython3EngineName)
                .ToList();

            var cnManager = DynamoViewModel?.Model?.CustomNodeManager;

            // Collect custom def IDs
            var customDefIds = CurrentWorkspace.Nodes
                .OfType<Function>()
                .Select(f => f.Definition?.FunctionId ?? Guid.Empty)
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var defsWithCurrentCPInside = new HashSet<Guid>();
            int rememberedTempCount = 0;

            foreach (var defId in customDefIds)
            {
                if (permMigratedCustomDefs.Contains(defId)) continue;

                WorkspaceModel defWs = null;

                // Only attempt to resolve if manager is available
                if (cnManager != null
                    && cnManager.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out CustomNodeWorkspaceModel defWsModel)
                    && defWsModel is WorkspaceModel workspace)
                {
                    defWs = workspace;

                    // If the def currently contains CPython, mark it for temp upgrade now
                    bool hasCPInsideNow = defWs.Nodes
                        .OfType<PythonNodeBase>()
                        .Any(n => n.EngineName == PythonEngineManager.CPython3EngineName);

                    if (hasCPInsideNow)
                    {
                        defsWithCurrentCPInside.Add(defId);
                    }
                }

                // If this def was already temp-migrated earlier in this session, count it
                if (tempMigratedCustomDefs.Contains(defId))
                {
                    rememberedTempCount++;
                    if (defWs != null)
                    {
                        touchedCustomWorkspaces.Add(defWs);
                    }
                }
            }

            int directCount = directCPythonNodes.Count;
            int customCount = defsWithCurrentCPInside.Count + rememberedTempCount;

            if (directCount <= 0 && customCount <= 0)
            {
                CurrentWorkspace.ShowCPythonNotifications = false;
                return;
            }

            if (preferenceSettings.HideCPython3Notifications)
            {
                CurrentWorkspace.ShowCPythonNotifications = false;
                // Perform temp upgrades silently
                TempUpgradeDirectCPythonNodes(directCPythonNodes);
                TempUpgradeCustomNodeDefinitions(defsWithCurrentCPInside);
                return;
            }

            // Show combined toast and perform temp upgrades
            CurrentWorkspace.ShowCPythonNotifications = true;
            ShowPythonEngineUpgradeToast(directCount, customCount);
            TempUpgradeDirectCPythonNodes(directCPythonNodes);
            TempUpgradeCustomNodeDefinitions(defsWithCurrentCPInside);
        }


        #region NOTIFICATION TOAST

        /// <summary>
        /// Shows a single canvas toast covering:
        ///  - direct CPython nodes converted to PythonNet3, and
        ///  - custom node definitions temporarily converted (save to persist)
        /// Uses your existing resource strings for each part and concatenates them.
        /// </summary>
        private void ShowPythonEngineUpgradeToast(int directCount, int customCount)
        {
            if (directCount < 1 && customCount < 1) return;

            string partDirect = string.Empty;
            if (directCount > 0)
            {
                partDirect = (directCount == 1)
                    ? Resources.CPythonUpgradeToastMessageSingular
                    : string.Format(Resources.CPythonUpgradeToastMessagePlural, directCount);
            }

            string partCustom = string.Empty;
            if (customCount > 0)
            {
                partCustom = (customCount == 1)
                    ? Resources.CustomNodeUpgradeToastMessageSingular
                    : string.Format(Resources.CustomNodeUpgradeToastMessagePlural, customCount);
            }

            var parts = new[] { partDirect, partCustom }.Where(s => !string.IsNullOrWhiteSpace(s));
            var msg = string.Join("\n", parts);

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => DynamoViewModel.ShowPythonEngineUpgradeCanvasToast(msg, stayOpen: true)));
        }

        #endregion



        #region TEMPORARY UPGRADES

        /// <summary>
        /// Temp-upgrade direct CPython nodes on the active graph
        /// </summary>
        private void TempUpgradeDirectCPythonNodes(List<PythonNodeBase> directCPythonNodes)
        {
            if (CurrentWorkspace == null) return;
            if (directCPythonNodes == null || directCPythonNodes.Count == 0) return;

            // Convert to (workspace,node) pairs against the active graph
            var pairs = directCPythonNodes
                .Where(n => n != null)
                .Select(n => new PyNodeWithWorkspace(CurrentWorkspace, n))
                .ToList();

            // For direct nodes: allow backup of the active Home workspace
            InternalUpgradePairs(pairs, backupHomeWorkspaces: true);
        }

        /// <summary>
        /// Temp-upgrade custom node definitions; tracks for later save
        /// </summary>
        private void TempUpgradeCustomNodeDefinitions(HashSet<Guid> defsWithCurrentCPInside)
        {
            if (defsWithCurrentCPInside == null || defsWithCurrentCPInside.Count == 0) return;

            var cnManager = DynamoViewModel?.Model?.CustomNodeManager;
            if (cnManager == null) return;

            var pairs = new List<PyNodeWithWorkspace>();

            foreach (var defId in defsWithCurrentCPInside)
            {
                if (!cnManager.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out CustomNodeWorkspaceModel defWsModel))
                    continue;

                if (!(defWsModel is WorkspaceModel defWs)) continue;

                // Gather just the CPython nodes currently inside the definition
                var nestedCP = defWs.Nodes
                    .OfType<PythonNodeBase>()
                    .Where(n => n.EngineName == PythonEngineManager.CPython3EngineName)
                    .ToList();

                if (nestedCP.Count == 0) continue;

                // Convert to pairs for in-memory flip
                pairs.AddRange(nestedCP.Select(n => new PyNodeWithWorkspace(defWs, n)));

                tempMigratedCustomDefs.Add(defId);
                touchedCustomWorkspaces.Add(defWs);
            }

            // For custom defs: never back up; overwrite only on Save
            InternalUpgradePairs(pairs, backupHomeWorkspaces: false);
        }

        /// <summary>
        /// Persist all touched custom definitions: overwrite .dyf, move temp→perm for this session
        /// </summary>
        private void PersistTouchedCustomDefinitionsOnSave()
        {
            // Ensure any referenced defs are touched
            try
            {
                var cnManager = DynamoViewModel?.Model?.CustomNodeManager;
                if (cnManager != null && CurrentWorkspace != null)
                {
                    foreach (var func in CurrentWorkspace.Nodes.OfType<Function>())
                    {
                        var defId = func.Definition?.FunctionId ?? Guid.Empty;
                        if (defId == Guid.Empty) continue;
                        if (!tempMigratedCustomDefs.Contains(defId)) continue;

                        if (cnManager.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out CustomNodeWorkspaceModel defWsModel) &&
                            defWsModel is WorkspaceModel defWs)
                        {
                            touchedCustomWorkspaces.Add(defWs);
                        }
                    }
                }
            }
            catch { }

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
                catch (Exception ex)
                {
                    DynamoViewModel?.Model?.Logger?.Log($"Failed to save custom node '{workspace?.Name}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Flip engines for the given (workspace,node) pairs. Optionally back up Home workspaces
        /// </summary>
        private void InternalUpgradePairs(List<PyNodeWithWorkspace> pairs, bool backupHomeWorkspaces)
        {
            if (CurrentWorkspace == null) return;
            if (pairs == null || pairs.Count == 0) return;

            var hasPyNet3 = PythonEngineManager.Instance.AvailableEngines
                .Any(e => e.Name == PythonEngineManager.PythonNet3EngineName);
            if (!hasPyNet3) return;

            if (pairs == null || pairs.Count == 0) return;

            var pm = LoadedParams?.StartupParams?.PathManager;
            if (pm == null) return;

            var groups = pairs
                .Where(p => p != null && p.Workspace != null && p.Node != null)
                .GroupBy(p => p.Workspace);

            foreach (var wsGroup in groups)
            {
                var workspace = wsGroup.Key;
                if (workspace == null) continue;

                var isCustom = workspace is CustomNodeWorkspaceModel;

                // Only back up when asked *and* it's not a custom definition
                if (backupHomeWorkspaces && !isCustom)
                {
                    bool isActiveWorkspace = workspace.Guid == CurrentWorkspace.Guid;
                    PythonMigrationBackup.SavePythonMigrationBackup(
                        workspace,
                        pm.BackupDirectory,
                        Properties.Resources.CPythonMigrationBackupExtension,
                        Properties.Resources.CPythonMigrationBackupFileCreatedMessage,
                        showMessage: isActiveWorkspace);
                }

                foreach (var p in wsGroup)
                {
                    var node = p.Node;

                    if (node is PythonNode pyNode)
                    {
                        pyNode.ShowAutoUpgradedBar = true;
                    }

                    node.EngineName = PythonEngineManager.PythonNet3EngineName;
                    node.OnNodeModified();
                }

                if (isCustom)
                {
                    touchedCustomWorkspaces.Add(workspace);
                } 
            }
        }

        #endregion




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

        private void OnCurrentWorkspaceSaving(SaveContext ctx)
        {
            if (CurrentWorkspace is not HomeWorkspaceModel) return;
            PersistTouchedCustomDefinitionsOnSave();
        }

        private void OnCurrentWorkspaceSaved()
        {
            touchedCustomWorkspaces.Clear();
        }

        #region PRMANENT UPGRADE HELPERS

        /// <summary>
        /// Returns the on-disk file path for the given workspace (using ws.FileName), or null if unknown
        /// </summary>
        private string GetWorkspaceFilePath(WorkspaceModel ws)  //
        {
            if (ws == null) return null;

            var path = ws.FileName;
            if (!string.IsNullOrEmpty(path)) return path;

            //// Fallback: resolve via CustomNodeManager.NodeInfos using the definition GUID
            //if (ws is Dynamo.Graph.Workspaces.ICustomNodeWorkspaceModel cws)
            //{
            //    var defId = cws.CustomNodeDefinition?.FunctionId ?? Guid.Empty;
            //    if (defId != Guid.Empty)
            //    {
            //        var cnm = DynamoViewModel?.Model?.CustomNodeManager;
            //        if (cnm != null &&
            //            cnm.NodeInfos.TryGetValue(defId, out var info) &&
            //            !string.IsNullOrEmpty(info.Path))
            //        {
            //            return info.Path;
            //        }
            //    }
            //}

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



        #region HELPER CLASS

        /// <summary>
        /// Lightweight pair that binds a <see cref="PythonNodeBase"/> to the <see cref="WorkspaceModel"/> it lives in,
        /// used when upgrading nodes so we know both the node and the workspace to operate on
        /// </summary>
        private sealed class PyNodeWithWorkspace
        {
            public WorkspaceModel Workspace { get; }
            public PythonNodeBase Node { get; }

            public PyNodeWithWorkspace(WorkspaceModel ws, PythonNodeBase node)
            {
                Workspace = ws;
                Node = node;
            }
        }

        #endregion
    }

    internal static class PythonMigrationBackup
    {
        /// <summary>
        /// Creates a one-time backup of the current workspace and (optionally) shows a message.
        /// </summary>
        internal static void SavePythonMigrationBackup(
            WorkspaceModel workspace,
            string backupDir,
            string backupExtensionToken,
            string messageResource,
            bool showMessage = true)
        {
            if (workspace == null || backupDir == null) return;
            if (Models.DynamoModel.IsTestMode) return;

            var extension = workspace is CustomNodeWorkspaceModel ? ".dyf" : ".dyn";
            var timeStamp = DateTime.Now.ToString("yyyyMMdd'T'HHmmss");
            var fileName = string.Concat(workspace.Name, ".", backupExtensionToken, ".", timeStamp, extension);

            // Only create a backup file the first time a migration is performed on this graph/custom node file
            var path = Path.Combine(backupDir, fileName);
            if (File.Exists(path)) return;

            workspace.Save(path, true);

            if (showMessage)
            {
                var title = Properties.Resources.CPythonMigrationBackupFileCreatedHeader;
                var message = string.Format(messageResource, path);

                // Show the MessageBox after the UI finishes rendering to avoid disrupting connector redraw
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBoxService.Show(message, title, MessageBoxButton.OK, MessageBoxImage.None);
                }), DispatcherPriority.ApplicationIdle);
            }            
        }
    }    
}
