using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dynamo.Models;

namespace Dynamo.UI.Prompts
{

    public partial class GenericTaskDialog : Window
    {
        private TaskDialogEventArgs taskDialogParams = null;

        #region Public Operational Methods

        public GenericTaskDialog() // Xaml design needs this.
        {
            InitializeComponent();
        }

        internal GenericTaskDialog(TaskDialogEventArgs taskDialogParams)
        {
            this.taskDialogParams = taskDialogParams;
            InitializeComponent();
            ClearDefaultContents();

            this.DialogIcon.Source = new BitmapImage(taskDialogParams.ImageUri);
            this.Title = taskDialogParams.DialogTitle;
            this.TitleTextBlock.Text = taskDialogParams.DialogTitle;
            this.SummaryText.Text = taskDialogParams.Summary;
            this.DescriptionText.Text = taskDialogParams.Description;

            InitializeButtons();
            InitializeDetailedContent();
        }

        #endregion

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
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

        #region Private Class Helper Methods

        private void ClearDefaultContents()
        {
            LeftButtonStackPanel.Children.Clear();
            RightButtonStackPanel.Children.Clear();
            DetailedContent.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void InitializeButtons()
        {
            var buttons = this.taskDialogParams.Buttons;
            if (buttons == null)
                return;

            var style = SharedDictionaryManager.DynamoModernDictionary["CtaButtonStyle"];

            foreach (var button in buttons)
            {
                Button buttonElement = new Button();
                buttonElement.Tag = button.Item1;
                buttonElement.Content = button.Item2;
                buttonElement.Style = style as Style;
                buttonElement.BorderBrush = (SolidColorBrush) new BrushConverter().ConvertFrom("#0696D7");
                buttonElement.Foreground = Brushes.White;
                buttonElement.Background = (SolidColorBrush) new BrushConverter().ConvertFrom("#0696D7");
                buttonElement.Click += OnButtonElementClicked;

                if (button.Item3 != false)
                {
                    buttonElement.Margin = new System.Windows.Thickness(0, 0, 10, 0);
                    LeftButtonStackPanel.Children.Add(buttonElement);
                }
                else
                {
                    buttonElement.Margin = new System.Windows.Thickness(10, 0, 0, 0);
                    RightButtonStackPanel.Children.Add(buttonElement);
                }
            }
        }

        private void InitializeDetailedContent()
        {
            if (this.taskDialogParams.Exception == null)
                return;

            var e = this.taskDialogParams.Exception;
            var contents = string.Format("{0}\n\n{1}", e.Message, e.StackTrace);
            DetailedContent.Text = contents;
            DetailedContent.Visibility = System.Windows.Visibility.Visible;
        }

        private void OnButtonElementClicked(object sender, RoutedEventArgs e)
        {
            int buttonId = (int)((sender as Button).Tag);
            this.taskDialogParams.ClickedButtonId = buttonId;
            this.Close();
        }

        // ESC Button pressed triggers Window close        
        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }
}
