using Dynamo.Wpf.ViewModels.GuidedTour;
using Dynamo.Wpf.Views.GuidedTour;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// The Welcome class represents a Popup that will be shown over the Dynamo UI when a Guided Tour starts
    /// </summary>
    public class Welcome : Step
    {
        public Welcome(HostControlInfo host, double width, double height)
            : base(host, width, height)
        {
            //In the Welcome constructor we call the base constructor passing the host information and the popup width and height
        }

        protected override void CreatePopup()
        {
            //Creating the ViewModel for the PopupWindow (View)
            var popupViewModel = new PopupWindowViewModel(this)
            { 
                //Controls setup specific for a Welcome popup
                ShowPopupPointer = false,
                ShowPopupButton = true,
                ShowTooltipNavigationControls = false,
                Width = Width,
                Height = Height
            };
            //The host and the viewmodel is passed as parameters to PopupWindow
            stepUIPopup = new PopupWindow(popupViewModel, HostPopupInfo);
        }
    }
}
