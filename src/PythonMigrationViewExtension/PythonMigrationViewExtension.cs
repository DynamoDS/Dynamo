using Dynamo.Controls;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PythonMigration.Controls;
using Dynamo.PythonMigration.Properties;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using PythonNodeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Dynamo.PythonMigration
{
    internal class PythonMigrationViewExtension : IViewExtension
    {
        private const string EXTENSION_NAME = "Python Migration";
        private const string EXTENSION_GUID = "1f8146d0-58b1-4b3c-82b7-34a3fab5ac5d";

        private ViewLoadedParams LoadedParams { get; set; }
        internal DynamoViewModel DynamoViewModel { get; set; }
        internal WorkspaceModel CurrentWorkspace { get; set; }
        internal GraphPythonDependencies PythonDependencies { get; set; }
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
            Dispose();
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
            PythonDependencies = new GraphPythonDependencies(p.CurrentWorkspaceModel);
            DynamoViewModel = LoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            CurrentWorkspace = LoadedParams.CurrentWorkspaceModel as WorkspaceModel;
            Dispatcher = Dispatcher.CurrentDispatcher;

            SubscribeToDynamoEvents();
        }

        private void DisplayIronPythonDialog()
        {
            // we only want to create the dialog ones for each graph per Dynamo session
            if (DialogTracker.ContainsKey(CurrentWorkspace.Guid))
                return;

            var dialog = new IronPythonInfoDialog(this);
            dialog.Owner = LoadedParams.DynamoWindow;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                dialog.Show();
            }), DispatcherPriority.Background);

            DialogTracker[CurrentWorkspace.Guid] = dialog;
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
            var viewModel = new PythonMigrationAssistantViewModel(node);
            var assistantWindow = new VisualDifferenceViewer(viewModel, parentWindow);
            // show modal window so user cant interact with dynamo while migration assistant is open
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
                && PythonDependencies.IsIronPythonNode(obj))
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
            if (!PythonDependencies.IsIronPythonNode(obj) &&
                !PythonDependencies.IsCPythonNode(obj))
                return;

            PythonDependencies.RemovePythonNode(obj);
            if (!(obj is PythonNodeBase pythonNode))
                return;

            UnSubscribePythonNodeEvents(pythonNode);

        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            SubscribeToWorkspaceEvents();
            NotificationTracker.Remove(CurrentWorkspace.Guid);
            CurrentWorkspace = workspace as WorkspaceModel;
            PythonDependencies = new GraphPythonDependencies(workspace);
            if (Configuration.DebugModes.IsEnabled("Python2ObsoleteMode")
                && !Models.DynamoModel.IsTestMode
                && PythonDependencies.ContainsIronPythonDependencies())
            {
                LogIronPythonNotification();
                DisplayIronPythonDialog();
            }

            if (PythonDependencies.ContainsPythonDependencies())
            {
                PythonDependencies.GetPythonNodes()
                    .ToList()
                    .ForEach(x => SubscribeToPythonNodeEvents(x));
            }
        }

        private void SubscribeToDynamoEvents()
        {
            LoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            DynamoViewModel.Model.Logger.NotificationLogged += OnNotificationLogged;
            SubscribeToWorkspaceEvents();
        }

        private void SubscribeToWorkspaceEvents()
        {
            DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded += OnNodeAdded;
            DynamoViewModel.CurrentSpaceViewModel.Model.NodeRemoved += OnNodeRemoved;
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
            DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded -= OnNodeAdded;
            DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded -= OnNodeRemoved;
        }

        private void UnsubscribeEvents()
        {
            LoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            DynamoViewModel.Model.Logger.NotificationLogged -= OnNotificationLogged;
            UnSubscribeWorkspaceEvents();
            LoadedParams.CurrentWorkspaceModel.Nodes
                .Where(n => n is PythonNode)
                .Cast<PythonNode>()
                .ToList()
                .ForEach(n => UnSubscribePythonNodeEvents(n));

        }
        #endregion
    }
}
