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
            try
            {
                CommandCompletedImpl(command as dynamic);
            }
            // No method was found for this command that means
            // there is no need to do anything after execution
            catch (RuntimeBinderException) { }
        }

        void model_CommandStarting(DM.RecordableCommand command)
        {
            try
            {
                CommandStartingImpl(command as dynamic);
            }
            // No method was found for this command that means
            // there is no need to do anything before execution
            catch (RuntimeBinderException) { }
        }

        private void CommandStartingImpl(DM.OpenFileCommand command)
        {
            this.VisualizationManager.Pause();
        }

        private void CommandCompletedImpl(DM.OpenFileCommand command)
        {
            this.AddToRecentFiles(command.XmlFilePath);
            this.VisualizationManager.UnPause();
        }

        private void CommandCompletedImpl(DM.MutateTestCommand command)
        {
            var mutatorDriver = new Dynamo.TestInfrastructure.MutatorDriver(this);
            mutatorDriver.RunMutationTests();
        }

        private void CommandCompletedImpl(DM.CreateNodeCommand command)
        {
            UndoRedoRaise();
        }

        private void CommandCompletedImpl(DM.CreateNoteCommand command)
        {
            UndoRedoRaise();
        }

        private void CommandCompletedImpl(DM.SelectInRegionCommand command)
        {
            CurrentSpaceViewModel.SelectInRegion(command.Region, command.IsCrossSelection);
        }

        private void CommandCompletedImpl(DM.DragSelectionCommand command)
        {
            if (DM.DragSelectionCommand.Operation.BeginDrag == command.DragOperation)
            {
                CurrentSpaceViewModel.BeginDragSelection(command.MouseCursor);
            }
            else
            {
                CurrentSpaceViewModel.EndDragSelection(command.MouseCursor);
            }
        }

        private void CommandStartingImpl(DM.MakeConnectionCommand command)
        {
            System.Guid nodeId = command.NodeId;

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

        private void CommandCompletedImpl(DM.DeleteModelCommand command)
        {
            UndoRedoRaise();
        }

        private void UndoRedoRaise()
        {
            UndoCommand.RaiseCanExecuteChanged();
            RedoCommand.RaiseCanExecuteChanged();
        }

        private void CommandCompletedImpl(DM.UndoRedoCommand command)
        {
            UndoRedoRaise();
        }

        private void CommandCompletedImpl(DM.ModelEventCommand command)
        {
            UndoRedoRaise();
        }

        private void CommandCompletedImpl(DM.UpdateModelValueCommand command)
        {
            UndoRedoRaise();
        }

        private void CommandCompletedImpl(DM.ConvertNodesToCodeCommand command)
        {
            UndoRedoRaise();
        }

        private void CommandCompletedImpl(DM.SwitchTabCommand command)
        {
            if (command.IsInPlaybackMode)
                RaisePropertyChanged("CurrentWorkspaceIndex");
        }

        #endregion

    }
}
