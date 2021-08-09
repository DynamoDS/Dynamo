using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
        public List<Step> GuideSteps;

        /// <summary>
        /// This property represent the name of the Guide, e.g. "Get Started", "Packages"
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// This variable will contain the current step according to the steps flow in the Guide
        /// </summary>
        public Step CurrentStep { get; set; }

        /// <summary>
        /// This variable represents the total number of steps that the guide has (every guide can have a different number of steps).
        /// </summary>
        public int TotalSteps { get; set; }

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
            GuideFlowEvents.GuidedTourNextStep -= Next;
            GuideFlowEvents.GuidedTourPrevStep -= Back;
        }

        /// <summary>
        /// This method will subscribe the handlers for when the Back and Next button are pressed (this subscription happens when the Guide is initialized)
        /// </summary>
        private void SubscribeFlowEvents()
        {
            GuideFlowEvents.GuidedTourNextStep += Next;
            GuideFlowEvents.GuidedTourPrevStep += Back;

        }

        /// <summary>
        /// This method will be called by the Guide client and this will show the first Step popup in the list
        /// </summary>
        public void Play()
        {
            if (GuideSteps.Any())
            {
                Step firstStep = (from step in GuideSteps where step.Sequence == 0 select step).FirstOrDefault();
                firstStep.Show();
            }
        }

        /// <summary>
        /// This method will be called by the Guide client and basically subscribe the handlers for the events OnGuidedTourNext and OnGuidedTourPrev
        /// </summary>
        public void Initialize()
        {
            TotalSteps = GuideSteps.Count;
            SubscribeFlowEvents();
        }

        /// <summary>
        /// This method unsubcribe from the corresponding event the Prev and Next handlers, this method is called when the OnGuidedTourFinish event is raised 
        /// </summary>
        public void ClearGuide()
        {
            UnsubscribeFlowEvents();
        }

        /// <summary>
        /// This event method will be executed then the user press the Next button in the tooltip/popup
        /// basically it searchs the next step in the list, show it and hides the current one.
        /// </summary>
        /// <param name="args">This parameter will contain the "sequence" of the current Step so we can get the next Step from the list</param>
        public void Next(GuidedTourMovementEventArgs args)
        {
            Step nextStep = null;
            if (args.StepSequence < TotalSteps)
            {
                nextStep = (from step in GuideSteps where step.Sequence == args.StepSequence + 1 select step).FirstOrDefault();
                if (nextStep != null)
                    nextStep.Show();
            }

            CurrentStep = (from step in GuideSteps where step.Sequence == args.StepSequence select step).FirstOrDefault();
            if (CurrentStep != null)
                CurrentStep.Hide();
        }


        /// <summary>
        /// This event method will be executed then the user press the Back button in the tooltip/popup
        /// basically it searchs the previous step in the list, show it and hides the current one.
        /// </summary>
        /// <param name="args">This parameter will contain the "sequence" of the current Step so we can get the previous Step from the list</param>
        public void Back(GuidedTourMovementEventArgs args)
        {
            Step prevStep = null;
            if (args.StepSequence > 0)
            {
                prevStep = (from step in GuideSteps where step.Sequence == args.StepSequence - 1 select step).FirstOrDefault();
                if (prevStep != null)
                    prevStep.Show();
            }

            CurrentStep = (from step in GuideSteps where step.Sequence == args.StepSequence select step).FirstOrDefault();
            if (CurrentStep != null)
                CurrentStep.Hide();
        }

        /// <summary>
        /// Static method that finds a UIElement child based in the child name of a given root item in the Visual Tree. 
        /// </summary>
        /// <param name="parent">Root element in which the search will start</param>
        /// <param name="childName">Name of child to be found in the VisualTree </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null parent is being returned.</returns>
        public static UIElement FindChild(DependencyObject parent, string childName)
        {
            // Confirm parent is valid. 
            if (parent == null) return null;

            // Confirm child name is valid. 
            if (string.IsNullOrEmpty(childName)) return null;

            UIElement foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child != null)
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name match the searching string
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (UIElement)child;
                        break;
                    }
                    else
                    {
                        foundChild = FindChild(child, childName);

                        // If the child is found, break so we do not overwrite the found child. 
                        if (foundChild != null) break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (UIElement)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}
