using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Newtonsoft.Json;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class HomeWorkspaceViewModel : WorkspaceViewModel
    {
        #region private members

        private NotificationLevel curentNotificationLevel;
        private string currentNotificationMessage;

        #endregion

        #region commands

        [JsonIgnore]
        public DelegateCommand StartPeriodicTimerCommand { get; set; }

        [JsonIgnore]
        public DelegateCommand StopPeriodicTimerCommand { get; set; }

        #endregion

        [JsonIgnore]
        public NotificationLevel CurrentNotificationLevel
        {
            get { return curentNotificationLevel; }
            set
            {
                curentNotificationLevel = value;
                RaisePropertyChanged("CurrentNotificationLevel");
            }
        }

        [JsonIgnore]
        public string CurrentNotificationMessage
        {
            get { return currentNotificationMessage; }
            set
            {
                currentNotificationMessage = value;
                RaisePropertyChanged("CurrentNotificationMessage");
            }
        }

        public HomeWorkspaceViewModel(HomeWorkspaceModel model, DynamoViewModel dynamoViewModel)
            : base(model, dynamoViewModel)
        {
            RunSettingsViewModel = new RunSettingsViewModel(((HomeWorkspaceModel)model).RunSettings, this, dynamoViewModel);
            RunSettingsViewModel.PropertyChanged += RunSettingsViewModel_PropertyChanged;

            StartPeriodicTimerCommand = new DelegateCommand(StartPeriodicTimer, CanStartPeriodicTimer);
            StopPeriodicTimerCommand = new DelegateCommand(StopPeriodicTimer, CanStopPeriodicTimer);

            CheckAndSetPeriodicRunCapability();

            var hwm = (HomeWorkspaceModel)Model;
            hwm.EvaluationStarted += hwm_EvaluationStarted;
            hwm.EvaluationCompleted += hwm_EvaluationCompleted;
            hwm.SetNodeDeltaState +=hwm_SetNodeDeltaState;

            dynamoViewModel.Model.ShutdownStarted += Model_ShutdownStarted;
        }

        void Model_ShutdownStarted(DynamoModel model)
        {
            StopPeriodicTimer(null);
        }

        /// <summary>
        /// When a node is modified, this calls the executing nodes on Homeworkspace
        /// to compute the delta state.
        /// </summary>
        /// <param name="obj">The object.</param>
        public override void OnNodeModified(NodeModel obj)
        {
            if (DynamoViewModel.HomeSpace.RunSettings.RunType == RunType.Manual)
                DynamoViewModel.HomeSpace.GetExecutingNodes(DynamoViewModel.ShowRunPreview);
        }

        private void hwm_SetNodeDeltaState(object sender, DeltaComputeStateEventArgs e)
        {
            var nodeGuids = e.NodeGuidList;
            // if runsettings is manual, and if the graph is not executed, then turing on showrunpreview 
            //should turn on showexectionpreview on every node.
            if (nodeGuids.Count == 0 && !e.GraphExecuted)
            {
                foreach (var nodeModel in Nodes)
                {
                    nodeModel.ShowExecutionPreview = DynamoViewModel.ShowRunPreview;
                }
            }

            //if the graph is executed then set the node preview to false , provided
            // there is no error on that node.
            if (nodeGuids.Count == 0 && e.GraphExecuted)
            {
                foreach (var nodeViewModel in Nodes)
                {
                    if (nodeViewModel.State != ElementState.Error && nodeViewModel.State != ElementState.Warning)
                    {
                        nodeViewModel.ShowExecutionPreview = false;
                        nodeViewModel.IsNodeAddedRecently = false;
                    }
                }                
            }

            foreach (Guid t in nodeGuids)
            {
                var nodeViewModel = Nodes.FirstOrDefault(x => x.NodeModel.GUID == t);
                if (nodeViewModel != null)
                {
                    nodeViewModel.ShowExecutionPreview = nodeViewModel.DynamoViewModel.ShowRunPreview;
                    nodeViewModel.IsNodeAddedRecently = false;
                }
            }

            /* Color the recently added nodes */
            var addedNodes = Nodes.Where(x => x.IsNodeAddedRecently).ToList();
            foreach (var nodes in addedNodes)
            {
                if (nodes.ShowExecutionPreview)
                    nodes.ShowExecutionPreview = nodes.DynamoViewModel.ShowRunPreview;
            }           
        }

        void hwm_EvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            bool hasWarnings = Model.Nodes.Any(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning);

            if (!hasWarnings)
            {
                if (Model.ScaleFactorChanged)
                {
                    SetCurrentWarning(NotificationLevel.Mild, Properties.Resources.RunCompletedWithScaleChangeMessage);
                }
                else
                {
                    SetCurrentWarning(NotificationLevel.Mild, Properties.Resources.RunCompletedMessage);
                }
            }
            else
            {
                if (Model.ScaleFactorChanged)
                {
                    SetCurrentWarning(NotificationLevel.Moderate, Properties.Resources.RunCompletedWithScaleChangeAndWarningsMessage);
                }
                else
                {
                    SetCurrentWarning(NotificationLevel.Moderate, Properties.Resources.RunCompletedWithWarningsMessage);
                }
            }
        }

        void hwm_EvaluationStarted(object sender, EventArgs e)
        {
            if (Model.ScaleFactorChanged)
            {
                SetCurrentWarning(NotificationLevel.Mild, Properties.Resources.RunStartedWithScaleChangeMessage);
            }
            else
            {
                SetCurrentWarning(NotificationLevel.Mild, Properties.Resources.RunStartedMessage);
            }
        }

        private void SetCurrentWarning(NotificationLevel level, string message)
        {
            CurrentNotificationLevel = level;
            CurrentNotificationMessage = message;
        }

        public void ClearWarning()
        {
            CurrentNotificationMessage = string.Empty;
        }

        private void RunSettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If any property changes on the run settings object
            // Raise a property change notification for the RunSettingsViewModel
            // property
            RaisePropertyChanged("RunSettingsViewModel");
        }

        private void StartPeriodicTimer(object parameter)
        {
            var hws = Model as HomeWorkspaceModel;
            if (hws == null)
                return;

            hws.StartPeriodicEvaluation();
        }

        private bool CanStartPeriodicTimer(object parameter)
        {
            return true;
        }

        private void StopPeriodicTimer(object parameter)
        {
            var hws = Model as HomeWorkspaceModel;
            if (hws == null)
                return;

            hws.StopPeriodicEvaluation();
        }

        private bool CanStopPeriodicTimer(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Object dispose function
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            var hwm = (HomeWorkspaceModel)Model;
            hwm.EvaluationStarted -= hwm_EvaluationStarted;
            hwm.EvaluationCompleted -= hwm_EvaluationCompleted;
            hwm.SetNodeDeltaState -= hwm_SetNodeDeltaState;
            RunSettingsViewModel.PropertyChanged -= RunSettingsViewModel_PropertyChanged;
            RunSettingsViewModel.Dispose();
            RunSettingsViewModel = null;
            DynamoViewModel.Model.ShutdownStarted -= Model_ShutdownStarted;
        }
    }

    public enum NotificationLevel { Mild, Moderate, Error }

    public class NotificationLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (NotificationLevel)value;
            switch (level)
            {
                case NotificationLevel.Mild:
                    return new SolidColorBrush(Colors.Gray);
                case NotificationLevel.Moderate:
                    return new SolidColorBrush(Colors.Gold);
                case NotificationLevel.Error:
                    return new SolidColorBrush(Colors.Tomato);
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
