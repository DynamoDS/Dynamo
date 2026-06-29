using Dynamo.Graph.Workspaces.Locking;
using Dynamo.UI.Prompts;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.UI;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace Dynamo.Wpf.Services
{
    /// <summary>
    /// Shows WPF UI for graph-lock conflicts.
    /// </summary>
    internal sealed class WpfGraphLockUserPrompt : IGraphLockUserPrompt
    {
        private readonly Func<Window> ownerProvider;
        private readonly string productNameProvider;
        private readonly Func<IFileSaver> dialogFactory;
        private readonly Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> showMessageBox;

        /// <summary>
        /// Initializes a WPF graph-lock prompt.
        /// </summary>
        /// <param name="ownerProvider">Provides the owner window when a prompt is shown.</param>
        /// <param name="productName">The product name for save-dialog filters.</param>
        /// <param name="dialogFactory">Optional factory for the save dialog; defaults to <see cref="CustomSaveFileDialog"/>.</param>
        /// <param name="showMessageBox">Optional message box function used for conflict and overwrite prompts.</param>
        internal WpfGraphLockUserPrompt(
            Func<Window> ownerProvider,
            string productName,
            Func<IFileSaver> dialogFactory = null,
            Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> showMessageBox = null)
        {
            this.ownerProvider = ownerProvider;
            this.productNameProvider = productName;
            this.dialogFactory = dialogFactory ?? (() => new CustomSaveFileDialog());
            this.showMessageBox = showMessageBox ?? System.Windows.Forms.MessageBox.Show;
        }

        /// <summary>
        /// Shows a Dynamo message box for a graph-lock conflict and optionally collects a copy destination.
        /// </summary>
        /// <param name="graphPath">The locked graph path.</param>
        /// <param name="existingLock">The existing lock metadata, or null if unavailable.</param>
        /// <returns>The user's graph-lock decision.</returns>
        public GraphLockUserResponse AskUser(string graphPath, GraphLockInfo existingLock)
        {
            var result = DynamoMessageBox.Show(
                ownerProvider?.Invoke(),
                Resources.GraphLockFileAlreadyOpenMessage,
                Resources.GraphLockFileAlreadyOpenTitle,
                MessageBoxButton.OKCancel,
                new[]
                {
                    Resources.GraphLockSaveAsButton,
                    Resources.CancelButton
                },
                MessageBoxImage.Warning,
                maxWidth: 600);

            if (result != MessageBoxResult.OK)
            {
                return GraphLockUserResponse.Cancel();
            }

            var saveAsPath = ShowSaveAsDialog(graphPath);
            return string.IsNullOrEmpty(saveAsPath) ? GraphLockUserResponse.Cancel() : GraphLockUserResponse.SaveAs(saveAsPath);
        }

        // Shows a Save As dialog for the copy path, matching the graph file extension
        internal string ShowSaveAsDialog(string graphPath)
        {
            var extension = Path.GetExtension(graphPath);
            var directory = Path.GetDirectoryName(graphPath);
            var isCustomNode = extension.Equals(".dyf", StringComparison.OrdinalIgnoreCase);
            var productName = productNameProvider ?? "Dynamo";
            var defaultExt = isCustomNode ? ".dyf" : ".dyn";
            var filter = isCustomNode
                ? string.Format(Resources.FileDialogDynamoCustomNode, productName, "*.dyf")
                : string.Format(Resources.FileDialogDynamoWorkspace, productName, "*.dyn");

            var normalizedSourcePath = Path.GetFullPath(graphPath);

            var dialog = dialogFactory();
            dialog.AddExtension = true;
            dialog.DefaultExt = defaultExt;
            dialog.Filter = filter;
            dialog.FileName = Path.GetFileName(graphPath);
            dialog.OverwritePrompt = false;

            if (Directory.Exists(directory))
            {
                dialog.InitialDirectory = directory;
            }

            dialog.FileOk += (sender, e) =>
            {
                var chosenPath = Path.GetFullPath(dialog.FileName);

                // Block saving over the locked source file
                if (string.Equals(chosenPath, normalizedSourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    showMessageBox(
                        Resources.GraphLockSaveAsSameFileMessage,
                        Resources.GraphLockFileAlreadyOpenTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    e.Cancel = true;
                    return;
                }

                // OverwritePrompt is false so we must replicate the OS overwrite warning
                // for any other existing file the user may have chosen.
                if (File.Exists(chosenPath))
                {
                    var confirm = showMessageBox(
                        string.Format(Resources.ConfirmReplaceFileMessage, Path.GetFileName(chosenPath)),
                        Resources.ConfirmReplaceFileTitle,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (confirm != DialogResult.Yes)
                    {
                        e.Cancel = true;
                    }
                }
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
