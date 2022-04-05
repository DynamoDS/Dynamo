using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Dynamo.Core;
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
        private ObservableCollection<FooterNotificationItem> footerNotificationItems;

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

        /// <summary>
        /// Contains all footer notification item bindings
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<FooterNotificationItem> FooterNotificationItems
        {
            get { return footerNotificationItems; }
            set
            {
                if (footerNotificationItems == value) return;
                footerNotificationItems = value;
                RaisePropertyChanged(nameof(FooterNotificationItems));
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

            SetupFooterNotificationItems();
        }

        /// <summary>
        /// Setup the initial collection of FooterNotificationItems
        /// </summary>
        private void SetupFooterNotificationItems()
        {
            FooterNotificationItem [] footerItems = new FooterNotificationItem[2]; //TODO : change to 3 when Info is implemented

            footerItems[0] = new FooterNotificationItem() { NotificationCount = 0, NotificationImage = "/DynamoCoreWpf;component/UI/Images/error.png" };
            footerItems[1] = new FooterNotificationItem() { NotificationCount = 0, NotificationImage = "/DynamoCoreWpf;component/UI/Images/warning_16px.png" };

            footerNotificationItems = new ObservableCollection<FooterNotificationItem>(footerItems);
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
            // Using Nodes here is not thread safe, as nodes can be added/removed by the UI thread midway.
            // Dispatching this to the UI thread would help to avoid concurrency issues but has caveats.
            // When Dynamo is shutting down, a deadlock situation can occur, where each thread waits on the other.
            // Moreover, tight periodic runs can create situations where we cannot safely check if we are shutting down.
            // In summary, even if locking may be more costly, it is the safer approach.
            lock (Nodes)
            {
                UpdateNodesDeltaState(e.NodeGuidList, e.GraphExecuted);
            }
        }

        private void UpdateNodesDeltaState(List<Guid> nodeGuids, bool graphExecuted)
        {
            // if runsettings is manual, and if the graph is not executed, then turing on showrunpreview 
            //should turn on showexectionpreview on every node.
            if (nodeGuids.Count == 0 && !graphExecuted)
            {
                foreach (var nodeModel in Nodes)
                {
                    nodeModel.ShowExecutionPreview = DynamoViewModel.ShowRunPreview;
                }
            }

            //if the graph is executed then set the node preview to false , provided
            // there is no error on that node.
            if (nodeGuids.Count == 0 && graphExecuted)
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
            if (DynamoViewModel.UIDispatcher != null)
            {
                DynamoViewModel.UIDispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateNodeInfoBubbleContent(e);
                }));
            }
            else
            {
                //just call it directly 
                UpdateNodeInfoBubbleContent(e);
            }
        
            bool hasWarnings = Model.Nodes.Any(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning);
            bool hasErrors = Model.Nodes.Any(n => n.State == ElementState.Error);

            if (!hasWarnings && !hasErrors)
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
            else if(hasWarnings && !hasErrors)
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
            else
            {
                if (Model.ScaleFactorChanged)
                {
                    SetCurrentWarning(NotificationLevel.Error, Properties.Resources.RunCompletedWithScaleChangeAndErrorsMessage);
                }
                else
                {
                    SetCurrentWarning(NotificationLevel.Error, Properties.Resources.RunCompletedWithErrorsMessage);
                }
            }

            UpdateFooterItems(hasWarnings, hasErrors);
        }

        /// <summary>
        /// Updates the Info, Warning and Error footer notification items
        /// </summary>
        /// <param name="hasWarnings"></param>
        /// <param name="hasErrors"></param>
        private void UpdateFooterItems(bool hasWarnings, bool hasErrors)
        {
            if (hasErrors)
                FooterNotificationItems[0].NotificationCount = Model.Nodes.Count(n => n.State == ElementState.Error);
            else
                if(FooterNotificationItems[0].NotificationCount != 0) FooterNotificationItems[0].NotificationCount = 0;
            if (hasWarnings)
                FooterNotificationItems[1].NotificationCount = Model.Nodes.Count(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning);
            else
                if (FooterNotificationItems[1].NotificationCount != 0) FooterNotificationItems[1].NotificationCount = 0;
        }

        private void UpdateNodeInfoBubbleContent(EvaluationCompletedEventArgs evalargs)
        {
            if (evalargs.MessageKeys == null) return;

            foreach (var messageID in evalargs.MessageKeys)
            {
                var node = Nodes.FirstOrDefault(n => n.Id == messageID);
                if (node == null) continue;

                node.UpdateBubbleContent();
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
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#FAA21B"));
                case NotificationLevel.Error:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#EB5555"));
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Value converter from 0 to Visibility Colapsed
    /// </summary>
    public class ZeroToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((int)value == 0)            
                return System.Windows.Visibility.Collapsed;
            return System.Windows.Visibility.Visible;            
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An object that contains information about the number of 
    /// Info, Warning or Error Nodes after a Run 
    /// </summary>
    public class FooterNotificationItem : NotificationObject
    {
        /// <summary>
        /// The number of Warnings, Errors or Info Nodes
        /// </summary>
        public int NotificationCount { get; set; }
        /// <summary>
        /// The glyph assocaited with this footer item
        /// </summary>
        public string NotificationImage { get; set; }
    }
}
