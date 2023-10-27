using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Dynamo.Logging;
using Dynamo.Services;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// Interaction logic for UsageReportingAgreementPrompt.xaml
    /// </summary>
    public partial class UsageReportingAgreementPrompt : Window
    {
        private DynamoViewModel viewModel = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceProvider"></param>
        /// <param name="dynamoViewModel"></param>
        public UsageReportingAgreementPrompt(IBrandingResourceProvider resourceProvider, DynamoViewModel dynamoViewModel)
        {
            InitializeComponent();
            if (resourceProvider != null)
            {
                TitleTextBlock.Text = resourceProvider.GetString(Wpf.Interfaces.ResourceNames.ConsentForm.Title);
            }
            viewModel = dynamoViewModel;

            var adpAnalyticsFile = "ADPAnalyticsConsent.rtf";

            if (viewModel.Model.PathManager.ResolveDocumentPath(ref adpAnalyticsFile))
                ADPAnalyticsConsent.File = adpAnalyticsFile;

            //disable adp configure dialog version check fails.
            //also disabled below id all analytics disabled.
            configure_adp_button.IsEnabled = AnalyticsService.IsADPAvailable();

            if (Analytics.DisableAnalytics)
            {
                configure_adp_button.IsEnabled = false;
            }
           
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            viewModel = null;
        }

        private void configure_adp_button_Click(object sender, RoutedEventArgs e)
        {
            IntPtr handle = new IntPtr();
            if (Owner != null)
            {
                handle = new WindowInteropHelper(Owner).Handle;
            }
            AnalyticsService.ShowADPConsentDialog(handle);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name.Equals("MaximizeButton"))
            {
                this.WindowState = WindowState.Maximized;
                ToggleButtons(true);
            }
            else
            {
                this.WindowState = WindowState.Normal;
                ToggleButtons(false);
            }
        }


        /// <summary>
        /// Toggles between the Maximize and Normalize buttons on the window
        /// </summary>
        /// <param name="toggle"></param>
        private void ToggleButtons(bool toggle)
        {
            if (toggle)
            {
                this.MaximizeButton.Visibility = Visibility.Collapsed;
                this.NormalizeButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.MaximizeButton.Visibility = Visibility.Visible;
                this.NormalizeButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Lets the user drag this window around with their left mouse button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            DragMove();
        }
    }
}
