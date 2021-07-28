using System;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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

        public enum StepTypes{ SURVEY, TOOLTIP, WELCOME, EXIT_TOUR };

        /// <summary>
        /// The step type will describe which type of window will be created after reading the json file, it can be  SURVEY, TOOLTIP, WELCOME, EXIT_TOUR
        /// </summary>
        [JsonProperty("type")]
        public StepTypes StepType { get; set; }

        /// <summary>
        /// The step content will contain the title and the popup content (included formatted text)
        /// </summary>
        [JsonProperty("step_content")]
        public Content StepContent { get; set; }

        /// <summary>
        /// There are some specific Steps that will contain extra content (like Survey.RatingTextTitle), then this list will store the information
        /// </summary>
        [JsonProperty("extra_content")]
        public List<ExtraContent> StepExtraContent { get; set; }

        /// <summary>
        /// Step name, it just represent a step identifier
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// When the Step Width property is not provided the default value will be 480
        /// </summary>
        [JsonProperty("width")]
        public double Width { get; set; } = 480;

        /// <summary>
        /// When the Step Height property is not provide the default value will be 190
        /// </summary>
        [JsonProperty("height")]
        public double Height { get; set; } = 190;

        /// <summary>
        /// Represent a sequencial numeric value for each step, when is a multiflow guide this value can be repeated
        /// </summary>
        [JsonProperty("sequence")]
        public int Sequence { get; set; } = 0;

        /// <summary>
        /// This property contains the Host information like the host popup element or the popup position
        /// </summary>
        [JsonProperty("host_popup_info")]
        public HostControlInfo HostPopupInfo { get; set; }

        public enum PointerDirection { TOP_RIGHT, TOP_LEFT, BOTTOM_RIGHT, BOTTOM_LEFT };

        /// <summary>
        /// This will contains the 3 points needed for drawing the Tooltip pointer direction
        /// </summary>
        public PointCollection TooltipPointerPoints { get; set; }
        #endregion

        #region Protected Properties
        protected Popup stepUIPopup;
        protected Func<bool, bool> validator;
        /// <summary>
        /// This property describe which will be the pointing direction of the tooltip (if is a Welcome or Survey popup then is not used)
        /// By default the tooltip pointer will be pointing to the left and will be located at the top
        /// </summary>
        [JsonProperty("pointer_direction")]
        public PointerDirection TooltipPointerDirection { get; set; } = PointerDirection.TOP_LEFT;
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
            CreatePopup();
        }

        /// <summary>
        /// Show the tooltip in the DynamoUI
        /// </summary>
        public void Show()
        {
            stepUIPopup.IsOpen = true;
        }

        /// <summary>
        /// Hide the tooltip in the DynamoUI
        /// </summary>
        public void Hide()
        {
            stepUIPopup.IsOpen = false;
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
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// The content of the Popup using a specific format for showing text, hyperlinks,images, bullet points in a RichTextBox
        /// </summary>
        [JsonProperty("formatted_text")]
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
        [JsonProperty("property")]
        public string Property { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
