using System.Windows;

namespace Dynamo.Wpf.Utilities
{
    // Wrapper over MessageBox.Show
    // Useful for testing
    internal interface IDialogService
    {
        MessageBoxResult ShowMessageBox(string msg, string title, MessageBoxButton button, MessageBoxImage img);
    }

    // Default implementation of the IDialogService interface
    // Use this instead of MessageBox.Show
    internal class DialogService : IDialogService
    {

        MessageBoxResult IDialogService.ShowMessageBox(string msg, string title, MessageBoxButton button, MessageBoxImage img)
        {
            return MessageBox.Show(msg, title, button, img);
        }
    }
}
