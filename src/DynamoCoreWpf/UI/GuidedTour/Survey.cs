using Dynamo.Wpf.ViewModels.GuidedTour;
using Dynamo.Wpf.Views.GuidedTour;

namespace Dynamo.Wpf.UI.GuidedTour
{
    public class Survey : Step
    {
        public string LinkNexGuide { get; set; }

        public string RatingTextTitle { get; set; }

        public double ContentWidth { get; set; }

        public bool IsRatingVisible { get; set; }

        public Survey(HostControlInfo host, double width, double height)
            : base(host, width, height)
        {
            //In the Survey constructor we call the base constructor passing the host information and the popup width and height
        }

        protected override void CreatePopup()
        {
            var surveyPopupViewModel = new SurveyPopupViewModel(this);

            stepUIPopup = new SurveyPopupWindow(surveyPopupViewModel, HostPopupInfo);
        }
    }
}