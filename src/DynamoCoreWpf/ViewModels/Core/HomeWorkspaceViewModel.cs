using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;

using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class HomeWorkspaceViewModel : WorkspaceViewModel, IDisposable
    {
        #region private members

        private NotificationLevel curentNotificationLevel;
        private string currentNotificationMessage;

        #endregion

        #region commands

        public DelegateCommand StartPeriodicTimerCommand { get; set; }
        public DelegateCommand StopPeriodicTimerCommand { get; set; }

        #endregion

        public NotificationLevel CurrentNotificationLevel
        {
            get { return curentNotificationLevel; }
            set
            {
                curentNotificationLevel = value;
                RaisePropertyChanged("CurrentNotificationLevel");
            }
        }

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
            hwm.RefreshCompleted +=hwm_RefreshCompleted;
            hwm.SetNodeDeltaState +=hwm_SetNodeDeltaState;

            dynamoViewModel.Model.ShutdownStarted += Model_ShutdownStarted;            
        }

        void Model_ShutdownStarted(DynamoModel model)
        {
            StopPeriodicTimer(null);
            StopSpin();
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

        private void hwm_RefreshCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            //Stop the Spinner here. If the nodes have something to render
            //then spinner starts again during Rendering
            StopSpin();       
        }

        void hwm_EvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {           
            bool hasWarnings = Model.Nodes.Any(n => n.State == ElementState.Warning);

            if (!hasWarnings)
            {
                StartSpin(Properties.Resources.EvalCompletedMessage);
            }
            else
            {
                StopSpin();
                SetCurrentWarning(NotificationLevel.Moderate, Properties.Resources.EvalCompletedWithWarningsMessage); 
            }
        }

        void hwm_EvaluationStarted(object sender, EventArgs e)
        {
            StartSpin(Properties.Resources.EvalStartedMessage);            
        }

        private void SetCurrentWarning(NotificationLevel level, string message)
        {
            CurrentNotificationLevel = level;
            CurrentNotificationMessage = message;           
        }

        /// <summary>
        /// Show the background spinner
        /// </summary>
        /// <param name="message">Message to be displayed with the spinner</param>
        internal override void StartSpin(String message = null)
        {
            RunSettingsViewModel.ShowBusyIndicator = true;
            RunSettingsViewModel.ShowRunMessage = message ?? String.Empty;
            SetCurrentWarning(NotificationLevel.Mild, message);        
        }

        /// <summary>
        /// Collapse the background spinner.
        /// </summary>
        /// <param name="message">Message to be displayed with the spinner</param>
        internal override  void StopSpin(String message = null)
        {
            RunSettingsViewModel.ShowBusyIndicator = false;
            if (message != null)
            {
                RunSettingsViewModel.ShowRunMessage = message;
                SetCurrentWarning(NotificationLevel.Mild, message);
            }
            else
            {
                ClearWarning();
            }
        }

        /// <summary>
        /// Toggles the background spinner.
        /// </summary>
        /// <param name="message">Message to be displayed with the spinner.</param>
        internal override void ToggleSpin(String message = null)
        {
            //If the spinner is not running (e.g during a Watch3D Rendering)
            //then start the spin 
            if (!RunSettingsViewModel.ShowBusyIndicator)
            {
                StartSpin(message);                
            }
            else
            {
                //If the spinner is running, then stop and start the spinner again and
                //update the message.
                StopSpin();
                StartSpin(message);               
            }
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

        public void Dispose()
        {
            var hwm = (HomeWorkspaceModel)Model;
            hwm.EvaluationStarted -= hwm_EvaluationStarted;
            hwm.EvaluationCompleted -= hwm_EvaluationCompleted;
            hwm.RefreshCompleted -= hwm_RefreshCompleted;
            hwm.SetNodeDeltaState -= hwm_SetNodeDeltaState;

            DynamoViewModel.Model.ShutdownStarted -= Model_ShutdownStarted;
            StopSpin();
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
