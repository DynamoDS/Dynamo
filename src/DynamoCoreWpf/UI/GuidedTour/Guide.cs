using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Controls;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.Views.GuidedTour;
using Newtonsoft.Json;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class represent just one guided tour that will be shown (just one guide can be shown at one time)
    /// </summary>
    public class Guide
    {
        /// <summary>
        /// This list will contain all the steps per guide read from a json file
        /// </summary>
        [JsonProperty("GuideSteps")]
        internal List<Step> GuideSteps { get; set; }

        /// <summary>
        /// This property represent the name of the Guide, e.g. "Get Started", "Packages"
        /// </summary>
        [JsonProperty("Name")]
        internal string Name { get; set; }

        /// <summary>
        /// This property represents the workflow of the guides
        /// I.E: 
        /// 1 - User interface guide
        /// 2 - Onboarding guide
        /// </summary>
        [JsonProperty("SequenceOrder")]
        internal int SequenceOrder { get; set; }

        /// <summary>
        /// This property has the resource key string for the guide
        /// </summary>
        [JsonProperty("GuideNameResource")]
        internal string GuideNameResource { get; set; }


        /// <summary>
        /// This variable will contain the current step according to the steps flow in the Guide
        /// </summary>
        internal Step CurrentStep { get; set; }

        /// <summary>
        /// This variable represents the total number of steps that the guide has (every guide can have a different number of steps).
        /// </summary>
        internal int TotalSteps { get; set; }

        /// <summary>
        /// This variable represents the Guide Background Element to manipulate it's hole rect
        /// </summary>
        internal GuideBackground GuideBackgroundElement { get; set; }

        /// <summary>
        /// This variable represents the element of the LibraryView 
        /// </summary>
        internal UIElement LibraryView { get; set; }

        /// <summary>
        /// This variable represents the element of the MainWindow 
        /// </summary>
        internal UIElement MainWindow { get; set; }

        internal enum GuideFlow { FORWARD = 1, BACKWARD = -1, CURRENT=0  }

        public Guide()
        {
            GuideSteps = new List<Step>();
        }

        /// <summary>
        ///  Guide class finalizer, it will unsubscribe all the events once the guide is destroyed
        /// </summary>
        ~Guide()
        {
            UnsubscribeFlowEvents();
        }

        /// <summary>
        /// Remove all the subscriptions to events
        /// </summary>
        private void UnsubscribeFlowEvents()
        {
            GuideFlowEvents.GuidedTourNextStep -= GuideFlowEvents_GuidedTourNextStep;
            GuideFlowEvents.GuidedTourPrevStep -= GuideFlowEvents_GuidedTourPrevStep;
            GuideFlowEvents.UpdatePopupLocation -= GuideFlowEvents_UpdatePopupLocation;
            GuideFlowEvents.UpdateLibraryInteractions -= GuideFlowEvents_UpdateLibraryInteractions;
        }

        /// <summary>
        /// This method handler will be executed when a package is installed in the LibraryView so the Popup over the library will be updated
        /// </summary>
        private void GuideFlowEvents_UpdateLibraryInteractions()
        {
            CurrentStep.UpdateLibraryInteractions();
        }

        /// <summary>
        /// This method will subscribe the handlers for when the Back and Next button are pressed (this subscription happens when the Guide is initialized)
        /// </summary>
        private void SubscribeFlowEvents()
        {
            GuideFlowEvents.GuidedTourNextStep += GuideFlowEvents_GuidedTourNextStep;
            GuideFlowEvents.GuidedTourPrevStep += GuideFlowEvents_GuidedTourPrevStep;
            GuideFlowEvents.UpdatePopupLocation += GuideFlowEvents_UpdatePopupLocation;
            GuideFlowEvents.UpdateLibraryInteractions += GuideFlowEvents_UpdateLibraryInteractions;
        }

        /// <summary>
        /// This event handler will be executed when the GuideFlowEvents.UpdatePopupLocation event is raised
        /// </summary>
        private void GuideFlowEvents_UpdatePopupLocation()
        {
            CurrentStep.UpdateLibraryPopupsLocation();
        }

        /// <summary>
        /// This method will be called by the Guide client and this will show the first Step popup in the list
        /// </summary>
        internal void Play()
        {
            if (GuideSteps.Any())
            {
                Step firstStep = GuideSteps.FirstOrDefault();
                CurrentStep = firstStep;

                firstStep.Show(GuideFlow.FORWARD);
            }
        }

        /// <summary>
        /// This method will be called by the Guide client and basically subscribe the handlers for the events OnGuidedTourNext and OnGuidedTourPrev
        /// </summary>
        internal void Initialize()
        {
            TotalSteps = GuideSteps.Count;

            SetLibraryViewVisible(false);

            SubscribeFlowEvents();
        }

        /// <summary>
        /// Sets library to visible or hidden 
        /// </summary>
        /// <param name="visible">This parameter will contain a boolean to define if the library should be visible or not</param>
        private void SetLibraryViewVisible(bool visible)
        {
            if (LibraryView != null)
            {
                if (visible)
                    LibraryView.Visibility = Visibility.Visible;
                else
                    LibraryView.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// This method unsubcribe from the corresponding event the Prev and Next handlers, this method is called when the OnGuidedTourFinish event is raised 
        /// </summary>
        internal void ClearGuide()
        {
            UnsubscribeFlowEvents();
            SetLibraryViewVisible(true);
            ClearSteps();
        }

        /// <summary>
        /// This method will remove/undo all the UI Automations previously done when showing each Step
        /// </summary>
        internal void ClearSteps()
        {
            foreach( var step in GuideSteps)
            {
                //In this case we don't need to know the Current Guides Flow when we pass CURRENT
                step.Hide(GuideFlow.CURRENT);
            }
        }
        /// <summary>
        /// This method will be executed for continuing to guide tour
        /// </summary>
        /// <param name="CurrentStepSequence">This parameter will contain the "sequence" of the current Step so we can continue the same step</param>
        internal void ContinueStep(int CurrentStepSequence)
        {
            if (CurrentStepSequence >= 0)
            {
                CalculateStep(GuideFlow.CURRENT, CurrentStepSequence);
                CurrentStep.Show(GuideFlow.FORWARD);
            }
        }

        /// <summary>
        /// This method will be executed for moving to the next step, basically searches the next step in the list, shows it and hides the current one.
        /// </summary>
        /// <param name="CurrentStepSequence">This parameter will contain the "sequence" of the current Step so we can get the next Step from the list</param>
        internal void NextStep(int CurrentStepSequence)
        {
            if (CurrentStepSequence < TotalSteps)
            {
                HideCurrentStep(CurrentStepSequence, GuideFlow.FORWARD);

                CalculateStep(GuideFlow.FORWARD, CurrentStepSequence);
                CurrentStep.Show(GuideFlow.FORWARD);
                Logging.Analytics.TrackEvent(Logging.Actions.Next, Logging.Categories.GuidedTourOperations, Resources.ResourceManager.GetString(GuideNameResource, System.Globalization.CultureInfo.InvariantCulture).Replace("_", ""), CurrentStep.Sequence);
            }
        }

        /// <summary>
        /// This method will be executed for moving to the previous step, basically searches the previous step in the list, shows it and hides the current one.
        /// </summary>
        /// <param name="CurrentStepSequence">This parameter is the "sequence" of the current Step so we can get the previous Step from the list</param>
        internal void PreviousStep(int CurrentStepSequence)
        {
            if (CurrentStepSequence > 0)
            {
                HideCurrentStep(CurrentStepSequence, GuideFlow.BACKWARD);

                CalculateStep(GuideFlow.BACKWARD, CurrentStepSequence);
                CurrentStep.Show(GuideFlow.BACKWARD);
                Logging.Analytics.TrackEvent(Logging.Actions.Previous, Logging.Categories.GuidedTourOperations, Resources.ResourceManager.GetString(GuideNameResource, System.Globalization.CultureInfo.InvariantCulture).Replace("_", ""), CurrentStep.Sequence);
            }     
        }


        /// <summary>
        /// This method will be executed for moving to the next, previous or current step, basically searches the step in the list.
        /// </summary>
        /// <param name="stepFlow">The direction flow of the Guide, can be BACKWARD, FORWARD or CURRENT</param>
        /// <param name="CurrentStepSequence">This parameter is the current Step sequence</param>
        internal void CalculateStep(GuideFlow stepFlow, int CurrentStepSequence)
        {
            Step resultStep = null;
            int stepFlowOffSet = Convert.ToInt32(stepFlow);
            var possibleSteps = (from step in GuideSteps where step.Sequence == CurrentStepSequence + stepFlowOffSet select step);
            if (possibleSteps != null && possibleSteps.Count() > 0)
            {
                //This section validates if the Current Step can be several ones, so we need to get the one validated
                //Means that there only one possible Current Steps
                if (possibleSteps.Count() == 1)
                    resultStep = possibleSteps.FirstOrDefault();
                //Means that there are several posible current Steps then we need to take the one validated
                else
                {
                    foreach (var step in possibleSteps)
                    {
                        step.ExecutePreValidation();
                    }

                    resultStep = possibleSteps.FirstOrDefault(x => x.PreValidationIsOpenFlag);
                }
                if (resultStep != null)
                {

                    SetLibraryViewVisible(resultStep.ShowLibrary);
                    CurrentStep = resultStep;
                }
            }
        }

        internal void HideCurrentStep(int CurrentStepSequence, GuideFlow currentFlow)
        {
            CurrentStep.Hide(currentFlow);
            GuideBackgroundElement.ClearHighlightSection();
        }

        /// <summary>
        /// This event method will be executed when the user press the Back button in the tooltip/popup
        /// </summary>
        /// <param name="args">This parameter will contain the "sequence" of the current Step so we can get the previous Step from the list</param>
        private void GuideFlowEvents_GuidedTourPrevStep()
        {
            PreviousStep(CurrentStep.Sequence);
        }

        /// <summary>
        /// This event method will be executed then the user press the Next button in the tooltip/popup
        /// </summary>
        /// <param name="args">This parameter will contain the "sequence" of the current Step so we can get the next Step from the list</param>
        private void GuideFlowEvents_GuidedTourNextStep()
        {
            NextStep(CurrentStep.Sequence);
        }
    }
}
