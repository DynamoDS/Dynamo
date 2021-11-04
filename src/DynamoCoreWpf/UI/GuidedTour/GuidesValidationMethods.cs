using System.Windows;
using System.Windows.Controls;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.ViewModels;
using static Dynamo.Wpf.UI.GuidedTour.Guide;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This static class will be used for adding static methods that will be executed for the Json PreValidation section (or other validations).
    /// </summary>
    internal static class GuidesValidationMethods
    {
        //We need the Step and the Guide due that some functions need to access information about it
        internal static Step CurrentExecutingStep;
        internal static Guide CurrentExecutingGuide;

        //This method will return a bool that describes if the Terms Of Service was accepted or not.
        internal static bool AcceptedTermsOfUse(DynamoViewModel dynViewModel)
        {
            bool termsOfServiceAccepted = false;
            if (dynViewModel.Model.PreferenceSettings != null)
                termsOfServiceAccepted = dynViewModel.Model.PreferenceSettings.PackageDownloadTouAccepted;
            return termsOfServiceAccepted;
        }

        /// <summary>
        /// This method will be executed when passing from Step 2 to Step 3 in the Packages guide, so it will show the TermsOfUse Window in case it was not accepted yet
        /// </summary>
        /// <param name="stepInfo"></param>
        /// <param name="uiAutomationData"></param>
        /// <param name="enableFunction"></param>
        /// <param name="currentFlow"></param>
        internal static void ExecuteTermsOfServiceFlow(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;

            //When enableFunction = true, means we want to show the TermsOfUse Window (this is executed in the UIAutomation step in the Show() method)
            if (enableFunction)
            {
                //If the TermsOfService is not accepted yet it will show the TermsOfUseView otherwise it will show the PackageManagerSearchView
                stepInfo.DynamoViewModelStep.ShowPackageManagerSearch(null);
                Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
                if (ownedWindow == null) return;
                Button buttonElement = Guide.FindChild(ownedWindow, stepInfo.HostPopupInfo.HostUIElementString) as Button;

                //When the Accept button is pressed in the TermsOfUseView then we need to move to the next Step
                if (buttonElement != null)
                    buttonElement.Click += AcceptButton_Click;
            }
            //When enableFunction = false, means we are hiding (closing) the TermsOfUse Window due that we are moving to the next Step or we are exiting the Guide
            else
            {
                Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
                if (ownedWindow == null) return;
                Button buttonElement = Guide.FindChild(ownedWindow, stepInfo.HostPopupInfo.HostUIElementString) as Button;
                if (buttonElement != null)
                    buttonElement.Click -= AcceptButton_Click;

                //Tries to close the TermsOfUseView or the PackageManagerSearchView if they were opened previously
                Guide.CloseWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            }
        }

        /// <summary>
        /// This method will be executed when the Accept button is pressed in the TermsOfUseView Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
           CurrentExecutingGuide.HideCurrentStep(CurrentExecutingStep.Sequence, GuideFlow.FORWARD);
            if (CurrentExecutingStep.Sequence < CurrentExecutingGuide.TotalSteps)
            {
                //Due that when the Guide is being executed the TermsOfUseView is not modal then the code that set the PackageDownloadTouAccepted is not reached then we need to set it manually
                CurrentExecutingStep.DynamoViewModelStep.Model.PreferenceSettings.PackageDownloadTouAccepted = true;

                //Move to the next Step in the Guide
                CurrentExecutingGuide.CalculateStep(GuideFlow.FORWARD, CurrentExecutingStep.Sequence);
                CurrentExecutingGuide.CurrentStep.Show(GuideFlow.FORWARD);
            }
        }

        /// <summary>
        /// This method will be used to open the PackageManagerSearchView and search for a specific Package
        /// </summary>
        /// <param name="stepInfo">Step information</param>
        /// <param name="uiAutomationData">Specific UI Automation step that is being executed</param>
        /// <param name="enableFunction">it says if the functionality should be enabled or disabled</param>
        /// <param name="currentFlow">The current flow of the Guide can be FORWARD or BACKWARD</param>
        internal static void ExecutePackageSearch(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            //We try to find the PackageManagerSearchView window
            Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            if(enableFunction)
            {
                //We need to check if the PackageManager search is already open if that is the case we don't need to open it again
                if (ownedWindow == null)
                {
                    stepInfo.DynamoViewModelStep.ShowPackageManagerSearch(null);
                    PackageManagerSearchView packageManager = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window) as PackageManagerSearchView;
                    PackageManagerSearchViewModel packageManagerViewModel = packageManager.DataContext as PackageManagerSearchViewModel;
                    //Put the name of the Package to be searched in the SearchTextBox
                    packageManagerViewModel.SearchText = Res.AutodeskSamplePackage;
                    //Execute the Search
                    packageManagerViewModel.RefreshAndSearchAsync();
                }             
            }
            else
            {
                if (ownedWindow == null) return;

                //Depending of the SetUp done in the Guides Json we will make the Clean Up or not, for example there are several Steps that use the PackageManagerSearchView then we won't close it

                //The Guide is moving to FORWARD and the ExecuteCleanUpForward = false, then we don't need to close the PackageManagerSearchView
                if (uiAutomationData.ExecuteCleanUpForward && currentFlow == GuideFlow.FORWARD)
                    ownedWindow.Close();

                //The Guide is moving to FORWARD and the ExecuteCleanUpForward = false, then we don't need to close the PackageManagerSearchView
                if (uiAutomationData.ExecuteCleanUpBackward && currentFlow == GuideFlow.BACKWARD)
                    ownedWindow.Close();

                //The currentFlow = GuideFlow.CURRENT when exiting the Guide
                if (currentFlow == GuideFlow.CURRENT)
                    ownedWindow.Close();
            }
        }
    }
}
