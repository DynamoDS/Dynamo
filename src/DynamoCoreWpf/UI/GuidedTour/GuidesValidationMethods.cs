﻿using System.Linq;
﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.PackageManager.ViewModels;
using Dynamo.ViewModels;
using static Dynamo.Wpf.UI.GuidedTour.Guide;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This static class will be used for adding static methods that will be executed for the Json PreValidation section (or other validations).
    /// </summary>
    internal class GuidesValidationMethods
    {
        //We need the Step and the Guide due that some functions need to access information about it
        internal static Step CurrentExecutingStep;
        internal static Guide CurrentExecutingGuide;
        internal static GuidesManager CurrentExecutingGuidesManager;
        
        private static ExitGuide exitGuide;

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

            if (stepInfo.ExitGuide != null)
                exitGuide = stepInfo.ExitGuide;

            //When enableFunction = true, means we want to show the TermsOfUse Window (this is executed in the UIAutomation step in the Show() method)
            if (enableFunction)
            {
                //If the TermsOfService is not accepted yet it will show the TermsOfUseView otherwise it will show the PackageManagerSearchView
                stepInfo.DynamoViewModelStep.ShowPackageManagerSearch(null);
                Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);

                foreach (var handler in uiAutomationData.AutomaticHandlers)
                {
                    if (ownedWindow == null) return;
                    UIElement element = Guide.FindChild(ownedWindow, handler.HandlerElement);

                    //When the Accept button is pressed in the TermsOfUseView then we need to move to the next Step
                    if (element != null)
                        ManageEventHandler(element, handler.HandlerElementEvent, handler.ExecuteMethod);
                }
            }
            //When enableFunction = false, means we are hiding (closing) the TermsOfUse Window due that we are moving to the next Step or we are exiting the Guide
            else
            {
                Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
                if (ownedWindow == null) return;

                foreach (var handler in uiAutomationData.AutomaticHandlers)
                {
                    UIElement element = Guide.FindChild(ownedWindow, handler.HandlerElement) as Button;
                    if (element != null)
                        ManageEventHandler(element, handler.HandlerElementEvent, handler.ExecuteMethod, false);
                }                

                //Tries to close the TermsOfUseView or the PackageManagerSearchView if they were opened previously
                Guide.CloseWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            }
        }

        /// <summary>
        /// This methos subscribes and unsubscribes an event by setting the method and the event dynamically
        /// </summary>
        /// <param name="element">The element to subscribe the method I.E: Button</param>
        /// <param name="eventname">The event name that will be subscribed I.E: Click</param>
        /// <param name="methodname">The method that will listen the event I.E: AcceptButton_Click</param>
        /// <param name="addEvent">A flag that will check if it's to subscribe or unsubscribe</param>
        internal static void ManageEventHandler(object element, string eventname, string methodname, bool addEvent = true)
        {
            EventInfo eventInfo = element.GetType().GetEvent(eventname);

            var validationMethods = new GuidesValidationMethods();

            var eventHandlerMethod = validationMethods.GetType().GetMethod(methodname, BindingFlags.NonPublic | BindingFlags.Instance);           

            Delegate del = Delegate.CreateDelegate(eventInfo.EventHandlerType, validationMethods, eventHandlerMethod);

            if(addEvent)
                eventInfo.AddEventHandler(element, del);
            else
                eventInfo.RemoveEventHandler(element, del);
        }

        /// <summary>
        /// This method will be executed when the Accept button is pressed in the TermsOfUseView Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void AcceptButton_Click(object sender, RoutedEventArgs e)
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
        /// This method will be executed when the Decline button is pressed in the TermsOfUseView Window and creates the exit tour modal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentExecutingGuide.HideCurrentStep(CurrentExecutingStep.Sequence, GuideFlow.FORWARD);
            CurrentExecutingGuidesManager.CreateExitModal(exitGuide);
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
            CurrentExecutingStep = stepInfo;
            //We try to find the PackageManagerSearchView window
            Window ownedWindow = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            if(enableFunction)
            {
                //We need to check if the PackageManager search is already open if that is the case we don't need to open it again
                if (ownedWindow == null)
                {
                    stepInfo.DynamoViewModelStep.ShowPackageManagerSearch(null);
                    PackageManagerSearchView packageManager = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window) as PackageManagerSearchView;
                    if (packageManager == null)
                        return;
                    PackageManagerSearchViewModel packageManagerViewModel = packageManager.DataContext as PackageManagerSearchViewModel;
                    if (packageManagerViewModel == null)
                        return;
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

        /// <summary>
        /// This method will be used to subscribe a method to the Button.Click event of the ViewDetails button located in the PackageManagerSearch Window
        /// </summary>
        /// <param name="stepInfo">Information about the Step</param>
        /// <param name="uiAutomationData">Information about UI Automation that is being executed</param>
        /// <param name="enableFunction">Variable used to know if we are executing the automation or undoing changes</param>
        /// <param name="currentFlow">Current Guide Flow</param>
        internal static void SubscribeViewDetailsEvent(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            PackageManagerSearchView packageManager = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window) as PackageManagerSearchView;
            if (packageManager == null) return;
            Button foundElement = Guide.FindChild(packageManager, stepInfo.HostPopupInfo.HighlightRectArea.WindowElementNameString) as Button;
            if (foundElement == null)
                return;
            if(enableFunction)
                foundElement.Click += ViewDetails_Click;
            else
                foundElement.Click -= ViewDetails_Click;
        }

        /// <summary>
        /// This method will move the flow to the next Step when the ViewDetails button is clicked
        /// </summary>
        /// <param name="sender">The ViewDetails button in PackageManagerSearch Window</param>
        /// <param name="e">No Arguments will be provided</param>
        private static void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            CurrentExecutingGuide.HideCurrentStep(CurrentExecutingStep.Sequence, GuideFlow.FORWARD);
            if (CurrentExecutingStep.Sequence < CurrentExecutingGuide.TotalSteps)
            {
                //Move to the next Step in the Guide
                CurrentExecutingGuide.CalculateStep(GuideFlow.FORWARD, CurrentExecutingStep.Sequence);
                CurrentExecutingGuide.CurrentStep.Show(GuideFlow.FORWARD);
            }
        }

        /// <summary>
        /// This method will be opening the SideBar Package Details (or closing it when enableFunction = false)
        /// </summary>
        /// <param name="stepInfo">Information about the Step</param>
        /// <param name="uiAutomationData">Information about UI Automation that is being executed</param>
        /// <param name="enableFunction">Variable used to know if we are executing the automation or undoing changes</param>
        /// <param name="currentFlow">Current Guide Flow</param>
        internal static void ExecuteViewDetailsSideBar(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            const string packageDetailsName = "Package Details";
            const string closeButtonName = "CloseButton";
            const string packageDetailsWindowName = "PackageDetailsWindow";

            CurrentExecutingStep = stepInfo;
            var stepMainWindow = stepInfo.MainWindow as Window;          
            var packageDetailsWindow = Guide.FindChild(stepMainWindow, packageDetailsWindowName) as UserControl;

            if (enableFunction)
            {
                //This section will open the Package Details Sidebar
                PackageManagerSearchView packageManager = Guide.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepMainWindow) as PackageManagerSearchView;
                if (packageManager == null)
                    return;
                PackageManagerSearchViewModel packageManagerViewModel = packageManager.DataContext as PackageManagerSearchViewModel;

                //If the results in the PackageManagerSearch are null then we cannot open the Package Detail tab
                if (packageManagerViewModel == null || packageManagerViewModel.SearchResults.Count == 0)
                    return;

                //We take the first result from the PackageManagerSearch
                PackageManagerSearchElementViewModel packageManagerSearchElementViewModel = packageManagerViewModel.SearchResults[0];
                if (packageManagerSearchElementViewModel == null) return;

                if (packageDetailsWindow == null)
                    packageManagerViewModel.ViewPackageDetailsCommand.Execute(packageManagerSearchElementViewModel.Model);
            }
            else
            {
                //This section will close the Package Details Sidebar (just in case is still opened)
                var dynamoView = (stepMainWindow as DynamoView);
                if (packageDetailsWindow == null)
                    return;
                //In order to close the Package Details tab we need first to get the Tab, then get the Close button and finally call the event to close it
                TabItem tabitem = dynamoView.ExtensionTabItems.OfType<TabItem>().SingleOrDefault(n => n.Header.ToString() == packageDetailsName);
                if (tabitem == null)
                    return;
                Button closeButton = Guide.FindChild(tabitem, closeButtonName) as Button;
                if (closeButton == null)
                    return;
                dynamoView.CloseExtensionTab(closeButton, null);
            }
        }
    }
}
