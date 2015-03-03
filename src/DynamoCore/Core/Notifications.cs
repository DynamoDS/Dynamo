using System;

namespace Dynamo.Core
{
    public enum NotificationLevel { Mild, Moderate, Error }

    /// <summary>
    /// The Notifications class is used to raise notifications that
    /// can be handled by the UI or the logger.
    /// </summary>
    public class Notifications
    {
        private static Notifications instance;

        /// <summary>
        /// The Notifications singleton.
        /// </summary>
        public static Notifications Instance
        {
            get { return instance ?? (instance = new Notifications()); }
        }

        public event Action<NotificationEventArgs> NotificationPosted;
        protected virtual void OnNotificationPosted(NotificationEventArgs args)
        {
            if (NotificationPosted != null)
            {
                NotificationPosted(args);
            }
        }

        private Notifications(){}

        public void PostNotification(string message, NotificationLevel level)
        {
            OnNotificationPosted(new NotificationEventArgs(message, level));
        }
    }

    public class NotificationEventArgs : EventArgs
    {
        /// <summary>
        /// The Notification's message;
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// The level of the Notification.
        /// </summary>
        public NotificationLevel Level { get; internal set; }

        public NotificationEventArgs(string message, NotificationLevel level)
        {
            Message = message;
            Level = level;
        }
    }
}
