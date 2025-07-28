using System;
using System.Windows.Controls.Primitives;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.ViewModels.GuidedTour;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for SurveyPopupWindow.xaml
    /// </summary>
    public partial class SurveyPopupWindow : Popup
    {
        private readonly SurveyPopupViewModel surveyViewModel;

        private Guide nextGuide;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="hInfo"></param>
        public SurveyPopupWindow(SurveyPopupViewModel viewModel, HostControlInfo hInfo)
        {
            InitializeComponent();
            if (viewModel != null)
                surveyViewModel = viewModel;

            DataContext = surveyViewModel;
            
            //Setting the host over which the popup will appear and the placement mode
            PlacementTarget = hInfo.HostUIElement;
            Placement = hInfo.PopupPlacement;

            //When setting a value for offset it will move the popup taking as a initial position the host control position in which the popup is shown
            HorizontalOffset = hInfo.HorizontalPopupOffSet;
            VerticalOffset = hInfo.VerticalPopupOffSet;

            Focus();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            SetupNextGuideLink();
        }

        private void SetupNextGuideLink()
        {
            nextGuide = surveyViewModel.Step.GuideManager.GetNextGuide();
            
            if(nextGuide != null)
            {
                NextGuideLink.Visibility = System.Windows.Visibility.Visible;
                NextGuideNameButton.Content = Res.ResourceManager.GetString(nextGuide.GuideNameResource).Replace("_", "");
            }
            else
            {
                NextGuideLink.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
            surveyViewModel.Step.OnStepClosed(surveyViewModel.Step.Name, surveyViewModel.Step.StepType);
            Logging.Analytics.TrackEvent(Logging.Actions.Rate, Logging.Categories.Command, surveyViewModel.Step.GuideName, SurveyRatingControl.Value);
        }

        private void GetStartedButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
            surveyViewModel.Step.OnStepClosed(surveyViewModel.Step.Name, surveyViewModel.Step.StepType);

            if (nextGuide.Name.Equals(GuidesManager.OnboardingGuideName))
            {
                if (surveyViewModel.Step.DynamoViewModelStep.ClearHomeWorkspaceInternal())
                {
                    surveyViewModel.Step.DynamoViewModelStep.OpenOnboardingGuideFile();
                    surveyViewModel.Step.GuideManager.LaunchTour(nextGuide.Name);
                }
            }
            else
            {
                surveyViewModel.Step.GuideManager.LaunchTour(nextGuide.Name);
            }

        }
        
        
    }
}
