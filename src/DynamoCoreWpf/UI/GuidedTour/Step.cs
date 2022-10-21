using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Dynamo.Controls;
using Dynamo.UI.Views;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.Views.GuidedTour;
using Newtonsoft.Json;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This abstract class include several properties that will be used for childs like Survey, Welcome, Tooltip, ExitTour ....
    /// </summary>
    public class Step
    {
        #region Private Fields
        private static string WindowNamePopup = "PopupWindow";
        private static readonly string NextButton = "NextButton";
        private static string calculateLibraryFuncName = "CalculateLibraryItemLocation";
        private static string libraryScrollToBottomFuncName = "LibraryScrollToBottom";
        private static string subscribePackageClickedFuncName = "subscribePackageClickedEvent";
        #endregion
        #region Events
        //This event will be raised when a popup (Step) is closed by the user pressing the close button (PopupWindow.xaml).
        public delegate void StepClosedEventHandler(string name, StepTypes stepType);

        public event StepClosedEventHandler StepClosed;
        internal void OnStepClosed(string name, StepTypes stepType)
        {
            if (StepClosed != null)
                StepClosed(name, stepType);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// This static variable represents the total number of steps that the guide has.
        /// </summary>
        public int TotalTooltips { get; set; }

        public enum StepTypes{ SURVEY, TOOLTIP, WELCOME, EXIT_TOUR};

        /// <summary>
        /// The step type will describe which type of window will be created after reading the json file, it can be  SURVEY, TOOLTIP, WELCOME, EXIT_TOUR
        /// </summary>
        [JsonProperty("Type")]
        public StepTypes StepType { get; set; }

        /// <summary>
        /// The step content will contain the title and the popup content (included formatted text)
        /// </summary>
        [JsonProperty("StepContent")]
        public Content StepContent { get; set; }

        /// <summary>
        /// There are some specific Steps that will contain extra content (like Survey.RatingTextTitle), then this list will store the information
        /// </summary>
        [JsonProperty("ExtraContent")]
        public List<ExtraContent> StepExtraContent { get; set; }

        /// <summary>
        /// Step name, it just represent a step identifier
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// When the Step Width property is not provided the default value will be 480
        /// </summary>
        [JsonProperty("Width")]
        public double Width { get; set; } = 480;

        /// <summary>
        /// When the Step Height property is not provide the default value will be 190
        /// </summary>
        [JsonProperty("Height")]
        public double Height { get; set; } = 190;

        /// <summary>
        /// Represent a sequencial numeric value for each step, when is a multiflow guide this value can be repeated
        /// </summary>
        [JsonProperty("Sequence")]
        public int Sequence { get; set; } = 0;

        /// <summary>
        /// This property contains the Host information like the host popup element or the popup position
        /// </summary>
        [JsonProperty("HostPopupInfo")]
        public HostControlInfo HostPopupInfo { get; set; }

        /// <summary>
        /// This property will hold a list of UI Automation actions (information) that will be executed when the Next or Back button are pressed
        /// </summary>
        [JsonProperty("UIAutomation")]
        public List<StepUIAutomation> UIAutomation { get; set; }

        /// <summary>
        /// This property will hold information about the methods/actions that should be executed before showing a Popup(Step)
        /// </summary>
        [JsonProperty("PreValidation")]
        internal PreValidation PreValidationInfo { get; set; }

        /// <summary>
        /// This property will show the library if It's set to true
        /// </summary>
        [JsonProperty("ShowLibrary")]
        public bool ShowLibrary { get; set; }

        /// <summary>
        /// This propertu will hold information about the exit guide modal 
        /// </summary>
        [JsonProperty("ExitGuide")]
        internal ExitGuide ExitGuide { get; set; }

        public enum PointerDirection { TOP_RIGHT, TOP_LEFT, BOTTOM_RIGHT, BOTTOM_LEFT, BOTTOM_DOWN, NONE };

        /// <summary>
        /// This will contains the 3 points needed for drawing the Tooltip pointer direction
        /// </summary>
        public PointCollection TooltipPointerPoints { get; set; }

        /// <summary>
        /// This will contains the shadow direction in degrees that will be shown in the pointer
        /// </summary>
        public double ShadowTooltipDirection { get; set; }

        /// <summary>
        /// This property holds the DynamoViewModel that will be used when executing PreValidation functions
        /// </summary>
        internal DynamoViewModel DynamoViewModelStep { get; set; }

        /// <summary>
        /// This property is for the Visibility of each Popup in conditional flows, then it will decide if the this Step should be shown or not
        /// </summary>
        internal bool PreValidationIsOpenFlag { get; set; } = false;

        /// <summary>
        /// Guide Background that will be used by the Step to show or hide the highlight rectangle
        /// </summary>
        internal GuideBackground StepGuideBackground { get; set; }

        /// <summary>
        /// Main Window (DynamoView) that will be used by the Step for finding Child items (MenuItems) and calculate UIElement coordinates
        /// </summary>
        internal UIElement MainWindow { get; set; }

        internal string GuideName { get; set; }

        /// <summary>
        /// This will give access to the Popup instance so we can find UIElements on it.
        /// </summary>
        internal Popup StepUIPopup 
        {
            get
            {
                return stepUIPopup;
            }
        }


        /// <summary>
        /// This property contains the object of the current GuideManager
        /// </summary>
        internal GuidesManager GuideManager { get; set; }

        #endregion

        #region Protected Properties
        internal Popup stepUIPopup;
        protected Func<bool, bool> validator;
        /// <summary>
        /// This property describe which will be the pointing direction of the tooltip (if is a Welcome or Survey popup then is not used)
        /// By default the tooltip pointer will be pointing to the left and will be located at the top
        /// </summary>
        [JsonProperty("TooltipPointerDirection")]
        public PointerDirection TooltipPointerDirection { get; set; } = PointerDirection.TOP_LEFT;
        /// <summary>
        /// A vertical offfset to the pointer of the popups 
        /// </summary>
        [JsonProperty("PointerVerticalOffset")]
        public double PointerVerticalOffset { get; set; }        
        #endregion

        #region Public Methods

        /// <summary>
        /// Step constructor
        /// </summary>
        /// <param name="host">The host and popup information in which the popup will be shown</param>
        /// <param name="width">Popup Width</param>
        /// <param name="height">Popup Height</param>
        public Step(HostControlInfo host, double width, double height)
        {
            HostPopupInfo = host;
            Width = width;
            Height = height;
            UIAutomation = new List<StepUIAutomation>();
            CreatePopup();
        }

        /// <summary>
        /// Show the tooltip in the DynamoUI, first execute UI Automation, then Prevalidation, then calculate target host and finally highlight an element
        /// </summary>
        internal void Show(Guide.GuideFlow currentFlow)
        {
            //In case the UIAutomation info is set for the Step we execute all the UI Automation actions when the Next button is pressed
            if (UIAutomation != null)
            {
                foreach (var automation in UIAutomation)
                    ExecuteUIAutomationStep(automation, true, currentFlow);
            }

            //If the PreValidation info was read from the json file then is executed and it will decide which Step should be shown and which not
            if (PreValidationInfo != null)
            {
                ExecutePreValidation();
            }

            //After the UI Automation is done we need to calculate the Target (if is not in DynamoView)
            CalculateTargetHost();

            //After the Popup.PlacementTarget was recalculated (or calculated) then we proceed to put the cut off section
            SetCutOffSectionSize(true);

            //After UI Automation and calculate the target we need to highlight the element (otherwise probably won't exist)
            SetHighlightSection(true);

            if (HostPopupInfo.WindowName != null  && HostPopupInfo.WindowName.Equals(nameof(LibraryView)))
            {
                var automationStep = (from automation in UIAutomation
                                      where automation.Name.Equals(calculateLibraryFuncName)
                                      select automation).FirstOrDefault();
                GuidesValidationMethods.CalculateLibraryItemLocation(this, automationStep, true, Guide.GuideFlow.CURRENT);
            }
   

            stepUIPopup.IsOpen = true;

            if (this.StepUIPopup is PopupWindow popupWindow)
            {
                if (GuideUtilities.FindChild((popupWindow).mainPopupGrid, NextButton) is Button nextbuttonFound)
                {
                    if (popupWindow.TitleLabel.Content.Equals(Resources.PackagesGuideTermsOfServiceTitle))
                    {
                        nextbuttonFound.IsEnabled = false;
                    }
                    else
                    {
                        nextbuttonFound.Focus();
                    }
                }
            }
        }

        /// <summary>
        /// Hide the tooltip in the DynamoUI, first undo highlighting an element, then undo UI Automation
        /// </summary>
        internal void Hide(Guide.GuideFlow currentFlow)
        {
            stepUIPopup.IsOpen = false;

            if (HostPopupInfo.HtmlPage != null && !string.IsNullOrEmpty(HostPopupInfo.HtmlPage.FileName))
            {
                var webBrowser = (stepUIPopup as PopupWindow).webBrowserComponent;
                if(webBrowser != null)
                {
                    webBrowser.Dispose();
                }               
            }

            //Disable the HightlightArea functionality
            SetHighlightSection(false);

            //We need to remove the cutoff section from the Overlay for the current Step (it will be set again for the next Step)
            SetCutOffSectionSize(false);

            //Disable the current action automation that is executed for the Current Step (if there is one)
            if (UIAutomation != null)
            {
                foreach (var automation in UIAutomation)
                    ExecuteUIAutomationStep(automation, false, currentFlow);
            }
            
        }

        /// <summary>
        /// This method will update the Target in case the UI Automation was executed and the Popup was waiting for a specific Window to be opened
        /// </summary>
        internal void UpdatePlacementTarget()
        {
            if (stepUIPopup == null)
            {
                return;
            }
            //This means that the HostPopupInfo.HostUIElementString is in a different Window than DynamoView
            if (!string.IsNullOrEmpty(HostPopupInfo.WindowName)) 
            {
                Window ownedWindow = GuideUtilities.FindWindowOwned(HostPopupInfo.WindowName, MainWindow as Window);
                if (ownedWindow == null)  return;
                HostPopupInfo.HostUIElement = ownedWindow;
                stepUIPopup.PlacementTarget = ownedWindow;
                UpdateLocation();
            }
            //This case will be used for UIElements that are in the Dynamo VisualTree but they are shown until there is a user interaction (like the SideBar cases)
            var hostUIElement = GuideUtilities.FindChild(MainWindow, HostPopupInfo.HostUIElementString);
            if (hostUIElement == null)
                return;
            HostPopupInfo.HostUIElement = hostUIElement;
            stepUIPopup.PlacementTarget = hostUIElement;
            UpdateLocation();
        }

        /// <summary>
        /// This method will update the CutOff rectangle size everytime that the step change
        /// </summary>
        /// <param name="bVisible">It will say if the CutOff Area will be disabled or enabled</param>
        private void SetCutOffSectionSize(bool bVisible)
        {
            if (HostPopupInfo.CutOffRectArea == null)
                return;
            if(bVisible)
            {
                string windowElementNameCutOffSection = string.Empty;
                //In case the element name was not provided in the CutOffArea when we use the one in the HostPopup                 
                if (!string.IsNullOrEmpty(HostPopupInfo.CutOffRectArea.WindowElementNameString))
                    windowElementNameCutOffSection = HostPopupInfo.CutOffRectArea.WindowElementNameString;                
                else
                    windowElementNameCutOffSection = HostPopupInfo.HostUIElementString;

                //This will validate that HostPopupInfo.HostUIElement is in the MainWindow VisualTree otherwise the TransformToAncestor() will crash
                UIElement foundUIElement = null;
                if (!string.IsNullOrEmpty(windowElementNameCutOffSection))
                    foundUIElement = GuideUtilities.FindChild(MainWindow, HostPopupInfo.HostUIElementString);
                if(!string.IsNullOrEmpty(HostPopupInfo.CutOffRectArea.NodeId))
                    foundUIElement = GuideUtilities.FindNodeByID(MainWindow, HostPopupInfo.CutOffRectArea.NodeId);
                if (foundUIElement == null)
                    return;

                Point relativePoint = foundUIElement.TransformToAncestor(MainWindow)
                              .Transform(new Point(0, 0));

                relativePoint.X += HostPopupInfo.CutOffRectArea.XPosOffset;
                relativePoint.Y += HostPopupInfo.CutOffRectArea.YPosOffset;

                var holeWidth = foundUIElement.DesiredSize.Width + HostPopupInfo.CutOffRectArea.WidthBoxDelta;
                var holeHeight = foundUIElement.DesiredSize.Height + HostPopupInfo.CutOffRectArea.HeightBoxDelta;

                if (StepGuideBackground.CutOffBackgroundArea != null)
                {
                    StepGuideBackground.CutOffBackgroundArea.CutOffRect = new Rect(relativePoint.X, relativePoint.Y, holeWidth, holeHeight);
                }
            }
            else
            {
                StepGuideBackground.ClearCutOffSection();
            }
        }

        /// <summary>
        /// This method will set the highlight rectangle color if there is any configured in the json file
        /// </summary>
        /// <param name="bVisible">It will say if the Highlight Area will be disabled or enabled</param>
        private void SetHighlightSection(bool bVisible)
        {
            if (HostPopupInfo.HighlightRectArea == null)
                return;
            if (bVisible)
            {
                if (!string.IsNullOrEmpty(HostPopupInfo.HighlightRectArea.UIElementTypeString))
                    HighlightWindowElement(bVisible);
                else
                {
                    //If is not empty means that the HighlightRectArea.WindowElementNameString doesn't belong to the DynamoView then another way for hightlighting the element will be applied
                    if (!string.IsNullOrEmpty(HostPopupInfo.HighlightRectArea.WindowName)) return;

                    string highlightColor = HostPopupInfo.HighlightRectArea.HighlightColor;

                    //This section will get the X,Y coordinates of the HostUIElement based in the Ancestor UI Element so we can put the highlight rectangle
                    Point relativePoint = HostPopupInfo.HostUIElement.TransformToAncestor(MainWindow)
                                      .Transform(new Point(0, 0));

                    var holeWidth = HostPopupInfo.HostUIElement.DesiredSize.Width + HostPopupInfo.HighlightRectArea.WidthBoxDelta;
                    var holeHeight = HostPopupInfo.HostUIElement.DesiredSize.Height + HostPopupInfo.HighlightRectArea.HeightBoxDelta;

                    StepGuideBackground.HighlightBackgroundArea.SetHighlighRectSize(relativePoint.Y, relativePoint.X, holeWidth, holeHeight);

                    if (string.IsNullOrEmpty(highlightColor))
                    {
                        StepGuideBackground.GuideHighlightRectangle.Stroke = Brushes.Transparent;
                    }
                    else
                    {
                        var converter = new BrushConverter();
                        var brush = (Brush)converter.ConvertFromString(highlightColor);
                        StepGuideBackground.GuideHighlightRectangle.Stroke = brush;
                    }
                }          
            }
            else
            {
                HighlightWindowElement(bVisible);
                StepGuideBackground.ClearHighlightSection();
            }

            //Hit tests is set to false so that the content inside the rectangle can be clicked
            StepGuideBackground.GuideHighlightRectangle.IsHitTestVisible = false;
        }


        /// <summary>
        /// This method will update the Popup location by calling the private method UpdatePosition using reflection (just when the PlacementTarget is moved or resized).
        /// </summary>
        public void UpdateLocation()
        {
            SetCutOffSectionSize(true);
            UpdatePopupLocationInvoke(stepUIPopup);
            if(stepUIPopup is PopupWindow)
            {
                var stepUiPopupWindow = (PopupWindow)stepUIPopup;
            }
        }

        /// <summary>
        /// Update the Location of Popups only when they are located over the Library (HostPopupInfo.WindowName = LibraryView)
        /// </summary>
        internal void UpdateLibraryPopupsLocation()
        {
            if (HostPopupInfo.WindowName != null && HostPopupInfo.WindowName.Equals(nameof(LibraryView)))
            {
                var automationScrollDownStep = (from automation in UIAutomation
                                      where automation.Name.Equals(libraryScrollToBottomFuncName)
                                      select automation).FirstOrDefault();
                if (automationScrollDownStep == null) return;
                GuidesValidationMethods.LibraryScrollToBottom(this, automationScrollDownStep, true, Guide.GuideFlow.CURRENT);

                var automationCalculateStep = (from automation in UIAutomation
                                      where automation.Name.Equals(calculateLibraryFuncName)
                                      select automation).FirstOrDefault();
                if (automationCalculateStep == null) return;
                GuidesValidationMethods.CalculateLibraryItemLocation(this, automationCalculateStep, true, Guide.GuideFlow.CURRENT);
            }
        }

        /// <summary>
        /// This method will update the interactions of the Popup with the Library like the highligthed items or the event subscriptions
        /// </summary>
        internal void UpdateLibraryInteractions()
        {
            if (UIAutomation == null) return;
            var jsAutomations = from automation in UIAutomation
                                 where !string.IsNullOrEmpty(automation.JSFunctionName)
                                 select automation;
            var automationSubscribePackage = (from automation in jsAutomations
                                              where automation.JSFunctionName.Equals(subscribePackageClickedFuncName)
                                              select automation).FirstOrDefault();
            if (automationSubscribePackage == null) return;
            ExecuteUIAutomationStep(automationSubscribePackage, true, Guide.GuideFlow.FORWARD);

            SetHighlightSection(true);
        }

      

        /// <summary>
        /// This method will update the Popup vertical location
        /// </summary>
        /// <param name="verticalPosition"></param>
        internal void UpdatePopupVerticalPlacement(double verticalPosition)
        {
            stepUIPopup.VerticalOffset = verticalPosition + HostPopupInfo.VerticalPopupOffSet;
            UpdatePopupLocationInvoke(stepUIPopup);
        }


        private void UpdatePopupLocationInvoke(Popup popUp)
        {
            if(popUp != null && popUp.IsOpen)
            {
                var positionMethod = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                positionMethod.Invoke(popUp, null);
            }
        }
        
        /// <summary>
        /// This method will execute an UIAutomation action over a specific UIElement
        /// </summary>
        /// <param name="uiAutomationData">UIAutomation info read from a json file</param>
        /// <param name="enableUIAutomation">Enable/Disable the automation action for a specific UIElement</param>
        private void ExecuteUIAutomationStep(StepUIAutomation uiAutomationData, bool enableUIAutomation, Guide.GuideFlow currentFlow)
        {
            var popupBorderName = "SubmenuBorder";
            //This section will search the UIElement dynamically in the Dynamo VisualTree in which an automation action will be executed
            UIElement automationUIElement = GuideUtilities.FindChild(MainWindow, uiAutomationData.Name);
            if (automationUIElement != null)
                uiAutomationData.UIElementAutomation = automationUIElement;
          
            switch (uiAutomationData.ControlType)
            {
                case StepUIAutomation.UIControlType.MENUITEM:
                    if (uiAutomationData.UIElementAutomation == null)
                    {
                        return;
                    }
                    MenuItem menuEntry = uiAutomationData.UIElementAutomation as MenuItem;
                    if (menuEntry == null) return;
                    switch(uiAutomationData.Action)
                    {
                        case StepUIAutomation.UIAction.OPEN:
                            menuEntry.IsSubmenuOpen = enableUIAutomation;
                            menuEntry.StaysOpenOnClick = enableUIAutomation;
                            break;                    
                    }
                    //Means that the Popup location needs to be updated after the MenuItem is opened
                    if(uiAutomationData.UpdatePlacementTarget)
                    {
                        //We need to find the border inside the MenuItem.Popup so we can get its Width (this template is defined in MenuStyleDictionary.xaml)
                        var menuPopupBorder = menuEntry.Template.FindName(popupBorderName, menuEntry) as Border;
                        if (menuPopupBorder == null) return;

                        //We need to do this substraction so the Step Popup wil be located at the end of the MenuItem Popup.
                        stepUIPopup.HorizontalOffset = (menuPopupBorder.ActualWidth - menuEntry.ActualWidth) + HostPopupInfo.HorizontalPopupOffSet;
                        UpdatePopupLocationInvoke(stepUIPopup);
                    }
                    break;
                //In this case the UI Automation will be done using a Function located in the static class GuidesValidationMethods
                case StepUIAutomation.UIControlType.FUNCTION:
                    MethodInfo builderMethod = typeof(GuidesValidationMethods).GetMethod(uiAutomationData.Name, BindingFlags.Static | BindingFlags.NonPublic);
                    object[] parametersArray = new object[] { this, uiAutomationData, enableUIAutomation, currentFlow};
                    builderMethod.Invoke(null, parametersArray);
                    //If UpdatePlacementTarget = true then means that a new Window was opened after executing the funtion then we need to update the Popup.PlacementTarget
                    if (uiAutomationData.UpdatePlacementTarget)
                    {
                        UpdatePlacementTarget();
                    }
                    break;
                //In this case the UI Automation will be done over a WPF Button 
                case StepUIAutomation.UIControlType.BUTTON:
                    if (string.IsNullOrEmpty(uiAutomationData.WindowName)) return;
                    
                    //This means that the Button is in a PopupWindow (instead of the DynamoView) so we need to find the button and then apply the automation
                    if(uiAutomationData.WindowName.Equals(WindowNamePopup))
                    {
                        //Finds the Button inside the PopupWindow
                        var buttonFound = GuideUtilities.FindChild((stepUIPopup as PopupWindow).mainPopupGrid, uiAutomationData.Name) as Button;
                        if (buttonFound == null) return;

                        switch (uiAutomationData.Action)
                        {
                            case StepUIAutomation.UIAction.DISABLE:
                                if (enableUIAutomation)
                                    buttonFound.IsEnabled = false;
                                else
                                    buttonFound.IsEnabled = true;
                                break;
                        }                      
                    }
                    break;
                case StepUIAutomation.UIControlType.JSFUNCTION:
                    if (string.IsNullOrEmpty(uiAutomationData.JSFunctionName)) return;
                    //We need to create a new list for the parameters due that we will be adding the enableUIAutomation boolean value
                    var parametersJSFunction = new List<object>(uiAutomationData.JSParameters);
                    parametersJSFunction.Add(enableUIAutomation);
                    //Create the array for the parameters that will be sent to the JS Function
                    object[] jsParameters = parametersJSFunction.ToArray();                 
                    //Create the array for the paramateres that will be sent to the WebBrowser.InvokeScript Method
                    object[] parametersInvokeScript = new object[] { uiAutomationData.JSFunctionName, jsParameters };
                    //Execute the JS function with the provided parameters
                    ResourceUtilities.ExecuteJSFunction(MainWindow, HostPopupInfo, parametersInvokeScript);
                    break;
            }
        }

        /// <summary>
        /// This method will execute the PreValidation action/method for this Step.
        /// </summary>
        internal void ExecutePreValidation()
        {
            object[] parametersArray;
            Window ownedWindow = GuideUtilities.FindWindowOwned(HostPopupInfo.WindowName, MainWindow as Window);

            if (PreValidationInfo != null)
            {
                if (PreValidationInfo.ControlType.Equals("visibility"))
                {
                    if (!string.IsNullOrEmpty(PreValidationInfo.FuncName))
                    {
                        MethodInfo builderMethod = typeof(GuidesValidationMethods).GetMethod(PreValidationInfo.FuncName, BindingFlags.Static | BindingFlags.NonPublic);

                        //Checks if needs to execute 'IsPackageInstalled' method to include the right parameters
                        if (PreValidationInfo.FuncName.Equals("IsPackageInstalled"))
                        {
                            PackageManager.PackageManagerSearchViewModel viewModel = null;

                            if (ownedWindow != null)
                            {
                                viewModel = ownedWindow.DataContext as PackageManager.PackageManagerSearchViewModel;
                            }

                            parametersArray = new object[] { viewModel };
                        }
                        else
                        {
                            parametersArray = new object[] { DynamoViewModelStep };
                        }

                        var validationResult = (bool)builderMethod.Invoke(null, parametersArray);

                        bool expectedValue = bool.Parse(PreValidationInfo.ExpectedValue);

                        //Once the execution of the PreValidation method was done we compare the result against the expected (also described in the json) so we set a flag
                        if (validationResult == expectedValue)
                            PreValidationIsOpenFlag = true;
                        else
                            PreValidationIsOpenFlag = false;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the Popup.PlacementTarget dynamically if is the case and highlight the sub MenuItem if the information was provided
        /// </summary>
        /// <param name="bVisible">When the Step is shown this variable will be false when is hidden(due to passing to the next Step) it will be false</param>
        internal void CalculateTargetHost()
        {
            if (HostPopupInfo.DynamicHostWindow == true)
            {
                UpdatePlacementTarget();
            }
        }

        /// <summary>
        /// This function will highlight a Window element (the element can be located in DynamoView or another Window or can be a MenuItem
        /// <param name="bVisible">Indicates if the highlight should be applied or removed</param>
        internal void HighlightWindowElement(bool bVisible)
        {
            //Check if the HighlightRectArea was provided in the json file and the HostUIElement was found in the DynamoView VisualTree
            if (HostPopupInfo.HighlightRectArea == null || HostPopupInfo.HostUIElement == null)
            {
                return;
            }
            //Check if the WindowElementNameString was provided in the json and is not empty
            if (string.IsNullOrEmpty(HostPopupInfo.HighlightRectArea.WindowElementNameString))
            {
                return;
            }

            //If is MenuItem type means that the Popup.TargetPlacement will be calculated dinamically
            if (HostPopupInfo.HighlightRectArea.UIElementTypeString.Equals(typeof(MenuItem).Name))
            {
                //We try to find the WindowElementNameString (in this case the MenuItem) in the DynamoView VisualTree
                var foundUIElement = GuideUtilities.FindChild(HostPopupInfo.HostUIElement, HostPopupInfo.HighlightRectArea.WindowElementNameString);

                if (foundUIElement != null)
                {
                    var subMenuItem = foundUIElement as MenuItem;

                    //If the HighlightRectArea.WindowElementNameString described is a MenuItem (Dynamo menu) then we need to add the Rectangle dynamically to the Template
                    HighlightMenuItem(subMenuItem, bVisible);
                }
            }
            //The HighlightRectArea.UIElementTypeString was provided but the type is DynamoView then we will search the element in the DynamoView VisualTree
            else if (HostPopupInfo.HighlightRectArea.UIElementTypeString.Equals(typeof(DynamoView).Name))
            {
                string highlightColor = HostPopupInfo.HighlightRectArea.HighlightColor;

                //Find the in the DynamoView VisualTree the specified Element (WindowElementNameString)
                var hostUIElement = GuideUtilities.FindChild(MainWindow, HostPopupInfo.HighlightRectArea.WindowElementNameString);

                if (hostUIElement == null)
                {
                    return;
                }

                //If the Element was found we need to calculate the X,Y coordinates based in the UIElement Ancestor
                Point relativePoint = hostUIElement.TransformToAncestor(MainWindow)
                            .Transform(new Point(0, 0));

                var holeWidth = hostUIElement.DesiredSize.Width + HostPopupInfo.HighlightRectArea.WidthBoxDelta;
                var holeHeight = hostUIElement.DesiredSize.Height + HostPopupInfo.HighlightRectArea.HeightBoxDelta;

                //Activate the Highlight rectangle from the GuideBackground
                StepGuideBackground.HighlightBackgroundArea.SetHighlighRectSize(relativePoint.Y, relativePoint.X, holeWidth, holeHeight);

                if (string.IsNullOrEmpty(highlightColor))
                {
                    StepGuideBackground.GuideHighlightRectangle.Stroke = Brushes.Transparent;
                }
                else
                {
                    //This section will put the desired color in the Highlight rectangle (read from the json file)
                    var converter = new BrushConverter();
                    var brush = (Brush)converter.ConvertFromString(highlightColor);
                    StepGuideBackground.GuideHighlightRectangle.Stroke = brush;
                }
            }
            //This case is for when the item to be highlighted is inside the LibraryView (WebBrowser component) 
            else if (HostPopupInfo.HighlightRectArea.UIElementTypeString.Equals(typeof(WebBrowser).Name))
            {
                //We need to access the WebBrowser instance and call a js function to highlight the html div border of the item
                HighlightLibraryItem(bVisible);
            }
            //If the UIElementTypeString is not a MenuItem and also not a DynamoView we need to find the Window in the OwnedWindows and search the element inside it
            else
            {
                string highlightColor = HostPopupInfo.HighlightRectArea.HighlightColor;
                Window ownedWindow = GuideUtilities.FindWindowOwned(HostPopupInfo.HighlightRectArea.WindowName, MainWindow as Window);
                if (ownedWindow == null) return;
                UIElement foundElement = GuideUtilities.FindChild(ownedWindow, HostPopupInfo.HighlightRectArea.WindowElementNameString);
                switch (HostPopupInfo.HighlightRectArea.UIElementTypeString.ToUpper())
                {
                    //We need to highlight a Button (if the Button template doesn't have a grid then the template needs to be updated)
                    case "BUTTON":
                        var buttonElement = foundElement as Button;
                        if (buttonElement == null) return;

                        //We will be searching for the Grid name provided in the json file and then add the Highlight Rectangle
                        var bordersGrid = buttonElement.Template.FindName(HostPopupInfo.HighlightRectArea.UIElementGridContainer, buttonElement) as Grid;
                        if (bordersGrid == null) return;

                        if (bVisible)
                        {
                            var buttonRectangle = CreateRectangle(bordersGrid, HostPopupInfo.HighlightRectArea.HighlightColor);
                            //The Rectangle will be added dynamically in a specific step and then when passing to next step we will remove it
                            bordersGrid.Children.Add(buttonRectangle);

                        }
                        else
                        {
                            //When we need to undo the highlight we find the Rectangle and remove it
                            var buttonRectangle = bordersGrid.Children.OfType<Rectangle>().Where(rect => rect.Name.Equals("HighlightRectangle")).FirstOrDefault();
                            if (buttonRectangle != null)
                                bordersGrid.Children.Remove(buttonRectangle);
                        }
                        break;
                }
            }        
        }

        /// <summary>
        /// Shows the Highlight rectangle or hides it depending of the bVisible parameter
        /// </summary>
        /// <param name="highlighMenuItem"></param>
        /// <param name="bVisible">True for showing the Highlight rectangle otherwise is false</param>
        internal void HighlightMenuItem(MenuItem highlighMenuItem, bool bVisible)
        {
            if (highlighMenuItem == null)
                return;

            //Due that for this Step we are using the Rectangle located in the ItemMenu we need to hide the GuideBackground Rectangle
            if (StepGuideBackground != null)
                StepGuideBackground.GuideHighlightRectangle.Stroke = new SolidColorBrush(Colors.Transparent);

            //Get the Grid in which the Rectangle was added so we can execute the animation with Storyboard.Begin
            Grid subItemsGrid = highlighMenuItem.Template.FindName("SubmenuItemGrid", highlighMenuItem) as Grid;
            if (subItemsGrid == null) return;
            if (bVisible)
            {
                //Create the rectangle with the specified color and adds the animation
                var menuItemRectangle = CreateRectangle(subItemsGrid, HostPopupInfo.HighlightRectArea.HighlightColor);

                //The Rectangle will be added dynamically in a specific step and then when passing to next step we will remove it
                subItemsGrid.Children.Add(menuItemRectangle);
                Grid.SetColumn(menuItemRectangle, 0);
                Grid.SetColumnSpan(menuItemRectangle, 2);
            }
            else
            {
                var menuItemHighlightRect = subItemsGrid.Children.OfType<Rectangle>().Where(rect => rect.Name.Equals("HighlightRectangle")).FirstOrDefault();
                if(menuItemHighlightRect != null)
                    subItemsGrid.Children.Remove(menuItemHighlightRect);
            }
        }

        /// <summary>
        /// This method will execute a js function for highlighting a item (<div></div>) in the WebBrowser instance located inside the LibraryView using reflection
        /// </summary>
        /// <param name="visible">enable or disable the highlight in a specific Library item</param>
        internal void HighlightLibraryItem(bool visible)
        {
            const string jsMethodName = "highlightLibraryItem";
            object[] parametersInvokeScript = new object[] { jsMethodName, new object[] { HostPopupInfo.HighlightRectArea.WindowElementNameString, visible } };
            ResourceUtilities.ExecuteJSFunction(MainWindow, HostPopupInfo, parametersInvokeScript);
        }

        /// <summary>
        /// This method will create a Rectangle with glow effect and animation
        /// </summary>
        /// <param name="targetElement">the element in which the rectangle will be animated (basically is for creating the Scope)</param>
        /// <param name="recColor">string representing the rectangle color</param>
        /// <returns>The Rectangle with the animation started</returns>
        internal Rectangle CreateRectangle(FrameworkElement targetElement, string recColor)
        {
            //This is the effect that will be animated with the StoryBoard
            var blur = new BlurEffect()
            {
                Radius = 1.0,
                KernelType = KernelType.Box
            };
            var converter = new BrushConverter();
            Rectangle menuItemHighlightRec = new Rectangle
            {
                Focusable = false,                
                Name = "HighlightRectangle",
                StrokeThickness = 2,
                Effect = blur,
                Stroke = (Brush)converter.ConvertFromString(recColor)
            };

            //We need to create an Scope and Register the Rectangle so the WPF XAML Processor can find the Rectangle.Name
            NameScope.SetNameScope(targetElement, new NameScope());
            targetElement.RegisterName(menuItemHighlightRec.Name, menuItemHighlightRec);

            //This is the animation over the BlurEffect.Radius that will be applied
            DoubleAnimation glowAnimation = new DoubleAnimation(0.0, 4.0, new Duration(TimeSpan.FromSeconds(1)));
            glowAnimation.AutoReverse = true;
            glowAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(glowAnimation, menuItemHighlightRec.Name);
            Storyboard.SetTargetProperty(glowAnimation, new PropertyPath("(Effect).Radius"));

            Storyboard myStoryboard = new Storyboard();
            myStoryboard.Children.Add(glowAnimation);
            myStoryboard.Begin(targetElement);

            return menuItemHighlightRec;
        }

        /// <summary>
        /// This method will be used to update the pointer direction if is needed
        /// </summary>
        /// <param name="direction"></param>
        public void SetPointerDirection(PointerDirection direction)
        {
            TooltipPointerDirection = direction;
        }

        /// <summary>
        /// This virtual method should be overriden by the childs and creates the Popup (SURVEY, TOOLTIP, WELCOME, EXIT_TOUR)
        /// </summary>
        protected virtual void CreatePopup() { }
        #endregion
    }

    public class Content
    {
        #region Private Fields
        private string formattedText;
        #endregion

        #region Public Properties
        /// <summary>
        /// Title of the Popup shown at the top-center
        /// </summary>
        [JsonProperty("Title")]
        public string Title { get; set; }

        /// <summary>
        /// The content of the Popup using a specific format for showing text, hyperlinks,images, bullet points in a RichTextBox
        /// </summary>
        [JsonProperty("FormattedText")]
        public string FormattedText
        {
            get
            {
                return formattedText;
            }
            set
            {
                //Because we are reading the value from a Resource, the \n is converted to char escaped and we need to replace it by the special char
                if(value != null)
                    formattedText = value.Replace("\\n", Environment.NewLine);
            }
        }
        #endregion
    }

    /// <summary>
    /// This class will be used in some specific cases in which the Popup needs extra information to be displayed in the UI
    /// </summary>
    public class ExtraContent
    {
        [JsonProperty("Property")]
        public string Property { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}
