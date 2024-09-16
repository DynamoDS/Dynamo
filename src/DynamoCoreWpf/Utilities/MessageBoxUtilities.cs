using System.Collections.Generic;
using System.Windows;
using Dynamo.UI.Prompts;

namespace Dynamo.Wpf.Utilities
{
    // Wrapper over MessageBox.Show
    // Useful for testing
    public class MessageBoxService {
        internal interface IMessageBox
        {
            MessageBoxResult Show(string msg, string title, MessageBoxButton button, MessageBoxImage img);
            MessageBoxResult Show(string msg, string title, bool showRichTextBox, MessageBoxButton button, MessageBoxImage img);
            MessageBoxResult Show(Window owner, string msg, string title, bool showRichTextBox, MessageBoxButton button, MessageBoxImage img);
            MessageBoxResult Show(Window owner, string msg, string title, Dictionary<DynamoMessageBox.DialogFlags, bool> flags, MessageBoxButton button, MessageBoxImage img);
            MessageBoxResult Show(Window owner,string msg, string title, MessageBoxButton button, MessageBoxImage img);
            MessageBoxResult Show(Window owner, string msg, string title, MessageBoxButton button, IEnumerable<string> buttonNames, MessageBoxImage img);
            MessageBoxResult Show(string msg, string title, MessageBoxButton button, IEnumerable<string> buttonNames, MessageBoxImage img);
        }

        // Default implementation of the IDialogService interface
        // Use this instead of MessageBox.Show
        internal class DefaultMessageBox : IMessageBox
        {
            MessageBoxResult IMessageBox.Show(string msg, string title, MessageBoxButton button, MessageBoxImage img)
            {
                return DynamoMessageBox.Show(msg, title, button, img);
            }

            MessageBoxResult IMessageBox.Show(string msg, string title, bool showRichTextBox, MessageBoxButton button, MessageBoxImage img)
            {
                return DynamoMessageBox.Show(msg, title, showRichTextBox, button, img);
            }
            MessageBoxResult IMessageBox.Show(Window owner, string msg, string title, bool showRichTextBox, MessageBoxButton button, MessageBoxImage img)
            {
                return DynamoMessageBox.Show(owner,msg, title, showRichTextBox, button, img);
            }
            MessageBoxResult IMessageBox.Show(Window owner, string msg, string title, Dictionary<DynamoMessageBox.DialogFlags, bool> flags, MessageBoxButton button, MessageBoxImage img)
            {
                return DynamoMessageBox.Show(owner, msg, title, flags, button, img);
            }

            public MessageBoxResult Show(Window owner, string msg, string title, MessageBoxButton button, MessageBoxImage img)
            {
                return DynamoMessageBox.Show(owner,msg, title, button, img);
            }

            MessageBoxResult IMessageBox.Show(string msg, string title, MessageBoxButton button, IEnumerable<string> buttonNames, MessageBoxImage img)
            {
                return DynamoMessageBox.Show(msg, title, button,buttonNames, img);
            }

            MessageBoxResult IMessageBox.Show(Window owner, string msg, string title, MessageBoxButton button, IEnumerable<string> buttonNames, MessageBoxImage img)
            {
                return DynamoMessageBox.Show(owner, msg, title, button, buttonNames, img);
            }
        }

        private static IMessageBox msg_box;

        // Use this method to override the internal IMessageBox interface with a mocked implementation.
        internal static void OverrideMessageBoxDuringTests(IMessageBox msgBox) { msg_box = msgBox; }

        public static MessageBoxResult Show(string msg, string title, MessageBoxButton button, MessageBoxImage img)
        {
            return (msg_box ?? (msg_box = new DefaultMessageBox())).Show(msg, title, button, img);
        }
        public static MessageBoxResult Show(string msg, string title, bool showRichTextBox, MessageBoxButton button, MessageBoxImage img)
        {
            return (msg_box ?? (msg_box = new DefaultMessageBox())).Show(msg, title, showRichTextBox, button, img);
        }
        public static MessageBoxResult Show(Window owner, string msg, string title, bool showRichTextBox, MessageBoxButton button, MessageBoxImage img)
        {
            return (msg_box ?? (msg_box = new DefaultMessageBox())).Show(owner,msg, title, showRichTextBox, button, img);
        }
        public static MessageBoxResult Show(Window owner, string msg, string title, Dictionary<DynamoMessageBox.DialogFlags, bool> flags, MessageBoxButton button, MessageBoxImage img)
        {
            return (msg_box ?? (msg_box = new DefaultMessageBox())).Show(owner, msg, title, flags, button, img);
        }
        public static MessageBoxResult Show(Window owner,string msg, string title, MessageBoxButton button, MessageBoxImage img)
        {
            return (msg_box ?? (msg_box = new DefaultMessageBox())).Show(owner,msg, title, button, img);
        }
        public static MessageBoxResult Show(Window owner, string msg, string title, MessageBoxButton button, IEnumerable<string> buttonNames, MessageBoxImage img)
        {
            return (msg_box ?? (msg_box = new DefaultMessageBox())).Show(owner, msg, title, button, buttonNames, img);
        }
        public static MessageBoxResult Show(string msg, string title, MessageBoxButton button, IEnumerable<string> buttonNames, MessageBoxImage img)
        {
            return (msg_box ?? (msg_box = new DefaultMessageBox())).Show(msg, title, button, buttonNames, img);
        }
    }
}
