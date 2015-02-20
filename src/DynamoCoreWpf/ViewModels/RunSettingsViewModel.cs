using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

using Dynamo.Core;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;

namespace Dynamo.Wpf.ViewModels
{
    /// <summary>
    /// The RunTypeItem class wraps a RunType for display in the ComboBox.
    /// </summary>
    public class RunTypeItem : NotificationObject
    {
        private bool enabled;

        /// <summary>
        /// The enabled flag sets whether the RunType is selectablable
        /// in the view.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                RaisePropertyChanged("Enabled");
                RaisePropertyChanged("ToolTipText");
            }
        }

        public RunType RunType { get; set; }

        public string Name
        {
            get { return RunType.ToString(); }
        }

        public string ToolTipText
        {
            get
            {
                switch (RunType)
                {
                    case RunType.Automatically:
                        return "Run whenever an input to the graph is updated.";
                    case RunType.Manually:
                        return "Run when you click the Run button.";
                    case RunType.Periodically:
                        return enabled
                            ? "Run at the specified interval."
                            : "Run Periodically is only available when there are nodes in the graph that support periodic update.";
                    default:
                        return string.Empty;
                }
            }
        }

        public RunTypeItem(RunType runType)
        {
            RunType = runType;
            Enabled = true;
        }
    
    }

    /// <summary>
    /// The RunSettingsViewModel is the view model for the 
    /// RunSettings object on a given HomeWorkspaceModel. This class
    /// handles property change notification from the underlying RunSettings
    /// object, raising corresponding property change notifications. Those
    /// property change notifications are, in turn, handled by the WorkspaceViewModel.
    /// Setters on the properties in this class do not raise property change
    /// notifications as those notifications are raised when the value is set on the
    /// model.
    /// </summary>
    public class RunSettingsViewModel : NotificationObject
    {
        #region private members

        private bool debug = false;
        private DynamoViewModel dynamoViewModel;
        private RunTypeItem selectedRunTypeItem;

        #endregion

        #region properties

        public RunSettings Model { get; private set; }

        public int RunPeriod
        {
            get { return Model.RunPeriod; }
            set
            {
                Model.RunPeriod = value; 
            }
        }

        public Visibility RunPeriodInputVisibility
        {
            get
            {
                // When switching the run type, also
                // set the run period input visibility
                switch (SelectedRunTypeItem.RunType)
                {
                    case RunType.Manually:
                    case RunType.Automatically:
                        return Visibility.Collapsed;
                    case RunType.Periodically:
                        return Visibility.Visible;
                    default:
                        return Visibility.Hidden;
                }
            }

        }

        public bool RunEnabled
        {
            get { return Model.RunEnabled; }
        }

        public bool RunButtonEnabled
        {
            get
            {
                return Model.RunEnabled &&
                    Model.RunType != RunType.Automatically &&
                    Model.RunType != RunType.Periodically;
            }
        }

        public string RunButtonToolTip
        {
            get
            {
                return RunButtonEnabled
                    ? Resources.DynamoViewRunButtonTooltip
                    : Resources.DynamoViewRunButtonToolTipDisabled;
            }
        }

        public virtual bool RunInDebug
        {
            get { return debug; }
            set
            {
                debug = value;

                if (debug)
                    SelectedRunTypeItem.RunType = RunType.Manually;

                RaisePropertyChanged("RunInDebug");
            }
        }

        public RunTypeItem SelectedRunTypeItem
        {
            get { return RunTypeItems.First(rt => rt.RunType == Model.RunType); }
            set
            {
                selectedRunTypeItem = value;
                Model.RunType = selectedRunTypeItem.RunType;
            }
        }

        public ObservableCollection<RunTypeItem> RunTypeItems { get; set; }
 
        public DelegateCommand RunExpressionCommand { get; private set; }
        public DelegateCommand CancelRunCommand { get; set; }

        #endregion

        #region constructors

        public RunSettingsViewModel(RunSettings settings, DynamoViewModel dynamoViewModel)
        {
            Model = settings;
            Model.PropertyChanged += Model_PropertyChanged;
            this.dynamoViewModel = dynamoViewModel;
            
            CancelRunCommand = new DelegateCommand(CancelRun, CanCancelRun);
            RunExpressionCommand = new DelegateCommand(RunExpression, CanRunExpression);

            RunTypeItems = new ObservableCollection<RunTypeItem>();
            foreach (RunType val in Enum.GetValues(typeof(RunType)))
            {
                RunTypeItems.Add(new RunTypeItem(val));
            }
            TogglePeriodicRunTypeSelection(false);
        }

        #endregion

        #region private and internal methods

        /// <summary>
        /// Called when the RunSettings model has property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine(string.Format("{0} property change handled on the RunSettingsViewModel object.", e.PropertyName));
            switch (e.PropertyName)
            {
                case "RunEnabled":
                    RaisePropertyChanged("RunEnabled");
                    RaisePropertyChanged("RunButtonEnabled");
                    RaisePropertyChanged("RunButtonToolTip");
                    break;
                case "RunPeriod":
                case "RunType":
                    RaisePropertyChanged("RunPeriod");
                    RaisePropertyChanged("RunEnabled");
                    RaisePropertyChanged("RunButtonEnabled");
                    RaisePropertyChanged("RunButtonToolTip");
                    //RaisePropertyChanged("RunType");
                    RaisePropertyChanged("RunPeriodInputVisibility");
                    RaisePropertyChanged("RunButtonEnabled");
                    RaisePropertyChanged("RunTypeItems");
                    RaisePropertyChanged("SelectedRunType");
                    RunTypeChangedRun(null);
                    break;
            }
        }

        private void RunTypeChangedRun(object obj)
        {
            dynamoViewModel.StopPeriodicTimerCommand.Execute(null);
            switch (Model.RunType)
            {
                case RunType.Manually:
                    return;
                case RunType.Automatically:
                    RunExpressionCommand.Execute(true);
                    return;
                case RunType.Periodically:
                    dynamoViewModel.StartPeriodicTimerCommand.Execute(null);
                    return;
            }
        }

        private void RunExpression(object parameters)
        {
            bool displayErrors = Convert.ToBoolean(parameters);
            var command = new DynamoModel.RunCancelCommand(displayErrors, false);
            dynamoViewModel.ExecuteCommand(command);
        }

        internal static bool CanRunExpression(object parameters)
        {
            return true;
        }

        private void CancelRun(object parameter)
        {
            var command = new DynamoModel.RunCancelCommand(false, true);
            dynamoViewModel.ExecuteCommand(command);
        }

        private static bool CanCancelRun(object parameter)
        {
            return true;
        }

        internal void TogglePeriodicRunTypeSelection(bool value)
        {
            var prt = RunTypeItems.First(rt => rt.RunType == RunType.Periodically);
            prt.Enabled = value;
        }

        #endregion

    }

    /// <summary>
    /// The RunPeriodConverter converts input text to and from an integer
    /// value with a trailing "ms".
    /// </summary>
    public class RunPeriodConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0}{1}", value, "ms");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int ms;
            return !Int32.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ms) ? 100 : Math.Abs(ms);
        }
    }
}
