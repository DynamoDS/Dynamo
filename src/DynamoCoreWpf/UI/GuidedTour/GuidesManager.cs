using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Dynamo.Controls;
using Dynamo.ViewModels;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.ViewModels.GuidedTour;
using Dynamo.Wpf.Views.GuidedTour;
using Newtonsoft.Json;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class will manage the Guides read from the json file
    /// </summary>
    public sealed class GuidesManager
    {
        #region private properties
        /// <summary>
        /// currentGuide will contain the Guide being played
        /// </summary>
        internal Guide currentGuide;

        private GuideBackground guideBackgroundElement;
        private readonly UIElement mainRootElement;
        private readonly DynamoViewModel dynamoViewModel;

        private RealTimeInfoWindow exitTourPopup;

        //Checks if the tour has already finished
        private static bool tourStarted;

        //The ExitTour popup will be shown at the top-right section of the Dynamo window (at the left side of the statusBarPanel element) only when the tour is closed
        //The ExitTourVerticalOffset is used to move vertically the popup (using as a reference the statusBarPanel position)
        private const double ExitTourVerticalOffset = 10;
        //The ExitTourHorizontalOffset is used to move horizontally the popup (using as a reference the statusBarPanel position)
        private const double ExitTourHorizontalOffset = 110;

        private const string guideBackgroundName = "GuidesBackground";
        private const string mainGridName = "mainGrid";
        private const string libraryViewName = "Browser";
        private Stopwatch stopwatch;
        #endregion

        #region internal properties
        //The list of guide name strings are hardcoded to match Json definition of guides (no need localization)
        internal static string GetStartedGuideName = "User Interface Tour";
        internal static string PackagesGuideName = "Packages";
        internal static string OnboardingGuideName = "Getting Started";

        internal ExitGuideWindow exitGuideWindow;

        internal static string GuidesExecutingDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        internal static string GuidesJsonFilePath
        {
            get
            {
                return Path.Combine(GuidesExecutingDirectory, @"UI\GuidedTour\dynamo_guides.json");
            }
        }

        internal static string OnboardingGuideWorkspaceEmbeededResource
        {
            get
            {
                return @"Dynamo.Wpf.UI.GuidedTour.DynamoOnboardingGuide_HouseCreationDS.dyn";
            }
        }
        #endregion

        /// <summary>
        /// This property contains the list of Guides read from the json file
        /// </summary>
        public List<Guide> Guides;

        /// <summary>
        /// GuidesManager Constructor 
        /// </summary>
        /// <param name="root">root item of the main Dynamo Window </param>
        /// <param name="dynViewModel"></param>
        public GuidesManager(UIElement root, DynamoViewModel dynViewModel)
        {
            mainRootElement = root;
            dynamoViewModel = dynViewModel;
            guideBackgroundElement = GuideUtilities.FindChild(root, guideBackgroundName) as GuideBackground;
        }

        /// <summary>
        /// Initialization that will read all the guides/steps from and json file and subscribe handlers for the Start and Finish events
        /// </summary>
        internal void Initialize()
        {
            //Subscribe the handlers when the Tour is started and finished, the handlers are unsubscribed in the method TourFinished()
            GuideFlowEvents.GuidedTourStart += TourStarted;
            GuideFlowEvents.GuidedTourFinish += TourFinished;

            Guides = new List<Guide>();

            //Due that we are passing the GuideBackground for each Step we need to create first the background and then Create the Steps
            CreateBackground();
            CreateGuideSteps(GuidesJsonFilePath);

            GuidesValidationMethods.CurrentExecutingGuidesManager = this;

            guideBackgroundElement.ClearCutOffSection();
            guideBackgroundElement.ClearHighlightSection();
            stopwatch = Stopwatch.StartNew();
    }

        /// <summary>
        /// Creates the background for the GuidedTour
        /// </summary>
        private void CreateBackground()
        {
            Window mainWindow = Window.GetWindow(mainRootElement);

            if (guideBackgroundElement == null)
            {
                guideBackgroundElement = new GuideBackground(mainWindow)
                {
                    Name = guideBackgroundName,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Visibility = Visibility.Hidden
                };

                Grid mainGrid = GuideUtilities.FindChild(mainRootElement, mainGridName) as Grid;
                mainGrid.Children.Add(guideBackgroundElement);
                Grid.SetColumnSpan(guideBackgroundElement, 5);
                Grid.SetRowSpan(guideBackgroundElement, 6);
            }
        }

        /// <summary>
        /// This method will be used when the PlacementTarget element of the Popup was moved or resize so we need to update each Step and the ExitTour popup
        /// </summary>
        internal void UpdateGuideStepsLocation()
        {
            //If there is no guide being executed then we shouldn't do anything
            if (!GuideFlowEvents.IsAnyGuideActive) return;

            if (currentGuide != null)
            {
                currentGuide.CurrentStep.UpdateLocation();
            }

            if (exitTourPopup != null)
            {
                exitTourPopup.UpdateLocation();
            }
        }

        /// <summary>
        /// This method will launch the tour when the user clicks in the Help->Interactive Guides->Guide
        /// </summary>
        /// <param name="tourName"></param>
        internal void LaunchTour(string tourName)
        {
            if (!tourStarted)
            {
                Initialize();
                GuideFlowEvents.OnGuidedTourStart(tourName);
                dynamoViewModel.OnEnableShortcutBarItems(false);
                Logging.Analytics.TrackScreenView("InteractiveGuidedTours");
                Logging.Analytics.TrackEvent(Logging.Actions.Start, Logging.Categories.GuidedTourOperations, Resources.ResourceManager.GetString(currentGuide.GuideNameResource, System.Globalization.CultureInfo.InvariantCulture).Replace("_", ""), currentGuide.SequenceOrder);
            }
        }

        /// <summary>
        /// This method will be executed when the OnGuidedTourStart event is raised
        /// </summary>
        /// <param name="args">This parameter will contain the GuideName as a string</param>
        private void TourStarted(GuidedTourStateEventArgs args)
        {
            tourStarted = true;

            currentGuide = (from guide in Guides where guide.Name.Equals(args.GuideName) select guide).FirstOrDefault();
            if (currentGuide != null)
            {
                //Show background overlay
                guideBackgroundElement.Visibility = Visibility.Visible;
                currentGuide.GuideBackgroundElement = guideBackgroundElement;
                currentGuide.MainWindow = mainRootElement;
                var dynamoView = (mainRootElement as DynamoView);
                if (dynamoView == null) return;
                currentGuide.LibraryView = dynamoView.sidebarGrid.Children.OfType<UserControl>().FirstOrDefault();

                currentGuide.Initialize();
                currentGuide.Play();
                GuidesValidationMethods.CurrentExecutingGuide = currentGuide;
            }
        }

        /// <summary>
        /// This method will be executed when the OnGuidedTourFinish event is raised
        /// </summary>
        /// <param name="args">This parameter will contain the GuideName as a string</param>
        private void TourFinished(GuidedTourStateEventArgs args)
        {
            currentGuide = (from guide in Guides where guide.Name.Equals(args.GuideName) select guide).FirstOrDefault();

            //Check if it's packages guide to open the exit modal 
            if (args.GuideName == GuidesManager.PackagesGuideName && currentGuide.CurrentStep.StepType != Step.StepTypes.SURVEY)
            {
                guideBackgroundElement.ClearHighlightSection();
                guideBackgroundElement.ClearCutOffSection();
                CreateExitModal(currentGuide.CurrentStep.ExitGuide);
            }
            else
            {
                ExitTour();
            }
        }

        /// <summary>
        /// This method exits from tour 
        /// </summary>
        internal void ExitTour()
        {
            if (currentGuide != null)
            {
                foreach (Step tmpStep in currentGuide.GuideSteps)
                {
                    tmpStep.StepClosed -= Popup_StepClosed;
                }

                string guidName = Resources.ResourceManager.GetString(currentGuide.GuideNameResource, System.Globalization.CultureInfo.InvariantCulture).Replace("_", "");
                if (currentGuide.TotalSteps - 1 == currentGuide.CurrentStep.Sequence)
                {
                    Logging.Analytics.TrackEvent(Logging.Actions.Completed, Logging.Categories.GuidedTourOperations, guidName, currentGuide.CurrentStep.Sequence);
                }
                else 
                {
                    Logging.Analytics.TrackEvent(Logging.Actions.End, Logging.Categories.GuidedTourOperations, guidName, currentGuide.CurrentStep.Sequence);
                }                
                Logging.Analytics.TrackTimedEvent(Logging.Categories.GuidedTourOperations, Logging.Actions.TimeElapsed.ToString(), stopwatch.Elapsed, guidName);

                currentGuide.ClearGuide();
                GuideFlowEvents.GuidedTourStart -= TourStarted;
                GuideFlowEvents.GuidedTourFinish -= TourFinished;

                if (exitGuideWindow != null)
                {
                    exitGuideWindow.ExitTourButton.Click -= ExitTourButton_Click;
                    exitGuideWindow.ContinueTourButton.Click -= ContinueTourButton_Click;
                }

                dynamoViewModel.OnEnableShortcutBarItems(true);

                //Hide guide background overlay
                guideBackgroundElement.Visibility = Visibility.Hidden;
                GuidesValidationMethods.CurrentExecutingGuide = null;
                tourStarted = false;
            }
        }

        /// <summary>
        /// Creates the exit modal when close button is pressed
        /// </summary>
        /// <param name="exitGuide">This parameter contains the properties to build exit guide modal</param>
        internal void CreateExitModal(ExitGuide exitGuide)
        {
            var viewModel = new ExitGuideWindowViewModel(exitGuide);
            exitGuideWindow = new ExitGuideWindow((FrameworkElement)mainRootElement, viewModel);

            exitGuideWindow.ExitTourButton.Click += ExitTourButton_Click;
            exitGuideWindow.ContinueTourButton.Click += ContinueTourButton_Click;

            exitGuideWindow.IsOpen = true;
        }

        private void ContinueTourButton_Click(object sender, RoutedEventArgs e)
        {
            exitGuideWindow.IsOpen = false;
            if (currentGuide != null)
            {
                GuideFlowEvents.IsAnyGuideActive = true;
                currentGuide.ContinueStep(currentGuide.CurrentStep.Sequence);
            }
        }

        private void ExitTourButton_Click(object sender, RoutedEventArgs e)
        {
            exitGuideWindow.IsOpen = false;
            GuideFlowEvents.OnGuidedTourExited(currentGuide.Name);
            ExitTour();
        }

        /// <summary>
        /// This method will read all the guides information from a json file located in the same directory than the DynamoSandbox.exe is located.
        /// </summary>
        /// <param name="jsonFile">Full path of the json file location containing information about the Guides and Steps</param>
        private static List<Guide> ReadGuides(string jsonFile)
        {
            string jsonString = string.Empty;
            using (StreamReader r = new StreamReader(jsonFile))
            {
                jsonString = r.ReadToEnd();
            }

            //Deserialize all the information read from the json file
            return JsonConvert.DeserializeObject<List<Guide>>(jsonString);
        }

        /// <summary>
        /// This method will create all the Guide and add them to the Guides List based in the deserialized info gotten from the json file passed as parameter
        /// </summary>
        /// <param name="jsonFile">Full path of the json file location containing information about the Guides and Steps</param>
        private void CreateGuideSteps(string jsonFile)
        {
            int totalTooltips = 0;

            foreach (Guide guide in GuidesManager.ReadGuides(jsonFile))
            {
                Guide newGuide = new Guide()
                {
                    Name = guide.Name,
                };

                newGuide.SequenceOrder = guide.SequenceOrder;
                newGuide.GuideNameResource = guide.GuideNameResource;

                totalTooltips = (from step in guide.GuideSteps
                                 where step.StepType == Step.StepTypes.TOOLTIP ||
                                       step.StepType == Step.StepTypes.SURVEY
                                 select step).GroupBy(x=>x.Sequence).Count();

                foreach (Step step in guide.GuideSteps)
                {
                    HostControlInfo hostControlInfo = CreateHostControl(step.HostPopupInfo);
                    Step newStep = CreateStep(step, hostControlInfo, totalTooltips);
                    newStep.GuideName = guide.Name;

                    //If the UI Automation info was read from the json file then we create an StepUIAutomation instance containing all the info for each automation entry
                    if (step.UIAutomation != null)
                    {
                        foreach (var automation in step.UIAutomation)
                        {
                            newStep.UIAutomation.Add(automation);
                        }
                    }

                    //If PreValidationInfo != null means that was deserialized correctly from the json file
                    if (step.PreValidationInfo != null)
                    {
                        newStep.PreValidationInfo = new PreValidation(step.PreValidationInfo);
                    }

                    if (newStep != null)
                    {
                        //Passing the DynamoViewModel to each step so we can execute the Pre Validation methods 
                        newStep.DynamoViewModelStep = dynamoViewModel;

                        newStep.StepGuideBackground = guideBackgroundElement;
                        newStep.MainWindow = mainRootElement;

                        newStep.ExitGuide = step.ExitGuide;

                        //The step is added to the new Guide being created
                        newGuide.GuideSteps.Add(newStep);

                        //We subscribe the handler to the StepClosed even, so every time the popup is closed then this method will be called.
                        newStep.StepClosed += Popup_StepClosed;
                    }
                }
                Guides.Add(newGuide);
            }
        }

        /// <summary>
        /// This method will return a new HostControlInfo object populated with the information passed as parameter
        /// Basically this method store the information coming from Step and search the UIElement in the main WPF VisualTree
        /// </summary>
        /// <param name="jsonStepInfo">Step that contains all the info deserialized from the Json file</param>
        /// <returns></returns>
        private HostControlInfo CreateHostControl(HostControlInfo jsonHostControlInfo)
        {
            //We use the HostControlInfo copy construtor
            var popupInfo = new HostControlInfo(jsonHostControlInfo, mainRootElement);

            //If the CutOff area was defined in the json file then a section of the background overlay will be removed
            if (jsonHostControlInfo.CutOffRectArea != null)
            {
                popupInfo.CutOffRectArea = new CutOffArea()
                {
                    WindowElementNameString = jsonHostControlInfo.CutOffRectArea.WindowElementNameString,
                    NodeId = jsonHostControlInfo.CutOffRectArea.NodeId,
                    WidthBoxDelta = jsonHostControlInfo.CutOffRectArea.WidthBoxDelta,
                    HeightBoxDelta = jsonHostControlInfo.CutOffRectArea.HeightBoxDelta,
                    XPosOffset = jsonHostControlInfo.CutOffRectArea.XPosOffset,
                    YPosOffset = jsonHostControlInfo.CutOffRectArea.YPosOffset
                };
            }

            //If the Highlight area was defined in the json file then a rectangle will be highlighted in the Overlay
            if (jsonHostControlInfo.HighlightRectArea != null)
            {
                //We use the HighlightArea copy construtor
                popupInfo.HighlightRectArea = new HighlightArea(jsonHostControlInfo.HighlightRectArea);
            }

            //The host_ui_element read from the json file need to exists otherwise the host will be null
            UIElement hostUIElement = GuideUtilities.FindChild(mainRootElement, popupInfo.HostUIElementString);
            if (hostUIElement != null)
                popupInfo.HostUIElement = hostUIElement;

            return popupInfo;
        }

        /// <summary>
        /// Creates a new Step with the information passed as parameter (the only extra-information calculated is the TotalTooltips, the Text for Title and Content and other properties like the Suvey.RatingTextTitle
        /// </summary>
        /// <param name="jsonStepInfo">Step that contains all the info deserialized from the Json file</param>
        /// <param name="hostControlInfo">Information of the host read previously</param>
        /// <param name="totalTooltips">Total number of tooltips, calculated once we deserialized all the steps from json</param>
        /// <returns></returns>
        private Step CreateStep(Step jsonStepInfo, HostControlInfo hostControlInfo, int totalTooltips)
        {
            Step newStep = null;
            //This section will retrive the strings from the Resources.resx file
            var formattedText = Res.ResourceManager.GetString(jsonStepInfo.StepContent.FormattedText);
            var title = Res.ResourceManager.GetString(jsonStepInfo.StepContent.Title);

            DirectoryInfo userDataFolder = null;
            if (dynamoViewModel != null)
            {
                var pathManager = dynamoViewModel.Model.PathManager;

                //When executing Dynamo as Sandbox or inside any host like Revit, FormIt, Civil3D the WebView2 cache folder will be located in the AppData folder
                var docsDir = new DirectoryInfo(pathManager.UserDataDirectory);
                userDataFolder = docsDir.Exists ? docsDir : null;

            }

            switch (jsonStepInfo.StepType)
            {
                case Step.StepTypes.TOOLTIP:
                    newStep = new Tooltip(hostControlInfo, jsonStepInfo.Width, jsonStepInfo.Height, jsonStepInfo.TooltipPointerDirection, jsonStepInfo.PointerVerticalOffset)
                    {
                        Name = jsonStepInfo.Name,
                        Sequence = jsonStepInfo.Sequence,
                        TotalTooltips = totalTooltips,
                        StepType = jsonStepInfo.StepType,
                        ShowLibrary = jsonStepInfo.ShowLibrary,
                        StepContent = new Content()
                        {
                            FormattedText = formattedText,
                            Title = title
                        }
                    };
                    var popupWindow = newStep.stepUIPopup as PopupWindow;
                    if(popupWindow != null && hostControlInfo.HtmlPage != null && !string.IsNullOrEmpty(hostControlInfo.HtmlPage.FileName))
                    {
                        popupWindow.WebBrowserUserDataFolder = userDataFolder != null ? userDataFolder.FullName : string.Empty;
                    }
                    break;
                case Step.StepTypes.SURVEY:
                    newStep = new Survey(hostControlInfo, jsonStepInfo.Width, jsonStepInfo.Height)
                    {
                        Sequence = jsonStepInfo.Sequence,
                        RatingTextTitle = formattedText.ToString(),
                        StepType = Step.StepTypes.SURVEY,
                        IsRatingVisible = dynamoViewModel.Model.PreferenceSettings.IsADPAnalyticsReportingApproved,
                        StepContent = new Content()
                        {
                            FormattedText = formattedText,
                            Title = title
                        }
                    };

                    //Due that the RatingTextTitle property is just for Survey then we need to set the property using reflection
                    foreach (var extraContent in jsonStepInfo.StepExtraContent)
                    {
                        // Get the Type object corresponding to Step.
                        Type myType = typeof(Survey);
                        // Get the PropertyInfo object by passing the property name.
                        PropertyInfo myPropInfo = myType.GetProperty(extraContent.Property);
                        if (myPropInfo != null)
                        {
                            //Retrieve the string value from the Resources.resx file
                            var valueStr = Res.ResourceManager.GetString(extraContent.Value);
                            myPropInfo.SetValue(newStep, valueStr);
                        }
                    }
                    break;
                case Step.StepTypes.WELCOME:
                    newStep = new Welcome(hostControlInfo, jsonStepInfo.Width, jsonStepInfo.Height)
                    {
                        Sequence = jsonStepInfo.Sequence,
                        StepType = Step.StepTypes.WELCOME,
                        StepContent = new Content()
                        {
                            FormattedText = formattedText,
                            Title = title
                        }
                    };
                    break;
            }//StepType

            newStep.GuideManager = this;

            return newStep;
        }

        internal Guide GetNextGuide()
        {
            var nextGuide = Guides.FirstOrDefault(x => x.SequenceOrder == currentGuide.SequenceOrder + 1);
            return nextGuide;
        }


        private void Popup_StepClosed(string name, Step.StepTypes stepType)
        {            
            GuideFlowEvents.OnGuidedTourFinish(currentGuide.Name);

            //The exit tour popup will be shown only when a popup (doesn't apply for survey) is closed or when the tour is closed. 
            if(stepType != Step.StepTypes.SURVEY)
                CreateRealTimeInfoWindow(Res.ExitTourWindowContent);
        }

        /// <summary>
        /// Display a notification to display on top right corner of canvas
        /// and display message passed as param
        /// </summary>
        /// <param name="content">The target content to display.</param>
        /// <param name="stayOpen">boolean indicates if the popup will stay open until user dismiss it.</param>
        /// TODO: Make this API out of guide manager to a more generic place
        internal void CreateRealTimeInfoWindow(string content, bool stayOpen = false)
        {
            //Search a UIElement with the Name "statusBarPanel" inside the Dynamo VisualTree
            UIElement hostUIElement = GuideUtilities.FindChild(mainRootElement, "statusBarPanel");

            // When popup already exist, replace it
            if ( exitTourPopup != null && exitTourPopup.IsOpen)
            {
                exitTourPopup.IsOpen = false;
            }
            // Otherwise creates the RealTimeInfoWindow popup and set up all the needed values
            // to show the popup over the Dynamo workspace
            exitTourPopup = new RealTimeInfoWindow()
            {
                VerticalOffset = ExitTourVerticalOffset,
                HorizontalOffset = ExitTourHorizontalOffset,
                Placement = PlacementMode.Left,
                TextContent = content,
                StaysOpen = stayOpen
            };

            if (hostUIElement != null)
                exitTourPopup.PlacementTarget = hostUIElement;
            exitTourPopup.IsOpen = true;
        }
    }
}
