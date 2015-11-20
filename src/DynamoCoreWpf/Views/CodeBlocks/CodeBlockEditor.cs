﻿using Dynamo.Controls;
﻿using Dynamo.Core;
using System;
﻿using Dynamo.Graph.Nodes;
﻿using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Code block editor.
    /// </summary>
    public partial class CodeBlockEditor : CodeCompletionEditor 
    {
        private bool createdForNewCodeBlock;
        private readonly CodeBlockNodeModel codeBlockNode;

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
        }

        protected override void OnEscape()
        {
            var text = InnerTextEditor.Text;
            if (codeBlockNode.Code != null && text.Equals(codeBlockNode.Code))
                ReturnFocus();
            
            if (string.IsNullOrEmpty(text))
            {
                nodeViewModel.DynamoViewModel.ExecuteCommand(
                   new DynCmd.DeleteModelCommand(codeBlockNode.GUID));
            }
        }

        protected override void OnCommitChange()
        {
            var recorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;

            if (string.IsNullOrEmpty(InnerTextEditor.Text))
                DiscardChangesAndOptionallyRemoveNode(recorder);
            else
                CommitChanges(recorder);

            createdForNewCodeBlock = false; // First commit is now over.
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

        private void DiscardChangesAndOptionallyRemoveNode(UndoRedoRecorder recorder)
        {
            if (!string.IsNullOrEmpty(InnerTextEditor.Text))
            {
                throw new InvalidOperationException(
                    "This method is meant only for empty text box");
            }

            if (createdForNewCodeBlock)
            {
                // If this editing was started due to a new code block node, 
                // then by this point the creation of the node would have been 
                // recorded, we need to pop that off the undo stack. Note that 
                // due to various external factors a code block node loaded 
                // from file may be created empty. In such cases, the creation 
                // step would not have been recorded (there was no explicit 
                // creation of the node, it was created from loading of a file),
                // and nothing should be popped off of the undo stack.
                // 
                if (recorder.CanUndo)
                    recorder.PopFromUndoGroup(); // Pop off creation action.               
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
