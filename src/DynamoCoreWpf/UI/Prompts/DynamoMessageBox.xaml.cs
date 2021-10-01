using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Dynamo.Annotations;
using Dynamo.Controls;
using Dynamo.Logging;
using Dynamo.Utilities;

namespace Dynamo.UI.Prompts
{
    /// <summary>
    /// A stylised version of the WPF MessageBox class, integrated with the MessageBoxService.
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

        #endregion

        /// <summary>
        /// Public constructor
        /// </summary>
        public DynamoMessageBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Displays a dialog to the user and returns their choice as a MessageBoxResult.
        /// </summary>
        /// <param name="messageBoxText"></param>
        /// <param name="caption"></param>
        /// <param name="button"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button,
            MessageBoxImage icon)
        {
            var dynamoMessageBox = new DynamoMessageBox
            {
                BodyText = messageBoxText,
                TitleText = caption,
                MessageBoxButton = button,
                MessageBoxImage = icon
            };

            dynamoMessageBox.ConfigureButtons(button);
            return dynamoMessageBox.ConvertResult(dynamoMessageBox.ShowDialog());
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
            return dynamoMessageBox.ConvertResult(dynamoMessageBox.ShowDialog());
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
            return dynamoMessageBox.ConvertResult(dynamoMessageBox.ShowDialog());
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
            return dynamoMessageBox.ConvertResult(dynamoMessageBox.ShowDialog());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Shows or hides buttons on the view as necessary.
        /// </summary>
        /// <param name="messageBoxButton"></param>
        private void ConfigureButtons(MessageBoxButton messageBoxButton)
        {
            switch (messageBoxButton)
            {
                case MessageBoxButton.OK:
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.OKCancel:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageBoxButton.YesNoCancel:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Converts the nullable bool result from ShowDialog into the appropriate MessageBoxResult.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public MessageBoxResult ConvertResult(bool? value)
        {
            switch (MessageBoxButton)
            {
                case MessageBoxButton.OK:
                    if (value == null || value == false) return MessageBoxResult.None;
                    return MessageBoxResult.OK;
                case MessageBoxButton.OKCancel:
                    if (value == null) return MessageBoxResult.None;
                    return value == true ? MessageBoxResult.OK : MessageBoxResult.Cancel;
                case MessageBoxButton.YesNoCancel:
                    if (value == null) return MessageBoxResult.None;
                    return value == true ? MessageBoxResult.Yes : MessageBoxResult.Cancel;
                case MessageBoxButton.YesNo:
                    if (value == null) return MessageBoxResult.None;
                    return value == true ? MessageBoxResult.Yes : MessageBoxResult.No;
                default:
                    return MessageBoxResult.None;
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
            Analytics.TrackEvent(
                Actions.Move,
                Categories.PackageManagerOperations);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void YesButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void NoButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
