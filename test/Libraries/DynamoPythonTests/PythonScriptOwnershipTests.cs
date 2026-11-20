using System;
using System.Threading;
using System.Threading.Tasks;
using Dynamo.Models;
using Dynamo.PythonMigration;
using Dynamo.PythonMigration.Differ;
using Dynamo.PythonMigration.MigrationAssistant;
using Dynamo.Tests;
using NUnit.Framework;
using PythonNodeModels;

namespace DynamoPythonTests
{
    /// <summary>
    /// Model-level tests for who owns a Python node's script while its editor is open,
    /// and for the Accept / Reject actions of the before/after review window.
    /// Editor (UI) behavior is covered in PythonNodeCustomizationTests.
    /// </summary>
    [TestFixture]
    [Category("UnitTests")]
    public class PythonScriptOwnershipTests : DynamoViewModelUnitTest
    {
        private const string OriginalScript = "OUT = 1";
        private const string AssistantScript = "OUT = 2";
        private const string LaterScript = "OUT = 3";
        private const string Turn = "prompt:1";

        #region Helpers

        private PythonNode CreatePythonNodeWithScript(string script)
        {
            var node = new PythonNode();
            ViewModel.Model.ExecuteCommand(
                new DynamoModel.CreateNodeCommand(node, 0, 0, false, false));
            WriteScriptContent(node, script);
            return node;
        }

        /// <summary>
        /// The same write the assistant (DynamoMCP set_node_value) makes.
        /// </summary>
        private void WriteScriptContent(PythonNode node, string value)
        {
            ViewModel.ExecuteCommand(
                new DynamoModel.UpdateModelValueCommand(
                    Guid.Empty, node.GUID, "ScriptContent", value));
        }

        private void Undo()
        {
            ViewModel.UndoCommand.Execute(null);
        }

        private PythonScriptConflictViewModel OpenReview(
            PythonNode node,
            PythonScriptReviewKind kind,
            string left,
            string right,
            Func<bool> onAccept = null,
            Func<bool> onReject = null)
        {
            return PythonScriptConflictViewModel.Create(
                node,
                new PythonScriptConflictEventArgs(kind, left, right, onAccept, onReject),
                ViewModel);
        }

        /// <summary>
        /// Asks for approval the way DynamoMCP does and builds the window's view model.
        /// </summary>
        private PythonScriptConflictViewModel OpenProposal(
            PythonNode node,
            string proposed,
            out Task<bool> answer,
            Action acceptAllForNode = null)
        {
            PythonScriptConflictEventArgs received = null;
            EventHandler<PythonScriptConflictEventArgs> handler = (_, e) => received = e;
            node.ScriptConflictReviewRequested += handler;
            answer = node.RequestScriptApproval(proposed, Turn, CancellationToken.None);
            node.ScriptConflictReviewRequested -= handler;

            Assert.IsNotNull(received);
            return PythonScriptConflictViewModel.Create(node, received, ViewModel, acceptAllForNode);
        }

        #endregion

        #region Ownership flag

        [Test]
        public void HasUnsavedEditorChangesFollowsScriptContentSaved()
        {
            var node = new PythonNode();
            Assert.IsTrue(node.ScriptContentSaved);
            Assert.IsFalse(node.HasUnsavedEditorChanges);

            node.ScriptContentSaved = false;
            Assert.IsTrue(node.HasUnsavedEditorChanges);
        }

        [Test]
        public void HasUnsavedEditorChangesIsNotSavedInTheGraphFile()
        {
            // DynamoMCP reads this property by reflection; it must never end up in the .dyn file.
            var property = typeof(PythonNode).GetProperty(nameof(PythonNode.HasUnsavedEditorChanges));

            Assert.IsNotNull(property);
            Assert.IsTrue(Attribute.IsDefined(property, typeof(Newtonsoft.Json.JsonIgnoreAttribute)));
        }

        #endregion

        #region Writes while the editor has unsaved edits

        [Test]
        public void WhenEditorIsSavedThenScriptContentWriteUpdatesScript()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);

