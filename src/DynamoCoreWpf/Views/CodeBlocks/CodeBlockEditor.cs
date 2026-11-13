using System;
using System.Windows.Input;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph.Nodes;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Code block node editor state machine.
    /// </summary>
    internal class EditorStateMachine
    {
        internal enum State
        {
            Editing,
            Commited
        }

        private State state;

        /// <summary>
        /// Event handler for editor state changed.
        /// </summary>
        internal event Action<State> OnStateChanged;

        /// <summary>
        /// Constructor state machine. 
        /// </summary>
        internal EditorStateMachine(State startState)
        {
            state = startState;
        }

        /// <summary>
        /// Transit to the new state.
        /// </summary>
        /// <param name="newState"></param>
        internal void Transit(State newState)
        {
            if (state != newState)
            {
                state = newState;
                if (OnStateChanged != null)
                {
                    OnStateChanged(state);
                }
            }
        }
    }

    /// <summary>
    /// Code block editor.
    /// </summary>
    public partial class CodeBlockEditor : CodeCompletionEditor 
    {
        private bool createdForNewCodeBlock;
        private readonly CodeBlockNodeModel codeBlockNode;
        private EditorStateMachine stateMachine = new EditorStateMachine(EditorStateMachine.State.Commited);

        /// <summary>
        /// Create code block editor by the view of code block node.
        /// </summary>
        /// <param name="nodeView"></param>
        public CodeBlockEditor(NodeView nodeView): base(nodeView)
        {
            this.codeBlockNode = nodeViewModel.NodeModel as CodeBlockNodeModel;
            if (codeBlockNode == null)
            {
                throw new InvalidOperationException(
                    "Should not be used for nodes other than code block");
            }

            // Determines if this editor is created for a new code block node.
            // In cases like an undo/redo operation, the editor is created for 
            // an existing code block node.
            createdForNewCodeBlock = string.IsNullOrEmpty(codeBlockNode.Code);

            // the code block should not be in focus upon undo/redo actions on node
            if (codeBlockNode.ShouldFocus)
            {
                Loaded += (obj, args) => SetFocus(); 
            }

            WatermarkLabel.Text = Properties.Resources.WatermarkLabelText;
            stateMachine.OnStateChanged += OnEditorStateChanged;
        }

        protected override void OnEscape()
        {
            var text = InnerTextEditor.Text;
            if (codeBlockNode.Code != null && text.Equals(codeBlockNode.Code))
                ReturnFocus();
        }

        protected override void OnCommitChange()
        {
            var recorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;

            if (string.IsNullOrEmpty(InnerTextEditor.Text))
                DiscardChangesAndOptionallyRemoveNode();
            else
                CommitChanges(recorder);

            createdForNewCodeBlock = false; // First commit is now over.

            stateMachine.Transit(EditorStateMachine.State.Commited);
        }

        protected override void OnTextAreaGotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            stateMachine.Transit(EditorStateMachine.State.Editing);
        }

        private void OnEditorStateChanged(EditorStateMachine.State state)
        {
            switch (state)
            {
                case EditorStateMachine.State.Editing:
                    EnableInputOutputPorts(false); 
                    break;

                case EditorStateMachine.State.Commited:
                    EnableInputOutputPorts(true); 
                    break;
            }
        }

        private void EnableInputOutputPorts(bool isEnabled)
        {
            for (int i = 0; i < codeBlockNode.InPorts.Count; i++)
            {
                codeBlockNode.InPorts[i].IsEnabled = isEnabled;
            }

            for (int i = 0; i < codeBlockNode.OutPorts.Count; i++)
            {
                codeBlockNode.OutPorts[i].IsEnabled = isEnabled;
            }
        }

        private void CommitChanges(UndoRedoRecorder recorder)
        {
            // Code block editor can lose focus in many scenarios (e.g. switching 
            // of tabs or application), if there has not been any changes, do not
            // commit the change.
            // 
            if (!codeBlockNode.Code.Equals(InnerTextEditor.Text))
            {
                UpdateNodeValue("Code");
            }

            if (createdForNewCodeBlock)
            {
                // If this editing was started due to a new code block node, 
                // then by this point there would have been two action groups 
                // recorded on the undo-stack: one for node creation, and 
                // another for node editing (as part of ExecuteCommand above).
                // Pop off the two action groups...
                // 
                recorder.PopFromUndoGroup(); // Pop off modification action.

                // Note that due to various external factors a code block node 
                // loaded from file may be created empty. In such cases, the 
                // creation step would not have been recorded (there was no 
                // explicit creation of the node, it was created from loading 
                // of a file), and nothing should be popped off of the undo stack.
                if (recorder.CanUndo)
                    recorder.PopFromUndoGroup(); // Pop off creation action.

                // ... and record this new node as new creation.
                using (recorder.BeginActionGroup())
                {
                    recorder.RecordCreationForUndo(codeBlockNode);
                }
            }
        }

        /// <summary>
        /// Intercepts Ctrl-Z when the editor is empty, has no AvalonEdit undo
        /// history, and was created for a new code block node. AvalonEdit's
        /// TextArea registers its own Undo command binding and marks the event
        /// Handled even when its undo stack is empty, so without this intercept
        /// the keystroke never reaches DynamoView's workspace undo key binding.
        /// The AvalonEdit CanUndo check ensures that if the user typed and then
        /// deleted back to empty, Ctrl-Z restores the text rather than removing
        /// the node from the canvas.
        /// Also marks graph navigation shortcuts as handled so window-level
        /// KeyBindings (zoom, pan, fit view, auto layout, 3D nav) do not run
        /// while typing in the code block. Escape-hold geometry preview is blocked
        /// in DynamoView_KeyDown when a code editor has focus.
        /// </summary>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Z
                && e.KeyboardDevice.Modifiers == ModifierKeys.Control
                && createdForNewCodeBlock
                && string.IsNullOrEmpty(InnerTextEditor.Text)
                && !InnerTextEditor.Document.UndoStack.CanUndo)
            {
                nodeViewModel.DynamoViewModel.UndoCommand.Execute(null);
                e.Handled = true;
                return;
            }

            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (IsBlockedGraphShortcut(key, e.KeyboardDevice.Modifiers))
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// Graph shortcuts that should not fire while the code block editor has focus.
        /// Kept local to the editor (no shared view-model flag).
        /// </summary>
        internal static bool IsBlockedGraphShortcut(Key key, ModifierKeys modifiers)
        {
            if (modifiers == ModifierKeys.Control)
            {
                switch (key)
                {
                    case Key.L:          // Auto layout
                    case Key.P:          // Toggle 3D pan
                    case Key.B:          // Toggle 3D background navigation
                    case Key.I:          // Isolation mode
                    case Key.O:          // Open graph
                    case Key.T:          // Open Template
                    case Key.N:          // New graph
                    case Key.W:          // Add note
                    case Key.K:          // Unpin all preview bubbles
                    case Key.D0:         // Fit view
                    case Key.OemPlus:    // Zoom in
                    case Key.OemMinus:   // Zoom out
                    case Key.Add:        // Zoom in (numpad)
                    case Key.Subtract:   // Zoom out (numpad)
                        return true;
                    default:
                        return false;
                }
            }

            // Canvas pan
            if (modifiers == (ModifierKeys.Control | ModifierKeys.Alt))
            {
                return key == Key.Left || key == Key.Right || key == Key.Up || key == Key.Down;
            }

            if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                switch (key)
                {
                    case Key.S:  // Save As
                    case Key.I:  // Insert graph
                    case Key.N:  // New custom node
                    case Key.Up: // Toggle console
                        return true;
                    default:
                        return false;
                }
            }

            // F5 runs the graph; Shift+F5 cancels — block both while editing
            if (key == Key.F5
                && (modifiers == ModifierKeys.None || modifiers == ModifierKeys.Shift))
            {
                return true;
            }

            return false;
        }

        private void DiscardChangesAndOptionallyRemoveNode()
        {
            if (!string.IsNullOrEmpty(InnerTextEditor.Text))
            {
                throw new InvalidOperationException(
                    "This method is meant only for empty text box");
            }

            if (createdForNewCodeBlock)
            {
                // Leave the creation action group on the undo stack so that
                // Ctrl-Z can remove the node. Previously this popped the
                // creation entry and left the node orphaned on the canvas with
                // no undo record.
            }
            else
            {
                // If the editing was started for an existing code block node,
                // and user deletes the text contents, it should be restored to
                // the original codes.
                InnerTextEditor.Text = codeBlockNode.Code;
            }
        }
    }
}
