using Dynamo.Core;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.UI.Prompts;
using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Windows;
using Dynamo.UI;

using NotificationObject = Dynamo.Core.NotificationObject;

namespace Dynamo.Services
{
    public class UsageReportingManager : NotificationObject
    {
        public DelegateCommand ToggleIsUsageReportingApprovedCommand { get; set; }
        public DelegateCommand ToggleIsAnalyticsReportingApprovedCommand { get; set; }

        #region Private

        private static DynamoModel dynamoModel;
        private static UsageReportingManager instance;

        #endregion

        #region Static Properties

        public static UsageReportingManager Instance
        {
            get { return instance ?? (instance = new UsageReportingManager()); }
        }

        public static void DestroyInstance()
        {
            instance = null;
        }

        #endregion

        #region Properties binded to PreferenceSettings

        /// <summary>
        /// UsageReporting is the opt-in component
        /// </summary>
        public bool IsUsageReportingApproved
        {
            get
            {
                if (DynamoModel.IsTestMode) // Do not want logging in unit tests.
                    return false;

                if (dynamoModel != null)
                    return dynamoModel.PreferenceSettings.IsUsageReportingApproved;
                
                return false;
            }
            private set
            {
                dynamoModel.PreferenceSettings.IsUsageReportingApproved = value;
                RaisePropertyChanged("IsUsageReportingApproved");

                // Call PreferenceSettings to save
                try
                {
                    dynamoModel.PreferenceSettings.Save();
                }
                catch (Exception args)
                {
                    DynamoModel.IsCrashing = true;
                    string filePath = PreferenceSettings.GetSettingsFilePath();
                    dynamoModel.OnRequestsCrashPrompt(this, new CrashPromptArgs(args.Message, Configurations.UsageReportingErrorMessage, filePath));
                }
            }
        }

        /// <summary>
        /// Provide access to the instance of DynamoModel to watch. 
        /// This operation should be called only once at
        /// the beginning of a Dynamo session.  It will not be mutated 
        /// in subsequent calls.
        /// </summary>
        public void InitializeCore(DynamoModel dynamoModel)
        {
            if (UsageReportingManager.dynamoModel == null)
                UsageReportingManager.dynamoModel = dynamoModel;
        }

        /// <summary>
        /// Analytics is the opt-out tracking system
        /// PII is prohibited from Analytics.
        /// </summary>
        public bool IsAnalyticsReportingApproved
        {
            get
            {
                if (DynamoModel.IsTestMode) // Do not want logging in unit tests.
                    return false;

                if (dynamoModel != null)
                    return dynamoModel.PreferenceSettings.IsAnalyticsReportingApproved;

                return true;
            }

            private set
            {
                dynamoModel.PreferenceSettings.IsAnalyticsReportingApproved = value;
                RaisePropertyChanged("IsAnalyticsReportingApproved");

                // Call PreferenceSettings to save
                try
                {
                    dynamoModel.PreferenceSettings.Save();
                }
                catch (Exception args)
                {
                    DynamoModel.IsCrashing = true;
                    string filePath = PreferenceSettings.GetSettingsFilePath();
                    dynamoModel.OnRequestsCrashPrompt(this, new CrashPromptArgs(args.Message, Configurations.UsageReportingErrorMessage, filePath));
                }
            }


        }

        public bool FirstRun
        {
            get
            {
                return dynamoModel.PreferenceSettings.IsFirstRun;
            }
            private set
            {
                dynamoModel.PreferenceSettings.IsFirstRun = value;
                RaisePropertyChanged("FirstRun");
            }
        }

        #endregion

        public UsageReportingManager()
        {
            ToggleIsUsageReportingApprovedCommand = new DelegateCommand(ToggleIsUsageReportingApproved, CanToggleIsUsageReportingApproved);
            ToggleIsAnalyticsReportingApprovedCommand = new DelegateCommand(ToggleIsAnalyticsReportingApproved, CanToggleIsAnalyticsReportingApproved);
        }

        public void CheckIsFirstRun(Window ownerWindow)
        {
            // First run of Dynamo
            if (dynamoModel.PreferenceSettings.IsFirstRun)
            {
                FirstRun = false;

                //Analytics enable by defaultwa
                IsAnalyticsReportingApproved = true;

                //Prompt user for detailed reporting
                if (!DynamoModel.IsTestMode)
                    ShowUsageReportingPrompt(ownerWindow);
            }
        }

        public void ToggleIsUsageReportingApproved(object parameter)
        {
            if (!(parameter is Dynamo.Controls.DynamoView))
            {
                var message = "DynamoView must be supplied for this command";
                throw new InvalidOperationException(message);
            }

            bool resultOption = !IsUsageReportingApproved;

            // If toggling to approve usage reporting, show agreement consent
            if (resultOption)
                ShowUsageReportingPrompt(parameter as Window);
            else
                IsUsageReportingApproved = false;
        }

        public void ToggleIsAnalyticsReportingApproved(object parameter)
        {
            IsAnalyticsReportingApproved = !IsAnalyticsReportingApproved;
        }

        internal bool CanToggleIsUsageReportingApproved(object parameter)
        {
            return true;
        }

        internal bool CanToggleIsAnalyticsReportingApproved(object parameter)
        {
            return true;
        }

        public void SetUsageReportingAgreement(bool approved)
        {
            IsUsageReportingApproved = approved;
        }

        private static void ShowUsageReportingPrompt(Window ownerWindow)
        {
            // If an owner window is not supplied, then we will fallback onto 
            // using the application's main window. In native host application
            // scenario (e.g. Revit), the "Application.Current" will be "null".
            // The owner window is important so that usage report window always 
            // get shown on top of the owner window, otherwise it is possible 
            // for usage report window to show up in the background (behind all
            // other full screen windows), and Dynamo main window will appear 
            // to be frozen because control cannot return to it.
            // 
            if (ownerWindow == null && (null != Application.Current))
                ownerWindow = Application.Current.MainWindow;

            var usageReportingPrompt = new UsageReportingAgreementPrompt();
            usageReportingPrompt.Owner = ownerWindow;
            usageReportingPrompt.ShowDialog();
        }
    }
}
