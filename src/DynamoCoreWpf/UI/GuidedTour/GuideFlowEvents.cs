using Dynamo.Wpf.Properties;
using System;

namespace Dynamo.Wpf.UI.GuidedTour
{
    public delegate void GuidedTourNextEventHandler();
    public delegate void GuidedTourPrevEventHandler();
    public delegate void UpdatePopupLocationEventHandler();
    public delegate void UpdateLibraryInteractionsEventHandler();
    public delegate void GuidedTourStartEventHandler(GuidedTourStateEventArgs args);
    public delegate void GuidedTourFinishEventHandler(GuidedTourStateEventArgs args);
    public delegate void GuidedTourExitedEventHandler(GuidedTourStateEventArgs args);

    /// <summary>
    /// This static class will be used to raise events when the tooltip, welcome and survey popup buttons are clicked.
    /// </summary>
    public static class GuideFlowEvents
    {
        //Event that will be raised when the Popup Next button is pressed, the value passed as parameter is the current Step Sequence
        public static event GuidedTourNextEventHandler GuidedTourNextStep;
        private static bool isAnyGuideActive { get; set; } = false;
        private static bool isGuideExited { get; set; } = false;
        public static void OnGuidedTourNext()
        {
            if (GuidedTourNextStep != null)
                GuidedTourNextStep();
        }

        //Event that will be raised when the Popup Back button is pressed, the value passed as parameter is the current Step Sequence
        public static event GuidedTourPrevEventHandler GuidedTourPrevStep;
        public static void OnGuidedTourPrev()
        {
            if (GuidedTourPrevStep != null)
                GuidedTourPrevStep();
        }

        //Event that will be raised when the Guide is started (the first popup will be shown)
        public static event GuidedTourStartEventHandler GuidedTourStart;
        internal static void OnGuidedTourStart(string name)
        {
            if (GuidedTourStart != null)
            {
                isAnyGuideActive = true;
                isGuideExited = false;
                GuidedTourStart(new GuidedTourStateEventArgs(name));
            }              
        }

        //Event that will be raised when the Guide is finished (when the user press the close button in the Survey)
        public static event GuidedTourFinishEventHandler GuidedTourFinish;
        internal static void OnGuidedTourFinish(string name)
        {
            if (GuidedTourFinish != null)
            {
                isAnyGuideActive = false;
                GuidedTourFinish(new GuidedTourStateEventArgs(name));
            }
        }

        //Event that will be raised when the Guide is completely closed (this is specific for the Packages guide that allows the user to continue or exit the guide)
        public static event GuidedTourExitedEventHandler GuidedTourExited;
        internal static void OnGuidedTourExited(string name)
        {
            isGuideExited = true;
        }

        //Event that will be raised when we want to update the Popup location of the current step being executed
        public static event UpdatePopupLocationEventHandler UpdatePopupLocation;
        public static void OnUpdatePopupLocation()
        {
            if (UpdatePopupLocation != null)
                UpdatePopupLocation();
        }

        //Event that will be raised when we want to update the Library interactions of Popups like event subscriptions and highlighted elements
        public static event UpdateLibraryInteractionsEventHandler UpdateLibraryInteractions;
        public static void OnUpdateLibraryInteractions()
        { 
            if (UpdateLibraryInteractions != null)
                UpdateLibraryInteractions();
        }

        /// <summary>
        /// This property will return if the a guide is being executed or not. 
        /// </summary>
        public static bool IsAnyGuideActive
        {
            get { return isAnyGuideActive; }
            set { isAnyGuideActive = value; }
        }

        /// <summary>
        /// This property will return a value that says if the tour has been completely closed or not
        /// </summary>
        public static bool IsGuideExited
        {
            get { return isGuideExited; }
            set { isGuideExited = value; }
        }
    }

    /// <summary>
    /// This event class will be used to hold the Guide.Name (Tour) parameter for the OnGuidedTourStart and OnGuidedTourFinish events
    /// </summary>
    public class GuidedTourStateEventArgs : EventArgs
    {
        public string GuideName { get; set; }

        public GuidedTourStateEventArgs(string name)
        {
            GuideName = name;
        }
    }
}
