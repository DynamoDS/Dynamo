using Dynamo.Utilities;
using System.Collections.Generic;
using System.Windows;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class will be used as a container for the UI Automation information read from the json file
    /// </summary>
    public class StepUIAutomation
    {
        /// <summary>
        /// Represent all the actions that can be done when executing UI Automation
        /// </summary>
        public enum UIAction { OPEN, DISABLE, EXECUTE };

        /// <summary>
        /// Represent all Control Types that can be used in UI Automation for executing actions like disable or open
        /// </summary>
        public enum UIControlType { MENUITEM, BUTTON, FUNCTION, JSFUNCTION };

        /// <summary>
        /// This Sequence will be unique for each automation step
        /// </summary>
        public double Sequence { get; set; }

        /// <summary>
        /// The Control type represent the WPF Control type that will be Automated, like MenuItem, Window, Dropdown
        /// </summary>
        public UIControlType ControlType { get; set; }

        /// <summary>
        /// This contain a string ID of the UI Automation 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This will represent the action that will be executed as part of the UI Automation
        /// </summary>
        public UIAction Action { get; set; }

        /// <summary>
        /// The list of arguments sent to the function to be executed
        /// </summary>
        public List<object> Parameters { get; set; }

        /// <summary>
        /// This will be the WPF UI Element in which the Action will be executed
        /// </summary>
        public UIElement UIElementAutomation { get; set; }

        /// <summary>
        /// This property will if that after executing the UI Automation action the Popup.PlacementTarget needs to be updated
        /// </summary>
        public bool UpdatePlacementTarget { get; set; }

        /// <summary>
        /// This property will decide if we should undo the UI Automation Steps when moving Forward in the Guide Flow
        /// </summary>
        public bool ExecuteCleanUpForward { get; set; }

        /// <summary>
        /// This property will decide if we should undo the UI Automation Steps when moving Backward in the Guide Flow
        /// </summary>
        public bool ExecuteCleanUpBackward { get; set; }

        /// <summary>
        /// This property will be used when we need to do UI Automation over an element that is not in the DynamoView (For example the Accept button in the TermsOfUseView)
        /// </summary>
        public string WindowName { get; set; }

        /// <summary>
        /// This is a list of events to be trigerred and methods subscribed to those events
        /// </summary>
        public List<AutomaticHandlers> AutomaticHandlers { get; set; }

        /// <summary>
        /// The javascript function name (located in library.html) to be executed if is the case
        /// </summary>
        public string JSFunctionName { get; set; }

        /// <summary>
        /// The list of arguments sent to the javascript function
        /// </summary>
        public List<object> JSParameters { get; set; }

        /// <summary>
        /// This flag checks if is necessary to enable next step button only if packages list is already loaded
        /// </summary>
        public bool CheckPackagesListEnableNextStep { get; set; }

        /// <summary>
        /// This string contains the element to execute the Automatic Function
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// This property is the position of the node that will be placed
        /// </summary>
        public Point2D NodePosition { get; set; }
    }

    public class AutomaticHandlers
    {
        /// <summary>
        /// Name of the element to get the event
        /// </summary>
        public string HandlerElement { get; set; }

        /// <summary>
        /// Name of the event
        /// </summary>
        public string HandlerElementEvent { get; set; }

        /// <summary>
        /// Method to be subscribed 
        /// </summary>
        public string ExecuteMethod { get; set; }
    }
}
