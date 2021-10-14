using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Wpf.Views.GuidedTour;
using Dynamo.ViewModels;
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
