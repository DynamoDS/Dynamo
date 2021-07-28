using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class will manage the Guides read from the json file
    /// </summary>
    public sealed class GuidesManager
    {
        /// <summary>
        /// This property contains the list of Guides read from the json file
        /// </summary>
        public List<Guide> Guides;

        /// <summary>
        /// currentGuide will contain the Guide being played
        /// </summary>
        private Guide currentGuide;
        private UIElement mainRootElement;

        /// <summary>
        /// GuidesManager Constructor that will read all the guides/steps from and json file and subscribe handlers for the Start and Finish events
        /// </summary>
        /// <param name="root">root item of the main Dynamo Window </param>
        public GuidesManager(UIElement root)
        {
            mainRootElement = root;
            Guides = new List<Guide>();
            ReadGuides("UI/GuidedTour/dynamo_guides.json");

            //Subscribe the handlers when the Tour is started and finished, the handlers are unsubscribed in the method TourFinished()
            GuideFlowEvents.GuidedTourStart += TourStarted;
            GuideFlowEvents.GuidedTourFinish += TourFinished;
        }

        /// <summary>
        /// This method will launch the tour when the user clicks in the Help->Interactive Guides->Guide
        /// </summary>
        /// <param name="tourName"></param>
        public void LaunchTour(string tourName)
        {
            GuideFlowEvents.OnGuidedTourStart(tourName);
        }

        /// <summary>
        /// This method will be executed when the OnGuidedTourStart event is raised
        /// </summary>
        /// <param name="args">This parameter will contain the GuideName as a string</param>
        private void TourStarted(GuidedTourStateEventArgs args)
        {
            currentGuide = (from guide in Guides where guide.Name.Equals(args.GuideName) select guide).FirstOrDefault();
            if (currentGuide != null)
            {
                currentGuide.Initialize();
                currentGuide.Play();
            }
        }

        /// <summary>
        /// This method will be executed when the OnGuidedTourFinish event is raised
        /// </summary>
        /// <param name="args">This parameter will contain the GuideName as a string</param>
        private void TourFinished(GuidedTourStateEventArgs args)
        {
            currentGuide = (from guide in Guides where guide.Name.Equals(args.GuideName) select guide).FirstOrDefault();
            if (currentGuide != null)
            {
                foreach (Step tmpStep in currentGuide.GuideSteps)
                {
                    tmpStep.StepClosed -= Popup_StepClosed;
                }
                currentGuide.ClearGuide();
                GuideFlowEvents.GuidedTourStart -= TourStarted;
                GuideFlowEvents.GuidedTourFinish -= TourFinished;
            }

        }

        /// <summary>
        /// This method will read all the guides information from a json file located in the same directory than the DynamoSandbox.exe is located.
        /// </summary>
        /// <param name="jsonFile">Full path of the json file location</param>
        private void ReadGuides(string jsonFile)
        {
            string jsonString = string.Empty;
            using (StreamReader r = new StreamReader(jsonFile))
            {
                jsonString = r.ReadToEnd();
            }
                
            //Deserialize all the information read from the json file
            List<Guide> tempGuidesList = JsonConvert.DeserializeObject<List<Guide>>(jsonString);
            int totalTooltips = 0;

            foreach (Guide guide in tempGuidesList)
            {
                Guide newGuide = new Guide()
                {
                    Name = guide.Name,
                };

                totalTooltips = (from step in guide.GuideSteps
                                 where step.StepType == Step.StepTypes.TOOLTIP ||
                                       step.StepType == Step.StepTypes.SURVEY
                                 select step).Count();
                foreach (Step step in guide.GuideSteps)
                {
                    var popupInfo = new HostControlInfo()
                    {
                        PopupPlacement = step.HostPopupInfo.PopupPlacement,
                        HostUIElementString = step.HostPopupInfo.HostUIElementString,
                        HostUIElement = mainRootElement,
                        VerticalPopupOffSet = step.HostPopupInfo.VerticalPopupOffSet,
                        HorizontalPopupOffSet = step.HostPopupInfo.HorizontalPopupOffSet
                    };

                    //The host_ui_element read from the json file need to exists otherwise the host will be null
                    UIElement hostUIElement = Guide.FindChild(mainRootElement, popupInfo.HostUIElementString);
                    if (hostUIElement != null)
                        popupInfo.HostUIElement = hostUIElement;

                    //This section will retrive the strings from the Resources.resx file
                    var formattedText = Res.ResourceManager.GetString(step.StepContent.FormattedText);
                    var title = Res.ResourceManager.GetString(step.StepContent.Title);

                    switch (step.StepType)
                    {
                        case Step.StepTypes.TOOLTIP:
                            var tooltip = new Tooltip(popupInfo, step.Width, step.Height, step.TooltipPointerDirection)
                            {
                                Name = step.Name,
                                Sequence = step.Sequence,
                                TotalTooltips = totalTooltips,
                                StepContent = new Content()
                                {
                                    FormattedText = formattedText,
                                    Title = title
                                }
                            };
                            newGuide.GuideSteps.Add(tooltip);
                            tooltip.StepClosed += Popup_StepClosed;
                            break;
                        case Step.StepTypes.SURVEY:
                            var surveyPopup = new Survey(popupInfo, step.Width, step.Height)
                            {
                                Sequence = step.Sequence,
                                ContentWidth = 300,
                                RatingTextTitle = formattedText.ToString(),
                                StepContent = new Content()
                                {
                                    FormattedText = formattedText,
                                    Title = title
                                }
                            };

                            //Due that the RatingTextTitle property is just for Survey then we need to set the property using reflection
                            foreach (var extraContent in step.StepExtraContent)
                            {
                                // Get the Type object corresponding to Step.
                                Type myType = typeof(Survey);
                                // Get the PropertyInfo object by passing the property name.
                                PropertyInfo myPropInfo = myType.GetProperty(extraContent.Property);
                                if (myPropInfo != null)
                                {
                                    //Retrieve the string value from the Resources.resx file
                                    var valueStr = Res.ResourceManager.GetString(extraContent.Value);
                                    myPropInfo.SetValue(surveyPopup, valueStr);
                                }
                            }

                            newGuide.GuideSteps.Add(surveyPopup);
                            surveyPopup.StepClosed += Popup_StepClosed;
                            break;
                        case Step.StepTypes.WELCOME:
                            var customWelcome = new Welcome(popupInfo, step.Width, step.Height)
                            {
                                Sequence = step.Sequence,
                                StepContent = new Content()
                                {
                                    FormattedText = formattedText,
                                    Title = title
                                }
                            };
                            newGuide.GuideSteps.Add(customWelcome);
                            customWelcome.StepClosed += Popup_StepClosed;
                            break;
                        case Step.StepTypes.EXIT_TOUR:
                            break;
                    }//StepType
                }//Steps
                Guides.Add(newGuide);
            }
        }

        private void Popup_StepClosed(string name, Step.StepTypes stepType)
        {
            GuideFlowEvents.OnGuidedTourFinish(currentGuide.Name);
        }
    }
}
