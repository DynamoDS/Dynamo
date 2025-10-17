using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Dynamo.Annotations;
using Dynamo.Controls;
using Dynamo.Events;
using Dynamo.Logging;
using Dynamo.Session;
using Dynamo.Utilities;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// A stylised version of the WPF MessageBox class, integrated with the MessageBoxService.
    /// Sets a CustomDialogResult property instead of DialogResult during ShowDialog().
    /// </summary>
    public partial class DynamoMessageBox : INotifyPropertyChanged
    {
        #region Private Fields
        private string titleText;
        private string bodyText;
        private MessageBoxButton messageBoxButton;
        private MessageBoxImage messageBoxImage;
        #endregion

        #region Public Properties
        
        [Obsolete("Do not use, instead use CustomDialogResult. This property will not be set during ShowDialog()")]
        public new bool? DialogResult
        {
            get => base.DialogResult;
            set => base.DialogResult = value;
        }

        internal MessageBoxResult CustomDialogResult { get; set; } = MessageBoxResult.None;

        /// <summary>
        /// The title/caption of the message
        /// </summary>
        public string TitleText
        {
            get => titleText;
            set
            {
                titleText = value; 
                OnPropertyChanged(nameof(TitleText));
            }
        }

        /// <summary>
        /// The body text, i.e. the longform message being conveyed to the user.
        /// </summary>
        public string BodyText
        {
            get => bodyText;
            set
            {
                bodyText = value; 
                OnPropertyChanged(nameof(BodyText));
            }
        }

        /// <summary>
        /// A Windows Enum, determines which standard buttons are displayed to the user.
        /// </summary>
        public MessageBoxButton MessageBoxButton
        {
            get => messageBoxButton;
            set
            {
                messageBoxButton = value;
                OnPropertyChanged(nameof(MessageBoxButton));
            }
        }

        /// <summary>
        /// A Windows Enum, determines the styling of the message, e.g. Info, Warning, Error, etc.
        /// </summary>
        public MessageBoxImage MessageBoxImage
        {
            get => messageBoxImage;
            set
            {
                messageBoxImage = value;
                OnPropertyChanged(nameof(MessageBoxImage));
            }
        }

        /// <summary>
        /// A tooltip is shown on the message box when this is set to true and if
        /// Tooltip is non-null and non-empty.  
        /// </summary>
        public bool ShowTooltip { get; private set; }

        /// <summary>
        /// A tooltip is shown on the message box when this is set to a non-empty string
        /// and ShowTooltip is true.
        /// </summary>
        public string Tooltip { get; private set; }

        /// <summary>
        /// A list of customization options for dialog box
        /// </summary>
        public enum DialogFlags
        {
            //Enables scrollable text in the message box
            Scrollable = 0,
        }

        #endregion

        /// <summary>
        /// Public constructor
        /// </summary>
        public DynamoMessageBox()
        {
            // CER: 52164327 - Catching all exceptions to prevent the application from crashing
            try
            {
                InitializeComponent();
                DataContext = this;
                ShowTooltip = false;
                ToolTip = "";
            }
            catch (Exception ex)
            {
                var dynamoLogger = ExecutionEvents.ActiveSession?.GetParameterValue(ParameterKeys.Logger) as DynamoLogger;
                dynamoLogger?.Log("Failed to initialize DynamoMessageBox: " + ex.Message, LogLevel.Console);
            }
        }

        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="messageBoxText"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button,
            MessageBoxImage icon, string tooltip = "")
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button,
                MessageBoxImage = icon,
                ShowTooltip = !string.IsNullOrEmpty(tooltip),
                Tooltip = tooltip
            };

            dynamoMessageBox.ConfigureButtons(button);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }


        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="messageBoxText">Content of the message</param>
        /// <param name="caption">MessageBox title</param>
        /// <param name="showRichTextBox">True if we will be using the RichTextBox instead of the usual one</param>
        /// <param name="button">OK button shown in the MessageBox</param>
        /// <param name="icon">Type of message: Warning, Error</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, bool showRichTextBox, MessageBoxButton button,
           MessageBoxImage icon)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button,
                MessageBoxImage = icon
            };
            
            if (showRichTextBox)
            {
                dynamoMessageBox.BodyTextBlock.Visibility = Visibility.Collapsed;
                dynamoMessageBox.ContentRichTextBox.Visibility = Visibility.Visible;
            }             
            dynamoMessageBox.ConfigureButtons(button);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }
        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="owner">owning window of the messagebox</param>
        /// <param name="messageBoxText">Content of the message</param>
        /// <param name="caption">MessageBox title</param>
        /// <param name="showRichTextBox">True if we will be using the RichTextBox instead of the usual one</param>
        /// <param name="button">OK button shown in the MessageBox</param>
        /// <param name="icon">Type of message: Warning, Error</param>
        /// <returns></returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, bool showRichTextBox, MessageBoxButton button,
           MessageBoxImage icon)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button,
                MessageBoxImage = icon
            };
            SetOwnerWindow(owner, dynamoMessageBox);
            if (showRichTextBox)
            {
                dynamoMessageBox.BodyTextBlock.Visibility = Visibility.Collapsed;
                dynamoMessageBox.ContentRichTextBox.Visibility = Visibility.Visible;
            }
            dynamoMessageBox.ConfigureButtons(button);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }

        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="owner">owner window</param>
        /// <param name="messageBoxText"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static MessageBoxResult Show(Window owner,string messageBoxText, string caption, MessageBoxButton button,
            MessageBoxImage icon)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button,
                MessageBoxImage = icon
            };
            SetOwnerWindow(owner, dynamoMessageBox);
            dynamoMessageBox.ConfigureButtons(button);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }

        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="owner">owning window of the messagebox</param>
        /// <param name="messageBoxText">Content of the message</param>
        /// <param name="caption">MessageBox title</param>
        /// <param name="flags">Provide a list of flags that can be used to customize the dialog box, e.g Scrollable</param>
        /// <param name="button">Type of button shown in the MessageBox: Ok, OkCancel; etc</param>
        /// <param name="icon">Type of message: Warning, Error</param>
        /// <returns></returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, Dictionary<DialogFlags, bool> flags, MessageBoxButton button,
           MessageBoxImage icon)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button,
                MessageBoxImage = icon
            };
            SetOwnerWindow(owner, dynamoMessageBox);
            if (flags.TryGetValue(DialogFlags.Scrollable, out bool scrollable) && scrollable)
            {
                dynamoMessageBox.BodyTextBlock.Visibility = Visibility.Collapsed;
                dynamoMessageBox.ScrollableBodyTextBlock.Visibility = Visibility.Visible;
            }
            dynamoMessageBox.ConfigureButtons(button);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }

        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="messageBoxText"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button
            };

            dynamoMessageBox.ConfigureButtons(button);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }

        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="messageBoxText"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption
            };

            dynamoMessageBox.ConfigureButtons(MessageBoxButton.OK);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }

        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="messageBoxText"></param>
        /// <returns></returns>
        public static MessageBoxResult? Show(string messageBoxText)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText
            };
            dynamoMessageBox.ConfigureButtons(MessageBoxButton.OK);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }

        internal static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, IEnumerable<string> buttonNames,
            MessageBoxImage icon)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button,
                MessageBoxImage = icon
            };

            dynamoMessageBox.ConfigureButtons(button,buttonNames);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }

        internal static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, IEnumerable<string> buttonNames,
            MessageBoxImage icon)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button,
                MessageBoxImage = icon
            };
            SetOwnerWindow(owner, dynamoMessageBox);
            dynamoMessageBox.ConfigureButtons(button, buttonNames);
            dynamoMessageBox.ShowDialog();
            return dynamoMessageBox.CustomDialogResult;
        }

        /// <summary>
        /// Set the owner window of the message box and prevent any exceptions that may occur
        /// </summary>
        /// <param name="owner">Owner Window</param>
        /// <param name="dynamoMessageBox">New message box</param>
        internal static void SetOwnerWindow(Window owner, DynamoMessageBox dynamoMessageBox)
        {
            if (owner != null && owner.IsLoaded)
            {
                try
                {
                    dynamoMessageBox.Owner = owner;
                }
                catch (Exception)
                {
                    // In this case, we will not set the owner window
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Shows or hides buttons on the view as necessary.
        /// </summary>
        /// <param name="messageBoxButton"></param>
        /// <param name="buttonNames">names that will be used to override the standard button names.
        /// Number of names must match the number of visible buttons set by messageBoxButton parameter.</param>
        internal void ConfigureButtons(MessageBoxButton messageBoxButton, IEnumerable<string> buttonNames = null)
        {
            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    if(buttonNames!=null && buttonNames.Count() == 1)
                    {
                        OkButton.Content = buttonNames.ElementAt(0);
                    }
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    if (buttonNames != null && buttonNames.Count() == 2)
                    {
                        OkButton.Content = buttonNames.ElementAt(0);
                        CancelButton.Content = buttonNames.ElementAt(1);
                    }
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    if (buttonNames != null && buttonNames.Count() == 3)
                    {
                        YesButton.Content = buttonNames.ElementAt(0);
                        NoButton.Content = buttonNames.ElementAt(1);
                        CancelButton.Content = buttonNames.ElementAt(2);
                    }
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNo:
                    if (buttonNames != null && buttonNames.Count() == 2)
                    {
                        YesButton.Content = buttonNames.ElementAt(0);
                        NoButton.Content = buttonNames.ElementAt(1);
                    }
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomDialogResult = MessageBoxResult.None;
            Close();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomDialogResult = MessageBoxResult.OK;
            Close();
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomDialogResult = MessageBoxResult.Yes;
            Close();
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomDialogResult = MessageBoxResult.No;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            CustomDialogResult = MessageBoxResult.Cancel;
            Close();
        }

        // ESC Button pressed triggers Window close        
        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
