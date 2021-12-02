using System;

namespace Dynamo.Wpf.UI.GuidedTour
{
    public delegate void GuidedTourNextEventHandler(GuidedTourMovementEventArgs args);
    public delegate void GuidedTourPrevEventHandler(GuidedTourMovementEventArgs args);
    public delegate void GuidedTourStartEventHandler(GuidedTourStateEventArgs args);
    public delegate void GuidedTourFinishEventHandler(GuidedTourStateEventArgs args);

    /// <summary>
    /// This static class will be used to raise events when the tooltip, welcome and survey popup buttons are clicked.
    /// </summary>
    public static class GuideFlowEvents
    {
        //Event that will be raised when the Popup Next button is pressed, the value passed as parameter is the current Step Sequence
        public static event GuidedTourNextEventHandler GuidedTourNextStep;
        private static bool isAnyGuideActive { get; set; } = false;
        internal static void OnGuidedTourNext(int sequence)
        {
            if (GuidedTourNextStep != null)
                GuidedTourNextStep(new GuidedTourMovementEventArgs(sequence));
        }

        //Event that will be raised when the Popup Back button is pressed, the value passed as parameter is the current Step Sequence
        public static event GuidedTourPrevEventHandler GuidedTourPrevStep;
        internal static void OnGuidedTourPrev(int sequence)
        {
            if (GuidedTourPrevStep != null)
                GuidedTourPrevStep(new GuidedTourMovementEventArgs(sequence));
        }

        //Event that will be raised when the Guide is started (the first popup will be shown)
        public static event GuidedTourStartEventHandler GuidedTourStart;
        internal static void OnGuidedTourStart(string name)
        {
            if (GuidedTourStart != null)
            {
                isAnyGuideActive = true;
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

        /// <summary>
        /// This property will returm if the a guide is being executed or not. 
        /// </summary>
        internal static bool IsAnyGuideActive
        {
            get { return isAnyGuideActive; }
            private set { isAnyGuideActive = value; }
        }
    }

    /// <summary>
    /// This event class will be used to hold the Step.Sequence parameter for the OnGuidedTourNext and OnGuidedTourPrev events
    /// </summary>
    public class GuidedTourMovementEventArgs : EventArgs
    {
        public int StepSequence { get; set; }

        public GuidedTourMovementEventArgs(int stepSequence)
        {
            StepSequence = stepSequence;
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
