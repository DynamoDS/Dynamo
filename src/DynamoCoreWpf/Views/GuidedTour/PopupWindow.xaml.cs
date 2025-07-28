using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Utilities;
using Dynamo.Wpf.ViewModels.GuidedTour;

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
        private bool canMoveStep = true;

        private const string packagesTourName = "packages";
        //Field that indicates wheter popups are left-aligned or right-aligned
        private const string menuDropAligment = "_menuDropAlignment";

        internal DynamoWebView2 webBrowserComponent;
        //Assembly path to the Font file
        private const string mainFontStylePath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources.ArtifaktElement-Regular.woff";
        //Assembly path to the Resources folder
        private const string resourcesPath = "Dynamo.Wpf.Views.GuidedTour.HtmlPages.Resources";

        /// <summary>
        /// This property will be hold the path of the WebView2 cache folder, the value will change based in if DynamoSandbox is executed or Dynamo is executed from a different host (like Revit, FormIt, Civil, etc).
        /// </summary>
        internal string WebBrowserUserDataFolder { get; set; }

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

                // Opened event ensures the webview2 will be visible when added to the popup layout tree.
                InitWebView2Component();
            }
        }

        private async void InitWebView2Component()
        {
            webBrowserComponent = new DynamoWebView2();

            webBrowserComponent.Margin = new System.Windows.Thickness(popupBordersOffSet, 0, 0, 0);
            webBrowserComponent.Width = popupViewModel.Width;
            //The height is subtracted by a const that sums the height of the header and footer of the popup
            var heightBottom = bottomGrid.ActualHeight;
            var heightTitle = titleGrid.ActualHeight;
            //popupBordersOffSet * 2 because is one offset at the top and the other one at the bottom of the Popup
            webBrowserComponent.Height = popupViewModel.Height - (heightBottom + heightTitle + popupBordersOffSet * 2);
            contentGrid.Children.Add(webBrowserComponent);
            Grid.SetRow(webBrowserComponent, 1);

            var localeStr = popupViewModel.Step.DynamoViewModelStep.PreferenceSettings.Locale;
            ResourceUtilities.LoadWebBrowser(hostControlInfo.HtmlPage, webBrowserComponent, resourcesPath, mainFontStylePath, GetType().Assembly, WebBrowserUserDataFolder, localeStr);
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

        private async void Popup_KeyDown(object sender, KeyEventArgs e)
        {
            if (canMoveStep)
            {
                canMoveStep = false;

                switch (e.Key)
                {
                    case Key.Left:
                        GuideFlowEvents.OnGuidedTourPrev();
                        break;
                    case Key.Right:
                        GuideFlowEvents.OnGuidedTourNext();
                        break;
                }
                //Adds a delay of 500ms to avoid Dynamo crash with a quick switch with the keys
                await Task.Delay(500);

                canMoveStep = true;
            }
        }
    }
}
