using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.ViewModels
{
    internal class PlaybackStateChangedEventArgs : EventArgs
    {
        internal PlaybackStateChangedEventArgs(string oldCommandTag, string newCommandTag,
            AutomationSettings.State oldState, AutomationSettings.State newState)
        {
            this.OldState = oldState;
            this.NewState = newState;
            this.OldTag = oldCommandTag;
            this.NewTag = newCommandTag;
        }

        internal string OldTag { get; private set; }
        internal string NewTag { get; private set; }
        internal AutomationSettings.State OldState { get; private set; }
        internal AutomationSettings.State NewState { get; private set; }
    }

    internal delegate void PlaybackStateChangedEventHandler(
        object sender, PlaybackStateChangedEventArgs e);

    internal class AutomationSettings
    {
        #region Class Data Members

        internal enum State
        {
            None, // The automation is in an uninitialized state
            Recording, // The automation commands are being recorded
            Loaded, // Command file is loaded and ready for playback
            Playing, // The playback timer is active
            Paused, // The playback is currently being paused
            ShuttingDown, // The shutdown timer is in progress
            Stopped // The playback has stopped without getting shutdown
        };

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

        /// <summary>
        /// This attribute specifies the interval between two consecutive 
        /// commands in milliseconds. When a command is completed, the next 
        /// command will be executed after this interval elapsed. The default
        /// value for command interval is 20 milliseconds.
        /// </summary>
        private const string IntervalAttribName = "CommandIntervalInMs";

        private System.Windows.Window mainWindow = null;
        private DynamoModel owningDynamoModel = null;
        private DispatcherTimer playbackTimer = null;
        private List<DynCmd.RecordableCommand> loadedCommands = null;
        private List<DynCmd.RecordableCommand> recordedCommands = null;

        #endregion

        #region Class Events

        internal event PlaybackStateChangedEventHandler PlaybackStateChanged;

        #endregion

        #region Class Properties

        internal int CommandInterval { get; private set; }
        internal int PauseAfterPlayback { get; private set; }
        internal bool ExitAfterPlayback { get; private set; }
        internal string CommandFileName { get; private set; }
        internal State CurrentState { get; private set; }
        internal Exception PlaybackException { get; private set; }

        internal DynCmd.RecordableCommand PreviousCommand { get; private set; }
        internal DynCmd.RecordableCommand CurrentCommand { get; private set; }

        internal bool IsInPlaybackMode
        {
            get { return (this.CurrentState != State.Recording); }
        }

        internal bool CanSaveRecordedCommands
        {
            get
            {
                if (null == recordedCommands)
                    return false;

                return (recordedCommands.Count > 0);
            }
        }

        /// <summary>
        /// When "DynamoModel.DynamicRunEnabled" is set to true, "HomeWorkspace"
        /// starts its internal "DispatcherTimer" whenever its content is being 
        /// modified. This timer starts a round of evaluation after a predefined 
        /// amount of time has ellapsed, preventing modifications in quick 
        /// succession from triggering too many evaluations. However, the timer 
        /// does not always have a chance to tick. This is especially true when 
        /// AutomationSettings exhausted all available commands in its list and 
        /// is ready to end the current test run. The shutdown timer that  
        /// AutomationSettings kicks start may tick before the evaluation timer 
        /// in WorkspaceModel has a chance to tick. When this happens, validation
        /// code at the end of the recorded test ends up with invalid evaluation
        /// results, failing the test case.
        /// </summary>
        /// <returns>Returns true if there is a pending evaluation and that the 
        /// shutdown process should be deferred.</returns>
        /// 
        private bool HasPendingEvaluation
        {
            get
            {
                var homeWorkspace = owningDynamoModel.HomeSpace;
                if (homeWorkspace == null)
                    return false;

                return homeWorkspace.IsEvaluationPending;
            }
        }

        #endregion

        #region Class Operational Methods

        internal AutomationSettings(DynamoModel dynamoModel, string commandFilePath)
        {
            this.CommandInterval = 20; // 20ms between two consecutive commands.
            this.PauseAfterPlayback = 10; // 10ms after playback is done.
            this.ExitAfterPlayback = true; // Exit Dynamo after playback.

            this.PreviousCommand = null;
            this.CurrentCommand = null;

            this.CurrentState = State.None;
            this.CommandFileName = string.Empty;
            if (LoadCommandFromFile(commandFilePath))
            {
                ChangeStateInternal(State.Loaded);
                CommandFileName = Path.GetFileNameWithoutExtension(commandFilePath);
            }
            else
            {
                ChangeStateInternal(State.Recording);
                recordedCommands = new List<DynCmd.RecordableCommand>();
            }

            this.owningDynamoModel = dynamoModel;
            if (null == this.owningDynamoModel)
                throw new ArgumentNullException("dynamoModel");
        }

        internal void BeginCommandPlayback(System.Windows.Window mainWindow)
        {
            if (this.CurrentState != State.Loaded)
                return; // Not currently in playback mode.

            if (null != this.playbackTimer)
            {
                // Ensure that this method is not called more than once.
                throw new InvalidOperationException(
                    "Internal error: 'BeginCommandPlayback' called twice");
            }

            this.mainWindow = mainWindow;
            this.mainWindow.Title = string.Format("{0} [Playing back: {1}]",
                this.mainWindow.Title, this.CommandFileName);

            System.Diagnostics.Debug.Assert(null == playbackTimer);
            playbackTimer = new DispatcherTimer();

            // Serialized commands for playback.
            playbackTimer.Interval = TimeSpan.FromMilliseconds(CommandInterval);
            playbackTimer.Tick += OnPlaybackTimerTick;
            playbackTimer.Start();
            ChangeStateInternal(State.Playing);
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
            helper.SetAttribute(IntervalAttribName, CommandInterval);

            foreach (DynCmd.RecordableCommand command in recordedCommands)
                commandRoot.AppendChild(command.Serialize(document));

            string format = "Commands-{0:yyyyMMdd-hhmmss}.xml";
            string xmlFileName = string.Format(format, DateTime.Now);
            string xmlFilePath = Path.Combine(Path.GetTempPath(), xmlFileName);

            // Save recorded commands into XML file and open it in viewer.
            document.Save(xmlFilePath);
            return xmlFilePath;
        }

        internal string InsertPausePlaybackCommand()
        {
            if (CurrentState != State.Recording)
            {
                var message = "Method should be called only when recording";
                throw new InvalidOperationException(message);
            }

            var pausePlaybackCommand = new DynCmd.PausePlaybackCommand(20);
            this.RecordCommand(pausePlaybackCommand);
            return pausePlaybackCommand.Tag;
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
                this.CommandInterval = helper.ReadInteger(IntervalAttribName, 20);

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

        private void PauseCommandPlayback(int pauseDurationInMs)
        {
            if (pauseDurationInMs <= 0)
            {
                var msg = "Argument should be greater than zero";
                throw new ArgumentException(msg, "pauseDurationInMs");
            }

            this.playbackTimer.Tick -= OnPlaybackTimerTick;
            this.playbackTimer.Tick += OnPauseTimerTick;

            var interval = TimeSpan.FromMilliseconds(pauseDurationInMs);
            this.playbackTimer.Interval = interval;
            this.playbackTimer.Start(); // Start pausing timer.
            ChangeStateInternal(State.Paused);
        }

        private void OnPlaybackTimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop(); // Stop the timer before command completes.

            if (loadedCommands.Count <= 0) // There's nothing else for playback.
            {
                if (this.ExitAfterPlayback == false)
                {
                    // The playback is done, but the command file indicates that
                    // Dynamo should not be shutdown after the playback, so here
                    // we simply invalidate the timer.
                    // 
                    this.playbackTimer = null;
                    ChangeStateInternal(State.Stopped);
                }
                else
                {
                    // The command file requires Dynamo be shutdown after all 
                    // commands has been played back. If that's the case, we'll
                    // reconfigure the callback to a shutdown timer, and then 
                    // change its interval to the duration specified earlier.
                    // 
                    this.playbackTimer.Tick -= OnPlaybackTimerTick;
                    this.playbackTimer.Tick += OnShutdownTimerTick;

                    var interval = TimeSpan.FromMilliseconds(PauseAfterPlayback);
                    this.playbackTimer.Interval = interval;
                    this.playbackTimer.Start(); // Start shutdown timer.
                    ChangeStateInternal(State.ShuttingDown);
                }

                return;
            }

            // Remove the first command from the loaded commands.
            DynCmd.RecordableCommand nextCommand = loadedCommands[0];
            loadedCommands.RemoveAt(0);

            // Update the cached command references.
            this.PreviousCommand = this.CurrentCommand;
            this.CurrentCommand = nextCommand;

            if (nextCommand is DynCmd.PausePlaybackCommand)
            {
                var command = nextCommand as DynCmd.PausePlaybackCommand;
                PauseCommandPlayback(command.PauseDurationInMs);
                return;
            }

            try
            {
                // Execute the command, this may take a while longer than the timer
                // inverval (usually very short), that's why the timer was stopped 
                // before the command execution starts. After the command is done,
                // the timer is then resumed for the next command in queue.
                // 
                this.owningDynamoModel.ExecuteCommand(nextCommand);
            }
            catch (Exception exception)
            {
                // An exception is thrown while playing back a command. Remove any 
                // pending commands and allow the "playbackTimer" to continue with
                // its next tick. Proper shutdown sequence will be initialized 
                // when the "playbackTimer" tries to pick up the next command and 
                // realized that there is no more commands waiting.
                // 
                loadedCommands.Clear();
                this.PlaybackException = exception;
            }

            timer.Start();
        }

        private void OnPauseTimerTick(object sender, EventArgs e)
        {
            this.playbackTimer.Tick -= OnPauseTimerTick;
            this.playbackTimer.Tick += OnPlaybackTimerTick;

            var interval = TimeSpan.FromMilliseconds(CommandInterval);
            this.playbackTimer.Interval = interval;
            this.playbackTimer.Start(); // Start regular playback timer.
            ChangeStateInternal(State.Playing);
        }

        private void OnShutdownTimerTick(object sender, EventArgs e)
        {
            this.playbackTimer.Stop();

            if (HasPendingEvaluation) // See method for documentation.
            {
                // When shutdown timer ticks and there is still an outstanding 
                // evaluation, then let the timer ticks away so it checks back 
                // later. Here the interval is updated to 20ms -- something that
                // is independent of the predefined shutdown interval.
                // 
                playbackTimer.Interval = TimeSpan.FromMilliseconds(20);
                playbackTimer.Start();
                return;
            }

            this.playbackTimer = null;
            ChangeStateInternal(State.Stopped);

            // This causes the main window to close (and exit application).
            mainWindow.Close();

            // If there is an exception as the result of command playback, 
            // then rethrow it here after closing the main window so that 
            // calls like NUnit test cases can properly display the exception.
            // 
            if (this.PlaybackException != null)
                throw this.PlaybackException;
        }

        private void ChangeStateInternal(State playbackState)
        {
            var os = this.CurrentState;
            var ns = playbackState;

            this.CurrentState = playbackState;
            if (os != ns && (this.PlaybackStateChanged != null))
            {
                var ot = ((PreviousCommand == null) ? "" : PreviousCommand.Tag);
                var nt = ((CurrentCommand == null) ? "" : CurrentCommand.Tag);

                var args = new PlaybackStateChangedEventArgs(ot, nt, os, ns);
                this.PlaybackStateChanged(this, args);
            }
        }

        #endregion
    }
}
