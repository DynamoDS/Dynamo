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
        private const int headerAndFooterCompensation = 150;
        private const int tooltipOffset = 10;
        private const int headerOffset = 50;
        public WebBrowserWindow(PopupWindowViewModel viewModel, HostControlInfo hInfo)
        {
            InitializeComponent();
            
            webBrowser.Width = viewModel.Width;
            //The height is subtracted by a const that sums the height of the header and footer of the popup
            webBrowser.Height = viewModel.Height - headerAndFooterCompensation;

            //Setting the host over which the popup will appear and the placement mode
            PlacementTarget = hInfo.HostUIElement;
            Placement = hInfo.PopupPlacement;

            //Horizontal offset plus 10 is to compensate the tooltip size
            HorizontalOffset = hInfo.HorizontalPopupOffSet + tooltipOffset;
            //Vertical offset plus 50 is to compensate the header size
            VerticalOffset = hInfo.VerticalPopupOffSet + headerOffset;

            string curDir = Directory.GetCurrentDirectory();
            Uri webBrowserUri = new Uri(string.Format("file:///{0}/Views/GuidedTour/HtmlPages/{1}.html", curDir, hInfo.HtmlPage));
            webBrowser.Navigate(webBrowserUri);
        }
    }
}