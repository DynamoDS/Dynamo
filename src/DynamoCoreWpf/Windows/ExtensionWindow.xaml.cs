using System;
using System.Windows;

namespace Dynamo.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for ExtensionWindow.xaml
    /// </summary>
    public partial class ExtensionWindow : ModelessChildWindow
    {
        /// <summary>
        /// Indicates whether the window was closed using the dock button.
        /// Note: Setter is internal for testing purposes only.
        /// </summary>
        public bool DockRequested { get; internal set; }

        public ExtensionWindow()
        {
            InitializeComponent();
        }

        private void OnDockButtonClick(object sender, RoutedEventArgs e)
        {
            DockRequested = true;
            Close();
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (WindowState == WindowState.Maximized)
            {
                maximizeButton.Visibility = Visibility.Collapsed;
                restoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                maximizeButton.Visibility = Visibility.Visible;
                restoreButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ExtensionWindow_StateChanged(object sender, EventArgs e)
        {
            RefreshMaximizeRestoreButton();
        }

        private void ExtensionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // This can't be done using markup, so we do it here.
            iconImage.Source = Icon;
        }
    }
}
