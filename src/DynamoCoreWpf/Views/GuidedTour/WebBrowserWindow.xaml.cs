using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        private const string mainFontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";

        private const string resourcesPath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources";

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

            LoadWebBrowser(hInfo.HtmlPage);


        }

        private void LoadWebBrowser(HtmlPage htmlPage)
        {
            var bodyHtmlPage = GuidedTourResources.LoadContentFromResources(htmlPage.FileName, GetType().Assembly);

            bodyHtmlPage = LoadResouces(bodyHtmlPage, htmlPage);
            bodyHtmlPage = LoadResourceAndReplaceByKey(bodyHtmlPage, "#fontStyle", mainFontStylePath);

            webBrowser.NavigateToString(bodyHtmlPage);
        }

        private string LoadResouces(string bodyHtmlPage, HtmlPage htmlPage)
        {
            if (htmlPage.Resources.Any())
            {
                foreach (var resource in htmlPage.Resources)
                {
                    bodyHtmlPage = LoadResourceAndReplaceByKey(bodyHtmlPage, resource.Key, $"{resourcesPath}.{resource.Value}");
                }
            }

            return bodyHtmlPage;
        }

        private string LoadResourceAndReplaceByKey(string bodyHtmlPage, string key, string resourceFile)
        {
            Stream resourceStream = GuidedTourResources.LoadResource(resourceFile);

            if (resourceStream != null)
            {
                var resourceBase64 = ConvertToBase64(resourceStream);
                bodyHtmlPage = bodyHtmlPage.Replace(key, resourceBase64);
            }

            return bodyHtmlPage;
        }

        private string ConvertToBase64(Stream stream)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }
    }
}