using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Core;
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
using PythonNodeModels;

namespace Dynamo.PythonMigration
{
    internal class PythonMigrationViewExtension : IViewExtension
    {
        private const string EXTENSION_NAME = "Python Migration";
        private const string EXTENSION_GUID = "1f8146d0-58b1-4b3c-82b7-34a3fab5ac5d";
        private bool hasCPython3Engine;
        private bool hasPythonNet3Engine;
        private bool enginesSubscribed;
        private bool initialBuildDone;

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

            if (obj is PythonNodeBase pyNode)
            {
                SubscribeToPythonNodeEvents(pyNode);
                RecomputeCPython3NotificationForNode(pyNode);
            }
        }

        private void OnNodeRemoved(Graph.Nodes.NodeModel obj)
        {
            if (!(obj is PythonNodeBase pythonNode))
                return;

            UnSubscribePythonNodeEvents(pythonNode);
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            UnSubscribeWorkspaceEvents();
            initialBuildDone = false;

            CurrentWorkspace = workspace as WorkspaceModel;
            PythonDependencies.UpdateWorkspace(CurrentWorkspace);
            SubscribeToWorkspaceEvents();

            NotificationTracker.Remove(CurrentWorkspace.Guid);
            GraphPythonDependencies.CustomNodePythonDependencyMap.Clear();

            if (CurrentWorkspace is HomeWorkspaceModel hws)
            {
                hws.EvaluationCompleted += OnFirstEvaluationCompleted;
            }
            else if (CurrentWorkspace is ICustomNodeWorkspaceModel)
            {
                initialBuildDone = true;
                RecomputeCPython3NotificationForWorkspace();
                
            }

            CurrentWorkspace.Nodes
                    .Where(x => x is PythonNodeBase)
                    .ToList()
                    .ForEach(x => SubscribeToPythonNodeEvents(x as PythonNodeBase));
        }

        private void OnFirstEvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            // unsubscribe so it runs only once
            if (sender is HomeWorkspaceModel hws)
                hws.EvaluationCompleted -= OnFirstEvaluationCompleted;

