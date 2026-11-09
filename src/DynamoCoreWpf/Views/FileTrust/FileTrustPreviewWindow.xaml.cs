using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Dynamo.Exceptions;
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

        /// <summary>
        /// Hash of the file text the preview was built from, or null when the file could not be read.
        /// Passed to the open so Dynamo loads only the content the user approved.
        /// </summary>
        internal string ContentHash { get; private set; }

        internal FileTrustPreviewWindow(string filePath, ILogger logger)
        {
            InitializeComponent();

            this.filePath = filePath;
            this.logger = logger;

            FilePathTextBlock.Text = filePath;
            FilePathTextBlock.ToolTip = filePath;

            // Yes stays disabled until the file has been read, so an approval is always tied to content.
            AcceptButton.IsEnabled = false;

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            FitToWorkArea();
            // The safe choice has focus, so a stray Enter or Space never accepts an untrusted file.
            DeclineButton.Focus();

            ImageSource previewImage = null;
            // Catch everything: this is an async void handler, so an escaping exception would take down
            // Dynamo, and the preview is optional. The failure is logged and the prompt still works.
            try
            {
                // Read once, off the UI thread, and hash exactly the text the preview is built from.
                var fileText = await Task.Run(() => File.ReadAllText(filePath));
                ContentHash = FileChangedSincePreviewException.ComputeContentHash(fileText);
                if (!IsLoaded) return;
                AcceptButton.IsEnabled = true;

                // Parse and draw off the UI thread so the prompt stays responsive for very large graphs.
                var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
                previewImage = await Task.Run(() =>
                {
                    var preview = GraphPreview.FromText(fileText);
                    return GraphPreviewRenderer.Render(preview, pixelsPerDip);
                });
            }
            catch (Exception ex)
            {
                // The preview is best effort: the trust decision must still be possible without it.
                logger?.Log($"Could not build a preview for '{filePath}': {ex.Message}");
            }

            if (!IsLoaded) return;
            AcceptButton.IsEnabled = true;

            if (previewImage == null)
            {
                PreviewStatusTextBlock.Text = Properties.Resources.FileTrustPreviewUnavailable;
                return;
            }

            PreviewImage.Source = previewImage;
            PreviewStatusTextBlock.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Shrinks the preview when the window is taller than the work area of the monitor it is on
        /// (for example on small high-DPI screens), so the checkbox and buttons always stay visible.
        /// </summary>
        private void FitToWorkArea()
        {
            var handle = new WindowInteropHelper(Owner ?? this).Handle;
            if (handle == IntPtr.Zero) return;

            var dpi = VisualTreeHelper.GetDpi(this);
            var workingArea = System.Windows.Forms.Screen.FromHandle(handle).WorkingArea;
            var workTop = workingArea.Top / dpi.DpiScaleY;
            var workHeight = workingArea.Height / dpi.DpiScaleY;

            UpdateLayout();
            var overflow = ActualHeight - workHeight;
            if (overflow <= 0) return;

            PreviewBorder.Height = Math.Max(MinPreviewHeight, PreviewBorder.ActualHeight - overflow);
            UpdateLayout();
            Top = workTop + Math.Max(0, (workHeight - ActualHeight) / 2);
        }

        private const double MinPreviewHeight = 80;

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
