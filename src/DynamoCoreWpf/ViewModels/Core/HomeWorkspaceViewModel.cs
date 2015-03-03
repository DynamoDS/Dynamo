using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class HomeWorkspaceViewModel : WorkspaceViewModel
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
        }

        void hwm_EvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            bool hasWarnings = Model.Nodes.Any(n => n.State == ElementState.Warning);

            if (!hasWarnings)
            {
                SetCurrentWarning(NotificationLevel.Mild, "Run completed.");
            }
            else
            {
                SetCurrentWarning(NotificationLevel.Moderate, "Run completed with warnings."); 
            }
        }

        void hwm_EvaluationStarted(object sender, EventArgs e)
        {
            SetCurrentWarning(NotificationLevel.Mild, "Run completed...");
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
            Debug.WriteLine(string.Format("{0} property change handled on the WorkspaceViewModel object.", e.PropertyName));
            RaisePropertyChanged("RunSettingsViewModel");
        }

        private void StartPeriodicTimer(object parameter)
        {
            var hws = Model as HomeWorkspaceModel;
            if (hws == null)
                return;

            hws.StartPeriodicEvaluation(hws.RunSettings.RunPeriod);
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
