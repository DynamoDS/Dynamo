using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dynamo.PackageManager.UI
{
    /// <summary>
    /// Interaction logic for TermsOfUseView.xaml
    /// </summary>
    public partial class TermsOfUseView : Window
    {
        public bool AcceptedTermsOfUse { get; private set; }

        public TermsOfUseView(string touFilePath)
        {
            InitializeComponent();
            AcceptedTermsOfUse = false;
            TermsOfUseContent.File = touFilePath;
        }

        private void AcceptTermsOfUseButton_OnClick(object sender, RoutedEventArgs e)
        {
            AcceptedTermsOfUse = true;
            Close();
        }

        private void DeclineTermsOfUseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
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
            if((sender as Button).Name.Equals("MaximizeButton"))
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
