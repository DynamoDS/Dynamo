using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Utilities;
using DynCmd = Dynamo.Models.DynamoModel;
using NodeUtils = Dynamo.Graph.Nodes.Utilities;
using Dynamo.UI;

namespace Dynamo.ViewModels
{
    internal class PlaybackStateChangedEventArgs : EventArgs
    {
        internal PlaybackStateChangedEventArgs(string oldCommandTag, string newCommandTag,
            AutomationSettings.State oldState, AutomationSettings.State newState)
        {
            OldState = oldState;
            NewState = newState;
            OldTag = oldCommandTag;
            NewTag = newCommandTag;
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
        private const string EXIT_ATTRIB_NAME = "ExitAfterPlayback";

        /// <summary>
        /// This attribute specifies the amount of time in milliseconds that 
        /// Dynamo window should pause before closing. This value is ignored 
        /// if ExitAttribName is set to "false" (in which case Dynamo 
        /// window will not be closed after playback is completed).
        /// </summary>
        /// 
        private const string PAUSE_ATTRIB_NAME = "PauseAfterPlaybackInMs";

        /// <summary>
        /// This attribute specifies the interval between two consecutive 
        /// commands in milliseconds. When a command is completed, the next 
        /// command will be executed after this interval elapsed. The default
        /// value for command interval is 20 milliseconds.
        /// </summary>
        private const string INTERVAL_ATTRIB_NAME = "CommandIntervalInMs";

        private Window mainWindow;
        private readonly DynamoModel owningDynamoModel;
        private DispatcherTimer playbackTimer;
        private List<DynamoModel.RecordableCommand> loadedCommands;
        private readonly List<DynamoModel.RecordableCommand> recordedCommands;

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

        internal DynamoModel.RecordableCommand PreviousCommand { get; private set; }
        internal DynamoModel.RecordableCommand CurrentCommand { get; private set; }

        internal bool IsInPlaybackMode
        {
            get { return (CurrentState != State.Recording); }
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
        /// When HomeWorkspace.RunSettings.RunType is set to Automatically, "HomeWorkspace"
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
                return
                    owningDynamoModel.Workspaces.OfType<HomeWorkspaceModel>()
                        .Any(hw => hw.IsEvaluationPending);
            }
        }

        #endregion

        #region Class Operational Methods

        internal AutomationSettings(DynamoModel dynamoModel, string commandFilePath)
        {
            CommandInterval = 20; // 20ms between two consecutive commands.
            PauseAfterPlayback = 10; // 10ms after playback is done.
            ExitAfterPlayback = true; // Exit Dynamo after playback.

            PreviousCommand = null;
            CurrentCommand = null;

            CurrentState = State.None;
            CommandFileName = string.Empty;
            if (LoadCommandFromFile(commandFilePath))
            {
                ChangeStateInternal(State.Loaded);
                CommandFileName = Path.GetFileNameWithoutExtension(commandFilePath);
            }
            else
            {
                ChangeStateInternal(State.Recording);
                recordedCommands = new List<DynamoModel.RecordableCommand>();
            }

            owningDynamoModel = dynamoModel;
            if (null == owningDynamoModel)
                throw new ArgumentNullException("dynamoModel");
        }

        internal void BeginCommandPlayback(Window window)
        {
            if (CurrentState != State.Loaded)
                return; // Not currently in playback mode.

            if (null != playbackTimer)
            {
                // Ensure that this method is not called more than once.
                throw new InvalidOperationException(
                    "Internal error: 'BeginCommandPlayback' called twice");
            }

            mainWindow = window;
            mainWindow.Title = string.Format("{0} [Playing back: {1}]",
                mainWindow.Title, CommandFileName);

            Debug.Assert(null == playbackTimer);
            playbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(CommandInterval)
            };
            // Serialized commands for playback.
            playbackTimer.Tick += OnPlaybackTimerTick;
            playbackTimer.Start();
            ChangeStateInternal(State.Playing);
        }

        internal void RecordCommand(DynamoModel.RecordableCommand command)
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
            var document = new XmlDocument();
            XmlElement commandRoot = document.CreateElement("Commands");
            document.AppendChild(commandRoot);

            const string format = "Commands-{0:yyyyMMdd-hhmmss}.xml";
            string xmlFileName = string.Format(format, DateTime.Now);
            string xmlFilePath = Path.Combine(Path.GetTempPath(), xmlFileName);
            
            // Create attributes that applied to the entire recording.
            var helper = new XmlElementHelper(commandRoot);
            helper.SetAttribute(EXIT_ATTRIB_NAME, ExitAfterPlayback);
            helper.SetAttribute(PAUSE_ATTRIB_NAME, PauseAfterPlayback);
            helper.SetAttribute(INTERVAL_ATTRIB_NAME, CommandInterval);
            // Serialization in SaveContext.File may need file path. Add it
            // temporarily and remove it after searilization.
            NodeUtils.SetDocumentXmlPath(document, xmlFilePath);

            foreach (DynamoModel.RecordableCommand command in recordedCommands)
                commandRoot.AppendChild(command.Serialize(document));

            NodeUtils.SetDocumentXmlPath(document, null);

