using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PythonMigration.Properties;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using PythonNodeModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamo.PythonMigration
{
    public class PythonMigrationViewExtension : IViewExtension
    {
        private const string EXTENSION_NAME = "Python Migration";
        private const string EXTENSION_GUID = "1f8146d0-58b1-4b3c-82b7-34a3fab5ac5d";

        private ViewLoadedParams LoadedParams { get; set; }
        internal DynamoViewModel DynamoViewModel { get; set; }
        private NotificationMessage IronPythonNotification { get; set; }
        internal WorkspaceModel CurrentWorkspace { get; set; }
        internal GraphPythonDependencies PythonDependencies { get; set; }

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
        }

        public void Startup(ViewStartupParams p)
        {
        }

        public void Dispose()
        {
            UnsubscribeFromDynamoEvents();
        }


        public void Loaded(ViewLoadedParams p)
        {
            LoadedParams = p;
            PythonDependencies = new GraphPythonDependencies(LoadedParams);
            DynamoViewModel = LoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            CurrentWorkspace = LoadedParams.CurrentWorkspaceModel as WorkspaceModel;
            SubscribeToDynamoEvents();

        }

        private void DisplayIronPythonDialog()
        {
            // we only want to create the dialog ones for each graph per Dynamo session
            if (DialogTracker.ContainsKey(CurrentWorkspace.Guid))
                return;

            string summary = Resources.IronPythonDialogSummary;
            var description = Resources.IronPythonDialogDescription;

            var dialog = new IronPythonInfoDialog(LoadedParams);
            dialog.Title = Resources.IronPythonDialogTitle;
            dialog.SummaryText.Text = summary;
            dialog.DescriptionText.Text = description;
            dialog.Owner = LoadedParams.DynamoWindow;
            DialogTracker[CurrentWorkspace.Guid] = dialog;
            dialog.Show();
        }

        private void LogIronPythonNotification()
        {
            if (NotificationTracker.ContainsKey(CurrentWorkspace.Guid))
                return;

            DynamoViewModel.Model.Logger.LogNotification(
                this.GetType().Name,
                EXTENSION_NAME,
                Resources.IronPythonNotificationShortMessage,
                Resources.IronPythonNotificationDetailedMessage);
        }

        #region Events

        private void SubscribeToDynamoEvents()
        {
            LoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded += OnNodeAdded;
            DynamoViewModel.Model.Logger.NotificationLogged += OnNotificationLogged;
        }
        private void UnsubscribeFromDynamoEvents()
        {
            LoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            DynamoViewModel.CurrentSpaceViewModel.Model.NodeAdded -= OnNodeAdded;
            DynamoViewModel.Model.Logger.NotificationLogged -= OnNotificationLogged;
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
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            NotificationTracker.Remove(CurrentWorkspace.Guid);
            CurrentWorkspace = workspace as WorkspaceModel;
            if (PythonDependencies.ContainsIronPythonDependencies())
            {
                LogIronPythonNotification();
                DisplayIronPythonDialog();
            }
        }

        #endregion
    }
}
