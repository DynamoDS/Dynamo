using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.PackageManager;
using Dynamo.PackageManager.UI;
using Dynamo.PackageManager.ViewModels;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Views.GuidedTour;
using Newtonsoft.Json.Linq;
using static Dynamo.PackageManager.PackageManagerSearchViewModel;
using static Dynamo.Wpf.UI.GuidedTour.Guide;

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
        private const string AutodeskSamplePackage = "Dynamo Samples";
        private static PackageManagerSearchViewModel viewModel;
        private static PackageDownloadHandle packageDownloadHandle;
        private static Button CloseButtonSearchPackages;

        private static Action<object, System.ComponentModel.PropertyChangedEventArgs> searchPackagesPropertyChanged;
        private static bool searchPackagesLoaded;

        internal static PackageManagerSearchViewModel packagesViewModel;

        private static NodeModel lastCreatedNode;

        private static Window CurrentGuideWindow;

        //This method will return a bool that describes if the Terms Of Service was accepted or not.
        internal static bool AcceptedTermsOfUse(DynamoViewModel dynViewModel)
        {
            bool termsOfServiceAccepted = false;
            if (dynViewModel.Model.PreferenceSettings != null)
                termsOfServiceAccepted = dynViewModel.Model.PreferenceSettings.PackageDownloadTouAccepted;
            return termsOfServiceAccepted;
        }

        internal static bool IsPackageInstalled(PackageManagerSearchViewModel viewModel = null)
        {
            if (viewModel == null)
                return true;

            bool canInstall = viewModel.CanInstallPackage(AutodeskSamplePackage);
            return !canInstall;
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
            exitGuide = stepInfo.ExitGuide;

            //When enableFunction = true, means we want to show the TermsOfUse Window (this is executed in the UIAutomation step in the Show() method)
            if (enableFunction)
            {
                //If the TermsOfService is not accepted yet it will show the TermsOfUseView otherwise it will show the PackageManagerSearchView
                stepInfo.DynamoViewModelStep.ShowPackageManagerSearch(null);
                CurrentGuideWindow = GuideUtilities.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
                foreach (var handler in uiAutomationData.AutomaticHandlers)
                {
                    if (CurrentGuideWindow == null) return;
                    UIElement element = GuideUtilities.FindChild(CurrentGuideWindow, handler.HandlerElement);

                    //When the Accept button is pressed in the TermsOfUseView then we need to move to the next Step
                    if (element != null)
                        ManageEventHandler(element, handler.HandlerElementEvent, handler.ExecuteMethod);
                }

                CurrentGuideWindow.Closed += OnPackageManagerTourClosed;
                GuideFlowEvents.GuidedTourFinish += GuideFlowEvents_GuidedTourFinish;
            }
            else
            {
                CurrentGuideWindow = GuideUtilities.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
                if (CurrentGuideWindow != null)
                {
                    CurrentGuideWindow.Closed -= OnPackageManagerTourClosed;
                    CurrentGuideWindow = null;
                }

                GuideFlowEvents.GuidedTourFinish -= GuideFlowEvents_GuidedTourFinish;

                //Tries to close the TermsOfUseView or the PackageManagerSearchView if they were opened previously
                GuideUtilities.CloseWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            }
        }

        private static void GuideFlowEvents_GuidedTourFinish(GuidedTourStateEventArgs args)
        {
            if(CurrentGuideWindow != null)
            {
                CurrentGuideWindow.Closed -= OnPackageManagerTourClosed;
                CurrentGuideWindow.Close();
            }
        }

        private static void OnPackageManagerTourClosed(object window, EventArgs e)
        {
            if (window is TermsOfUseView)
            {
                TermsOfUseView termsOfUseView = (TermsOfUseView)window;
                if (!termsOfUseView.AcceptedTermsOfUse)
                {
                    CloseTour();
                }
            }
        }

        /// <summary>
        /// This method will be executed when packages search window is opened, so it can identify the close button of the window
        /// </summary>
        /// <param name="stepInfo"></param>
        /// <param name="uiAutomationData"></param>
        /// <param name="enableFunction"></param>
        /// <param name="currentFlow"></param>
        internal static void ExecuteClosePackagesSearch(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            if (enableFunction)
            {
                CurrentExecutingStep = stepInfo;
                exitGuide = stepInfo.ExitGuide;

                Window ownedWindow = GuideUtilities.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);

                foreach (var handler in uiAutomationData.AutomaticHandlers)
                {
                    if (ownedWindow == null) return;

                    if(CloseButtonSearchPackages == null)
                    {
                        CloseButtonSearchPackages = GuideUtilities.FindChild(ownedWindow, handler.HandlerElement) as Button;
                        CloseButtonSearchPackages.Click += CloseButton_Click;
                    }
                }
            }
            else
            {
                if (CloseButtonSearchPackages != null)
                    CloseButtonSearchPackages.Click -= CloseButton_Click;
            }
        }

        /// <summary>
        /// This method will be executed when passing from Step 6 to Step 7 in the Packages guide, so it will subscribe the install button event
        /// </summary>
        /// <param name="stepInfo"></param>
        /// <param name="uiAutomationData"></param>
        /// <param name="enableFunction"></param>
        /// <param name="currentFlow"></param>
        internal static void ExecuteInstallPackagesFlow(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            Window ownedWindow = GuideUtilities.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);

            if (enableFunction)
            {
                if (ownedWindow != null)
                    viewModel = ownedWindow.DataContext as PackageManagerSearchViewModel;

                Button buttonElement = GuideUtilities.FindChild(ownedWindow, stepInfo.HostPopupInfo.HostUIElementString) as Button;
                viewModel.PackageManagerClientViewModel.Downloads.CollectionChanged += Downloads_CollectionChanged;
            }
            else
            {
                //Tries to close the TermsOfUseView or the PackageManagerSearchView if they were opened previously (just if the flow if FORWARD from Step6 to Step7)
                if (uiAutomationData.ExecuteCleanUpForward && currentFlow == GuideFlow.FORWARD)
                    GuideUtilities.CloseWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            }
        }

        //This methos is called when a download is added in the list 
        private static void Downloads_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var downloads = (System.Collections.ObjectModel.ObservableCollection<PackageDownloadHandle>)sender;

            if (downloads.Any())
            {
                //Gets the first package of the list
                packageDownloadHandle = downloads.First();
                packageDownloadHandle.PropertyChanged += GuidesValidationMethods_PropertyChanged1;
            }

        }

        //This method is called when the package download state is changed
        private static void GuidesValidationMethods_PropertyChanged1(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (packageDownloadHandle.DownloadState == PackageDownloadHandle.State.Installed)
            {
                CurrentExecutingGuide.HideCurrentStep(CurrentExecutingStep.Sequence, GuideFlow.FORWARD);
                if (CurrentExecutingStep.Sequence < CurrentExecutingGuide.TotalSteps)
                {
                    //Move to the next Step in the Guide
                    CurrentExecutingGuide.CalculateStep(GuideFlow.FORWARD, CurrentExecutingStep.Sequence);
                    CurrentExecutingGuide.CurrentStep.Show(GuideFlow.FORWARD);
                }

                viewModel.PackageManagerClientViewModel.Downloads.CollectionChanged -= Downloads_CollectionChanged;
                packageDownloadHandle.PropertyChanged -= GuidesValidationMethods_PropertyChanged1;
            }
        }

        /// <summary>
        /// This methos subscribes and unsubscribes an event by setting the method and the event dynamically
        /// </summary>
        /// <param name="element">The element to subscribe the method I.E: Button</param>
        /// <param name="eventname">The event name that will be subscribed I.E: Click</param>
        /// <param name="methodname">The method that will listen the event I.E: AcceptButton_Click
        /// The method need to have Access Modifier internal and return void 
        /// </param>
        /// <param name="addEvent">A flag that will check if it's to subscribe or unsubscribe</param>
        internal static void ManageEventHandler(object element, string eventname, string methodname, bool addEvent = true)
        {
            EventInfo eventInfo = element.GetType().GetEvent(eventname);

            var validationMethods = new GuidesValidationMethods();

            var eventHandlerMethod = validationMethods.GetType().GetMethod(methodname, BindingFlags.NonPublic | BindingFlags.Instance);

            Delegate del = Delegate.CreateDelegate(eventInfo.EventHandlerType, validationMethods, eventHandlerMethod);

            if (addEvent)
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
            if (!CurrentExecutingGuidesManager.exitGuideWindow.IsOpen)
            {
                CloseTour();
            }
        }

        /// <summary>
        /// This method will be executed when the close Button is clicked in the packages search window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseButtonSearchPackages = null;
            CloseTour();
        }

        private static void CloseTour()
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
            exitGuide = stepInfo.ExitGuide;
            //We try to find the PackageManagerSearchView window
            Window ownedWindow = GuideUtilities.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window);
            if (enableFunction)
            {
                //We need to check if the PackageManager search is already open if that is the case we don't need to open it again
                if (ownedWindow != null) return;
                stepInfo.DynamoViewModelStep.ShowPackageManagerSearch(null);

                PackageManagerSearchView packageManager = GuideUtilities.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window) as PackageManagerSearchView;
                if (packageManager == null)
                    return;
                PackageManagerSearchViewModel packageManagerViewModel = packageManager.DataContext as PackageManagerSearchViewModel;
                if (packageManagerViewModel == null)
                    return;

                searchPackagesLoaded = false;
                //Due that we need to search the Autodesk Sample package after the initial search is completed 
                //we need to subscribe to the PropertyChanged event so we will know when the SearchState property is equal to Results (meaning that got results)
                searchPackagesPropertyChanged = (sender, e) => { PackageManagerViewModel_PropertyChanged(sender, e, uiAutomationData); };
                packageManagerViewModel.PropertyChanged += searchPackagesPropertyChanged.Invoke;
            }
            else
            {

                PackageManagerSearchView packageManager = GuideUtilities.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window) as PackageManagerSearchView;
                if (packageManager == null)
                    return;
                PackageManagerSearchViewModel packageManagerViewModel = packageManager.DataContext as PackageManagerSearchViewModel;
                if (packageManagerViewModel == null)
                    return;

                //Depending of the SetUp done in the Guides Json we will make the Clean Up or not, for example there are several Steps that use the PackageManagerSearchView then we won't close it

                //The Guide is moving to FORWARD and the ExecuteCleanUpForward = false, then we don't need to close the PackageManagerSearchView
                if (uiAutomationData.ExecuteCleanUpForward && currentFlow == GuideFlow.FORWARD)
                {
                    ClosePackageManager(packageManager);
                }

                //The Guide is moving to FORWARD and the ExecuteCleanUpForward = false, then we don't need to close the PackageManagerSearchView
                if (uiAutomationData.ExecuteCleanUpBackward && currentFlow == GuideFlow.BACKWARD)
                {
                    ClosePackageManager(packageManager);
                }

                //The currentFlow = GuideFlow.CURRENT when exiting the Guide
                if (currentFlow == GuideFlow.CURRENT)
                {
                    ClosePackageManager(packageManager);
                }
            }
        }

        /// <summary>
        /// This method will find the PackageManagerSearch window and then close it
        /// </summary>
        /// <param name="packageManager"></param>
        private static void ClosePackageManager(PackageManagerSearchView packageManager)
        {
            PackageManagerSearchViewModel packageManagerViewModel = packageManager.DataContext as PackageManagerSearchViewModel;
            if (packageManagerViewModel == null)
                return;
            packageManagerViewModel.PropertyChanged -= searchPackagesPropertyChanged.Invoke;
            packageManager.Close();
        }

        /// <summary>
        /// This method is subscribed to the PropertyChanged event so we will be notified when the SearchState changed
        /// </summary>
        /// <param name="sender">PackageManagerSearchViewModel</param>
        /// <param name="e">PropertyChanged</param>
        /// <param name="uiAutomationData"></param>
        private static void PackageManagerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e, StepUIAutomation uiAutomationData)
        {
            PackageManagerSearchViewModel packageManagerViewModel = sender as PackageManagerSearchViewModel;
            if (packageManagerViewModel == null) return;
            if (e.PropertyName == nameof(packageManagerViewModel.SearchState))
            {
                //Let wait until the initial Package Search is completed and we got results then we will search the Autodesk Sample package
                if (packageManagerViewModel.SearchState == PackageSearchState.Results)
                {
                    //Put the name of the Package to be searched in the SearchTextBox
                    packageManagerViewModel.SearchText = AutodeskSamplePackage;

                    searchPackagesLoaded = true;

                    EnableNextButton(null, uiAutomationData, true, GuideFlow.FORWARD);
                    packageManagerViewModel.DisableSearchTextBox();

                    //Unsubscribe from the PropertyChanged event otherwise it will enter everytime the SearchTextBox is updated
                    packageManagerViewModel.PropertyChanged -= searchPackagesPropertyChanged.Invoke;

                }
            }
        }

        internal static void EnableNextButton(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            if (searchPackagesLoaded)
            {
                var nextButton = GuideUtilities.FindChild((CurrentExecutingStep.StepUIPopup as PopupWindow).mainPopupGrid, uiAutomationData.ElementName) as Button;
                if (nextButton != null)
                    nextButton.IsEnabled = true;
            }
        }


        /// <summary>
        /// This method will subcribe the menu "Packages->Search for a Package" MenuItem to the Next event so when is pressed we will moved to the next Step
        /// </summary>
        /// <param name="stepInfo">Information about the Step</param>
        /// <param name="uiAutomationData">Information about UI Automation that is being executed</param>
        /// <param name="enableFunction">Variable used to know if we are executing the automation or undoing changes</param>
        /// <param name="currentFlow">Current Guide Flow</param>
        internal static void SubscribeSearchForPackagesOption(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;

            //We try to find the WindowElementNameString (in this case the MenuItem) in the DynamoView VisualTree
            var foundUIElement = GuideUtilities.FindChild(CurrentExecutingStep.MainWindow, CurrentExecutingStep.HostPopupInfo.HighlightRectArea.WindowElementNameString) as MenuItem;
            if (foundUIElement == null)
                return;
            if (enableFunction)
                //Executed then Showing the Step
                foundUIElement.Click += SearchForPackage_Click;
            else
                //Just executed when exiting the Guide or when passing to the next Step
                foundUIElement.Click -= SearchForPackage_Click;
        }

        /// <summary>
        /// This method will subscribe the Next button from the Step8 Popup for clicking the Package already installed (then it will be expanded).
        /// </summary>
        /// <param name="stepInfo">Information about the Step</param>
        /// <param name="uiAutomationData">Specific UI Automation step that is being executed</param>
        /// <param name="enableFunction">it says if the functionality should be enabled or disabled</param>
        /// <param name="currentFlow">The current flow of the Guide can be FORWARD or BACKWARD</param>
        internal static void SubscribeNextButtonClickEvent(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            //if there is not handler then the function should return
            if (uiAutomationData.AutomaticHandlers == null || uiAutomationData.AutomaticHandlers.Count == 0) return;
            //Due that only one handler was configured we get the first one
            var automaticHandler = uiAutomationData.AutomaticHandlers.FirstOrDefault();
            //Find the NextButton inside the Popup
            var nextbuttonFound = GuideUtilities.FindChild((CurrentExecutingStep.StepUIPopup as PopupWindow).mainPopupGrid, automaticHandler.HandlerElement) as Button;
            if (nextbuttonFound == null) return;
            //Add or Remove the handler assigned to the Button.Click
            ManageEventHandler(nextbuttonFound, automaticHandler.HandlerElementEvent, automaticHandler.ExecuteMethod, enableFunction);

        }

        /// <summary>
        /// This handler will be executed when clicking the next button in the Step 8 Popup so it will be expanding the package content in the LibraryView
        /// </summary>
        /// <param name="sender">Next Button</param>
        /// <param name="e">Event Arguments</param>
        internal void ExecuteAutomaticPackage_Click(object sender, RoutedEventArgs e)
        {
            CollapseExpandPackage(CurrentExecutingStep);
        }

        /// <summary>
        /// This method will call the collapseExpandPackage javascript method with reflection, so the package expander in LibraryView will be clicked
        /// </summary>
        internal static void CollapseExpandPackage(Step stepInfo)
        {
            CurrentExecutingStep = stepInfo;
            var firstUIAutomation = stepInfo.UIAutomation.FirstOrDefault();
            if (firstUIAutomation == null || firstUIAutomation.JSParameters.Count == 0) return;
            object[] parametersInvokeScript = new object[] { firstUIAutomation.JSFunctionName, new object[] { firstUIAutomation.JSParameters.FirstOrDefault() } };
            ResourceUtilities.ExecuteJSFunction(stepInfo.MainWindow, stepInfo.HostPopupInfo, parametersInvokeScript);
        }

        /// <summary>
        /// When the "Search for a Package" MenuItem is clicked this method will be executed moving to the next Step
        /// </summary>
        /// <param name="sender">MenuItem</param>
        /// <param name="e"></param>
        internal static void SearchForPackage_Click(object sender, RoutedEventArgs e)
        {
            CurrentExecutingGuide.NextStep(CurrentExecutingStep.Sequence);
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
            PackageManagerSearchView packageManager = GuideUtilities.FindWindowOwned(stepInfo.HostPopupInfo.WindowName, stepInfo.MainWindow as Window) as PackageManagerSearchView;
            if (packageManager == null) return;
            Button foundElement = GuideUtilities.FindChild(packageManager, stepInfo.HostPopupInfo.HighlightRectArea.WindowElementNameString) as Button;
            if (foundElement == null)
                return;
            if (enableFunction)
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
            const string packageSearchWindowName = "PackageSearch";

            CurrentExecutingStep = stepInfo;
            var stepMainWindow = stepInfo.MainWindow as Window;
            var packageDetailsWindow = GuideUtilities.FindChild(stepMainWindow, stepInfo.HostPopupInfo.HostUIElementString) as UserControl;

            if (enableFunction)
            {
                //This section will open the Package Details Sidebar
                PackageManagerSearchView packageManager = GuideUtilities.FindWindowOwned(packageSearchWindowName, stepMainWindow) as PackageManagerSearchView;
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

                //The PackageDetails sidebar is using events when is being shown then we need to execute those events before setting the Popup.PlacementTarget.
                //otherwise the sidebar will not be present (and we don't have host for the Popup) and the Popup will be located out of the Dynamo window
                CurrentExecutingStep.MainWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            else
            {
                //This section will close the Package Details Sidebar (just in case is still opened), 
                //due that the sidebar (UserControl) is inserted inside a TabItem the only way to close is by using the method dynamoView.CloseExtensionTab
                var dynamoView = (stepMainWindow as DynamoView);
                if (packageDetailsWindow == null)
                    return;
                //In order to close the Package Details tab we need first to get the Tab, then get the Close button and finally call the event to close it
                TabItem tabitem = stepInfo.DynamoViewModelStep.SideBarTabItems.OfType<TabItem>().SingleOrDefault(n => n.Header.ToString() == packageDetailsName);
                if (tabitem == null)
                    return;
                //Get the Close button from the PackageDetailsView
                Button closeButton = GuideUtilities.FindChild(tabitem, closeButtonName) as Button;
                if (closeButton == null)
                    return;
                dynamoView.OnCloseRightSideBarTab(closeButton, null);
            }
        }

        /// <summary>
        /// This method will calculate the Popup location based in a item from the Library
        /// </summary>
        internal static async void CalculateLibraryItemLocation(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            if (uiAutomationData == null) return;
            var jsFunctionName = uiAutomationData.JSFunctionName;
            object[] jsParameters = new object[] { uiAutomationData.JSParameters[0] };
            //Create the array for the paramateres that will be sent to the WebBrowser.InvokeScript Method
            object[] parametersInvokeScript = new object[] { jsFunctionName, jsParameters };
            //Execute the JS function with the provided parameters
            var returnedObject = await ResourceUtilities.ExecuteJSFunction(CurrentExecutingStep.MainWindow, CurrentExecutingStep.HostPopupInfo, parametersInvokeScript);
            if (returnedObject == null) return;
            //Due that the returned object is a json then we get the values from the json
            string jsonString = returnedObject.ToString().Replace("\\\"", "'").Replace("\"", "");
            JObject json = JObject.Parse(jsonString);
            double top = Convert.ToDouble(json["client"]["top"].ToString());
            double bottom = Convert.ToDouble(json["client"]["bottom"].ToString());
            //We calculate the Vertical location taking the average position "(top + bottom) / 2" and the height of the popup
            double verticalPosition = (top + bottom) / 2 - CurrentExecutingStep.Height / 2;
            CurrentExecutingStep.UpdatePopupVerticalPlacement(verticalPosition);
        }

        /// <summary>
        /// This method will call a js function that will scroll down until the bottom of the page
        /// </summary>
        internal static void LibraryScrollToBottom(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            if (uiAutomationData == null) return;
            string jsFunctionName = uiAutomationData.JSFunctionName;
            //Create the array for the paramateres that will be sent to the WebBrowser.InvokeScript Method
            object[] parametersInvokeScript = new object[] { jsFunctionName, new object[] { } };
            //Execute the JS function with the provided parameters
            ResourceUtilities.ExecuteJSFunction(CurrentExecutingStep.MainWindow, CurrentExecutingStep.HostPopupInfo, parametersInvokeScript);
        }

        /// <summary>
        /// This method will highlight the input port of a node (NodeView)
        /// </summary>
        /// <param name="stepInfo">Information about the Step</param>
        /// <param name="uiAutomationData">Information about UI Automation that is being executed</param>
        /// <param name="enableFunction">Variable used to know if we are executing the automation or undoing changes</param>
        /// <param name="currentFlow">Current Guide Flow</param>
        internal static void HighlightPort(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            CurrentExecutingStep.MainWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            var stepMainWindow = CurrentExecutingStep.MainWindow as Window;

            if (uiAutomationData.Parameters == null || uiAutomationData.Parameters.Count < 2)
                return;
            //Parse the parameters location in the json file
            var highlightColor = uiAutomationData.Parameters[0] as string;
            if (highlightColor == null) return;
            var portHighlighted = uiAutomationData.Parameters[1] as string;
            if (portHighlighted == null) return;
            string nodeID = string.Empty;
            try
            {
                nodeID = uiAutomationData.Parameters[2] as string;
            }
            catch
            {
                nodeID = string.Empty;
            }

            //Find a specific node(NodeView) in the MainWindow (DynamoView) based by ID or by the first of a type found
            NodeView byOriginNode = null;
            if(!string.IsNullOrEmpty(nodeID))
                byOriginNode = GuideUtilities.FindNodeByID(stepMainWindow, nodeID);
            else
            {
                if (lastCreatedNode == null) return;
                byOriginNode = GuideUtilities.FindNodeByID(stepMainWindow, lastCreatedNode.GUID.ToString("N"));
            }

            //Inside the NodeView try to find the ItemsControl that contains Input ports or Output ports
            if (!(GuideUtilities.FindChild(byOriginNode, portHighlighted) is ItemsControl itemsControlPort)) return;
            var inPorts = itemsControlPort.Items.Cast<PortViewModel>().ToList();

            //Once we have the ItemsControl we get the ContentPresenter
            var inputViewModel = inPorts.FirstOrDefault(x => x.PortName == (string)uiAutomationData.Parameters[3]);
            var dependencyObject = itemsControlPort.ItemContainerGenerator.ContainerFromItem(inputViewModel);


            Grid mainGrid = dependencyObject.ChildOfType<Grid>();

            if (enableFunction)
            {
                //Creates the highlight rectangle so it can be added and shown over the port
                var portRectangle = stepInfo.CreateRectangle(mainGrid, highlightColor);

                //The Rectangle will be added dynamically in a specific step and then when passing to next step we will remove it
                mainGrid.Children.Add(portRectangle);

                if(portHighlighted.Contains("input"))
                {
                    Grid.SetColumn(portRectangle, 1);
                    Grid.SetColumnSpan(portRectangle, 7);
                }
                else if(portHighlighted.Contains("output"))
                {
                    Grid.SetColumn(portRectangle, 0);
                    Grid.SetColumnSpan(portRectangle, 2);
                }

                
                Grid.SetRow(portRectangle, 0);
            }
            else
            {
                //When moving to the next/previous or exiting the guide, then the Rectangle previously added will be removed
                var buttonRectangle = mainGrid.Children.OfType<Rectangle>().Where(rect => rect.Name.Equals("HighlightRectangle")).FirstOrDefault();
                if (buttonRectangle != null)
                    mainGrid.Children.Remove(buttonRectangle);
            }

        }

        /// <summary>
        /// This method will enable or disable the Next Popup Button and also subcribe to the PropertyChanged event so we will know if the Nodes are connected or not
        /// </summary>
        /// <param name="stepInfo">Information about the Step</param>
        /// <param name="uiAutomationData">Information about UI Automation that is being executed</param>
        /// <param name="enableFunction">Variable used to know if we are executing the automation or undoing changes</param>
        /// <param name="currentFlow">Current Guide Flow</param>
        internal static void ManagePortConnectionEvents(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;

            if (uiAutomationData.Parameters == null || uiAutomationData.Parameters.Count < 3)
                return;

            var portHighlighted = uiAutomationData.Parameters[1] as string;
            if (portHighlighted == null) return;

            //Get the button name to be enabled/disabled coming from the json file
            var buttonName = uiAutomationData.Parameters[0] as string;
            if (buttonName == null) return;

            var nodeName = uiAutomationData.Parameters[2] as string;
            if (nodeName == null) return;

            //Get the Popup and search for the NextButton in the VisualTree
            var popupWindow = CurrentExecutingStep.StepUIPopup as PopupWindow;
            var buttonFound = GuideUtilities.FindChild(popupWindow.mainPopupGrid, buttonName) as Button;
            if (buttonFound == null) return;

            var stepMainWindow = CurrentExecutingStep.MainWindow as Window;

            var byOriginNotConnected = GetNotConnectedNode(stepMainWindow, nodeName);
            if (byOriginNotConnected == null) return;

            var idString = byOriginNotConnected.ViewModel.Id.ToString("N");
            var outputPortViewModel = GetPortViewModel(stepMainWindow, idString, portHighlighted, nodeName) as OutPortViewModel;
            if (outputPortViewModel == null) return;

            //If the nodes are connected then we enable the Next button otherwise will be disabled
            buttonFound.IsEnabled = outputPortViewModel.IsConnected;

            //Subscribe/Unsubscribe to the PropertyChanged event based if we are passing to the Step "Connect the Nodes" or we are leaving this Step.
            if (enableFunction)
                outputPortViewModel.PropertyChanged += OutputPortViewModel_PropertyChanged;
            else
                outputPortViewModel.PropertyChanged -= OutputPortViewModel_PropertyChanged;
        }

        private static void OutputPortViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (CurrentExecutingStep.UIAutomation == null) return;

            var currentUiAutomation = CurrentExecutingStep.UIAutomation.FirstOrDefault(x => x.Name.Equals(nameof(ManagePortConnectionEvents)));
            
            if (currentUiAutomation == null) return;
            if (currentUiAutomation.Parameters == null) return;
            if (currentUiAutomation.Parameters.Count == 0) return;

            var buttonName = currentUiAutomation.Parameters[0] as string;
            if (buttonName == null) return;

            var popupWindow = CurrentExecutingStep.StepUIPopup as PopupWindow;
            var buttonFound = GuideUtilities.FindChild(popupWindow.mainPopupGrid, buttonName) as Button;
            if (buttonFound == null) return;

            var portViewModel = sender as OutPortViewModel;
            if (portViewModel == null) return;

            switch (e.PropertyName)
            {
                case "IsConnected":
                    if (portViewModel.IsConnected)
                        buttonFound.IsEnabled = true;
                    else
                        buttonFound.IsEnabled = false;
                    break;
            }
        }

        /// <summary>
        /// This method will be executed when we are pressing back and it will remove the connection between nodes (if it exists)
        /// </summary>
        /// <param name="stepInfo"></param>
        /// <param name="uiAutomationData"></param>
        /// <param name="enableFunction"></param>
        /// <param name="currentFlow"></param>
        internal static void CreateNode(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;

            //If any backward action is triggered, the created needs to be deleted 
            if (currentFlow == GuideFlow.BACKWARD && lastCreatedNode != null)
            {
                var stepMainWindow = CurrentExecutingStep.MainWindow as Window;

                foreach (var connector in lastCreatedNode.AllConnectors.ToList())
                {
                    connector.Delete();
                }

                stepInfo.DynamoViewModelStep.CurrentSpaceViewModel.Model.RemoveAndDisposeNode(lastCreatedNode);

                lastCreatedNode = null;
            }

            if (enableFunction)
            {
                stepInfo.DynamoViewModelStep.CurrentSpaceViewModel.Model.NodeAdded += Model_NodeAdded;
            }
            else
            {
                stepInfo.DynamoViewModelStep.CurrentSpaceViewModel.Model.NodeAdded -= Model_NodeAdded;
            }
        }

        //This function compares if the created node is the expected one to move to the next step by comparing it's name.
        private static void Model_NodeAdded(NodeModel createdNode)
        {
            if (CurrentExecutingStep.UIAutomation == null || CurrentExecutingStep.UIAutomation.Count < 6) return;

            lastCreatedNode = createdNode;
            var uiAutomationData = CurrentExecutingStep.UIAutomation[5];
            var uiAutomationElementName = (string)uiAutomationData.JSParameters.FirstOrDefault();

            if (createdNode.Name.Equals(uiAutomationElementName))
            {
                createdNode.X = uiAutomationData.NodePosition.X;
                createdNode.Y = uiAutomationData.NodePosition.Y;
                CurrentExecutingGuide.NextStep(CurrentExecutingStep.Sequence);
            }
        }
                
        internal static void RemovePortConnector(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            var stepMainWindow = CurrentExecutingStep.MainWindow as Window;

            if(currentFlow == GuideFlow.BACKWARD && enableFunction == false)
            {
                if (uiAutomationData.Parameters == null || uiAutomationData.Parameters.Count < 4)
                    return;
                var buttonName = uiAutomationData.Parameters[0] as string;
                if (buttonName == null) return;
                var portHighlighted = uiAutomationData.Parameters[1] as string;
                if (portHighlighted == null) return;

                string nodeID = string.Empty;
                try
                {
                    nodeID = uiAutomationData.Parameters[2] as string;
                }
                catch
                {
                    nodeID = string.Empty;
                }

                var nodeName = uiAutomationData.Parameters[3] as string;
                if (nodeName == null) return;

                var outputPortViewModel = GetPortViewModel(stepMainWindow, nodeID, portHighlighted, nodeName) as OutPortViewModel;
                if (outputPortViewModel == null) return;
                var userAddedConnector = outputPortViewModel.PortModel.Connectors.FirstOrDefault();
                if(userAddedConnector != null)
                    userAddedConnector.Delete();

                CurrentExecutingStep.MainWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }       
        }

        private static PortViewModel GetPortViewModel(Window stepMainWindow, string nodeID, string itemsControlName, string nodeName)
        {

            //Find a specific node(NodeView) in the MainWindow (DynamoView) based by ID or by the first of a type found
            NodeView byOriginNode = null;
            if (!string.IsNullOrEmpty(nodeID))
                byOriginNode = GuideUtilities.FindNodeByID(stepMainWindow, nodeID);
            else
            {
                byOriginNode = GetNotConnectedNode(stepMainWindow, nodeName);
                if (byOriginNode == null) return null;
            }

            //Inside the NodeView try to find the ItemsControl that contains Input ports or Output ports
            var itemsControlOutputPort = GuideUtilities.FindChild(byOriginNode, itemsControlName) as ItemsControl;
            if (itemsControlOutputPort == null) return null;

            //Once we have the ItemsControil we try to find a specific port (OutputPortViewModel or InputPortViewModel)
            var firstOutputPortModel = itemsControlOutputPort.ItemContainerGenerator.Items.FirstOrDefault();
            if (firstOutputPortModel == null) return null;

            //Finally return the first PortViewModel
            return firstOutputPortModel as PortViewModel;
        }


        /// <summary>
        /// Get the first not connected node of the type passed as parameter
        /// </summary>
        /// <param name="stepMainWindow">MainWindow that contains the node</param>
        /// <param name="nodeType">Type of the node to be found</param>
        /// <returns></returns>
        private static NodeView GetNotConnectedNode(Window stepMainWindow, string nodeType)
        {
            CurrentExecutingStep.MainWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));

            var allByOriginNodes = GuideUtilities.FindNodesOfType(stepMainWindow, nodeType);
            NodeView byOriginUserAdded = null;
            //Try to find the ByOrigin node that doesn't have input/output connections (it means it was the one added by the user)
            foreach (var byOriginEntry in allByOriginNodes)
            {
                var byOriginEntryViewModel = byOriginEntry.DataContext as NodeViewModel;
                var outputPorts = from port in byOriginEntryViewModel.OutPorts
                                  where port.IsConnected == false
                                  select port;
                if (outputPorts.Any())
                {
                    byOriginUserAdded = byOriginEntry;
                    break;
                }
            }
            return byOriginUserAdded;
        }


        internal static void SubscribeRunButtonNextStep(Step stepInfo, StepUIAutomation uiAutomationData, bool enableFunction, GuideFlow currentFlow)
        {
            CurrentExecutingStep = stepInfo;
            if (uiAutomationData.Parameters.Any() && uiAutomationData.Parameters[0] is string)
            {
                Button runButton = GuideUtilities.FindChildInVisualTree(stepInfo.MainWindow, uiAutomationData.Parameters[0].ToString()) as Button;

                if (runButton == null) return;
                
                if (enableFunction)
                {
                    runButton.Click += RunButton_Click;
                }
                else
                {
                    runButton.Click -= RunButton_Click;
                }
                
            }
        }

        private static void RunButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentExecutingGuide.NextStep(CurrentExecutingStep.Sequence);
        }
    }
}
