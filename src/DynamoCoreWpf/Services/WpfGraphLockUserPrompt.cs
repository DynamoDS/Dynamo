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

        /// <summary>
        /// Initializes a WPF graph-lock prompt.
        /// </summary>
        /// <param name="ownerProvider">Provides the owner window when a prompt is shown.</param>
        /// <param name="productNameProvider">Provides the product name for save-dialog filters.</param>
        internal WpfGraphLockUserPrompt(Func<Window> ownerProvider, string productNameProvider)
        {
            this.ownerProvider = ownerProvider;
            this.productNameProvider = productNameProvider;
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
        private string ShowSaveAsDialog(string graphPath)
        {
            var extension = Path.GetExtension(graphPath);
            var directory = Path.GetDirectoryName(graphPath);
            var isCustomNode = extension.Equals(".dyf", StringComparison.OrdinalIgnoreCase);
            var productName = productNameProvider ?? "Dynamo";
            var defaultExt = isCustomNode ? ".dyf" : ".dyn";
            var filter = isCustomNode
                ? string.Format(Resources.FileDialogDynamoCustomNode, productName, "*.dyf")
                : string.Format(Resources.FileDialogDynamoWorkspace, productName, "*.dyn");

            var dialog = new CustomSaveFileDialog
            {
                AddExtension = true,
                DefaultExt = defaultExt,
                Filter = filter,
                FileName = Path.GetFileName(graphPath),
                OverwritePrompt = false,
            };

            if (Directory.Exists(directory))
            {
                dialog.InitialDirectory = directory;
            }

            // Prevent the dialog from closing as long as the chosen path matches the original.
            dialog.FileOk += (sender, e) =>
            {
                if (string.Equals(dialog.FileName, graphPath, StringComparison.OrdinalIgnoreCase))
                {
                    System.Windows.Forms.MessageBox.Show(
                        "This file is currently open in another instance of Dynamo.\nTo avoid data loss or corruption, please choose a different file name.",
                        "File already open in another Dynamo instance",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
