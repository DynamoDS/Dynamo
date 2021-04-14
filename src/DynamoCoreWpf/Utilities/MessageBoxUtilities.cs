using System.Windows;

namespace Dynamo.Wpf.Utilities
{
    // Wrapper over MessageBox.Show
    // Useful for testing
    internal class MessageBoxService {
        internal interface IMessageBox
        {
            MessageBoxResult Show(string msg, string title, MessageBoxButton button, MessageBoxImage img);
        }

        // Default implementation of the IDialogService interface
        // Use this instead of MessageBox.Show
        internal class DefaultMessageBox : IMessageBox
        {
            MessageBoxResult IMessageBox.Show(string msg, string title, MessageBoxButton button, MessageBoxImage img)
            {
                return MessageBox.Show(msg, title, button, img);
            }
        }

        private static IMessageBox msg_box;

        // Use this method to override the internal IMessageBox interface with a mocked implementation.
        internal static void OverrideMessageBoxDuringTests(IMessageBox msgBox) { msg_box = msgBox; }

        internal static MessageBoxResult Show(string msg, string title, MessageBoxButton button, MessageBoxImage img)
        {
            return (msg_box ?? (msg_box = new DefaultMessageBox())).Show(msg, title, button, img);
        }
    }
}
