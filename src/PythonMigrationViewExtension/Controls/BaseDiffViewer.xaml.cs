using System;
using System.Windows;
using System.Windows.Input;
using Dynamo.Logging;
using Dynamo.PythonMigration.MigrationAssistant;

namespace Dynamo.PythonMigration.Controls
{
    /// <summary>
    /// Interaction logic for BaseDiffViewer.xaml
    /// </summary>
    public partial class BaseDiffViewer : Window
    {
        private PythonMigrationAssistantViewModel ViewModel { get; set; }

        // we would like to match initial text wrapping of code inside diff view
        // to that of the code inside the script editor window as much as possible
        private int scriptEditorWindowDefaultWidth = 600;
        private int differAdditionalWidthPerPanel = 20;

        internal BaseDiffViewer(PythonMigrationAssistantViewModel viewModel) : base()
        {
            ViewModel = viewModel;
            DataContext = viewModel;
            // The diff viewer is initialized with the SideBySide view
            SetSideBySideWindowWidth();
            InitializeComponent();
        }

        private void OnDiffButtonClick(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentViewModel.ViewMode == Differ.ViewMode.Inline)
            {
                ViewModel.ChangeViewModel(Differ.ViewMode.SideBySide);
                SetSideBySideWindowWidth();
            }
            else
            {
                ViewModel.ChangeViewModel(Differ.ViewMode.Inline);
                SetInlineWindowWidth();
            }
        }

        private void SetSideBySideWindowWidth()
        {
            Width = scriptEditorWindowDefaultWidth * 2 + differAdditionalWidthPerPanel * 1.5;
        }

        private void SetInlineWindowWidth()
        {
            Width = scriptEditorWindowDefaultWidth + differAdditionalWidthPerPanel;
        }

        private void OnAcceptButtonClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.ChangeCode();
            this.Close();
            // Record if changes are accepted and if there are proposed changes
            Analytics.TrackEvent(
                Dynamo.Logging.Actions.Migration,
                Dynamo.Logging.Categories.PythonOperations,
                "Accept",
                Convert.ToInt32(ViewModel.CurrentViewModel.HasChanges));
        }

        private void OnRejectButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
            Analytics.TrackEvent(
                Dynamo.Logging.Actions.Migration,
                Dynamo.Logging.Categories.PythonOperations,
                "Reject",
                Convert.ToInt32(ViewModel.CurrentViewModel.HasChanges));
        }

        // Handles Close button 'X' 
        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button).Name.Equals("MaximizeButton"))
            {
                WindowState = WindowState.Maximized;
                ToggleButtons(true);
            }
            else
            {
                WindowState = WindowState.Normal;
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
                MaximizeButton.Visibility = Visibility.Collapsed;
                NormalizeButton.Visibility = Visibility.Visible;
            }
            else
            {
                MaximizeButton.Visibility = Visibility.Visible;
                NormalizeButton.Visibility = Visibility.Collapsed;
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
