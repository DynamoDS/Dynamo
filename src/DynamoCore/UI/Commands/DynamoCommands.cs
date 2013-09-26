using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Xml;
using Dynamo.Core.Automation;
using Dynamo.Models;
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
            playbackTimer.Interval = TimeSpan.FromMilliseconds(200);
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

            document.Save(xmlFilePath);
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
                recordedCommands.Add(command);

            command.Execute(this);
        }

        #endregion

        #region The Actual Command Handlers

        internal void CreateNodeImpl(CreateNodeCommand command)
        {
            this.Model.CreateNodeInternal(command, null);
        }

        #endregion

        #region Private Class Helper Methods

        private NodeModel CreateNodeInstance(string nodeName)
        {
            return null;
        }

        #endregion
    }
}
