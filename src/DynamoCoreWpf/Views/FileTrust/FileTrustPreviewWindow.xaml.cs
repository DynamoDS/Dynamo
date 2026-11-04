using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dynamo.Logging;
using Dynamo.Wpf.ViewModels.FileTrust;

namespace Dynamo.Wpf.Views.FileTrust
{
    /// <summary>
    /// Modal trust prompt shown before a graph from an untrusted location is opened.
    /// It shows a read-only schematic of the graph built straight from the file's JSON,
    /// so the user can see what they are opening without Dynamo creating any nodes.
    /// </summary>
    internal partial class FileTrustPreviewWindow : Window
    {
        private readonly string filePath;
        private readonly ILogger logger;

        /// <summary>
        /// True when the user chose to open the file.
        /// </summary>
        internal bool Accepted { get; private set; }

        /// <summary>
        /// True when the "trust this location" checkbox was checked when the window closed.
        /// </summary>
        internal bool TrustLocation => TrustLocationCheckBox.IsChecked == true;

        internal FileTrustPreviewWindow(string filePath, ILogger logger)
        {
            InitializeComponent();

            this.filePath = filePath;
            this.logger = logger;

            FilePathTextBlock.Text = filePath;
            FilePathTextBlock.ToolTip = filePath;

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            AcceptButton.Focus();

            ImageSource previewImage = null;
            try
            {
                // Parse and draw off the UI thread so the prompt stays responsive for very large graphs.
                var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
                previewImage = await Task.Run(() =>
                {
                    var preview = GraphPreview.FromFile(filePath);
                    return GraphPreviewRenderer.Render(preview, pixelsPerDip);
                });
            }
            catch (Exception ex)
            {
                // The preview is best effort: the trust decision must still be possible without it.
                logger?.Log($"Could not build a preview for '{filePath}': {ex.Message}");
            }

            if (!IsLoaded) return;

            if (previewImage == null)
            {
                PreviewStatusTextBlock.Text = Properties.Resources.FileTrustPreviewUnavailable;
                return;
            }

            PreviewImage.Source = previewImage;
            PreviewStatusTextBlock.Visibility = Visibility.Collapsed;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            DialogResult = true;
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            DialogResult = false;
        }

        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            DeclineButton_Click(sender, e);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
