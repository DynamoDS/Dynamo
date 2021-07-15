using System;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This abstract class include several properties that will be used for childs like Survey, Welcome, Tooltip, ExitTour ....
    /// </summary>
    public abstract class Step
    {
        #region Public Properties
        /// <summary>
        /// The step content will contain the title and the popup content (included formatted text)
        /// </summary>
        public Content StepContent { get; set; }

        /// <summary>
        /// Step name, it just represent a step identifier
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// When the Step Width property is not provided the default value will be 480
        /// </summary>
        public double Width { get; set; } = 480;

        /// <summary>
        /// When the Step Height property is not provide the default value will be 190
        /// </summary>
        public double Height { get; set; } = 190;

        /// <summary>
        /// Represent a sequencial numeric value for each step, when is a multiflow guide this value can be repeated
        /// </summary>
        public int Sequence { get; set; } = 0;

        /// <summary>
        /// This static variable will be shared by all the steps and represent the total number of steps that the guide has.
        /// </summary>
        public static int TotalSteps { get; set; } = 8;

        /// <summary>
        /// This property contains the Host information like the host popup element or the popup position
        /// </summary>
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
        protected PointerDirection TooltipPointerDirection { get; set; } = PointerDirection.TOP_LEFT;
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
        /// This abstract method need to be overriden by the childs and creates the Popup
        /// </summary>
        protected abstract void CreatePopup();
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
        public string Title { get; set; }

        /// <summary>
        /// The content of the Popup using a specific format for showing text, hyperlinks,images, bullet points in a RichTextBox
        /// </summary>
        public string FormattedText 
        { 
            get
            {
                return formattedText;
            }
            set
            {
                //Because we are reading the value from a Resource, the \n is converted to char escaped and we need to replace it by the special char
                formattedText = value.Replace("\\n", Environment.NewLine);
            }
        }
        #endregion
    }
}
