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

                var localPath = ((a.Uri == null) ? string.Empty : a.Uri.LocalPath);
                if (localPath.ToLowerInvariant().Equals("/register"))
                {
                    // This is navigating to new user registration page,
                    // which requires the window to be of a larger size.
                    Width = 460;
                    Height = 722;
                }
            };

            Browser.Navigate(_location.AbsoluteUri);
        }
    }
}
