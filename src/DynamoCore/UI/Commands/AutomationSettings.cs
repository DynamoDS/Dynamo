using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Xml;
using Dynamo.Utilities;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.ViewModels
{
    class AutomationSettings
    {
        #region Class Data Members

        internal enum Mode { None, Playback, Recording }

        /// <summary>
        /// This attribute specifies if Dynamo main window should be 
        /// closed after all the loaded commands have been played back. 
        /// This is set to "true" by default for recorded command files.
        /// </summary>
        /// 
        private const string ExitAttribName = "ExitAfterPlayback";

        /// <summary>
        /// This attribute specifies the amount of time in milliseconds that 
        /// Dynamo window should pause before closing. This value is ignored 
        /// if ExitAttribName is set to "false" (in which case Dynamo 
        /// window will not be closed after playback is completed).
        /// </summary>
        /// 
        private const string PauseAttribName = "PauseAfterPlaybackInMs";

        private DynamoViewModel owningViewModel = null;
        private DispatcherTimer playbackTimer = null;
        private List<DynCmd.RecordableCommand> loadedCommands = null;
        private List<DynCmd.RecordableCommand> recordedCommands = null;

        #endregion

        #region Class Properties

        internal int PauseAfterPlayback { get; private set; }
        internal bool ExitAfterPlayback { get; private set; }
        internal Mode CurrentMode { get; private set; }

        internal bool CanSaveRecordedCommands
        {
            get
            {
                if (null == recordedCommands)
                    return false;

                return (recordedCommands.Count > 0);
            }
        }

        #endregion

        #region Class Operational Methods

        internal AutomationSettings(DynamoViewModel vm, string commandFilePath)
        {
            this.PauseAfterPlayback = 10; // 10ms after playback is done.
            this.ExitAfterPlayback = true; // Exit Dynamo after playback.

            this.CurrentMode = Mode.None;
            if (LoadCommandFromFile(commandFilePath))
                this.CurrentMode = Mode.Playback;
            else
            {
                this.CurrentMode = Mode.Recording;
                recordedCommands = new List<DynCmd.RecordableCommand>();
            }

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

        internal void RecordCommand(DynCmd.RecordableCommand command)
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
        }

        internal string SaveRecordedCommands()
        {
            XmlDocument document = new XmlDocument();
            XmlElement commandRoot = document.CreateElement("Commands");
            document.AppendChild(commandRoot);

            // Create attributes that applied to the entire recording.
            XmlElementHelper helper = new XmlElementHelper(commandRoot);
            helper.SetAttribute(ExitAttribName, ExitAfterPlayback);
            helper.SetAttribute(PauseAttribName, PauseAfterPlayback);

            foreach (DynCmd.RecordableCommand command in recordedCommands)
                commandRoot.AppendChild(command.Serialize(document));

            string format = "Commands-{0:yyyyMMdd-hhmmss}.xml";
            string xmlFileName = string.Format(format, DateTime.Now);
            string xmlFilePath = Path.Combine(Path.GetTempPath(), xmlFileName);

            // Save recorded commands into XML file and open it in viewer.
            document.Save(xmlFilePath);
            return xmlFilePath;
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
                XmlElement commandRoot = document.FirstChild as XmlElement;
                if (null == commandRoot || (null == commandRoot.ChildNodes))
                    return false;

                // Read in optional attributes from the command root element.
                XmlElementHelper helper = new XmlElementHelper(commandRoot);
                this.ExitAfterPlayback = helper.ReadBoolean(ExitAttribName, true);
                this.PauseAfterPlayback = helper.ReadInteger(PauseAttribName, 10);

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
    }
}