            initialBuildDone = true;
            RecomputeCPython3NotificationForWorkspace();
        }

        private void OnCurrentWorkspaceCleared(IWorkspaceModel workspace)
        {
            // Close the CPython toast notification when workspace is cleared/closed
            DynamoViewModel.MainGuideManager?.CloseRealTimeInfoWindow();
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
                CurrentWorkspace.Nodes
                    .Where(n => n is PythonNode)
                    .Cast<PythonNode>()
                    .ToList()
                    .ForEach(n => UnSubscribePythonNodeEvents(n));

                if (CurrentWorkspace is HomeWorkspaceModel hws)
                {
                    hws.EvaluationCompleted -= OnFirstEvaluationCompleted;
                }
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

        private void RecomputeCPython3NotificationForNode(PythonNodeBase pyNode)
        {
            // check if this sis CPython node and we don't have CPython3 engine installed
            if (GraphPythonDependencies.IsCPythonNode(pyNode) && !hasCPython3Engine)
            {
                var preferenceSettings = DynamoViewModel?.Model?.PreferenceSettings;
                if (preferenceSettings != null && !preferenceSettings.HideCPython3Notifications)
                {
                    // Flag that python engine upgrade notice should be shown when the user
                    // saves or closes the workspace, then call the toas and upgrade the nodes
                    CurrentWorkspace.ShowCPythonNotifications = true;
                    ShowPythonEngineUpgradeToast();
                    UpgradeCPython3Nodes(new List<PythonNodeBase> { pyNode });
                }
            }
        }

        private void RecomputeCPython3NotificationForWorkspace()
        {
            if (CurrentWorkspace == null) return;
            if (CurrentWorkspace is HomeWorkspaceModel && !initialBuildDone) return;

            var preferenceSettings = DynamoViewModel?.Model?.PreferenceSettings;
            if (preferenceSettings == null || hasCPython3Engine)
            {
                CurrentWorkspace.ShowCPythonNotifications = false;
                return;
            }

            //var cPy3Nodes = CurrentWorkspace.Nodes
            //    .OfType<PythonNodeBase>()
            //    .Where(n => GraphPythonDependencies.IsCPythonNode(n))
            //    .ToList();
            var cPy3Nodes = FindCPythonNodesIncludingCustoms(CurrentWorkspace);

            if (cPy3Nodes.Count == 0)
            {
                CurrentWorkspace.ShowCPythonNotifications = false;
                return;
            }

            if (preferenceSettings.HideCPython3Notifications)
            {
                CurrentWorkspace.ShowCPythonNotifications = false;
                UpgradeCPython3Nodes(cPy3Nodes);
                return;
            }

            // Flag that python engine upgrade notice should be shown when the user
            // saves or closes the workspace, then call the toas and upgrade the nodes
            CurrentWorkspace.ShowCPythonNotifications = true;
            ShowPythonEngineUpgradeToast(cPy3Nodes.Count);
            UpgradeCPython3Nodes(cPy3Nodes);
        }

        private List<PythonNodeBase> FindCPythonNodesIncludingCustoms(WorkspaceModel root)
        {
            var result = new List<PythonNodeBase>();
            if (root == null) return result;
            var visitedCustomDefs = new HashSet<Guid>();

            void CollectFromWorkspace(WorkspaceModel ws)
            {
                // Add CPython nodes from this workspace
                foreach (var n in ws.Nodes.OfType<PythonNodeBase>())
                {
                    if (n.EngineName == PythonEngineManager.CPython3EngineName)
                        result.Add(n);
                }

                // Traverse any Custom Nodes referenced by this workspace
                foreach (var func in ws.Nodes.OfType<Function>())
                {
                    var defId = func.Definition?.FunctionId ?? Guid.Empty;
                    if (defId == Guid.Empty) continue;
                    if (!visitedCustomDefs.Add(defId)) continue;

                    // Resolve the definition workspace
                    var cnm = DynamoViewModel?.Model?.CustomNodeManager;
                    if (cnm != null && cnm.TryGetFunctionWorkspace(defId, Models.DynamoModel.IsTestMode, out CustomNodeWorkspaceModel defWsModel))
                    {
                        var defWs = defWsModel as WorkspaceModel;
                        if (defWs != null)
                            CollectFromWorkspace(defWs);
                    }
                }
            }

            CollectFromWorkspace(root);
            return result;
        }

        /// <summary>
        /// Shows a canvas toast informing the user that legacy CPython nodes were switched to PythonNet3
        /// </summary>
        private void ShowPythonEngineUpgradeToast(int nodesCount = 1)
        {
            if (nodesCount < 1) return;

            var msg = nodesCount == 1
                ? Resources.CPythonUpgradeToastMessageSingular
                : string.Format(Resources.CPythonUpgradeToastMessagePlural, nodesCount);

            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => DynamoViewModel.ShowPythonEngineUpgradeCanvasToast(msg, stayOpen: true)));
        }

        private void UpgradeCPython3Nodes(List<PythonNodeBase> cPythonNodes)
        {
            if (CurrentWorkspace == null) return;

            var hasPyNet3 = PythonEngineManager.Instance.AvailableEngines
                .Any(e => e.Name == PythonEngineManager.PythonNet3EngineName);
            if (!hasPyNet3) return;

            // Save backup
            PythonMigrationBackup.SavePythonMigrationBackup(
                    CurrentWorkspace,
                    LoadedParams.StartupParams.PathManager.BackupDirectory,
                    Properties.Resources.CPythonMigrationBackupExtension,
                    Properties.Resources.CPythonMigrationBackupFileCreatedMessage
                    );

            foreach (var node in cPythonNodes)
            {
                if (node is PythonNode pyNode)
                {
                    pyNode.ShowAutoUpgradedBar = true;
                }

                node.EngineName = PythonEngineManager.PythonNet3EngineName;
                node.OnNodeModified();
            }
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

    internal static class PythonMigrationBackup
    {
        /// <summary>
        /// Creates a one-time backup of the current workspace and (optionally) shows a message.
        /// </summary>
        internal static void SavePythonMigrationBackup(
            WorkspaceModel workspace,
            string backupDir,
            string backupExtensionToken,
            string messageResource)
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

            var title = Properties.Resources.CPythonMigrationBackupFileCreatedTitle;
            var message = string.Format(messageResource, path);

            // Show the MessageBox after the UI finishes rendering to avoid disrupting connector redraw
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MessageBoxService.Show(message, title, MessageBoxButton.OK, MessageBoxImage.None);
            }), DispatcherPriority.ApplicationIdle);
        }
    }
}
