using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PythonMigration.Properties;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
using PythonNodeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dynamo.PythonMigration
{
    public class PythonMigrationViewExtension : IViewExtension
    {
        private const string EXTENSION_NAME = "Python Migration";
        private const string EXTENSION_GUID = "1f8146d0-58b1-4b3c-82b7-34a3fab5ac5d";
        private const string PYTHON_SCRIPT_NODE_TYPE = "PythonScriptNode";

        private ViewLoadedParams LoadedParams { get; set; }
        private DynamoViewModel DynamoViewModel { get; set; }
        private NotificationMessage IronPythonNotification { get; set; }
        private WorkspaceModel CurrentWorkspace { get; set; }

        private Dictionary<Guid, NotificationMessage> NotificationTracker = new Dictionary<Guid, NotificationMessage>();
        private Dictionary<Guid, IronPythonInfoDialog> DialogTracker = new Dictionary<Guid, IronPythonInfoDialog>();

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
            DynamoViewModel = LoadedParams.DynamoWindow.DataContext as DynamoViewModel;
            CurrentWorkspace = LoadedParams.CurrentWorkspaceModel as WorkspaceModel;
            SubscribeToDynamoEvents();

        }

        private void CheckForIronPythonDependencies(WorkspaceModel workspace)
        {
            if (workspace == null)
                return;

            var workspacePythonNodes = workspace.Nodes
                .Where(n => IsPythonNode(n))
                .Select(n => n as PythonNode);

            if (workspacePythonNodes == null)
                return;

            if (workspacePythonNodes.Any(n => n.Engine == PythonEngineVersion.IronPython2))
            {
                LogIronPythonNotification();
                DisplayIronPythonDialog();
            }                  
        }

        private void DisplayIronPythonDialog()
        {
            // we only want to create the dialog ones for each graph per Dynamo session
            if (DialogTracker.ContainsKey(CurrentWorkspace.Guid))
                return;

            string summary = Resources.IronPythonDialogSummary;
            var description = Resources.IronPythonDialogDescription;

            var dialog = new IronPythonInfoDialog();
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
            if (NotificationTracker.ContainsKey(CurrentWorkspace.Guid)
                && IsPythonNode(obj)
                && ((PythonNode)obj).Engine == PythonEngineVersion.IronPython2)
            {
                LogIronPythonNotification();
            }
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel workspace)
        {
            NotificationTracker.Remove(CurrentWorkspace.Guid);
            CurrentWorkspace = workspace as WorkspaceModel;
            CheckForIronPythonDependencies(CurrentWorkspace);
        }

        #endregion

        private static bool IsPythonNode(NodeModel obj)
        {
            return obj.NodeType == PYTHON_SCRIPT_NODE_TYPE || obj.Name == nameof(PythonNodeModels.PythonStringNode);
        }
    }
}
