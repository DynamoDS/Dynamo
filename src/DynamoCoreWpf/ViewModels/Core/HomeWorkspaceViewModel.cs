using System.ComponentModel;
using System.Diagnostics;

using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.Core
{
    public class HomeWorkspaceViewModel : WorkspaceViewModel
    {
        #region commands

        public DelegateCommand StartPeriodicTimerCommand { get; set; }
        public DelegateCommand StopPeriodicTimerCommand { get; set; }

        #endregion

        public HomeWorkspaceViewModel(HomeWorkspaceModel model, DynamoViewModel dynamoViewModel)
            : base(model, dynamoViewModel)
        {
            RunSettingsViewModel = new RunSettingsViewModel(((HomeWorkspaceModel)model).RunSettings, this, dynamoViewModel);
            RunSettingsViewModel.PropertyChanged += RunSettingsViewModel_PropertyChanged;

            StartPeriodicTimerCommand = new DelegateCommand(StartPeriodicTimer, CanStartPeriodicTimer);
            StopPeriodicTimerCommand = new DelegateCommand(StopPeriodicTimer, CanStopPeriodicTimer);

            CheckAndSetPeriodicRunCapability();
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
}
