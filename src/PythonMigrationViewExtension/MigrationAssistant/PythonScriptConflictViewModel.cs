using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.PythonMigration.Differ;
using Dynamo.PythonMigration.Properties;
using Dynamo.ViewModels;
using Dynamo.Wpf.Utilities;
using PythonNodeModels;

namespace Dynamo.PythonMigration.MigrationAssistant
{
    /// <summary>
    /// Before/after review of a Python script. Does not run the 2-to-3 migrator.
    /// The view model only shows the diff and runs the Accept / Reject action it was given;
    /// <see cref="Create"/> builds those actions for each <see cref="PythonScriptReviewKind"/>.
    /// Left is what the user has now, right is the incoming change; Accept takes it, Reject keeps the left.
    /// </summary>
    internal sealed class PythonScriptConflictViewModel : NotificationObject, ICodeDiffHost
    {
        private readonly PythonNode node;
        private readonly string nodeScriptAtOpen;
        private readonly Func<bool> accept;
        private readonly Func<bool> reject;
        private readonly SideBySideDiffModel diffModel;
        private IDiffViewViewModel currentViewModel;

        public string WindowTitle { get; }

        public bool TrackAsMigration => false;

        public IDiffViewViewModel CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                RaisePropertyChanged(nameof(CurrentViewModel));
            }
        }
        
        private PythonScriptConflictViewModel(
            PythonNode node,
            string titleFormat,
            PythonScriptConflictEventArgs e,
            Func<bool> accept,
            Func<bool> reject)
        {
            this.node = node;
            this.accept = accept;
            this.reject = reject;
            nodeScriptAtOpen = node.Script ?? string.Empty;
            WindowTitle = string.Format(CultureInfo.CurrentCulture, titleFormat, node.Name);

            diffModel = new SideBySideDiffBuilder().BuildDiffModel(e.LeftCode, e.RightCode, false);
            CurrentViewModel = new SideBySideViewModel(diffModel);
        }

        /// <summary>
        /// Builds the review for one request: its title and what Accept / Reject do.
        /// </summary>
        internal static PythonScriptConflictViewModel Create(
            PythonNode node,
            PythonScriptConflictEventArgs e,
            DynamoViewModel dynamoViewModel)
        {
            switch (e.Kind)
            {
                case PythonScriptReviewKind.AssistantConflict:
                    // Nothing was written yet. Accept stores the assistant script and replaces the unsaved
                    // editor text (the editor keeps it in its undo history). Reject keeps the user's edits.
                    return new PythonScriptConflictViewModel(
                        node, Resources.PythonScriptConflictWindowTitle, e,
                        accept: () => WriteScript(dynamoViewModel, node, e.RightCode, overrideUnsavedEditor: true),
                        reject: null);

                case PythonScriptReviewKind.AssistantReview:
                    // Already written. Accept keeps it. Reject restores the previous script,
                    // but never over unsaved editor edits.
                    return new PythonScriptConflictViewModel(
                        node, Resources.PythonScriptReviewWindowTitle, e,
                        accept: null,
                        reject: () => WriteScript(dynamoViewModel, node, e.LeftCode, overrideUnsavedEditor: false));

                case PythonScriptReviewKind.EditorChangedUnderneath:
                    // The editor owns both actions.
                    return new PythonScriptConflictViewModel(
                        node, Resources.PythonScriptChangedOutsideEditorWindowTitle, e,
                        accept: e.OnAccept,
                        reject: e.OnReject);

                default:
                    throw new ArgumentOutOfRangeException(nameof(e), e.Kind, null);
            }
        }

        public void ChangeViewModel(ViewMode viewMode)
        {
            CurrentViewModel = viewMode == ViewMode.Inline
                ? new InLineViewModel(diffModel)
                : new SideBySideViewModel(diffModel);
        }

        public bool ChangeCode() => Run(accept);

        public bool RejectCode() => Run(reject);

        /// <summary>
        /// Runs an action only if the node still holds the script it had when the window opened,
        /// so a stale review never overwrites newer work. When nothing was done, tells the user
        /// and returns false so the window stays open.
        /// </summary>
        private bool Run(Func<bool> action)
        {
            if (NodeUnchangedSinceOpen() && (action == null || action()))
            {
                return true;
            }

            if (!DynamoModel.IsTestMode)
            {
                MessageBoxService.Show(
                    Resources.PythonScriptReviewNothingChangedMessage,
                    WindowTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            return false;
        }

        private bool NodeUnchangedSinceOpen()
            => string.Equals(node.Script ?? string.Empty, nodeScriptAtOpen, StringComparison.Ordinal);

        /// <summary>
        /// Writes <paramref name="script"/> to the node in one undo step.
        /// </summary>
        /// <param name="overrideUnsavedEditor">
        /// True when the user chose to replace unsaved editor text (conflict Accept).
        /// </param>
        private static bool WriteScript(
            DynamoViewModel dynamoViewModel,
            PythonNode node,
            string script,
            bool overrideUnsavedEditor)
        {
            script = script ?? string.Empty;
            if (string.Equals(node.Script ?? string.Empty, script, StringComparison.Ordinal))
            {
                return true;
            }

            var hadUnsavedEdits = node.HasUnsavedEditorChanges;
            if (hadUnsavedEdits && !overrideUnsavedEditor)
            {
                return false;
            }

            node.ScriptContentSaved = true;
            dynamoViewModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    WorkspaceIdOf(dynamoViewModel, node), node.GUID, "ScriptContent", script));

            if (string.Equals(node.Script ?? string.Empty, script, StringComparison.Ordinal))
            {
                return true;
            }

            node.ScriptContentSaved = !hadUnsavedEdits;
            return false;
        }

        private static Guid WorkspaceIdOf(DynamoViewModel dynamoViewModel, PythonNode node)
        {
            var workspace = dynamoViewModel.Model.Workspaces
                .FirstOrDefault(w => w.Nodes.Any(n => n.GUID == node.GUID));
            return (workspace ?? dynamoViewModel.Model.CurrentWorkspace).Guid;
        }
    }
}
