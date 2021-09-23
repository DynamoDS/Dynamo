using System;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.ViewModels.GuidedTour;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class WebBrowserWindow : Popup
    {
        public WebBrowserWindow(PopupWindowViewModel viewModel, HostControlInfo hInfo)
        {
            InitializeComponent();

            //Due that we are drawing the Direction pointers on left and right side of the Canvas(10 width each one) then we need to add 20
            webBrowser.Width = viewModel.Width;
            webBrowser.Height = viewModel.Height - 150;

            //Setting the host over which the popup will appear and the placement mode
            PlacementTarget = hInfo.HostUIElement;
            Placement = hInfo.PopupPlacement;

            //The CustomRichTextBox has a margin of 10 in left and 10 in right, also there is a indentation for drawing the PointerDirection of 10 of each side so that's why we are subtracting 40.
            HorizontalOffset = hInfo.HorizontalPopupOffSet + 10;
            VerticalOffset = hInfo.VerticalPopupOffSet + 50;

            string curDir = Directory.GetCurrentDirectory();
            Uri webBrowserUri = new Uri(string.Format("file:///{0}/Views/GuidedTour/HtmlPages/{1}.html", curDir, hInfo.HtmlPage));
            webBrowser.Navigate(webBrowserUri);
        }
    }
}