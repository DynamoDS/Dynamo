
namespace Dynamo.Logging
{
    /// <summary>
    /// Class representing a notification that a host Application intends to send
    /// to a Dynamo user.
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// The sender of this notification.
        /// </summary>
        public string Sender {get;private set;}
        /// <summary>
        /// The title of the notification message.
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// A short message that is displayed to inform the user
        /// briefly for the reason of the notification
        /// </summary>
        public string ShortMessage { get; private set; }
        /// <summary>
        /// a more detailed message that displayed 
        /// </summary>
        public string DetailedMessage { get; private set; }

        /// <summary>
        /// Constructor for a NotificationMessage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="shortMessage"></param>
        /// <param name="detailedMessage"></param>
        /// <param name="title"></param>
        public NotificationMessage(string sender,string shortMessage, string detailedMessage,string title = "Notification")
        {
            this.Sender = sender;
            Title = title;
            ShortMessage = shortMessage;
            DetailedMessage = detailedMessage;
        }

    }
}