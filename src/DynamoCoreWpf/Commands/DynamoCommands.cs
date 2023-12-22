using System;
using System.Diagnostics;
using Dynamo.Models;

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
                        System.Diagnostics.Process.Start(new ProcessStartInfo(xmlFilePath) { UseShellExecute = true });
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

        /// <summary>
        /// Saves all recorded commands on disk (%TMP%/Commands-{0:yyyyMMdd-hhmmss}.xml)
        /// </summary>
        /// <returns>The path to the commands file</returns>
        internal string DumpRecordedCommands()
        {
            return automationSettings.SaveRecordedCommands();
        }

        #endregion

        #region Workspace Command Entry Point

        public void ExecuteCommand(DynamoModel.RecordableCommand command)
        {
            if (null != this.automationSettings)
                this.automationSettings.RecordCommand(command);

            if (Model.DebugSettings.VerboseLogging)
                model.Logger.Log("Command: " + command);

            OnRequestReturnFocusToView(); 
            this.model.ExecuteCommand(command);
        }

        #endregion

        #region The Actual Command Handlers (Private)

        void OnModelCommandCompleted(DynamoModel.RecordableCommand command)
        {
            var name = command.GetType().Name;
            switch (name)
            {
                case "OpenFileCommand":
                    this.AddToRecentFiles((command as DynamoModel.OpenFileCommand).FilePath);
                    break;

                case "MutateTestCommand":
                    var mutatorDriver = new Dynamo.TestInfrastructure.MutatorDriver(this);
                    mutatorDriver.RunMutationTests();
                    break;

                case "SelectInRegionCommand":
                    var selectC = command as DynamoModel.SelectInRegionCommand;
                    CurrentSpaceViewModel.SelectInRegion(selectC.Region, selectC.IsCrossSelection);
                    break;

                case "DragSelectionCommand":
                    var dragC = command as DynamoModel.DragSelectionCommand;

                    if (DynamoModel.DragSelectionCommand.Operation.BeginDrag == dragC.DragOperation)
                    {
                        try
                        {
                            CurrentSpaceViewModel.BeginDragSelection(dragC.MouseCursor);
                        }
                        catch (Exception ex)
                        {
                            model.Logger.Log(ex.Message);
                        }
                    }
                    else
                    {
                        CurrentSpaceViewModel.EndDragSelection(dragC.MouseCursor);
                    }
                    break;

                case "DeleteModelCommand":
                    CurrentSpaceViewModel.CancelActiveState();
                    RaiseCanExecuteUndoRedo();
                    break;
                case "CreateNodeCommand":
                case "CreateProxyNodeCommand":
                case "CreateNoteCommand":
                case "CreateAnnotationCommand":
                case "UndoRedoCommand":
                case "ModelEventCommand":
                case "UpdateModelValueCommand":
                case "ConvertNodesToCodeCommand":
                case "UngroupModelCommand":
                case "AddModelToGroupCommand":
                case "CreateAndConnectNodeCommand":
                case "AddGroupToGroupCommand":
                case "InsertFileCommand":
                    RaiseCanExecuteUndoRedo();
                    break;

                case "SwitchTabCommand":
                    if (command.IsInPlaybackMode)
                        RaisePropertyChanged("CurrentWorkspaceIndex");
                    break;

                case "RunCancelCommand":
                case "ForceRunCancelCommand":
                case "SelectModelCommand":
                case "MakeConnectionCommand":
                case "CreateCustomNodeCommand":
                case "AddPresetCommand":
                case "ApplyPresetCommand":
                case "OpenFileFromJsonCommand":
                    // for this commands there is no need
                    // to do anything after execution
                    break;

                default:
                    throw new InvalidOperationException("Unhandled command name");
            }

            if (Logging.Analytics.ReportingAnalytics && !command.IsInPlaybackMode)
            {
                command.TrackAnalytics();
            }
        }

        void OnModelCommandStarting(DynamoModel.RecordableCommand command)
        {
            var name = command.GetType().Name;
            switch (name)
            {
                case "MakeConnectionCommand":
                    MakeConnectionImpl(command as DynamoModel.MakeConnectionCommand);
                    break;

                case "UndoRedoCommand":
                    UndoRedoImpl(command as DynamoModel.UndoRedoCommand);
                    break;

                case "OpenFileCommand":
                case "OpenFileFromJsonCommand":
                case "InsertFileCommand":
                case "RunCancelCommand":
                case "ForceRunCancelCommand":
                case "CreateNodeCommand":
                case "CreateProxyNodeCommand":
                case "CreateNoteCommand":
                case "CreateAnnotationCommand":
                case "SelectModelCommand":
                case "SelectInRegionCommand":
                case "DragSelectionCommand":
                case "DeleteModelCommand":
                case "ModelEventCommand":
                case "UpdateModelValueCommand":
                case "ConvertNodesToCodeCommand":
                case "CreateCustomNodeCommand":
                case "SwitchTabCommand":
                case "MutateTestCommand":
                case "UngroupModelCommand":
                case "AddModelToGroupCommand":
                case "AddPresetCommand":
                case "ApplyPresetCommand":
                case "CreateAndConnectNodeCommand":
                case "AddGroupToGroupCommand":
                    // for this commands there is no need
                    // to do anything before execution
                    break;

                default:
                    throw new InvalidOperationException("Unhandled command name");
            }
        }

        private void MakeConnectionImpl(DynamoModel.MakeConnectionCommand command)
        {
            Guid nodeId = command.ModelGuid;

            switch (command.ConnectionMode)
            {
                case DynamoModel.MakeConnectionCommand.Mode.Begin:
                    CurrentSpaceViewModel.BeginConnection(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case DynamoModel.MakeConnectionCommand.Mode.End:
                    CurrentSpaceViewModel.EndConnection(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case DynamoModel.MakeConnectionCommand.Mode.BeginShiftReconnections:
                    CurrentSpaceViewModel.BeginShiftReconnections(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case DynamoModel.MakeConnectionCommand.Mode.EndShiftReconnections:
                    CurrentSpaceViewModel.EndConnection(
                        nodeId, command.PortIndex, command.Type);
                    break;

                // TODO - can be removed in a future version of Dynamo - DYN-1729
                case DynamoModel.MakeConnectionCommand.Mode.EndAndStartCtrlConnection:
                    CurrentSpaceViewModel.BeginCreateConnections(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case DynamoModel.MakeConnectionCommand.Mode.BeginDuplicateConnection:
                    CurrentSpaceViewModel.BeginConnection(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case DynamoModel.MakeConnectionCommand.Mode.BeginCreateConnections:
                    CurrentSpaceViewModel.BeginCreateConnections(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case DynamoModel.MakeConnectionCommand.Mode.Cancel:
                    CurrentSpaceViewModel.CancelConnection();
                    break;
            }
        }

        private void UndoRedoImpl(DynamoModel.UndoRedoCommand command)
        {
            if (command.CmdOperation == DynamoModel.UndoRedoCommand.Operation.Undo)
            {
                CurrentSpaceViewModel.CancelActiveState();
            }
        }

        #endregion

    }
}
