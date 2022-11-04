using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DSCore;
using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class HomeWorkspaceViewModel : WorkspaceViewModel
    {
        #region private members

        private NotificationLevel curentNotificationLevel;
        private string currentNotificationMessage;
        private ObservableCollection<FooterNotificationItem> footerNotificationItems;
        private int notificationsCounter = 0;
        private FooterNotificationItem.FooterNotificationType footerNotificationType;

        #endregion

        #region commands

        [JsonIgnore]
        public DelegateCommand StartPeriodicTimerCommand { get; set; }

        [JsonIgnore]
        public DelegateCommand StopPeriodicTimerCommand { get; set; }

        [JsonIgnore]
        public DelegateCommand SelectIssuesCommand { get; set; }

        #endregion

        #region public members
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
        /// Boolean indicates if home workspace run with errors
        /// </summary>
        public bool HasErrors
        {
            get { return Model.Nodes.Any(n => n.State == ElementState.Error); }
        }

        /// <summary>
        /// Boolean indicates if home workspace is displayed with infos
        /// </summary>
        public bool HasInfos
        {
            get { return Model.Nodes.Any(n => n.State == ElementState.Info); }
        }

        /// <summary>
        /// Boolean indicates if home workspace run with warnings
        /// </summary>
        public bool HasWarnings
        {
            get { return Model.Nodes.Any(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning); }
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
                footerNotificationItems = value;
                RaisePropertyChanged(nameof(FooterNotificationItems));
            }
        }

        #endregion

        public HomeWorkspaceViewModel(HomeWorkspaceModel model, DynamoViewModel dynamoViewModel)
            : base(model, dynamoViewModel)
        {
            RunSettingsViewModel = new RunSettingsViewModel(((HomeWorkspaceModel)model).RunSettings, this, dynamoViewModel);
            RunSettingsViewModel.PropertyChanged += RunSettingsViewModel_PropertyChanged;

            StartPeriodicTimerCommand = new DelegateCommand(StartPeriodicTimer, CanStartPeriodicTimer);
            StopPeriodicTimerCommand = new DelegateCommand(StopPeriodicTimer, CanStopPeriodicTimer);
            SelectIssuesCommand = new DelegateCommand(SelectIssues);

            CheckAndSetPeriodicRunCapability();

            var hwm = (HomeWorkspaceModel)Model;
            hwm.EvaluationStarted += hwm_EvaluationStarted;
            hwm.EvaluationCompleted += hwm_EvaluationCompleted;
            hwm.SetNodeDeltaState +=hwm_SetNodeDeltaState;

            dynamoViewModel.Model.ShutdownStarted += Model_ShutdownStarted;
            dynamoViewModel.PropertyChanged += DynamoViewModel_PropertyChanged;
            SetupFooterNotificationItems();
        }

        private void DynamoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DynamoViewModel.CurrentSpace) && !(sender as DynamoViewModel).ViewingHomespace)
            {
                ClearWarning();
            }
        }

        /// <summary>
        /// Setup the initial collection of FooterNotificationItems
        /// </summary>
        private void SetupFooterNotificationItems()
        {
            CurrentNotificationMessage =
                Properties.Resources.RunReady; // Default value of the notification text block on opening

            FooterNotificationItem[]
                footerItems = new FooterNotificationItem[3]; 
            footerItems[0] = new FooterNotificationItem()
            {
                NotificationCount = 0,
                NotificationImage = Resources.FooterNotificationErrorImage,
                NotificationToolTip = Resources.FooterNotificationErrorTooltip,
                NotificationType = FooterNotificationItem.FooterNotificationType.Error
            };
            footerItems[1] = new FooterNotificationItem()
            {
                NotificationCount = 0,
                NotificationImage = Resources.FooterNotificationWarningImage,
                NotificationToolTip = Resources.FooterNotificationWarningTooltip,
                NotificationType = FooterNotificationItem.FooterNotificationType.Warning
            };
            footerItems[2] = new FooterNotificationItem()
            {
                NotificationCount = 0,
                NotificationImage = Resources.FooterNotificationInfoImage,
                NotificationToolTip = Resources.FooterNotificationInfoTooltip,
                NotificationType = FooterNotificationItem.FooterNotificationType.Information
            };

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

            //if the graph is executed then set the node preview to false, provided
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

            UpdateRunStatusMsgBasedOnStates();
            UpdateFooterItems(HasInfos, HasWarnings, HasErrors);
        }


        /// <summary>
        /// Update run status message based on error/warning/info states
        /// </summary>
        internal void UpdateRunStatusMsgBasedOnStates()
        {
            // Clear run status message if home workspace is not current workspace (custom node workspace)
            if(IsHomeSpace && !IsCurrentSpace)
            {
                SetCurrentWarning(NotificationLevel.Mild, string.Empty);
                return;
            }
            if (RunSettings.ForceBlockRun)
            {
                SetCurrentWarning(NotificationLevel.Moderate, Properties.Resources.RunBlockedMessage);
                return;
            }

            if (!HasWarnings && !HasErrors)
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
            else if (HasWarnings && !HasErrors)
            {
                if (Model.ScaleFactorChanged)
                {
                    SetCurrentWarning(NotificationLevel.Moderate, Properties.Resources.RunCompletedWithScaleChangeAndWarningsMessage);
                }
                // If all nodes with warnings dismissed and graph has no errors, update the run status msg
                else
                {
                    if (Nodes.All(x => x.ErrorBubble?.DismissedMessages.Count() == x.ErrorBubble?.NodeMessages.Count()))
                    {
                        SetCurrentWarning(NotificationLevel.Moderate, Properties.Resources.RunCompletedWithWarningsDismissedMessage);
                    }
                    else
                    {
                        SetCurrentWarning(NotificationLevel.Moderate, Properties.Resources.RunCompletedWithWarningsMessage);
                    }
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
        }

        /// <summary>
        /// Updates the Info, Warning and Error footer notification items
        /// </summary>
        /// <param name="hasInfo"></param>
        /// <param name="hasWarnings"></param>
        /// <param name="hasErrors"></param>
        private void UpdateFooterItems(bool hasInfo, bool hasWarnings, bool hasErrors)
        {
            
            if (hasErrors)
                FooterNotificationItems[0].NotificationCount = Model.Nodes.Count(n => n.State == ElementState.Error);
            else
                if(FooterNotificationItems[0].NotificationCount != 0) FooterNotificationItems[0].NotificationCount = 0;
            if (hasWarnings)
                FooterNotificationItems[1].NotificationCount = Model.Nodes.Count(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning);
            else
                if (FooterNotificationItems[1].NotificationCount != 0) FooterNotificationItems[1].NotificationCount = 0;
            if (hasInfo)
                FooterNotificationItems[2].NotificationCount = Model.Nodes.Count(n => n.State == ElementState.Info);
            else
                if (FooterNotificationItems[2].NotificationCount != 0) FooterNotificationItems[2].NotificationCount = 0;
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
        /// Fit the current workspace view to the current selection
        /// </summary>
        private void SelectIssues(object parameter)
        {
            switch ((FooterNotificationItem.FooterNotificationType)parameter)
            {
                case FooterNotificationItem.FooterNotificationType.Error:
                    var nodes = Model.Nodes.Where(n => n.State == ElementState.Error);
                    FitSelection(nodes, (FooterNotificationItem.FooterNotificationType)parameter);
                    break;
                case FooterNotificationItem.FooterNotificationType.Warning:
                    nodes = Model.Nodes.Where(n => n.State == ElementState.Warning || n.State == ElementState.PersistentWarning);
                    FitSelection(nodes, (FooterNotificationItem.FooterNotificationType)parameter);
                    break;
                case FooterNotificationItem.FooterNotificationType.Information:
                    nodes = Model.Nodes.Where(n => n.State == ElementState.Info);
                    FitSelection(nodes, (FooterNotificationItem.FooterNotificationType)parameter);
                    break;
            }
        }
        /// <summary>
        /// A sequence of methods to zoom around a selection of nodes 
        /// </summary>
        /// <param name="selectedNodes"></param>
        private void FitSelection(IEnumerable<NodeModel> selectedNodes, FooterNotificationItem.FooterNotificationType currentNotificationType)
        {
            Guid nodeToSelect = Guid.Empty;

            // Don't allow prior to evaluation 
            if ((Model as HomeWorkspaceModel).EvaluationCount == 0) return;
            var nodeModels = selectedNodes as NodeModel[] ?? selectedNodes.ToArray();
            if (!nodeModels.Any()) return;

            // Reset the counter if you swap to a different notification type
            if (!currentNotificationType.Equals(this.footerNotificationType))
            {
                this.notificationsCounter = 0;
                this.footerNotificationType = currentNotificationType;
            }
            
            // If we have reached the maximum nodes for this type, select all and reset the counter
            int maxCount = nodeModels.Length;
            if (IsMaxNotificationCounter(this.notificationsCounter, maxCount))
            {
                this.notificationsCounter = 0;
            }
            
            var node = nodeModels.ElementAt(this.notificationsCounter);
            nodeToSelect = node.GUID;
            this.notificationsCounter++;
            

            // Select
            var command = new DynamoModel.SelectModelCommand(nodeToSelect, ModifierKeys.None);
            this.DynamoViewModel.ExecuteCommand(command);
            
            // Focus on selected
            this.DynamoViewModel.CurrentSpaceViewModel.FocusNodeCommand.Execute(nodeToSelect.ToString());
        }

        private bool IsMaxNotificationCounter(int counter, int max)
        {
            if (counter >= max) return true;
            return false;
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
            DynamoViewModel.Model.PropertyChanged -= DynamoViewModel_PropertyChanged;
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
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#989898"));
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
    /// Value converter from 0 to Visibility Collapsed
    /// </summary>
    public class ZeroToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ("Inverse".Equals((string)parameter))
            {
                if ((int) value == 0)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            if ((int) value == 0)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
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
        private int _notificationCount;
        private string _notificationImage;

        /// <summary>
        /// Represents the different types of item - Error, Warning or Info
        /// </summary>
        public enum FooterNotificationType
        {
            Error,
            Warning,
            Information,
        }

        /// <summary>
        /// The number of Warnings, Errors or Info Nodes
        /// </summary>
        public int NotificationCount 
        { 
            get { return _notificationCount; }
            set
            {
                _notificationCount = value;
                RaisePropertyChanged(nameof(NotificationCount));
            }
        }

        /// <summary>
        /// The glyph associated with this footer item
        /// </summary>
        public string NotificationImage
        {
            get { return _notificationImage; }
            set
            {
                _notificationImage = value;
                RaisePropertyChanged(nameof(NotificationImage));
            }
        }
        /// <summary>
        /// The tooltip associated with the respective item
        /// The tooltip message stays the same, no update required
        /// </summary>
        public string NotificationToolTip { get; set; }

        /// <summary>
        /// The type of control, either Error, Warning or Info
        /// </summary>
        public FooterNotificationType NotificationType { get; set; }
    }
}
