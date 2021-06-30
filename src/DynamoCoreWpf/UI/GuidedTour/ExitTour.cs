using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Wpf.ViewModels.GuidedTour;
using Dynamo.Wpf.Views.GuidedTour;

namespace Dynamo.Wpf.UI.GuidedTour
{
    /// <summary>
    /// This class represent the Notification Toast popup that will be shown when a Dynamo Guided Tour is closed
    /// </summary>
    public class ExitTour : Step
    {
        public ExitTour(HostInfo host, double width, double height)
            : base(host, width, height)
        {

        }

        protected override void CreatePopup()
        {
            //In this place will go the code for creating a ExitTourWindow window (Notification Toast)
        }
    }
}
