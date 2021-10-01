using System;
using System.Globalization;
using System.IO;
using System.Reflection;
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
        //This variable represents a sum of the header and footer of the main popup to compensate the hight of this popup
        private const int headerAndFooterCompensation = 150;
        //This variable represents the width size of the tooltip to relocate this popup
        private const int tooltipOffset = 10;
        //The headers size of the main popup to add an offset and adjust this popup
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

            var bodyHtmlPage = GuidedTourResources.LoadContentFromResources(hInfo.HtmlPage, GetType().Assembly);

            webBrowser.NavigateToString(bodyHtmlPage);
        }
    }
}