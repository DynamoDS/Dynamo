using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PackageManager;
using Dynamo.PythonMigration.Controls;
using Dynamo.PythonMigration.MigrationAssistant;
using Dynamo.PythonMigration.Properties;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using PythonNodeModels;

namespace Dynamo.PythonMigration
{
    internal class PythonMigrationViewExtension : IViewExtension
    {
        private const string EXTENSION_NAME = "Python Migration";
        private const string EXTENSION_GUID = "1f8146d0-58b1-4b3c-82b7-34a3fab5ac5d";

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
            if (!NotificationTracker.ContainsKey(CurrentWorkspace.Guid)
                && GraphPythonDependencies.IsIronPythonNode(obj))
            {
                LogIronPythonNotification();
            }

            if (obj is PythonNodeBase)
            {
                SubscribeToPythonNodeEvents(obj as PythonNodeBase);
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
            CurrentWorkspace = workspace as WorkspaceModel;
            PythonDependencies.UpdateWorkspace(CurrentWorkspace);
            SubscribeToWorkspaceEvents();

            NotificationTracker.Remove(CurrentWorkspace.Guid);
            GraphPythonDependencies.CustomNodePythonDependencyMap.Clear();


            CurrentWorkspace.Nodes
                .Where(x => x is PythonNodeBase)
                .ToList()
                .ForEach(x => SubscribeToPythonNodeEvents(x as PythonNodeBase));
        }

        private void SubscribeToDynamoEvents()
        {
            LoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
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
            }
        }

        private void UnsubscribeEvents()
        {
            if (LoadedParams != null)
            {
                LoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
                DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded -= OnNodeAdded;
                DynamoViewModel.Model.Logger.NotificationLogged -= OnNotificationLogged;
            }
            
            if (CurrentWorkspace  != null)
            {
                CurrentWorkspace.RequestPythonEngineMapping -= PythonDependencies.GetPythonEngineMapping;
            }
            UnSubscribeWorkspaceEvents();
        }
        #endregion
    }
}
