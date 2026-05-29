using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Dynamo.Graph.Workspaces.Locking;

namespace Dynamo.UI.Prompts
{
    internal sealed class WpfGraphLockUserPrompt : IGraphLockUserPrompt
    {
        private readonly Func<Window> ownerProvider;

        internal WpfGraphLockUserPrompt(Func<Window> ownerProvider)
        {
            this.ownerProvider = ownerProvider;
        }

        public GraphLockUserResponse AskUser(string graphPath, GraphLockInfo existingLock, bool isStale)
        {
            var owner = ownerProvider?.Invoke();
            var result = DynamoMessageBox.Show(
                owner,
                BuildBody(graphPath, existingLock, isStale),
                "File already open in another Dynamo instance",                                                                         // LOCALISE THAT STRING
                MessageBoxButton.OKCancel,
                new[]
                {
                    "Save as",
                    "Cancel"
                },
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.OK)
            {
                return GraphLockUserResponse.Cancel();
            }

            var saveAsPath = ShowSaveAsDialog(graphPath);
            return string.IsNullOrEmpty(saveAsPath) ? GraphLockUserResponse.Cancel() : GraphLockUserResponse.SaveAs(saveAsPath);
        }

        private static string ShowSaveAsDialog(string graphPath)
        {
            using (var dialog = new System.Windows.Forms.SaveFileDialog())
            {
                var extension = Path.GetExtension(graphPath);
                dialog.DefaultExt = string.IsNullOrEmpty(extension) ? "dyn" : extension.TrimStart('.');
                dialog.Filter = "Dynamo graphs (*.dyn;*.dyf)|*.dyn;*.dyf|All files (*.*)|*.*";
                dialog.FileName = Path.GetFileName(graphPath);

                var directory = Path.GetDirectoryName(graphPath);
                if (Directory.Exists(directory))
                {
                    dialog.InitialDirectory = directory;
                }

                return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                    ? dialog.FileName
                    : null;
            }
        }

        private static string BuildBody(string graphPath, GraphLockInfo existingLock, bool isStale)
        {
            var fileName = Path.GetFileName(graphPath);

            var message = string.Format(                                                                                                   // LOCALISE THIS STRING
                    CultureInfo.CurrentCulture,
                    "This file is currently open in another instance of Dynamo."
                     + Environment.NewLine
                     + "To avoid data loss or corruption, please choose one of the following actions:"
                     + Environment.NewLine
                     + " • Save the graph with a different name to create a new version of the file."
                     + Environment.NewLine
                     + " • Cancel to return to the workspace and close the other instance of Dynamo.",
                    graphPath);

            if (existingLock == null) 
            {
                return message;
            }

                var format = isStale
                ? "This file appears to already be open, but the lock may be stale. Cancel or save a copy."                                           // LOCALISE THIS STRING                 
                : message;

            return string.Format(
                CultureInfo.CurrentCulture,
                format,
                fileName,
                existingLock.UserName,
                existingLock.MachineName);                                                                                                  // WE CAN USE THOSE
        }
    }
}
