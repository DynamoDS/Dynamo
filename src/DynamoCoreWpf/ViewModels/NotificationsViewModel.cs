using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using Dynamo.Core;

namespace Dynamo.Wpf.ViewModels
{
    public class NotificationsViewModel : NotificationObject, IDisposable
    {
        private NotificationLevel curentNotificationLevel;
        private string currentNotificationMessage;

        public NotificationLevel CurrentNotificationLevel
        {
            get { return curentNotificationLevel; }
            set
            {
                curentNotificationLevel = value;
                RaisePropertyChanged("CurrentNotificationLevel");
            }
        }

        public string CurrentNotificationMessage
        {
            get{return currentNotificationMessage;}
            set
            {
                currentNotificationMessage = value;
                RaisePropertyChanged("CurrentNotificationMessage");
            }
        }

        public NotificationsViewModel()
        {
            Notifications.Instance.NotificationPosted += NotificationPosted;
        }

        private void NotificationPosted(NotificationEventArgs obj)
        {
            CurrentNotificationLevel = obj.Level;
            CurrentNotificationMessage = obj.Message;
        }

        public void ClearWarning()
        {
            CurrentNotificationMessage = string.Empty;
        }

        public void Dispose()
        {
            Notifications.Instance.NotificationPosted -= NotificationPosted;
        }
    }


    public class NotificationLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = (NotificationLevel)value;
            switch (level)
            {
                case NotificationLevel.Mild:
                    return new SolidColorBrush(Colors.Gray);
                case NotificationLevel.Moderate:
                    return new SolidColorBrush(Colors.Gold);
                case NotificationLevel.Error:
                    return new SolidColorBrush(Colors.Tomato);
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
