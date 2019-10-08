using System;
using System.Windows;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using NotificationObject = Dynamo.Core.NotificationObject;

namespace Dynamo.Services
{
    public class UsageReportingManager : NotificationObject
    {
        public DelegateCommand ToggleIsUsageReportingApprovedCommand { get; set; }
        public DelegateCommand ToggleIsAnalyticsReportingApprovedCommand { get; set; }

        #region Private

        private static DynamoViewModel dynamoViewModel;
        private static UsageReportingManager instance;
        private static IBrandingResourceProvider resourceProvider;

        #endregion

        #region Static Properties

        public static UsageReportingManager Instance
        {
            get { return instance ?? (instance = new UsageReportingManager()); }
        }

        public static void DestroyInstance()
        {
            instance = null;
            dynamoViewModel = null;
        }

        #endregion

        #region Properties binded to PreferenceSettings

        /// <summary>
        /// UsageReporting is the opt-in component
        /// </summary>
        public bool IsUsageReportingApproved
        {
            get {
                return !DynamoModel.IsTestMode
                    && (dynamoViewModel != null
                        && dynamoViewModel.Model.PreferenceSettings.IsUsageReportingApproved);
            }
            private set
            {
                dynamoViewModel.Model.PreferenceSettings.IsUsageReportingApproved = value;
                RaisePropertyChanged("IsUsageReportingApproved");
                var path = dynamoViewModel.Model.PathManager.PreferenceFilePath;

                // Call PreferenceSettings to save
                try
                {
                    dynamoViewModel.Model.PreferenceSettings.SaveInternal(path);
                }
                catch (Exception args)
                {
                    DynamoModel.IsCrashing = true;
                    dynamoViewModel.Model.OnRequestsCrashPrompt(this, new CrashPromptArgs(
                        args.Message, Properties.Resources.UsageReportingErrorMessage, path));
                }
            }
        }

        /// <summary>
        /// Provide access to the instance of DynamoModel to watch. 
        /// This operation should be called only once at
        /// the beginning of a Dynamo session.  It will not be mutated 
        /// in subsequent calls.
        /// </summary>
        public void InitializeCore(DynamoViewModel dynamoViewModel)
        {
            if (UsageReportingManager.dynamoViewModel == null)
                UsageReportingManager.dynamoViewModel = dynamoViewModel;
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

                if (dynamoViewModel.Model != null)
                    return dynamoViewModel.Model.PreferenceSettings.IsAnalyticsReportingApproved;

                return true;
            }

            private set
            {
                dynamoViewModel.Model.PreferenceSettings.IsAnalyticsReportingApproved = value;
                RaisePropertyChanged("IsAnalyticsReportingApproved");
                var path = dynamoViewModel.Model.PathManager.PreferenceFilePath;

                // Call PreferenceSettings to save
                try
                {
                    dynamoViewModel.Model.PreferenceSettings.SaveInternal(path);
                }
                catch (Exception args)
                {
                    DynamoModel.IsCrashing = true;
                    dynamoViewModel.Model.OnRequestsCrashPrompt(this, new CrashPromptArgs(
                        args.Message, Properties.Resources.UsageReportingErrorMessage, path));
                }
            }


        }

        public bool FirstRun
        {
            get
            {
                return dynamoViewModel.Model.PreferenceSettings.IsFirstRun;
            }
            private set
            {
                dynamoViewModel.Model.PreferenceSettings.IsFirstRun = value;
                RaisePropertyChanged("FirstRun");
            }
        }

        #endregion

        public UsageReportingManager()
        {
            ToggleIsUsageReportingApprovedCommand = new DelegateCommand(
                ToggleIsUsageReportingApproved, p => true);
            ToggleIsAnalyticsReportingApprovedCommand = new DelegateCommand(
                ToggleIsAnalyticsReportingApproved, p => true);
        }

        public void CheckIsFirstRun(Window ownerWindow, IBrandingResourceProvider resource)
        {
            resourceProvider = resource;
            // First run of Dynamo
            if (dynamoViewModel.Model.PreferenceSettings.IsFirstRun)
            {
                FirstRun = false;

                //Prompt user for detailed reporting
                if (!DynamoModel.IsTestMode)
                    ShowUsageReportingPrompt(ownerWindow);
            }
        }

        public void ToggleIsUsageReportingApproved(object parameter)
        {
            var ownerWindow = parameter as Window;
            if (ownerWindow == null)
            {
                throw new InvalidOperationException(
                    "DynamoView must be supplied for this command");
            }

            // If reporting is not currently enabled, then the user should be 
            // shown the agreement dialog (on which he/she can choose to accept 
            // or reject the reporting). If the reporting is currently enabled,
            // then set it to false (user chooses not to accept the reporting).
            // 
            if (IsUsageReportingApproved)
            {
                IsUsageReportingApproved = false;
            }
            else
            {
                ShowUsageReportingPrompt(ownerWindow);
            }
        }

        public void ToggleIsAnalyticsReportingApproved(object parameter)
        {
            var ownerWindow = parameter as Window;
            if (ownerWindow == null)
            {
                throw new InvalidOperationException(
                    "DynamoView must be supplied for this command");
            }

            // If reporting is not currently enabled, then the user should be 
            // shown the agreement dialog (on which he/she can choose to accept 
            // or reject the reporting). If the reporting is currently enabled,
            // then set it to false (user chooses not to accept the reporting).
            // 
            if (IsAnalyticsReportingApproved)
            {
                IsAnalyticsReportingApproved = false;
            }
            else
            {
                ShowUsageReportingPrompt(ownerWindow);
            }
        }

        public void SetUsageReportingAgreement(bool approved)
        {
            IsUsageReportingApproved = approved;
        }

        public void SetAnalyticsReportingAgreement(bool approved)
        {
            IsAnalyticsReportingApproved = approved;
        }

        private void ShowUsageReportingPrompt(Window ownerWindow)
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

            var usageReportingPrompt = new UsageReportingAgreementPrompt(resourceProvider, dynamoViewModel)
            {
                Owner = ownerWindow
            };
            usageReportingPrompt.Loaded += UsageReportingPromptLoaded;
            usageReportingPrompt.ShowDialog();
            usageReportingPrompt.Loaded -= UsageReportingPromptLoaded;
        }

        void UsageReportingPromptLoaded(object sender, RoutedEventArgs e)
        {
                DynamoModel.OnRequestMigrationStatusDialog(new SettingsMigrationEventArgs(
                            SettingsMigrationEventArgs.EventStatusType.End));           
        }
    }
}
