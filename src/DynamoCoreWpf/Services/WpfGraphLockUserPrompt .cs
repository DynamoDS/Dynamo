using Dynamo.Graph.Workspaces.Locking;
using Dynamo.UI.Prompts;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.UI;
using System;
using System.IO;
using System.Windows;

namespace Dynamo.Wpf.Services
{
    internal sealed class WpfGraphLockUserPrompt : IGraphLockUserPrompt
    {
        private readonly Func<Window> ownerProvider;
        private readonly Func<string> productNameProvider;

        internal WpfGraphLockUserPrompt(Func<Window> ownerProvider, Func<string> productNameProvider)
        {
            this.ownerProvider = ownerProvider;
            this.productNameProvider = productNameProvider;
        }

        /// <summary>
        /// Asks the user whether to cancel opening the locked graph or save a copy to open instead.
        /// </summary>
        /// <param name="graphPath">The path of the graph that is already locked.</param>
        /// <param name="existingLock">The existing lock metadata, or null if the lock file could not be read.</param>
        /// <param name="isStale">Whether the existing lock appears stale.</param>
        /// <returns>The user's graph-lock decision.</returns>
        public GraphLockUserResponse AskUser(string graphPath, GraphLockInfo existingLock, bool isStale)
        {
            var owner = ownerProvider?.Invoke();
            var message =
@"This file is currently open in another instance of Dynamo.
To avoid data loss or corruption, please choose one of the following actions:
 • Save the graph with a different name to create a new version of the file.
 • Cancel to return to the workspace and close the other instance of Dynamo.";
            var header = "File already open in another Dynamo instance";                        // LOCALISE THAT STRING

            var result = DynamoMessageBox.Show(
                owner,
                message,
                header,                                                                         // LOCALISE THAT STRING
                MessageBoxButton.OKCancel,
                new[]
                {
                    "Save as",                                                                  // LOCALISE THAT STRING
                    "Cancel"                                                                    // LOCALISE THAT STRING
                },
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.OK)
            {
                return GraphLockUserResponse.Cancel();
            }

            var saveAsPath = ShowSaveAsDialog(graphPath);
            return string.IsNullOrEmpty(saveAsPath) ? GraphLockUserResponse.Cancel() : GraphLockUserResponse.SaveAs(saveAsPath);
        }

        private string ShowSaveAsDialog(string graphPath)
        {
            var extension = Path.GetExtension(graphPath);
            var directory = Path.GetDirectoryName(graphPath);
            var isCustomNode = extension.Equals(".dyf", StringComparison.OrdinalIgnoreCase);
            var productName = productNameProvider?.Invoke() ?? "Dynamo";
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
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
