using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.ViewModels
{
    class AutomationSettings
    {
        #region Class Data Members

        internal enum Mode { None, Playback, Recording }

        private DynamoViewModel owningViewModel = null;
        private DispatcherTimer playbackTimer = null;
        private List<DynCmd.RecordableCommand> loadedCommands = null;

        #endregion

        #region Class Properties

        internal Mode CurrentMode { get; private set; }

        #endregion

        #region Class Operational Methods

        internal AutomationSettings(DynamoViewModel vm, string commandFilePath)
        {
            this.CurrentMode = Mode.Recording;
            if (LoadCommandFromFile(commandFilePath))
                this.CurrentMode = Mode.Playback;

            this.owningViewModel = vm;
            if (null == this.owningViewModel)
                throw new ArgumentNullException("vm");
        }

        internal void BeginCommandPlayback()
        {
            if (this.CurrentMode != Mode.Playback)
                return; // Not currently in playback mode.

            if (null != this.playbackTimer)
            {
                throw new InvalidOperationException(
                    "Internal error: 'BeginCommandPlayback' called twice");
            }

            // Ensure that this method is not called more than once.
            System.Diagnostics.Debug.Assert(null == playbackTimer);
            playbackTimer = new DispatcherTimer();

            // Serialized commands for playback.
            playbackTimer.Interval = TimeSpan.FromMilliseconds(20);
            playbackTimer.Tick += OnPlaybackTimerTick;
            playbackTimer.Start();
        }

        private void OnPlaybackTimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop(); // Stop the timer before command completes.

            if (loadedCommands.Count <= 0)
            {
                this.playbackTimer = null;
                System.Windows.Application.Current.Shutdown();
                return;
            }

            // Remove the first command from the loaded commands.
            DynCmd.RecordableCommand nextCommand = loadedCommands[0];
            loadedCommands.RemoveAt(0);

            // Execute the command, this may take a while longer than the timer
            // inverval (usually very short), that's why the timer was stopped 
            // before the command execution starts. After the command is done,
            // the timer is then resumed for the next command in queue.
            // 
            nextCommand.Execute(this.owningViewModel);
            timer.Start();
        }

        #endregion

        #region Private Class Helper Methods

        private bool LoadCommandFromFile(string commandFilePath)
        {
            if (string.IsNullOrEmpty(commandFilePath))
                return false;
            if (File.Exists(commandFilePath) == false)
                return false;

            if (null != loadedCommands)
            {
                throw new InvalidOperationException(
                    "Internal error: 'LoadCommandFromFile' called twice");
            }

            try
            {
                // Attempt to load the XML from the specified path.
                XmlDocument document = new XmlDocument();
                document.Load(commandFilePath);

                // Get to the root node of this Xml document.
                XmlNode commandRoot = document.FirstChild;
                if (null == commandRoot || (null == commandRoot.ChildNodes))
                    return false;

                loadedCommands = new List<DynCmd.RecordableCommand>();
                foreach (XmlNode node in commandRoot.ChildNodes)
                {
                    DynCmd.RecordableCommand command = null;
                    command = DynCmd.RecordableCommand.Deserialize(node as XmlElement);
                    if (null != command)
                        loadedCommands.Add(command);
                }
            }
            catch (Exception)
            {
                // Something is wrong with the Xml file, invalidate the 
                // data member that points to it, and return from here.
                return false;
            }

            // Even though the Xml file can properly be loaded, it is still 
            // possible that the loaded content did not result in any useful 
            // commands. In this case simply return false, indicating failure.
            // 
            return (null != loadedCommands && (loadedCommands.Count > 0));
        }

        #endregion
    }

    partial class DynamoViewModel
    {
        // Automation related data members.
        private AutomationSettings automationSettings = null;
        private List<RecordableCommand> recordedCommands = null;

        #region Automation Related Methods

        /// <summary>
        /// DynamoController calls this method at the end of its initialization
        /// sequence so that loaded commands, if any, begin to playback.
        /// </summary>
        internal void BeginCommandPlayback()
        {
            if (null != automationSettings)
                automationSettings.BeginCommandPlayback();
        }

        private void SaveRecordedCommands(object parameters)
        {
            XmlDocument document = new XmlDocument();
            XmlElement commandRoot = document.CreateElement("Commands");
            document.AppendChild(commandRoot);

            foreach (RecordableCommand command in recordedCommands)
                commandRoot.AppendChild(command.Serialize(document));

            string format = "Commands-{0:yyyyMMdd-hhmmss}.xml";
            string xmlFileName = string.Format(format, DateTime.Now);
            string xmlFilePath = Path.Combine(Path.GetTempPath(), xmlFileName);

            // Save recorded commands into XML file and open it in viewer.
            document.Save(xmlFilePath);
            if (System.IO.File.Exists(xmlFilePath))
                System.Diagnostics.Process.Start(xmlFilePath);
        }

        private bool CanSaveRecordedCommands(object parameters)
        {
            return (null != recordedCommands && (recordedCommands.Count > 0));
        }

        #endregion

        #region Workspace Command Entry Point

        internal void ExecuteCommand(RecordableCommand command)
        {
            // In the playback mode 'this.recordedCommands' will be 
            // 'null' so that the incoming command will not be recorded.
            if (null != recordedCommands)
            {
                if (command.Redundant && (recordedCommands.Count > 0))
                {
                    // If a command is being marked "Redundant", then we will 
                    // only be interested in the most recent one. If we already
                    // have another instance recorded immediately prior to this,
                    // then replace the old instance with the new (for details,
                    // see "RecordableCommand.Redundant" property).
                    // 
                    var previousCommand = recordedCommands.Last();
                    if (previousCommand.GetType() != command.GetType())
                        recordedCommands.Add(command);
                    else
                    {
                        // Replace the existing command instead of adding.
                        recordedCommands[recordedCommands.Count - 1] = command;
                    }
                }
                else
                    recordedCommands.Add(command);
            }

            command.Execute(this);
        }

        #endregion

        #region The Actual Command Handlers (Private)

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
        }

        private void CreateNoteImpl(CreateNoteCommand command)
        {
            NoteModel noteModel = Model.AddNoteInternal(command, null);
            CurrentSpace.RecordCreatedModel(noteModel);
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

        #endregion
    }
}
