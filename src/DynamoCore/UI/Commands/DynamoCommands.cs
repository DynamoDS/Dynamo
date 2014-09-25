using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dynamo.Models;
using DM = Dynamo.Models.DynamoModel;
using Dynamo.Selection;
using Microsoft.CSharp.RuntimeBinder;

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

        public void ExecuteCommand(DM.RecordableCommand command)
        {
            if (null != this.automationSettings)
                this.automationSettings.RecordCommand(command);

            if (Model.DebugSettings.VerboseLogging)
                model.Logger.Log("Command: " + command);

            this.model.ExecuteCommand(command);
        }

        #endregion

        #region The Actual Command Handlers (Private)

        void model_CommandCompleted(DM.RecordableCommand command)
        {
            var name = command.GetType().Name;
            switch (name)
            {
                case "OpenFileCommand":
                    this.AddToRecentFiles((command as DM.OpenFileCommand).XmlFilePath);
                    this.VisualizationManager.UnPause();
                    break;

                case "MutateTestCommand":
                    var mutatorDriver = new Dynamo.TestInfrastructure.MutatorDriver(this);
                    mutatorDriver.RunMutationTests();
                    break;

                case "SelectInRegionCommand":
                    var selectC = command as DM.SelectInRegionCommand;
                    CurrentSpaceViewModel.SelectInRegion(selectC.Region, selectC.IsCrossSelection);
                    break;

                case "DragSelectionCommand":
                    var dragC = command as DM.DragSelectionCommand;

                    if (DM.DragSelectionCommand.Operation.BeginDrag == dragC.DragOperation)
                        CurrentSpaceViewModel.BeginDragSelection(dragC.MouseCursor);
                    else
                        CurrentSpaceViewModel.EndDragSelection(dragC.MouseCursor);
                    break;

                case "DeleteModelCommand":
                case "CreateNodeCommand":
                case "CreateNoteCommand":
                case "UndoRedoCommand":
                case "ModelEventCommand":
                case "UpdateModelValueCommand":
                case "ConvertNodesToCodeCommand":
                    UndoCommand.RaiseCanExecuteChanged();
                    RedoCommand.RaiseCanExecuteChanged();
                    break;

                case "SwitchTabCommand":
                    if (command.IsInPlaybackMode)
                        RaisePropertyChanged("CurrentWorkspaceIndex");
                    break;

                default:
                    // for the other commands
                    // there is no need to do anything after execution
                    break;
            }
        }

        void model_CommandStarting(DM.RecordableCommand command)
        {
            var name = command.GetType().Name;
            switch (name)
            {
                case "OpenFileCommand":
                    this.VisualizationManager.Pause();
                    break;

                case "MakeConnectionCommand":
                    MakeConnectionImpl(command as DM.MakeConnectionCommand);
                    break;

                default:
                    // for the other commands
                    // there is no need to do anything before execution
                    break;
            }
        }

        private void MakeConnectionImpl(DM.MakeConnectionCommand command)
        {
            Guid nodeId = command.NodeId;

            switch (command.ConnectionMode)
            {
                case DM.MakeConnectionCommand.Mode.Begin:
                    CurrentSpaceViewModel.BeginConnection(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case DM.MakeConnectionCommand.Mode.End:
                    CurrentSpaceViewModel.EndConnection(
                        nodeId, command.PortIndex, command.Type);
                    break;

                case DM.MakeConnectionCommand.Mode.Cancel:
                    CurrentSpaceViewModel.CancelConnection();
                    break;
            }
        }

        #endregion

    }
}
