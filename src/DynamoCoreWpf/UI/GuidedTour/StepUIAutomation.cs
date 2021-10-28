using System.Windows;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class will be used as a container for the UI Automation information read from the json file
    /// </summary>
    public class StepUIAutomation
    {
        /// <summary>
        /// This Sequence will be unique for each automation step
        /// </summary>
        public double Sequence { get; set; }

        /// <summary>
        /// The Control type represent the WPF Control type that will be Automated, like MenuItem, Window, Dropdown
        /// </summary>
        public string ControlType { get; set; }

        /// <summary>
        /// This contain a string ID of the UI Automation 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This will represent the action that will be executed as part of the UI Automation
        /// </summary>
        public string Action { get; set; }

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

    }
}
