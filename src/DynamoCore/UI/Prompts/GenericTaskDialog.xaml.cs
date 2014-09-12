using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Dynamo.UI.Prompts
{
    internal class TaskDialogEventArgs : EventArgs
    {
        List<Tuple<int, string, bool>> buttons = null;

        #region Public Operational Methods

        internal TaskDialogEventArgs(Uri imageUri, string dialogTitle,
            string summary, string description)
        {
            this.ImageUri = imageUri;
            this.DialogTitle = dialogTitle;
            this.Summary = summary;
            this.Description = description;
        }

        internal void AddLeftAlignedButton(int id, string content)
        {
            if (buttons == null)
                buttons = new List<Tuple<int, string, bool>>();

            buttons.Add(new Tuple<int, string, bool>(id, content, true));
        }

        internal void AddRightAlignedButton(int id, string content)
        {
            if (buttons == null)
                buttons = new List<Tuple<int, string, bool>>();

            buttons.Add(new Tuple<int, string, bool>(id, content, false));
        }

        #endregion

        #region Public Class Properties

        // Settable properties.
        internal int ClickedButtonId { get; set; }
        internal Exception Exception { get; set; }

        // Read-only properties.
        internal Uri ImageUri { get; private set; }
        internal string DialogTitle { get; private set; }
        internal string Summary { get; private set; }
        internal string Description { get; private set; }

        internal IEnumerable<Tuple<int, string, bool>> Buttons
        {
            get { return buttons; }
        }

        #endregion
    }

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
            this.SummaryText.Text = taskDialogParams.Summary;
            this.DescriptionText.Text = taskDialogParams.Description;

            InitializeButtons();
            InitializeDetailedContent();
        }

        #endregion

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

            var style = SharedDictionaryManager.DynamoModernDictionary["STextButton"];

            foreach (var button in buttons)
            {
                Button buttonElement = new Button();
                buttonElement.Tag = button.Item1;
                buttonElement.Content = button.Item2;
                buttonElement.Style = style as Style;
                buttonElement.Click += OnButtonElementClicked;

                if (button.Item3 != false)
                {
                    buttonElement.Margin = new Thickness(0, 0, 10, 0);
                    LeftButtonStackPanel.Children.Add(buttonElement);
                }
                else
                {
                    buttonElement.Margin = new Thickness(10, 0, 0, 0);
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

        #endregion
    }
}
