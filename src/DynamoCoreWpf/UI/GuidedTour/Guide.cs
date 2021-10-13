using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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
        }

        /// <summary>
        /// This method will subscribe the handlers for when the Back and Next button are pressed (this subscription happens when the Guide is initialized)
        /// </summary>
        private void SubscribeFlowEvents()
        {
            GuideFlowEvents.GuidedTourNextStep += GuideFlowEvents_GuidedTourNextStep;
            GuideFlowEvents.GuidedTourPrevStep += GuideFlowEvents_GuidedTourPrevStep;
        }

        /// <summary>
        /// This method will be called by the Guide client and this will show the first Step popup in the list
        /// </summary>
        internal void Play()
        {
            if (GuideSteps.Any())
            {
                Step firstStep = (from step in GuideSteps where step.Sequence == 0 select step).FirstOrDefault();
                CurrentStep = firstStep;

                //When the first step of the guide is a tooltip, then it need to have a background and a hole to highlight the element
                if (firstStep.HostPopupInfo != null && 
                    firstStep.StepType == Step.StepTypes.TOOLTIP)
                    SetupBackgroundHole(firstStep);

                firstStep.Show();
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
        }

        /// <summary>
        /// This method will be executed for moving to the next step, basically searches the next step in the list, shows it and hides the current one.
        /// </summary>
        /// <param name="CurrentStepSequence">This parameter will contain the "sequence" of the current Step so we can get the next Step from the list</param>
        internal void NextStep(int CurrentStepSequence)
        {
            HideCurrentStep(CurrentStepSequence);
            if (CurrentStepSequence < TotalSteps)
            {
                CalculateStep(GuideFlow.FORWARD, CurrentStepSequence);           
            }
        }

        /// <summary>
        /// This method will be executed for moving to the previous step, basically searches the previous step in the list, shows it and hides the current one.
        /// </summary>
        /// <param name="CurrentStepSequence">This parameter is the "sequence" of the current Step so we can get the previous Step from the list</param>
        internal void PreviousStep(int CurrentStepSequence)
        {
            HideCurrentStep(CurrentStepSequence);
            if (CurrentStepSequence > 0)
            {
                CalculateStep(GuideFlow.BACKWARD, CurrentStepSequence);
            }     
        }


        /// <summary>
        /// This method will be executed for moving to the next, previous or current step, basically searches the step in the list.
        /// </summary>
        /// <param name="stepFlow">The direction flow of the Guide, can be BACKWARD, FORWARD or CURRENT</param>
        /// <param name="CurrentStepSequence">This parameter is the current Step sequence</param>
        private void CalculateStep(GuideFlow stepFlow, int CurrentStepSequence)
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
                    resultStep = (from step in possibleSteps
                                  where step.PreValidationIsOpenFlag
                                  select step).FirstOrDefault();
                }
                if (resultStep != null)
                {
                    SetLibraryViewVisible(resultStep.ShowLibrary);
                    CurrentStep = resultStep;

                    if (resultStep.StepType != Step.StepTypes.WELCOME &&
                       resultStep.StepType != Step.StepTypes.SURVEY
                       && resultStep.HostPopupInfo != null)
                        SetupBackgroundHole(resultStep);
                    else
                        GuideBackgroundElement.HoleRect = new Rect();

                    resultStep.Show();
                }
            }
        }

        private void HideCurrentStep(int CurrentStepSequence)
        {
            CalculateStep(GuideFlow.CURRENT, CurrentStepSequence);
            CurrentStep.Hide();
        }

        /// <summary>
        /// This event method will be executed when the user press the Back button in the tooltip/popup
        /// </summary>
        /// <param name="args">This parameter will contain the "sequence" of the current Step so we can get the previous Step from the list</param>
        private void GuideFlowEvents_GuidedTourPrevStep(GuidedTourMovementEventArgs args)
        {
            PreviousStep(args.StepSequence);
        }

        /// <summary>
        /// This event method will be executed then the user press the Next button in the tooltip/popup
        /// </summary>
        /// <param name="args">This parameter will contain the "sequence" of the current Step so we can get the next Step from the list</param>
        private void GuideFlowEvents_GuidedTourNextStep(GuidedTourMovementEventArgs args)
        {
            NextStep(args.StepSequence);
        }

        /// <summary>
        /// This method styles the bacground in every step
        /// </summary>
        /// <param name="step">This parameter represents the step with informations of the element and color of the border</param>
        private void SetupBackgroundHole(Step step)
        {
            SetupBackgroundHoleSize(step.HostPopupInfo);
            SetupBackgroundHoleBorderColor(step.HostPopupInfo.HighlightColor);
        }

        /// <summary>
        /// This method will set the border color with there is any configured
        /// </summary>
        /// <param name="highlightColor">This parameter represents the color in hexadecimal</param>
        private void SetupBackgroundHoleBorderColor(string highlightColor)
        {
            if (string.IsNullOrEmpty(highlightColor))
            {
                GuideBackgroundElement.HolePath.Stroke = Brushes.Transparent;
            }
            else
            {
                var converter = new BrushConverter();
                var brush = (Brush)converter.ConvertFromString(highlightColor);
                GuideBackgroundElement.HolePath.Stroke = brush;
            }
        }

        /// <summary>
        /// This method will update the hole size everytime that the step change
        /// </summary>
        /// <param name="hostElement">Element for size and position reference</param>
        private void SetupBackgroundHoleSize(HostControlInfo hostControlInfo)
        {
            Point relativePoint = hostControlInfo.HostUIElement.TransformToAncestor(MainWindow)
                              .Transform(new Point(0, 0));

            var holeWidth = hostControlInfo.HostUIElement.DesiredSize.Width + hostControlInfo.WidthBoxDelta;
            var holeHeight = hostControlInfo.HostUIElement.DesiredSize.Height + hostControlInfo.HeightBoxDelta;

            GuideBackgroundElement.HoleRect = new Rect(relativePoint.X, relativePoint.Y, holeWidth, holeHeight);
        }

        /// <summary>
        /// Static method that finds a UIElement child based in the child name of a given root item in the Visual Tree. 
        /// </summary>
        /// <param name="parent">Root element in which the search will start</param>
        /// <param name="childName">Name of child to be found in the VisualTree </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null parent is being returned.</returns>
        internal static UIElement FindChild(DependencyObject parent, string childName)
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