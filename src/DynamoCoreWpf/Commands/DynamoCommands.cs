﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
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
                    this.AddToRecentFiles((command as DynamoModel.OpenFileCommand).XmlFilePath);
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
                        CurrentSpaceViewModel.BeginDragSelection(dragC.MouseCursor);
                    else
                        CurrentSpaceViewModel.EndDragSelection(dragC.MouseCursor);
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
                    // for this commands there is no need
                    // to do anything after execution
                    break;

                default:
                    throw new InvalidOperationException("Unhandled command name");
            }

            if (Dynamo.Logging.Analytics.ReportingAnalytics)
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

                case "OpenFileCommand":
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
                case "UndoRedoCommand":
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

                case DynamoModel.MakeConnectionCommand.Mode.Cancel:
                    CurrentSpaceViewModel.CancelConnection();
                    break;
            }
        }

        #endregion

    }
}
