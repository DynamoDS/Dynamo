using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Utilities;
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
        //Assembly path to the Font file
        private const string mainFontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";
        //Assembly path to the Resources folder
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

        /// <summary>
        /// Loads HTML file from resource assembly and replace it's key values by base64 files
        /// </summary>
        /// <param name="htmlPage">Contains filename and resources to be loaded in page</param>
        private void LoadWebBrowser(HtmlPage htmlPage)
        {
            var bodyHtmlPage = GuidedTourResources.LoadContentFromResources(htmlPage.FileName, GetType().Assembly);

            bodyHtmlPage = LoadResouces(bodyHtmlPage, htmlPage.Resources);
            bodyHtmlPage = LoadResourceAndReplaceByKey(bodyHtmlPage, "#fontStyle", mainFontStylePath);

            webBrowser.NavigateToString(bodyHtmlPage);
        }

        /// <summary>
        /// Loads resource from a dictionary and replaces its key by an embedded file
        /// </summary>
        /// <param name="bodyHtmlPage">Html page string</param>
        /// <param name="resources">Resources to be loaded</param>
        /// <returns></returns>
        private string LoadResouces(string bodyHtmlPage, Dictionary<string, string> resources)
        {
            if (resources != null && resources.Any())
            {
                foreach (var resource in resources)
                {
                    bodyHtmlPage = LoadResourceAndReplaceByKey(bodyHtmlPage, resource.Key, $"{resourcesPath}.{resource.Value}");
                }
            }

            return bodyHtmlPage;
        }

        /// <summary>
        /// Finds a key word inside the html page and replace by a resource file
        /// </summary>
        /// <param name="bodyHtmlPage">Current html page</param>
        /// <param name="key">Key that is going to be replaced</param>
        /// <param name="resourceFile">Resource file to be included in the page</param>
        /// <returns></returns>
        private string LoadResourceAndReplaceByKey(string bodyHtmlPage, string key, string resourceFile)
        {
            Stream resourceStream = GuidedTourResources.LoadResource(resourceFile);

            if (resourceStream != null)
            {
                var resourceBase64 = ResourceUtilities.ConvertToBase64(resourceStream);
                bodyHtmlPage = bodyHtmlPage.Replace(key, resourceBase64);
            }

            return bodyHtmlPage;
        }
    }
}