            WriteScriptContent(node, AssistantScript);

            Assert.AreEqual(AssistantScript, node.Script);
        }

        [Test]
        public void WhenEditorHasUnsavedChangesThenScriptContentWriteIsBlockedAndReported()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            node.ScriptContentSaved = false;
            string proposed = null;
            node.ScriptUpdateBlocked += value => proposed = value;

            WriteScriptContent(node, AssistantScript);

            Assert.AreEqual(OriginalScript, node.Script);
            Assert.AreEqual(AssistantScript, proposed);
            Assert.IsTrue(node.HasUnsavedEditorChanges);
        }

        [Test]
        public void WhenUnsavedEditorWriteIsSameAsNodeScriptThenNothingIsReported()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            node.ScriptContentSaved = false;
            var raised = false;
            node.ScriptUpdateBlocked += _ => raised = true;

            WriteScriptContent(node, OriginalScript);

            Assert.IsFalse(raised);
            Assert.AreEqual(OriginalScript, node.Script);
        }

        #endregion

        #region Undo

        [Test]
        public void WhenAssistantWriteIsUndoneThenPreviousScriptIsBackAndEditorIsTold()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            WriteScriptContent(node, AssistantScript);
            var scriptChangeRaised = false;
            System.ComponentModel.PropertyChangedEventHandler handler = (_, e) =>
            {
                if (e.PropertyName == nameof(PythonNode.Script)) scriptChangeRaised = true;
            };
            node.PropertyChanged += handler;

            Undo();

            node.PropertyChanged -= handler;
            Assert.AreEqual(OriginalScript, node.Script);
            // An open editor listens for this to refresh (or to warn on the next Save).
            Assert.IsTrue(scriptChangeRaised);
        }

        [Test]
        public void WhenEditorHasUnsavedChangesThenCanvasUndoStillChangesScript()
        {
            // Undo does not go through the unsaved-editor guard. This is why the editor checks
            // on Save that the node still holds the script it is based on.
            var node = CreatePythonNodeWithScript(OriginalScript);
            WriteScriptContent(node, AssistantScript);
            node.ScriptContentSaved = false;

            Undo();

            Assert.AreEqual(OriginalScript, node.Script);
        }

        #endregion

        #region Review request

        [Test]
        public void WhenNoReviewWindowIsListeningThenRequestReturnsFalse()
        {
            var node = new PythonNode();

            var shown = node.RequestScriptConflictReview(
                PythonScriptReviewKind.AssistantReview, OriginalScript, AssistantScript);

            Assert.IsFalse(shown);
        }

        [Test]
        public void WhenReviewWindowIsListeningThenRequestPassesKindAndBothSides()
        {
            var node = new PythonNode();
            PythonScriptConflictEventArgs received = null;
            node.ScriptConflictReviewRequested += (_, e) => received = e;

            var shown = node.RequestScriptConflictReview(
                PythonScriptReviewKind.AssistantConflict, OriginalScript, AssistantScript);

            Assert.IsTrue(shown);
            Assert.AreEqual(PythonScriptReviewKind.AssistantConflict, received.Kind);
            Assert.AreEqual(OriginalScript, received.LeftCode);
            Assert.AreEqual(AssistantScript, received.RightCode);
        }

        [Test]
        public void WhenNotifyScriptChangeReviewThenAssistantReviewIsRequested()
        {
            var node = new PythonNode();
            PythonScriptConflictEventArgs received = null;
            node.ScriptConflictReviewRequested += (_, e) => received = e;

            node.NotifyScriptChangeReview(OriginalScript, AssistantScript);

            Assert.AreEqual(PythonScriptReviewKind.AssistantReview, received.Kind);
            Assert.AreEqual(OriginalScript, received.LeftCode);
            Assert.AreEqual(AssistantScript, received.RightCode);
        }

        #endregion

        #region Review window: assistant conflict (nothing written yet)

        [Test]
        public void WhenAssistantConflictAcceptThenNodeHoldsAssistantScriptAndEditorIsSaved()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            node.ScriptContentSaved = false;
            var review = OpenReview(node, PythonScriptReviewKind.AssistantConflict, "OUT = 'typing'", AssistantScript);

            var closed = review.ChangeCode();

            Assert.IsTrue(closed);
            Assert.AreEqual(AssistantScript, node.Script);
            Assert.IsFalse(node.HasUnsavedEditorChanges);
        }

        [Test]
        public void WhenAssistantConflictRejectThenNodeAndEditorAreUnchanged()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            node.ScriptContentSaved = false;
            var review = OpenReview(node, PythonScriptReviewKind.AssistantConflict, "OUT = 'typing'", AssistantScript);

            var closed = review.RejectCode();

            Assert.IsTrue(closed);
            Assert.AreEqual(OriginalScript, node.Script);
            Assert.IsTrue(node.HasUnsavedEditorChanges);
        }

        [Test]
        public void WhenNodeChangesAfterConflictOpensThenAcceptDoesNotWrite()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            node.ScriptContentSaved = false;
            var review = OpenReview(node, PythonScriptReviewKind.AssistantConflict, "OUT = 'typing'", AssistantScript);
            Undo(); // Canvas Undo changes the node while the window is open.

            var closed = review.ChangeCode();

            Assert.IsFalse(closed);
            Assert.AreNotEqual(AssistantScript, node.Script);
            Assert.IsTrue(node.HasUnsavedEditorChanges);
        }

        #endregion

        #region Review window: assistant review (already written)

        [Test]
        public void WhenAssistantReviewAcceptThenNodeKeepsAssistantScript()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            WriteScriptContent(node, AssistantScript);
            var review = OpenReview(node, PythonScriptReviewKind.AssistantReview, OriginalScript, AssistantScript);

            var closed = review.ChangeCode();

            Assert.IsTrue(closed);
            Assert.AreEqual(AssistantScript, node.Script);
        }

        [Test]
        public void WhenAssistantReviewRejectThenNodeHoldsPreviousScriptByTextCompare()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            WriteScriptContent(node, AssistantScript);
            var review = OpenReview(node, PythonScriptReviewKind.AssistantReview, OriginalScript, AssistantScript);

            var closed = review.RejectCode();

            Assert.IsTrue(closed);
            Assert.AreEqual(OriginalScript, node.Script);
        }

        [Test]
        public void WhenAssistantReviewRejectIsUndoneThenAssistantScriptIsBackInOneStep()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            WriteScriptContent(node, AssistantScript);
            var review = OpenReview(node, PythonScriptReviewKind.AssistantReview, OriginalScript, AssistantScript);
            review.RejectCode();

            Undo();

            Assert.AreEqual(AssistantScript, node.Script);
        }

        [Test]
        public void WhenAssistantReviewRejectAndEditorUnsavedThenNodeScriptUnchanged()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            WriteScriptContent(node, AssistantScript);
            node.ScriptContentSaved = false;
            var review = OpenReview(node, PythonScriptReviewKind.AssistantReview, OriginalScript, AssistantScript);

            var closed = review.RejectCode();

            Assert.IsFalse(closed);
            Assert.AreEqual(AssistantScript, node.Script);
            Assert.IsTrue(node.HasUnsavedEditorChanges);
        }

        [Test]
        public void WhenNodeChangesAfterReviewOpensThenRejectDoesNotWrite()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            WriteScriptContent(node, AssistantScript);
            var review = OpenReview(node, PythonScriptReviewKind.AssistantReview, OriginalScript, AssistantScript);
            WriteScriptContent(node, LaterScript);

            var closed = review.RejectCode();

            Assert.IsFalse(closed);
            Assert.AreEqual(LaterScript, node.Script);
        }

        #endregion

        #region Review window: changed outside the editor (editor-owned actions)

        [Test]
        public void WhenChangedOutsideEditorAcceptThenEditorAcceptRunsAndItsResultIsReturned()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var acceptCalls = 0;
            var rejectCalls = 0;
            var review = OpenReview(node, PythonScriptReviewKind.EditorChangedUnderneath, "OUT = 'typing'", OriginalScript,
                onAccept: () => { acceptCalls++; return true; },
                onReject: () => { rejectCalls++; return true; });

            var closed = review.ChangeCode();

            Assert.IsTrue(closed);
            Assert.AreEqual(1, acceptCalls);
            Assert.AreEqual(0, rejectCalls);
        }

        [Test]
        public void WhenChangedOutsideEditorRejectThenEditorRejectRunsAndItsResultIsReturned()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var rejectCalls = 0;
            var review = OpenReview(node, PythonScriptReviewKind.EditorChangedUnderneath, "OUT = 'typing'", OriginalScript,
                onAccept: () => true,
                onReject: () => { rejectCalls++; return false; });

            var closed = review.RejectCode();

            // The editor could not save (for example it was closed), so the window stays open.
            Assert.IsFalse(closed);
            Assert.AreEqual(1, rejectCalls);
        }

        [Test]
        public void WhenNodeChangesAfterChangedOutsideEditorOpensThenEditorActionsAreNotRun()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var calls = 0;
            var review = OpenReview(node, PythonScriptReviewKind.EditorChangedUnderneath, "OUT = 'typing'", OriginalScript,
                onAccept: () => { calls++; return true; },
                onReject: () => { calls++; return true; });
            WriteScriptContent(node, LaterScript);

            Assert.IsFalse(review.ChangeCode());
            Assert.IsFalse(review.RejectCode());
            Assert.AreEqual(0, calls);
            Assert.AreEqual(LaterScript, node.Script);
        }

        #endregion

        #region Approval before writing (DYN-11043)

        [Test]
        public void WhenNoReviewWindowIsListeningThenApprovalIsGrantedAtOnce()
        {
            var node = new PythonNode();

            var answer = node.RequestScriptApproval(AssistantScript, Turn, CancellationToken.None);

            Assert.IsTrue(answer.IsCompleted);
            Assert.IsTrue(answer.Result);
        }

        [Test]
        public void WhenApprovalIsRequestedThenProposalShowsBothSidesAndNothingIsWritten()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            PythonScriptConflictEventArgs received = null;
            node.ScriptConflictReviewRequested += (_, e) => received = e;

            var answer = node.RequestScriptApproval(AssistantScript, Turn, CancellationToken.None);

            Assert.IsFalse(answer.IsCompleted);
            Assert.AreEqual(PythonScriptReviewKind.AssistantProposal, received.Kind);
            Assert.AreEqual(Turn, received.TurnId);
            Assert.AreEqual(OriginalScript, received.LeftCode);
            Assert.AreEqual(AssistantScript, received.RightCode);
            Assert.AreEqual(OriginalScript, node.Script);
        }

        [Test]
        public void WhenApprovalIsAlreadyCancelledThenItIsRefusedWithoutAWindow()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var raised = false;
            node.ScriptConflictReviewRequested += (_, _) => raised = true;

            var answer = node.RequestScriptApproval(AssistantScript, Turn, new CancellationToken(true));

            Assert.IsFalse(raised);
            Assert.IsFalse(answer.Result);
        }

        [Test]
        public void WhenProposalAcceptThenAnswerIsYesAndNothingIsWrittenByTheWindow()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var review = OpenProposal(node, AssistantScript, out var answer);

            Assert.IsTrue(review.ChangeCode());

            Assert.IsTrue(answer.Result);
            Assert.AreEqual(OriginalScript, node.Script);
        }

        [Test]
        public void WhenProposalRejectThenAnswerIsNo()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var review = OpenProposal(node, AssistantScript, out var answer);

            Assert.IsTrue(review.RejectCode());

            Assert.IsFalse(answer.Result);
            Assert.AreEqual(OriginalScript, node.Script);
        }

        [Test]
        public void WhenProposalWindowClosesWithoutAnAnswerThenAnswerIsNo()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var review = OpenProposal(node, AssistantScript, out var answer);

            review.OnWindowClosed();

            Assert.IsFalse(answer.Result);
        }

        [Test]
        public void WhenNodeChangesAfterProposalOpensThenAcceptStaysOpenAndRejectStillCloses()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var review = OpenProposal(node, AssistantScript, out var answer);
            WriteScriptContent(node, LaterScript);

            Assert.IsFalse(review.ChangeCode());
            Assert.IsFalse(answer.IsCompleted);

            Assert.IsTrue(review.RejectCode());
            Assert.IsFalse(answer.Result);
        }

        [Test]
        public void AcceptAllIsOfferedOnlyOnAProposalWithARequest()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            Action acceptAll = () => { };

            PythonScriptConflictViewModel Review(PythonScriptReviewKind kind, string turnId, Action onAcceptAll)
                => PythonScriptConflictViewModel.Create(
                    node,
                    kind == PythonScriptReviewKind.AssistantProposal
                        ? new PythonScriptConflictEventArgs(OriginalScript, AssistantScript, null, CancellationToken.None, turnId)
                        : new PythonScriptConflictEventArgs(kind, OriginalScript, AssistantScript),
                    ViewModel, onAcceptAll);

            Assert.IsTrue(Review(PythonScriptReviewKind.AssistantProposal, Turn, acceptAll).CanAcceptAll);
            Assert.IsFalse(Review(PythonScriptReviewKind.AssistantProposal, null, acceptAll).CanAcceptAll);
            Assert.IsFalse(Review(PythonScriptReviewKind.AssistantProposal, Turn, null).CanAcceptAll);
            Assert.IsFalse(Review(PythonScriptReviewKind.AssistantReview, Turn, acceptAll).CanAcceptAll);
            Assert.IsFalse(Review(PythonScriptReviewKind.AssistantConflict, Turn, acceptAll).CanAcceptAll);
            Assert.IsFalse(Review(PythonScriptReviewKind.EditorChangedUnderneath, Turn, acceptAll).CanAcceptAll);
        }

        [Test]
        public void WhenProposalAcceptAllThenAnswerIsYesAndTheNodeIsRemembered()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            var remembered = false;
            var review = OpenProposal(node, AssistantScript, out var answer, () => remembered = true);

            Assert.IsTrue(review.AcceptAllCode());

            Assert.IsTrue(answer.Result);
            Assert.IsTrue(remembered);
        }

        [Test]
        public void AcceptAllSkipsTheWindowOnlyForTheSameNodeAndRequest()
        {
            Assert.IsTrue(PythonMigrationViewExtension.ShouldAsk(true, null, Turn));
            Assert.IsFalse(PythonMigrationViewExtension.ShouldAsk(true, Turn, Turn));
            Assert.IsTrue(PythonMigrationViewExtension.ShouldAsk(true, Turn, "prompt:2"));
            Assert.IsTrue(PythonMigrationViewExtension.ShouldAsk(true, Turn, null));
        }

        [Test]
        public void WhenAskPreferenceIsOffThenNoAssistantChangeIsAsked()
        {
            Assert.IsTrue(new Dynamo.Configuration.PreferenceSettings().AskBeforeAIPythonChanges);
            Assert.IsFalse(PythonMigrationViewExtension.ShouldAsk(false, null, Turn));
        }

        #endregion

        #region Review window: display

        [Test]
        public void ReviewWindowTitleNamesTheNode()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            node.Name = "Wall Filter";

            var review = OpenReview(node, PythonScriptReviewKind.AssistantReview, OriginalScript, AssistantScript);

            StringAssert.Contains("Wall Filter", review.WindowTitle);
        }

        [Test]
        public void ReviewShowsTheDiffAndIsNotTrackedAsMigration()
        {
            var node = CreatePythonNodeWithScript(OriginalScript);
            WriteScriptContent(node, AssistantScript);

            var review = OpenReview(node, PythonScriptReviewKind.AssistantReview, OriginalScript, AssistantScript);

            Assert.IsFalse(review.TrackAsMigration);
            Assert.IsTrue(review.CurrentViewModel.HasChanges);
            Assert.AreEqual(State.HasChanges, review.CurrentViewModel.DiffState);
        }

        #endregion
    }
}