            // Save recorded commands into XML file and open it in viewer.
            document.Save(xmlFilePath);
            return xmlFilePath;
        }

        internal string InsertPausePlaybackCommand()
        {
            if (CurrentState != State.Recording)
            {
                const string message = "Method should be called only when recording";
                throw new InvalidOperationException(message);
            }

            var pausePlaybackCommand = new DynamoModel.PausePlaybackCommand(20);
            RecordCommand(pausePlaybackCommand);
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
                var document = new XmlDocument();
                document.Load(commandFilePath);

                // Get to the root node of this Xml document.
                var commandRoot = document.FirstChild as XmlElement;
                if (null == commandRoot)
                    return false;

                // Read in optional attributes from the command root element.
                var helper = new XmlElementHelper(commandRoot);
                ExitAfterPlayback = helper.ReadBoolean(EXIT_ATTRIB_NAME, true);
                PauseAfterPlayback = helper.ReadInteger(PAUSE_ATTRIB_NAME, 10);
                CommandInterval = helper.ReadInteger(INTERVAL_ATTRIB_NAME, 20);

                loadedCommands = new List<DynamoModel.RecordableCommand>();
                foreach (
                    DynamoModel.RecordableCommand command in
                        commandRoot.ChildNodes.Cast<XmlNode>()
                            .Select(
                                node => DynamoModel.RecordableCommand.Deserialize(
                                    node as XmlElement))
                            .Where(command => null != command))
                {
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
                const string msg = "Argument should be greater than zero";
                throw new ArgumentException(msg, "pauseDurationInMs");
            }

            playbackTimer.Tick -= OnPlaybackTimerTick;
            playbackTimer.Tick += OnPauseTimerTick;

            var interval = TimeSpan.FromMilliseconds(pauseDurationInMs);
            playbackTimer.Interval = interval;
            playbackTimer.Start(); // Start pausing timer.
            ChangeStateInternal(State.Paused);
        }

        private void OnPlaybackTimerTick(object sender, EventArgs e)
        {
            var timer = (DispatcherTimer)sender;
            timer.Stop(); // Stop the timer before command completes.

            if (loadedCommands.Count <= 0) // There's nothing else for playback.
            {
                if (ExitAfterPlayback == false)
                {
                    // The playback is done, but the command file indicates that
                    // Dynamo should not be shutdown after the playback, so here
                    // we simply invalidate the timer.
                    // 
                    playbackTimer = null;
                    ChangeStateInternal(State.Stopped);
                }
                else
                {
                    // The command file requires Dynamo be shutdown after all 
                    // commands has been played back. If that's the case, we'll
                    // reconfigure the callback to a shutdown timer, and then 
                    // change its interval to the duration specified earlier.
                    // 
                    playbackTimer.Tick -= OnPlaybackTimerTick;
                    playbackTimer.Tick += OnShutdownTimerTick;

                    var interval = TimeSpan.FromMilliseconds(PauseAfterPlayback);
                    playbackTimer.Interval = interval;
                    playbackTimer.Start(); // Start shutdown timer.
                    ChangeStateInternal(State.ShuttingDown);
                }

                return;
            }

            // Remove the first command from the loaded commands.
            DynamoModel.RecordableCommand nextCommand = loadedCommands[0];
            loadedCommands.RemoveAt(0);

            // Update the cached command references.
            PreviousCommand = CurrentCommand;
            CurrentCommand = nextCommand;

            if (nextCommand is DynamoModel.PausePlaybackCommand)
            {
                var command = nextCommand as DynamoModel.PausePlaybackCommand;
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
                owningDynamoModel.ExecuteCommand(nextCommand);
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
                PlaybackException = exception;
            }

            timer.Start();
        }

        private void OnPauseTimerTick(object sender, EventArgs e)
        {
            playbackTimer.Tick -= OnPauseTimerTick;
            playbackTimer.Tick += OnPlaybackTimerTick;

            var interval = TimeSpan.FromMilliseconds(CommandInterval);
            playbackTimer.Interval = interval;
            playbackTimer.Start(); // Start regular playback timer.
            ChangeStateInternal(State.Playing);
        }

        private void OnShutdownTimerTick(object sender, EventArgs e)
        {
            playbackTimer.Stop();

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

            playbackTimer = null;
            ChangeStateInternal(State.Stopped);

            // This causes the main window to close (and exit application).
            mainWindow.Close();

            // If there is an exception as the result of command playback, 
            // then rethrow it here after closing the main window so that 
            // calls like NUnit test cases can properly display the exception.
            // 
            if (PlaybackException != null)
                ExceptionDispatchInfo.Capture(PlaybackException).Throw();
        }

        private void ChangeStateInternal(State playbackState)
        {
            var os = CurrentState;
            var ns = playbackState;

            CurrentState = playbackState;
            if (os != ns && (PlaybackStateChanged != null))
            {
                var ot = ((PreviousCommand == null) ? "" : PreviousCommand.Tag);
                var nt = ((CurrentCommand == null) ? "" : CurrentCommand.Tag);

                var args = new PlaybackStateChangedEventArgs(ot, nt, os, ns);
                PlaybackStateChanged(this, args);
            }
        }

        #endregion
    }
}
