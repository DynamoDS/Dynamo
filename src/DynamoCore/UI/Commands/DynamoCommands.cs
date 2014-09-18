using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Selection;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        // Automation related data members.
        private AutomationSettings automationSettings = null;

        #region Automation Related Methods

        /// <summary>
        /// DynamoView calls this method at the end of its initialization
        /// sequence so that loaded commands, if any, begin to playback.
        /// </summary>
        internal void BeginCommandPlayback(System.Windows.Window mainWindow)
        {
            if (null != automationSettings)
                automationSettings.BeginCommandPlayback(mainWindow);
        }

        private void SaveRecordedCommands(object parameters)
        {
            if (null != automationSettings)
            {
                string xmlFilePath = automationSettings.SaveRecordedCommands();
                if (string.IsNullOrEmpty(xmlFilePath) == false)
                {
                    if (System.IO.File.Exists(xmlFilePath))
                        System.Diagnostics.Process.Start(xmlFilePath);
                }
            }
        }

        private bool CanSaveRecordedCommands(object parameters)
        {
            if (null == automationSettings)
                return false;

            return automationSettings.CanSaveRecordedCommands;
        }

        private void ExecInsertPausePlaybackCommand(object parameters)
        {
            if (automationSettings != null)
            {
                var msg = string.Format("PausePlaybackCommand '{0}' inserted",
                    automationSettings.InsertPausePlaybackCommand());
                model.Logger.Log(msg);
            }
        }

        private bool CanInsertPausePlaybackCommand(object parameters)
        {
            if (null == automationSettings)
                return false;

            return (automationSettings.CurrentState == AutomationSettings.State.Recording);
        }

        #endregion

        #region Workspace Command Entry Point

        public void ExecuteCommand(RecordableCommand command)
        {
            if (null != this.automationSettings)
                this.automationSettings.RecordCommand(command);

            if (Model.DebugSettings.VerboseLogging)
                model.Logger.Log("Command: " + command);

            command.Execute(this);
        }

        #endregion

        #region The Actual Command Handlers (Private)

        private void OpenFileImpl(OpenFileCommand command)
        {
            this.VisualizationManager.Pause();

            string xmlFilePath = command.XmlFilePath;
            model.OpenInternal(xmlFilePath);

            this.AddToRecentFiles(xmlFilePath);

            //clear the clipboard to avoid copying between dyns
            model.ClipBoard.Clear();
            this.VisualizationManager.UnPause();
        }

        private void RunCancelImpl(RunCancelCommand command)
        {
            model.RunCancelInternal(
                command.ShowErrors, command.CancelRun);
        }

        private void ForceRunCancelImpl(RunCancelCommand command)
        {
            model.ForceRunCancelInternal(
                command.ShowErrors, command.CancelRun);
        }

        private void MutateTestImpl()
        {
            var mutatorDriver = new Dynamo.TestInfrastructure.MutatorDriver(this);
            mutatorDriver.RunMutationTests();
        }

        private void CreateNodeImpl(CreateNodeCommand command)
        {
            NodeModel nodeModel = CurrentSpace.AddNode(
                command.NodeId,
                command.NodeName,
                command.X,
                command.Y,
                command.DefaultPosition,
                command.TransformCoordinates);

            CurrentSpace.RecordCreatedModel(nodeModel);

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private void CreateNoteImpl(CreateNoteCommand command)
        {
            NoteModel noteModel = Model.CurrentWorkspace.AddNote(
                command.DefaultPosition,
                command.X,
                command.Y,
                command.NoteText,
                command.NodeId);
            CurrentSpace.RecordCreatedModel(noteModel);

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private void SelectModelImpl(SelectModelCommand command)
        {
            // Empty ModelGuid means clear selection.
            if (command.ModelGuid == Guid.Empty)
            {
                DynamoSelection.Instance.ClearSelection();
                return;
            }

            ModelBase model = CurrentSpace.GetModelInternal(command.ModelGuid);

            if (false == model.IsSelected)
            {
                if (!command.Modifiers.HasFlag(ModifierKeys.Shift))
                    DynamoSelection.Instance.ClearSelection();

                if (!DynamoSelection.Instance.Selection.Contains(model))
                    DynamoSelection.Instance.Selection.Add(model);
            }
            else
            {
                if (command.Modifiers.HasFlag(ModifierKeys.Shift))
                    DynamoSelection.Instance.Selection.Remove(model);
            }
        }

        private void SelectInRegionImpl(SelectInRegionCommand command)
        {
            CurrentSpaceViewModel.SelectInRegion(command.Region, command.IsCrossSelection);
        }

        private void DragSelectionImpl(DragSelectionCommand command)
        {
            if (DragSelectionCommand.Operation.BeginDrag == command.DragOperation)
                CurrentSpaceViewModel.BeginDragSelection(command.MouseCursor);
            else
                CurrentSpaceViewModel.EndDragSelection(command.MouseCursor);
        }

        private void MakeConnectionImpl(MakeConnectionCommand command)
        {
            System.Guid nodeId = command.NodeId;

            switch (command.ConnectionMode)
            {
                case MakeConnectionCommand.Mode.Begin:
                    CurrentSpaceViewModel.BeginConnection(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case MakeConnectionCommand.Mode.End:
                    CurrentSpaceViewModel.EndConnection(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case MakeConnectionCommand.Mode.Cancel:
                    CurrentSpaceViewModel.CancelConnection();
                    break;
            }
        }

        private void DeleteModelImpl(DeleteModelCommand command)
        {
            List<ModelBase> modelsToDelete = new List<ModelBase>();
            if (command.ModelGuid != Guid.Empty)
            {
                modelsToDelete.Add(CurrentSpace.GetModelInternal(command.ModelGuid));
            }
            else
            {
                // When nothing is specified then it means all selected models.
                foreach (ISelectable selectable in DynamoSelection.Instance.Selection)
                {
                    if (selectable is ModelBase)
                        modelsToDelete.Add(selectable as ModelBase);
                }
            }

            model.DeleteModelInternal(modelsToDelete);

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private void UndoRedoImpl(UndoRedoCommand command)
        {
            if (command.CmdOperation == UndoRedoCommand.Operation.Undo)
                CurrentSpace.Undo();
            else if (command.CmdOperation == UndoRedoCommand.Operation.Redo)
                CurrentSpace.Redo();

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private void SendModelEventImpl(ModelEventCommand command)
        {
            CurrentSpace.SendModelEvent(command.ModelGuid, command.EventName);

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private void UpdateModelValueImpl(UpdateModelValueCommand command)
        {
            CurrentSpace.UpdateModelValue(command.ModelGuid,
                command.Name, command.Value);

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private void ConvertNodesToCodeImpl(ConvertNodesToCodeCommand command)
        {
            CurrentSpace.ConvertNodesToCodeInternal(command.NodeId);

            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
            CurrentSpace.HasUnsavedChanges = true;
        }

        private void CreateCustomNodeImpl(CreateCustomNodeCommand command)
        {
            this.model.NewCustomNodeWorkspace(command.NodeId,
                command.Name, command.Category, command.Description, true);
        }

        private void SwitchTabImpl(SwitchTabCommand command)
        {
            // We don't attempt to null-check here, we need it to fail fast.
            model.CurrentWorkspace = model.Workspaces[command.TabIndex];

            if (command.IsInPlaybackMode)
                RaisePropertyChanged("CurrentWorkspaceIndex");
        }

        #endregion

    }
}
