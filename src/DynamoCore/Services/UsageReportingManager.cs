using Dynamo.Core;
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
            get
            {
                if (instance == null)
                    instance = new UsageReportingManager();
                return instance;
            }
        }
        #endregion

        #region Properties binded to PreferenceSettings
        public bool IsUsageReportingApproved
        {
            get
            {
                return DynamoSettings.Controller.PreferenceSettings.IsUsageReportingApproved;
            }
            private set
            {
                DynamoSettings.Controller.PreferenceSettings.IsUsageReportingApproved = value;
                RaisePropertyChanged("IsUsageReportingApproved");

                // Call PreferenceSettings to save
                try
                {
                    DynamoSettings.Controller.PreferenceSettings.Save();
                }
                catch (Exception args)
                {
                    string filePath = PreferenceSettings.GetSettingsFilePath();
                    DynamoSettings.Controller.OnRequestsCrashPrompt(this, new CrashPromptArgs(args.Message, Configurations.UsageReportingErrorMessage, filePath));
                }
            }
        }

        public bool FirstRun
        {
            get
            {
                return DynamoSettings.Controller.PreferenceSettings.IsFirstRun;
            }
            private set
            {
                DynamoSettings.Controller.PreferenceSettings.IsFirstRun = value;
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
            if (DynamoSettings.Controller.PreferenceSettings.IsFirstRun)
            {
                FirstRun = false;

                if (!DynamoSettings.Controller.Testing)
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

        private void ShowUsageReportingPrompt()
        {
            UsageReportingPrompt = new UsageReportingAgreementPrompt();
            if (null != Application.Current)
                UsageReportingPrompt.Owner = Application.Current.MainWindow;

            UsageReportingPrompt.ShowDialog();
        }
    }
}
