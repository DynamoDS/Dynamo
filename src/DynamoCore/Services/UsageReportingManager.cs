using Dynamo.UI.Commands;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Windows;
using Dynamo.UI;

namespace Dynamo.Services
{
    public class UsageReportingManager : NotificationObject
    {
        public DelegateCommand ToggleIsUsageReportingApprovedCommand { get; set; }

        #region Private

        private static UsageReportingManager instance;

        #endregion

        #region Static Properties

        public static UsageReportingAgreementPrompt UsageReportingPrompt { get; set; }

        public static UsageReportingManager Instance
        {
            get { return instance ?? (instance = new UsageReportingManager()); }
        }

        #endregion

        #region Properties binded to PreferenceSettings

        public bool IsUsageReportingApproved
        {
            get
            {
                if (DynamoController.IsTestMode) // Do not want logging in unit tests.
                    return false;

                if (dynSettings.Controller != null)
                    return dynSettings.Controller.PreferenceSettings.IsUsageReportingApproved;
                
                return false;
            }
            private set
            {
                dynSettings.Controller.PreferenceSettings.IsUsageReportingApproved = value;
                RaisePropertyChanged("IsUsageReportingApproved");

                // Call PreferenceSettings to save
                try
                {
                    dynSettings.Controller.PreferenceSettings.Save();
                }
                catch (Exception args)
                {
                    dynSettings.Controller.IsCrashing = true;
                    string filePath = PreferenceSettings.GetSettingsFilePath();
                    dynSettings.Controller.OnRequestsCrashPrompt(this, new CrashPromptArgs(args.Message, Configurations.UsageReportingErrorMessage, filePath));
                }
            }
        }

        public bool FirstRun
        {
            get
            {
                return dynSettings.Controller.PreferenceSettings.IsFirstRun;
            }
            private set
            {
                dynSettings.Controller.PreferenceSettings.IsFirstRun = value;
                RaisePropertyChanged("FirstRun");
            }
        }

        #endregion

        public UsageReportingManager()
        {
            ToggleIsUsageReportingApprovedCommand = new DelegateCommand(ToggleIsUsageReportingApproved, CanToggleIsUsageReportingApproved);
        }

        public void CheckIsFirstRun()
        {
            // First run of Dynamo
            if (dynSettings.Controller.PreferenceSettings.IsFirstRun)
            {
                FirstRun = false;

                if (!DynamoController.IsTestMode)
                    ShowUsageReportingPrompt();
            }
        }

        public void ToggleIsUsageReportingApproved(object parameter)
        {
            bool resultOption = !IsUsageReportingApproved;

            // If toggling to approve usage reporting, show agreement consent
            if (resultOption)
                ShowUsageReportingPrompt();
            else
                IsUsageReportingApproved = false;
        }

        internal bool CanToggleIsUsageReportingApproved(object parameter)
        {
            return true;
        }

        public void SetUsageReportingAgreement(bool approved)
        {
            IsUsageReportingApproved = approved;
        }

        private static void ShowUsageReportingPrompt()
        {
            UsageReportingPrompt = new UsageReportingAgreementPrompt();
            if (null != Application.Current)
                UsageReportingPrompt.Owner = Application.Current.MainWindow;

            UsageReportingPrompt.ShowDialog();
        }
    }
}
