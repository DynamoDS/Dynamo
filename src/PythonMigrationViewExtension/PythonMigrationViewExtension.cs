using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PythonMigration.Properties;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

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
        internal CustomNodeManager CustomNodeManager { get; set; }
        internal static Uri Python3HelpLink = new Uri(PythonNodeModels.Properties.Resources.PythonMigrationWarningUriString, UriKind.Relative);
        private Dispatcher Dispatcher { get; set; }
        private DynamoView DynamoView { get; set; }

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
            UnsubscribeFromDynamoEvents();
        }


        public void Loaded(ViewLoadedParams p)
        {
            LoadedParams = p;
            PythonDependencies = new GraphPythonDependencies(LoadedParams);
            DynamoViewModel = LoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            CurrentWorkspace = LoadedParams.CurrentWorkspaceModel as WorkspaceModel;
            CustomNodeManager = (CustomNodeManager) LoadedParams.StartupParams.CustomNodeManager;
            CurrentWorkspace.RequestPackageDependencies += PythonDependencies.AddPythonPackageDependency;
            Dispatcher = Dispatcher.CurrentDispatcher;
            DynamoView = LoadedParams.DynamoWindow as DynamoView;
             
            SubscribeToDynamoEvents();
        }

        private void DisplayIronPythonDialog()
        {
            // we only want to create the dialog ones for each graph per Dynamo session
            if (DialogTracker.ContainsKey(CurrentWorkspace.Guid))
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

        internal void OpenPythonMigrationWarningDocumentation()
        {
            LoadedParams.ViewModelCommandExecutive.OpenDocumentationLinkCommand(Python3HelpLink);
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
            //CurrentWorkspace.RequestPackageDependencies -= PythonDependencies.AddPythonPackageDependency;
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
                NotificationTracker.Remove(CurrentWorkspace.Guid);
                GraphPythonDependencies.CustomNodePythonDependencyMap.Clear();
                //if (CurrentWorkspace.RequestPackageDependencies != null)
                //{
                //    CurrentWorkspace.RequestPackageDependencies -= PythonDependencies.AddPythonPackageDependency;
                //    CurrentWorkspace.RequestPackageDependencies = null;
                //}
                CurrentWorkspace = workspace as WorkspaceModel;

                CurrentWorkspace.RequestPackageDependencies += PythonDependencies.AddPythonPackageDependency;

                if (Configuration.DebugModes.IsEnabled("Python2ObsoleteMode")
                    && !Models.DynamoModel.IsTestMode
                    && PythonDependencies.ContainsIronPythonDependencyInCurrentWS(CurrentWorkspace))
                {
                    LogIronPythonNotification();
                    DisplayIronPythonDialog();
                }
            }
        }
        #endregion
    }
}
