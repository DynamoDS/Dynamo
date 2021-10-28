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
using Dynamo.ViewModels;
using Dynamo.Wpf.Views.GuidedTour;
using Newtonsoft.Json;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This abstract class include several properties that will be used for childs like Survey, Welcome, Tooltip, ExitTour ....
    /// </summary>
    public class Step
    {
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

        public enum PointerDirection { TOP_RIGHT, TOP_LEFT, BOTTOM_RIGHT, BOTTOM_LEFT };

        /// <summary>
        /// This will contains the 3 points needed for drawing the Tooltip pointer direction
        /// </summary>
        public PointCollection TooltipPointerPoints { get; set; }

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

        #endregion

        #region Protected Properties
        protected Popup stepUIPopup;
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
        /// Show the tooltip in the DynamoUI
        /// </summary>
        public void Show()
        {
            stepUIPopup.IsOpen = true;

            //In case the UIAutomation info is set for the Step we execute all the UI Automation actions when the Next button is pressed
            if (UIAutomation != null)
            {
                foreach (var automation in UIAutomation)
                    ExecuteUIAutomationStep(automation, true);
            }

            //If the PreValidation info was read from the json file then is executed and it will decide which Step should be shown and which not
            if (PreValidationInfo != null)
            {
                ExecutePreValidation();
            }

            CalculateTargetHost(true);
        }

        /// <summary>
        /// Hide the tooltip in the DynamoUI
        /// </summary>
        public void Hide()
        {
            stepUIPopup.IsOpen = false;

            //Disable the current action automation that is executed for the Current Step (if there is one)
            if (UIAutomation != null)
            {
                foreach (var automation in UIAutomation)
                    ExecuteUIAutomationStep(automation, false);
            }
            CalculateTargetHost(false);
        }

        /// <summary>
        /// This method will update the Popup location by calling the private method UpdatePosition using reflection (just when the PlacementTarget is moved or resized).
        /// </summary>
        public void UpdateLocation()
        {
            UpdatePopupLocationInvoke(stepUIPopup);
            if(stepUIPopup is PopupWindow)
            {
                var stepUiPopupWindow = (PopupWindow)stepUIPopup;
                UpdatePopupLocationInvoke(stepUiPopupWindow?.webBrowserWindow);
            }
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
        private void ExecuteUIAutomationStep(StepUIAutomation uiAutomationData, bool enableUIAutomation)
        {
            switch (uiAutomationData.ControlType.ToUpper())
            {
                case "MENUITEM":
                    if (uiAutomationData.UIElementAutomation != null)
                    {
                        if (uiAutomationData.Action.ToUpper().Equals("OPEN"))
                        {
                            MenuItem menuEntry = uiAutomationData.UIElementAutomation as MenuItem;
                            menuEntry.IsSubmenuOpen = enableUIAutomation;
                            menuEntry.StaysOpenOnClick = enableUIAutomation;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// This method will execute the PreValidation action/method for this Step.
        /// </summary>
        internal void ExecutePreValidation()
        {
            if (PreValidationInfo != null)
            {
                if (PreValidationInfo.ControlType.Equals("visibility"))
                {
                    if (!string.IsNullOrEmpty(PreValidationInfo.FuncName))
                    {
                        //Due that the function name was read from a json file then we need to use Reflection for executing the Static method in the GuidesValidationMethods class 
                        MethodInfo builderMethod = typeof(GuidesValidationMethods).GetMethod(PreValidationInfo.FuncName, BindingFlags.Static | BindingFlags.NonPublic);
                        object[] parametersArray = new object[] { DynamoViewModelStep };
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
        internal void CalculateTargetHost(bool bVisible)
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

            //If the HighlightRectArea.UIElementTypeString was provided in the json file and is MenuItem type means that the Popup.TargetPlacement will be calculated dinamically due that the element is not in the DynamoView VisualTree.
            if (HostPopupInfo.HighlightRectArea.UIElementTypeString.Equals(typeof(MenuItem).Name))
            {
                //We try to find the WindowElementNameString in the DynamoView VisualTree
                var foundUIElement = Guide.FindChild(HostPopupInfo.HostUIElement, HostPopupInfo.HighlightRectArea.WindowElementNameString);

                if(foundUIElement != null)
                {
                    var subMenuItem = foundUIElement as MenuItem;

                    //If the HighlightRectArea.WindowElementNameString described is a MenuItem (Dynamo menu) then we need to activate the Rectangle in the MenuStyleDictionary.xaml style
                    HighlightMenuItem(subMenuItem, bVisible);
                }
            }
            //The HighlightRectArea.UIElementTypeString was provided but the type is DynamoView then we will search the element in the DynamoView VisualTree
            else if (HostPopupInfo.HighlightRectArea.UIElementTypeString.Equals(typeof(DynamoView).Name))
            {
                string highlightColor = HostPopupInfo.HighlightRectArea.HighlightColor;

                //Find the in the DynamoView VisualTree the specified Element (WindowElementNameString)
                var hostUIElement = Guide.FindChild(MainWindow, HostPopupInfo.HighlightRectArea.WindowElementNameString);

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

            if (bVisible)
            {
                var menuItem = (HostPopupInfo.HostUIElement as MenuItem);
                
                //This is the effect that will be animated with the StoryBoard
                var blur = new BlurEffect()
                {
                    Radius = 1.0,
                    KernelType = KernelType.Box
                };
                var converter = new BrushConverter();
                Rectangle menuItemHighlightRec = new Rectangle
                {
                    Name = "HighlightRectangle",
                    StrokeThickness = 2,
                    Effect = blur,
                    Stroke = (Brush)converter.ConvertFromString(HostPopupInfo.HighlightRectArea.HighlightColor)
                };

                //This is the animation over the BlurEffect.Radius that will be applied
                DoubleAnimation glowAnimation = new DoubleAnimation(0.0, 4.0, new Duration(TimeSpan.FromSeconds(1)));
                glowAnimation.AutoReverse = true;
                glowAnimation.RepeatBehavior = RepeatBehavior.Forever;
                Storyboard.SetTargetName(glowAnimation, menuItemHighlightRec.Name);
                Storyboard.SetTargetProperty(glowAnimation, new PropertyPath("(Effect).Radius"));

                //Get the Grid in which the Rectangle was added so we can execute the animation with Storyboard.Begin
                Grid subItemsGrid = highlighMenuItem.Template.FindName("SubmenuItemGrid", highlighMenuItem) as Grid;

                //We need to create an Scope and Register the Rectangle so the WPF XAML Processor can find the Rectangle.Name
                NameScope.SetNameScope(subItemsGrid, new NameScope());
                subItemsGrid.RegisterName(menuItemHighlightRec.Name, menuItemHighlightRec);

                Storyboard myStoryboard = new Storyboard();
                myStoryboard.Children.Add(glowAnimation);
                myStoryboard.Begin(subItemsGrid);

                //The Rectangle will be added dynamically in a specific step and then when passing to next step we will remove it
                subItemsGrid.Children.Add(menuItemHighlightRec);
                Grid.SetColumn(menuItemHighlightRec, 0);
                Grid.SetColumnSpan(menuItemHighlightRec, 2);
            }
            else
            {
                //The HighlightRectangle needs to be removed once we hide to move to the next Step
                Grid subItemsGrid = highlighMenuItem.Template.FindName("SubmenuItemGrid", highlighMenuItem) as Grid;
                var menuItemHighlightRect = subItemsGrid.Children.OfType<Rectangle>().Where(rect => rect.Name.Equals("HighlightRectangle")).FirstOrDefault();
                if(menuItemHighlightRect != null)
                    subItemsGrid.Children.Remove(menuItemHighlightRect);
            }
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
