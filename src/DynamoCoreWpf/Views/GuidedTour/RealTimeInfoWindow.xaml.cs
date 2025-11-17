using Dynamo.Wpf.UI.GuidedTour;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace Dynamo.Wpf.Views.GuidedTour
{
    /// <summary>
    /// Interaction logic for RealTimeInfoWindow.xaml
    /// </summary>
    public partial class RealTimeInfoWindow : Popup
    {
        private Hyperlink fileLink;

        /// <summary>
        /// This property contains the text that will be shown in the popup and it can be updated on runtime.
        /// </summary>
        public string TextContent { get; set; }

        /// <summary>
        /// This property contains the text that will be shown in the Hyperlink
        /// </summary>
        public string HyperlinkText { get; set; }

        /// <summary>
        /// This property contains the text that will be shown in the Header
        /// </summary>
        public string HeaderContent { get; set; }

        /// <summary>
        /// This property contains the URI that will be opened when the Hyperlink is clicked
        /// </summary>
        public Uri HyperlinkUri { get; set; }

        /// <summary>
        /// This property contains the path that will be opened when the file link is clicked
        /// </summary>
        public Uri FileLinkUri { get; set; }

        public RealTimeInfoWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void CleanRealTimeInfoWindow()
        {
            IsOpen = false;
        }

        /// <summary>
        /// This method is executed when the Close button in the RealTimeInfo window is pressed, so we clean the subscriptions to events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CleanRealTimeInfoWindow();
        }

        /// <summary>
        /// This method will update the current location of the RealTimeInfo window due that probably the window was moved or resized
        /// </summary>
        public void UpdateLocation()
        {
            if (IsOpen == true)
            {
                //This section will update the Popup location by calling the private method UpdatePosition using reflection
                var positionMethod = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                positionMethod.Invoke(this, null);
            }
        }
        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BorderLine.Y2 = PopupGrid.ActualHeight /*+ ((TextBlock)sender).Margin.Bottom*/;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        internal void SetToastMessage(string sentencePrefix, bool showFileLink, Uri fileUri)
        {
            MessageTextBlock.Inlines.Clear();
            MessageTextBlock.Inlines.Add(new Run(sentencePrefix ?? string.Empty));

            if (showFileLink && fileUri != null)
            {
                if (fileLink != null)
                {
                    fileLink.RequestNavigate -= Hyperlink_RequestNavigate;
                }

                fileLink = new Hyperlink(new Run(Dynamo.Wpf.Properties.Resources.ToastHyperlinkPathText))
                {
                    NavigateUri = fileUri
                };

                var brush = TryFindResource("TextBlockLinkForegroundColor") as Brush;
                if (brush != null)
                {
                    fileLink.Foreground = brush;
                }

                fileLink.TextDecorations = null;

                fileLink.RequestNavigate += Hyperlink_RequestNavigate;

                MessageTextBlock.Inlines.Add(fileLink);
                MessageTextBlock.Inlines.Add(new Run("."));
            }
        }

        internal void UpdateVisualState()
        {
            HeaderTextBlock.Visibility =
                string.IsNullOrWhiteSpace(HeaderContent)
                ? Visibility.Collapsed
                : Visibility.Visible;

            HyperlinkTextBlock.Visibility =
                string.IsNullOrWhiteSpace(HyperlinkText)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}
