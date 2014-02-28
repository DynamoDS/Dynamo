using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        // Automation related data members.
        private AutomationSettings automationSettings = null;

        #region Automation Related Methods

        /// <summary>
        /// DynamoController calls this method at the end of its initialization
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

        #endregion

        #region Workspace Command Entry Point

        public void ExecuteCommand(RecordableCommand command)
        {
            if (null != this.automationSettings)
                this.automationSettings.RecordCommand(command);

            command.Execute(this);
        }

        #endregion

        #region The Actual Command Handlers (Private)

        private void OpenFileImpl(OpenFileCommand command)
        {
            string xmlFilePath = command.XmlFilePath;
            dynSettings.Controller.DynamoModel.OpenInternal(xmlFilePath);
        }

        private void RunCancelImpl(RunCancelCommand command)
        {
            dynSettings.Controller.RunCancelInternal(
                command.ShowErrors, command.CancelRun);
        }

        private void CreateNodeImpl(CreateNodeCommand command)
        {
            NodeModel nodeModel = Model.CreateNode(
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
            NoteModel noteModel = Model.AddNoteInternal(command, null);
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

            _model.DeleteModelInternal(modelsToDelete);

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

        #endregion
    }
}
