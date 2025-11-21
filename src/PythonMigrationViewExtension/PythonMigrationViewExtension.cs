using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
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
using PythonNodeModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private bool enginesSubscribed;
        private Guid lastWorkspaceGuid = Guid.Empty;
        private PythonEngineUpgradeService upgradeService;

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
            upgradeService = new PythonEngineUpgradeService(DynamoViewModel.Model, LoadedParams.StartupParams.PathManager);

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
                var defId = customNode.Definition?.FunctionId ?? Guid.Empty;
                if (defId != Guid.Empty && upgradeService != null)
                {
                    var workspace = upgradeService.TryGetFunctionWorkspace(DynamoViewModel.Model, defId) as WorkspaceModel;
                    if (workspace != null)
                    {
                        var usageInside = upgradeService.DetectPythonUsage(workspace, IsCPythonNode);

                        if (usageInside.DirectPythonNodes.Any())
                        {
                            // Track this def as temp-migrated for the session
                            upgradeService.TempMigratedCustomDefs.Add(defId);

                            var count = upgradeService.UpgradeNodesInMemory(
                                usageInside.DirectPythonNodes,
                                workspace,
                                SetEngine);

                            ShowPythonEngineUpgradeToast(0, count);
                        }
                    }
                }
            }

            // Cpython Node added directly to the graph
            if (obj is PythonNodeBase pyNode)
            {
                SubscribeToPythonNodeEvents(pyNode);

                if (upgradeService != null)
                {
                    if (GraphPythonDependencies.IsCPythonNode(pyNode))
                    {
                        var count = upgradeService.UpgradeNodesInMemory(
                            new List<NodeModel> { pyNode },
                            CurrentWorkspace,
                            SetEngine);

                        ShowPythonEngineUpgradeToast(count, 0);
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
                var defId = customNode.Definition?.FunctionId ?? Guid.Empty;
                if (defId == Guid.Empty || upgradeService == null) return;

                // Remove tracking if this was the last instance of that custom node
                var anyRemaining = CurrentWorkspace.Nodes
                    .OfType<Dynamo.Graph.Nodes.CustomNodes.Function>()
                    .Any(f => f.Definition?.FunctionId == defId);

                if (!anyRemaining)
                {
                    var workspace = upgradeService.TryGetFunctionWorkspace(DynamoViewModel.Model, defId) as WorkspaceModel;
                    if (workspace == null) return;
                    {
                        upgradeService.CustomToastShownDef.Remove(defId);
                    }
                }
            }
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            UnSubscribeWorkspaceEvents();

            var previous = CurrentWorkspace;
            CurrentWorkspace = workspace as WorkspaceModel;
            PythonDependencies.UpdateWorkspace(CurrentWorkspace);

            SubscribeToWorkspaceEvents();

            CurrentWorkspace.HasShownPythonAutoMigrationNotification = false;
            DynamoViewModel.ToastManager?.CloseRealTimeInfoWindow();

            if (previous != null)
            {
                NotificationTracker.Remove(previous.Guid);
            }
            GraphPythonDependencies.CustomNodePythonDependencyMap.Clear();

            var usage = upgradeService.DetectPythonUsage(CurrentWorkspace, IsCPythonNode);
            bool saveBackup = usage.DirectPythonNodes.Any() || usage.CustomNodeDefIdsWithPython.Any();

            if (!saveBackup)
            {
                CurrentWorkspace.Nodes
                    .Where(x => x is PythonNodeBase)
                    .ToList()
                    .ForEach(x => SubscribeToPythonNodeEvents(x as PythonNodeBase));
                return;
            }

            if (!string.IsNullOrEmpty(CurrentWorkspace.FileName))
            {
                upgradeService.SaveMigrationBackup(
                    CurrentWorkspace,
                    CurrentWorkspace.FileName,
                    PythonEngineManager.CPython3EngineName);
            }            

            if (CurrentWorkspace is HomeWorkspaceModel hws)
            {
                if (DynamoModel.IsTestMode)
                {
                    // In test mode, do not toggle RunType or we’ll break auto-run expectations
                    MigrateCPythonNodesForWorkspace();
                }
                else if (lastWorkspaceGuid != hws.Guid)
                {
                    lastWorkspaceGuid = hws.Guid;

                    // Temporarily switch to Manual to avoid mutating during evaluation
                    var oldRunType = hws.RunSettings.RunType;
                    hws.RunSettings.RunType = RunType.Manual;

                    MigrateCPythonNodesForWorkspace();

                    Dispatcher.BeginInvoke(
                        () => hws.RunSettings.RunType = oldRunType,
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle);
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
                    if (upgradeService.PermMigratedCustomDefs.Contains(defId))
                    {
                        // no notifications needed
                    }
                    else if (upgradeService.TempMigratedCustomDefs.Contains(defId))
                    {
                        if (!upgradeService.CustomToastShownDef.Contains(defId))
                        {
                            var pyNodes = CurrentWorkspace.Nodes.OfType<PythonNodeBase>().ToList();
                            ShowPythonEngineUpgradeToast(pyNodes.Count, 0);
                        }
                    }
                    else
                    {
                        MigrateCPythonNodesForWorkspace();
                    }

                    upgradeService.CustomToastShownDef.Add(defId);
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
            DynamoViewModel.ToastManager?.CloseRealTimeInfoWindow();
            lastWorkspaceGuid = Guid.Empty;
            CurrentWorkspace.ShowPythonAutoMigrationNotifications = false;
        }

        private void OnWorkspaceRemoveStarted(IWorkspaceModel workspace)
        {
            // Ensure that after we close custom node once, when we open it again we can show the toast again
            if (workspace is CustomNodeWorkspaceModel cws)
            {
                var defId = cws.CustomNodeDefinition?.FunctionId ?? Guid.Empty;
                if (defId != Guid.Empty)
                {
                    upgradeService.CustomToastShownDef.Remove(defId);
                }
            }
        }

        private void OnCurrentWorkspaceSaved()
        {
            // If we are in a Custom Node workspace, remove this definition from tracking
            if (CurrentWorkspace is CustomNodeWorkspaceModel cws)
            {
                upgradeService.TempMigratedCustomDefs.Remove(cws.CustomNodeId);
                return;
            }

            upgradeService.CommitCustomNodeMigrationsOnSave(CurrentWorkspace);

            // Show the notification only once
            CurrentWorkspace.HasShownPythonAutoMigrationNotification = true;
        }

        private void SubscribeToDynamoEvents()
        {
            LoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            LoadedParams.CurrentWorkspaceCleared += OnCurrentWorkspaceCleared;
            LoadedParams.CurrentWorkspaceRemoveStarted += OnWorkspaceRemoveStarted;
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
                LoadedParams.CurrentWorkspaceRemoveStarted -= OnWorkspaceRemoveStarted;
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
        /// Detects CPython3 usage in the current workspace, migrates those nodes in memory
        /// to PythonNet3 and updates CPython notification UI.
        /// </summary>
        private void MigrateCPythonNodesForWorkspace()
        {
            if (CurrentWorkspace == null) return;

            var preferenceSettings = DynamoViewModel?.Model?.PreferenceSettings;
            if (preferenceSettings == null)
            {
                if (PythonEngineManager.Instance.HasEngine(PythonEngineManager.PythonNet3EngineName))
                {
                    CurrentWorkspace.ShowPythonAutoMigrationNotifications = false;
                    return;
                }
            }

            // When opening a custom node workspace that is already PythonNet3, clear stale banners
            if (CurrentWorkspace is CustomNodeWorkspaceModel)
            {
                foreach (var py in CurrentWorkspace.Nodes.OfType<PythonNode>()
                             .Where(n => n.EngineName == PythonEngineManager.PythonNet3EngineName))
                {
                    py.ShowAutoUpgradedBar = false;
                }
            }

            var usage = upgradeService.DetectPythonUsage(CurrentWorkspace, IsCPythonNode);

            // Migrate the direct CPython nodes in memory
            if (usage.DirectPythonNodes.Any())
            {
                upgradeService.UpgradeNodesInMemory(
                    usage.DirectPythonNodes,
                    CurrentWorkspace,
                    SetEngine);
            }

            // Prepare custom node definitions that contain CPython nodes
            foreach (var defId in usage.CustomNodeDefIdsWithPython)
            {
                var workspace = upgradeService.TryGetFunctionWorkspace(DynamoViewModel.Model, defId) as WorkspaceModel;
                if (workspace != null)
                {
                    var inner = upgradeService.DetectPythonUsage(workspace, IsCPythonNode);

                    if (inner.DirectPythonNodes.Any())
                    {
                        upgradeService.TempMigratedCustomDefs.Add(defId);
                        upgradeService.UpgradeNodesInMemory(
                        inner.DirectPythonNodes,
                        workspace,
                        SetEngine);
                    }
                }
            }

            var directCount = usage.DirectPythonNodes.Count();
            var customCount = usage.CustomNodeDefIdsWithPython.Count();
            var workspaceModified = directCount > 0 || customCount > 0;

            if (workspaceModified)
            {
                ShowPythonEngineUpgradeToast(
                    directCount,
                    customCount,
                    LoadedParams.StartupParams.PathManager.BackupDirectory);

                CurrentWorkspace.ShowPythonAutoMigrationNotifications = true;
            }
        }

        private static bool IsCPythonNode(NodeModel n)
        {
            if (n is PythonNodeBase pyNode)
            {
                return pyNode.EngineName == PythonEngineManager.CPython3EngineName;
            }
            return false;
        }

        private static void SetEngine(NodeModel node, WorkspaceModel workspace)
        {
            if (node is PythonNodeBase pyNode && pyNode.EngineName == PythonEngineManager.CPython3EngineName)
            {
                pyNode.EngineName = PythonEngineManager.PythonNet3EngineName;
                pyNode.OnNodeModified();

                if (pyNode is PythonNode py)
                {
                    py.ShowAutoUpgradedBar = workspace is HomeWorkspaceModel;
                }
            }
        }

        #endregion

        #region Notification Toast

        /// <summary>
        /// Shows a single canvas toast covering:
        ///  - direct CPython nodes converted to PythonNet3, and
        ///  - custom node definitions temporarily converted (save to persist)
        /// Uses your existing resource strings for each part and concatenates them.
        /// </summary>
        private void ShowPythonEngineUpgradeToast(int cpythonNodeCount, int customDefCount, string backupPath = "")
        {
            if (cpythonNodeCount < 1 && customDefCount < 1) return;

            string combined = string.Format(
                Dynamo.PythonMigration.Properties.Resources.CPythonUpgradeToastMessage,
                cpythonNodeCount,
                customDefCount);

            string backupText = string.Empty;
            if (backupPath != "")
            {
                backupText = Resources.CPythonMigrationBackupFileCreatedMessage;
            }

            var parts = new[] { combined, backupText }.Where(s => !string.IsNullOrWhiteSpace(s));
            var msg = string.Join("\n", parts);

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => DynamoViewModel.ShowPythonEngineUpgradeCanvasToast(msg, stayOpen: true, filePath: backupPath)));
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
            MigrateCPythonNodesForWorkspace();
        }

        /// <summary>
        /// Checks if we have CPython3 and PythonNet3 engines installed
        /// </summary>
        private void RecomputeEngineFlags()
        {
            hasCPython3Engine = PythonEngineManager.Instance.AvailableEngines
                .Any(e => e.Name == PythonEngineManager.CPython3EngineName);
        }
    }
}
