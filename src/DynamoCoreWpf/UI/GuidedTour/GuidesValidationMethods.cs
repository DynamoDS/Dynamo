using System.Windows;
using System.Windows.Controls;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.ViewModels;
using static Dynamo.Wpf.UI.GuidedTour.Guide;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This static class will be used for adding static methods that will be executed for the Json PreValidation section (or other validations).
    /// </summary>
    internal static class GuidesValidationMethods
    {
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
        internal static void ExecuteTermsOfServiceFlow(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            if (enableFunction)
            {
                stepInfo.DynamoViewModelStep.ShowPackageManagerSearch(null);
                Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
                if (ownedWindow == null) return;
                Button buttonElement = Guide.FindChild(ownedWindow, stepInfo.HostPopupInfo.HostUIElementString) as Button;
                if (buttonElement != null)
                    buttonElement.Click += ButtonElement_Click;
            }
            else
            {
                Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
                if (ownedWindow == null) return;
                Button buttonElement = Guide.FindChild(ownedWindow, stepInfo.HostPopupInfo.HostUIElementString) as Button;
                if (buttonElement != null)
                    buttonElement.Click -= ButtonElement_Click;
                Guide.CloseWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            }
        }

        private static void ButtonElement_Click(object sender, RoutedEventArgs e)
        {
           CurrentExecutingGuide.HideCurrentStep(CurrentExecutingStep.Sequence, GuideFlow.FORWARD);
            if (CurrentExecutingStep.Sequence < CurrentExecutingGuide.TotalSteps)
            {
                CurrentExecutingStep.DynamoViewModelStep.Model.PreferenceSettings.PackageDownloadTouAccepted = true;
                CurrentExecutingGuide.CalculateStep(GuideFlow.FORWARD, CurrentExecutingStep.Sequence);
                CurrentExecutingGuide.CurrentStep.Show(GuideFlow.FORWARD);
            }
        }

        internal static void ExecutePackageSearch(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            if(enableFunction)
            {
                //We need to check if the PackageManager search is already open if that is the case we don't need to open it again
                if (ownedWindow == null)
                {
                    stepInfo.DynamoViewModelStep.ShowPackageManagerSearch(null);
                    PackageManagerSearchView packageManager = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window) as PackageManagerSearchView;
                    (packageManager.DataContext as PackageManagerSearchViewModel).SearchAndUpdateResults("Clockwork");
                }             
            }
            else
            {
                if (ownedWindow == null) return;
                
                //Depending of the SetUp done in the Guides Json we will make the Clean Up or not.
                if(uiAutomationData.ExecuteCleanUpForward && currentFlow == GuideFlow.FORWARD)
                    ownedWindow.Close();

                if (uiAutomationData.ExecuteCleanUpBackward && currentFlow == GuideFlow.BACKWARD)
                    ownedWindow.Close();
            }
        }
    }
}
