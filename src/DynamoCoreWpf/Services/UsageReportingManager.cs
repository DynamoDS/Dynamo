using System;
using System.Windows;
using Dynamo.Core;
using Dynamo.Logging;
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
            ToggleIsAnalyticsReportingApprovedCommand = new DelegateCommand(
                ToggleIsAnalyticsReportingApproved, p => true);
        }

        public void CheckIsFirstRun(Window ownerWindow, IBrandingResourceProvider resource)
        {
            resourceProvider = resource;
            // First run of Dynamo
            if (dynamoViewModel != null
                && dynamoViewModel.Model.PreferenceSettings.IsFirstRun
                && !dynamoViewModel.HideReportOptions
                && !Analytics.DisableAnalytics
                && !DynamoModel.IsTestMode)
            {
                //Prompt user for detailed reporting
                ShowUsageReportingPrompt(ownerWindow);
            }
            FirstRun = false;
        }
        

        public void ToggleIsAnalyticsReportingApproved(object parameter)
        {
            var ownerWindow = parameter as Window;
            if (ownerWindow == null)
            {
                throw new InvalidOperationException(
                    "DynamoView must be supplied for this command");
            }
            ShowUsageReportingPrompt(ownerWindow);
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
            usageReportingPrompt.ShowDialog();
        }
    }
}
