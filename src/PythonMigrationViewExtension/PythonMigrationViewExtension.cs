﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
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
        internal Dictionary<Guid, IronPythonInfoDialog> DialogTracker = new Dictionary<Guid, IronPythonInfoDialog>();

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
            PythonDependencies = new GraphPythonDependencies(LoadedParams.CurrentWorkspaceModel, LoadedParams.StartupParams.CustomNodeManager);
            DynamoViewModel = LoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            CurrentWorkspace = LoadedParams.CurrentWorkspaceModel as WorkspaceModel;
            CustomNodeManager = (CustomNodeManager)LoadedParams.StartupParams.CustomNodeManager;
            CurrentWorkspace.RequestPackageDependencies += PythonDependencies.AddPythonPackageDependency;
            Dispatcher = Dispatcher.CurrentDispatcher;

            SubscribeToDynamoEvents();
        }

        private void DisplayIronPythonDialog()
        {
            // we only want to create the dialog if the global setting is not disabled and once per Dynamo session, for each graph/custom node
            if (DynamoViewModel.IsIronPythonDialogDisabled || DialogTracker.ContainsKey(CurrentWorkspace.Guid)) return;
            if (CurrentWorkspace is CustomNodeWorkspaceModel && DialogTracker.ContainsKey((CurrentWorkspace as CustomNodeWorkspaceModel).CustomNodeId))
                return;

            var dialog = new IronPythonInfoDialog(this)
            {
                Owner = LoadedParams.DynamoWindow
            };

            Dispatcher.BeginInvoke(new Action(() =>
            {
                dialog.Show();
            }), DispatcherPriority.Background);

            DialogTracker[CurrentWorkspace.Guid] = dialog;
            if (CurrentWorkspace is CustomNodeWorkspaceModel){
                DialogTracker[(CurrentWorkspace as CustomNodeWorkspaceModel).CustomNodeId] = dialog;
            }
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
            if (Configuration.DebugModes.IsEnabled("Python2ObsoleteMode")
                && !NotificationTracker.ContainsKey(CurrentWorkspace.Guid)
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

        private bool IsIronPythonDialogOpen()
        {
            var view = LoadedParams.DynamoWindow.OwnedWindows
               .Cast<Window>()
               .Where(x => x.GetType() == typeof(IronPythonInfoDialog))
               .Select(x => x as IronPythonInfoDialog);

            if (view.Any())
            {
                return true;
            }

            return false;
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            if (!IsIronPythonDialogOpen())
            {
                UnSubscribeWorkspaceEvents();
                CurrentWorkspace = workspace as WorkspaceModel;
                PythonDependencies.UpdateWorkspace(CurrentWorkspace);
                SubscribeToWorkspaceEvents();

                NotificationTracker.Remove(CurrentWorkspace.Guid);
                GraphPythonDependencies.CustomNodePythonDependencyMap.Clear();

                if (Configuration.DebugModes.IsEnabled("Python2ObsoleteMode")
                    && !Models.DynamoModel.IsTestMode
                    && PythonDependencies.CurrentWorkspaceHasIronPythonDependency())
                {
                    LogIronPythonNotification();
                    DisplayIronPythonDialog();
                }
            }

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
            CurrentWorkspace.RequestPackageDependencies += PythonDependencies.AddPythonPackageDependency;
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
            CurrentWorkspace.RequestPackageDependencies -= PythonDependencies.AddPythonPackageDependency;
            CurrentWorkspace.NodeAdded -= OnNodeAdded;
            CurrentWorkspace.NodeRemoved -= OnNodeRemoved;
            CurrentWorkspace.Nodes
                .Where(n => n is PythonNode)
                .Cast<PythonNode>()
                .ToList()
                .ForEach(n => UnSubscribePythonNodeEvents(n));
        }

        private void UnsubscribeEvents()
        {
            LoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded -= OnNodeAdded;
            DynamoViewModel.Model.Logger.NotificationLogged -= OnNotificationLogged;
            CurrentWorkspace.RequestPackageDependencies -= PythonDependencies.AddPythonPackageDependency;
            UnSubscribeWorkspaceEvents();
        }
        #endregion
    }
}
