using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dynamo.Utilities;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.ViewModels.GuidedTour;
using Microsoft.Web.WebView2.Wpf;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Popup
    {
        private const int popupBordersOffSet = 10;
        private PopupWindowViewModel popupViewModel;
        private HostControlInfo hostControlInfo;
        private bool isClosingTour;

        private const string packagesTourName = "packages";
        //Field that indicates wheter popups are left-aligned or right-aligned
        private const string menuDropAligment = "_menuDropAlignment";

        internal WebView2 webBrowserComponent;
        //Assembly path to the Font file
        private const string mainFontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";
        //Assembly path to the Resources folder
        private const string resourcesPath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources";

        public PopupWindow(PopupWindowViewModel viewModel, HostControlInfo hInfo)
        {
            InitializeComponent();

            hostControlInfo = hInfo;
            
            if (viewModel != null)
                popupViewModel = viewModel;

            DataContext = popupViewModel;

            //Due that we are drawing the Direction pointers on left and right side of the Canvas (10 width each one) then we need to add 20
            RootLayout.Width = popupViewModel.Width + (popupBordersOffSet * 2); 
            //Also a shadow of 10 pixels in top and 10 pixels at the bottom will be shown then we need to add 20
            RootLayout.Height = popupViewModel.Height + (popupBordersOffSet * 2);

            //The BackgroundRectangle represent the tooltip background rectangle that is drawn over a Canvas
            //Needs to be moved 10 pixels over the X axis to show the direction pointers (Height was already increased above)
            //Needs to be moved 10 pixels over the Y axis to show the shadow at top and bottom.
            BackgroundRectangle.Rect = new Rect(popupBordersOffSet, popupBordersOffSet, popupViewModel.Width, popupViewModel.Height);

            //Setting the host over which the popup will appear and the placement mode
            PlacementTarget = hInfo.HostUIElement;
            Placement = hInfo.PopupPlacement;

            //The CustomRichTextBox has a margin of 10 in left and 10 in right, also there is a indentation for drawing the PointerDirection of 10 of each side so that's why we are subtracting 40.
            ContentRichTextBox.Width = popupViewModel.Width - (popupBordersOffSet * 4);
            HorizontalOffset = hInfo.HorizontalPopupOffSet;
            VerticalOffset = hInfo.VerticalPopupOffSet;

            Opened += PopupWindow_Opened;
            Closed += PopupWindow_Closed;

            isClosingTour = false; 
            
            EnsureStandardPopupAlignment();
        }

        private void EnsureStandardPopupAlignment()
        {
            var menuDropAlignmentField = typeof(SystemParameters).GetField(menuDropAligment, BindingFlags.NonPublic | BindingFlags.Static);
            if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null)
            {
                //Sets field to false and ignores the alignment
                menuDropAlignmentField.SetValue(null, false);
            }
        }

        private void PopupWindow_Closed(object sender, EventArgs e)
        {
            if (webBrowserComponent != null)
            {
                webBrowserComponent.Visibility = Visibility.Collapsed;
            }


            if (isClosingTour)
            {
                Opened -= PopupWindow_Opened;
                Closed -= PopupWindow_Closed;
            }
        }

        private void PopupWindow_Opened(object sender, EventArgs e)
        {
            if (hostControlInfo.HtmlPage != null && !string.IsNullOrEmpty(hostControlInfo.HtmlPage.FileName))
            {
                ContentRichTextBox.Visibility = Visibility.Hidden;            
                InitWebView2Component();
            }
        }

        private async void InitWebView2Component()
        {
            webBrowserComponent = new WebView2();
            webBrowserComponent.Margin = new System.Windows.Thickness(popupBordersOffSet, 0, 0, 0);
            webBrowserComponent.Width = popupViewModel.Width;
            //The height is subtracted by a const that sums the height of the header and footer of the popup
            var heightBottom = bottomGrid.ActualHeight;
            var heightTitle = titleGrid.ActualHeight;
            //popupBordersOffSet * 2 because is one offset at the top and the other one at the bottom of the Popup
            webBrowserComponent.Height = popupViewModel.Height - (heightBottom + heightTitle + popupBordersOffSet * 2);
            contentGrid.Children.Add(webBrowserComponent);
            Grid.SetRow(webBrowserComponent, 1);

            LoadWebBrowser(hostControlInfo.HtmlPage);
        }

        /// <summary>
        /// Loads HTML file from resource assembly and replace it's key values by base64 files
        /// </summary>
        /// <param name="htmlPage">Contains filename and resources to be loaded in page</param>
        private async void LoadWebBrowser(HtmlPage htmlPage)
        {
            var bodyHtmlPage = ResourceUtilities.LoadContentFromResources(htmlPage.FileName, GetType().Assembly, false, false);

            bodyHtmlPage = LoadResouces(bodyHtmlPage, htmlPage.Resources);
            bodyHtmlPage = LoadResourceAndReplaceByKey(bodyHtmlPage, "#fontStyle", mainFontStylePath);
            await webBrowserComponent.EnsureCoreWebView2Async();
            webBrowserComponent.NavigateToString(bodyHtmlPage);
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
            Stream resourceStream = ResourceUtilities.LoadResourceByUrl(resourceFile);

            if (resourceStream != null)
            {
                var resourceBase64 = ResourceUtilities.ConvertToBase64(resourceStream);
                bodyHtmlPage = bodyHtmlPage.Replace(key, resourceBase64);
            }

            return bodyHtmlPage;
        }

        private void StartTourButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GuideFlowEvents.OnGuidedTourNext();
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IsOpen = false;
            if (popupViewModel.Step.GuideName.ToLower() == packagesTourName)
            {
                GuideFlowEvents.OnGuidedTourFinish(popupViewModel.Step.GuideName);
            }
            else
            {
                isClosingTour = true;
                popupViewModel.Step.OnStepClosed(popupViewModel.Step.Name, popupViewModel.Step.StepType);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            GuideFlowEvents.OnGuidedTourNext();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GuideFlowEvents.OnGuidedTourPrev();
        }

        private void Popup_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    GuideFlowEvents.OnGuidedTourPrev();
                    break;
                case Key.Right:
                    GuideFlowEvents.OnGuidedTourNext();
                    break;
            }
        }
    }
}
