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

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        // Automation related data members.
        private string commandFilePath = null;
        private DispatcherTimer playbackTimer = null;
        private List<RecordableCommand> recordedCommands = null;

        #region Automation Related Methods

        /// <summary>
        /// DynamoController calls this method at the end of its initialization
        /// sequence so that loaded commands, if any, begin to playback.
        /// </summary>
        /// <returns>Returns true if the playback is successfully set up, or 
        /// false otherwise.</returns>
        internal bool BeginCommandPlayback()
        {
            if (string.IsNullOrEmpty(this.commandFilePath))
                return false; // Dynamo is not created to playback commands.

            List<RecordableCommand> loadedCommands = null;

            try
            {
                // Attempt to load the XML from the specified path.
                XmlDocument document = new XmlDocument();
                document.Load(this.commandFilePath);

                // Get to the root node of this Xml document.
                XmlNode commandRoot = document.FirstChild;
                if (null == commandRoot || (null == commandRoot.ChildNodes))
                    return false;

                loadedCommands = new List<RecordableCommand>();
                foreach (XmlNode node in commandRoot.ChildNodes)
                {
                    RecordableCommand command = null;
                    command = RecordableCommand.Deserialize(node as XmlElement);
                    if (null != command)
                        loadedCommands.Add(command);
                }
            }
            catch (Exception)
            {
                // Something is wrong with the Xml file, invalidate the 
                // data member that points to it, and return from here.
                this.commandFilePath = null;
                return false;
            }

            // Even though the Xml file can properly be loaded, it is still 
            // possible that the loaded content did not result in any useful 
            // commands. In this case simply return false, indicating failure.
            // 
            if (null == loadedCommands || (loadedCommands.Count <= 0))
                return false;

            // Ensure that this method is not called more than once.
            System.Diagnostics.Debug.Assert(null == playbackTimer);
            playbackTimer = new DispatcherTimer();

            // Serialized commands for playback.
            playbackTimer.Tag = loadedCommands;
            playbackTimer.Interval = TimeSpan.FromMilliseconds(500);
            playbackTimer.Tick += OnPlaybackTimerTick;
            playbackTimer.Start();
            return true;
        }

        private void OnPlaybackTimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop(); // Stop the timer before command completes.

            List<RecordableCommand> loadedCommands = null;
            loadedCommands = timer.Tag as List<RecordableCommand>;

            if (loadedCommands.Count <= 0)
            {
                this.playbackTimer = null;
                return;
            }

            // Remove the first command from the loaded commands.
            RecordableCommand nextCommand = loadedCommands[0];
            loadedCommands.RemoveAt(0);

            // Execute the command, this may take a while longer than the timer
            // inverval (usually very short), that's why the timer was stopped 
            // before the command execution starts. After the command is done,
            // the timer is then resumed for the next command in queue.
            // 
            nextCommand.Execute(this);
            timer.Start();
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
