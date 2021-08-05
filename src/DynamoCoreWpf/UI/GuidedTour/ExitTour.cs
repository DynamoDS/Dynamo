using Dynamo.Wpf.Views.GuidedTour;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class represent the Notification Toast popup that will be shown when a Dynamo Guided Tour is closed
    /// </summary>
    public class ExitTour : Step
    {
        public ExitTour(HostControlInfo host, double width, double height)
            : base(host, width, height)
        {
            //In the ExitTour constructor we call the base constructor passing the host information and the popup width and height
        }

        protected override void CreatePopup()
        {
            //The Popup Viewmodel and Host is passed as parameters to the PopupWindow so it will create the popup with the needed values (width, height)
            stepUIPopup = new ExitTourWindow(this, HostPopupInfo);
        }
    }
}
