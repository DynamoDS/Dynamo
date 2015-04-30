using System;
using System.Windows;

namespace Dynamo.Wpf
{
    public partial class BrowserWindow
    {
        private readonly Uri _location;

        public BrowserWindow(Uri location)
        {
            _location = location;
            Loaded += Window_Loaded;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // hide loading grid when navigation ready
            Browser.Navigated += (s, a) =>
            {
                LoadingTextBlock.Visibility = Visibility.Collapsed;
                Browser.Visibility = Visibility.Visible;

                var windowSize = new Size();
                var localPath = ((a.Uri == null) ? string.Empty : a.Uri.LocalPath);
                if (GetWindowSizeForContent(localPath, ref windowSize))
                {
                    Width = windowSize.Width;
                    Height = windowSize.Height;
                }
            };

            Browser.Navigate(_location.AbsoluteUri);
        }

        private bool GetWindowSizeForContent(string localPath, ref Size size)
        {
            switch (localPath.ToLowerInvariant()) // All lower case!
            {
                case "/logon":
                    size.Width = 360;
                    size.Height = 365;
                    return true;

                case "/register":
                    size.Width = 460;
                    size.Height = 722;
                    return true;

                case "/account/forgotcredentials":
                    size.Width = 640;
                    size.Height = 260;
                    return true;
            }

            return false; // No window size change required.
        }
    }
}
