using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

using Dynamo.Core;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels
{
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

        //protected bool debug = false;
        //protected bool canRunDynamically = true;

        private DynamoViewModel dynamoViewModel;
        private Visibility runPeriodInputVisibility;

        #endregion

        #region properties

        public RunSettings Model { get; private set; }

        public RunType RunType
        {
            get
            {
                return Model.RunType;
            }
            set
            {
                Model.RunType = value;
            }
        }

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
                switch (RunType)
                {
                    case RunType.Manual:
                    case RunType.Automatic:
                        return Visibility.Collapsed;
                    case RunType.Periodic:
                        return Visibility.Visible;
                    default:
                        return Visibility.Hidden;
                }
            }

        }

        public bool RunEnabled
        {
            get
            {
                return Model.RunEnabled &&
                    Model.RunType != RunType.Automatic &&
                    Model.RunType != RunType.Periodic;
            }
        }

        //public virtual bool RunInDebug
        //{
        //    get { return debug; }
        //    set
        //    {
        //        debug = value;

        //        //toggle off dynamic run
        //        CanRunDynamically = !debug;

        //        if (debug)
        //            RunType = RunType.Manual;

        //        RaisePropertyChanged("RunInDebug");
        //    }
        //}

        //public virtual bool CanRunDynamically
        //{
        //    get
        //    {
        //        //we don't want to be able to run
        //        //dynamically if we're in debug mode
        //        return !debug;
        //    }
        //    set
        //    {
        //        canRunDynamically = value;
        //        RaisePropertyChanged("CanRunDynamically");
        //    }
        //}

        public DelegateCommand RunExpressionCommand { get; private set; }
        public DelegateCommand CancelRunCommand { get; set; }
        public DelegateCommand RunTypeChangedRunCommand { get; set; }

        #endregion

        #region constructors

        public RunSettingsViewModel(RunSettings settings, DynamoViewModel dynamoViewModel)
        {
            Model = settings;
            Model.PropertyChanged += Model_PropertyChanged;
            this.dynamoViewModel = dynamoViewModel;
            
            CancelRunCommand = new DelegateCommand(CancelRun, CanCancelRun);
            RunExpressionCommand = new DelegateCommand(RunExpression, CanRunExpression);
            RunTypeChangedRunCommand = new DelegateCommand(RunTypeChangedRun, CanRunTypeChangedRun);
        
        }

        private void RunTypeChangedRun(object obj)
        {
            dynamoViewModel.StopPeriodicTimerCommand.Execute(null);
            switch (Model.RunType)
            {
                case RunType.Manual:
                    return;
                case RunType.Automatic:
                    RunExpressionCommand.Execute(true);
                    return;
                case RunType.Periodic:
                    dynamoViewModel.StartPeriodicTimerCommand.Execute(null);
                    return;
            }
        }

        private bool CanRunTypeChangedRun(object obj)
        {
            return true;
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
                    break;
                case "RunPeriod":
                    RaisePropertyChanged("RunPeriod");
                    break;
                case "RunType":
                    RaisePropertyChanged("RunType");
                    RaisePropertyChanged("RunPeriodInputVisibility");
                    break;
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
