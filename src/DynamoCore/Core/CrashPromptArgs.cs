using System;
using System.Collections.Generic;

namespace Dynamo.Core
{
    /// <summary>
    ///     Event argument for CrashPrompt. It contains display options, details, overriding text and file path.
    /// </summary>
    public class CrashPromptArgs : EventArgs
    {
        [Flags]
        public enum DisplayOptions
        {
            IsDefaultTextOverridden = 0x00000001,
            HasDetails = 0x00000002,
            HasFilePath = 0x00000004
        }

        /// <summary>
        ///     Returns <see cref="Dispalyoptions"/> flag which indicates whether args contain default text overriden,
        ///     has details or has file path
        /// </summary>
        public DisplayOptions Options { get; private set; }

        /// <summary>
        ///     Returns details of crash
        /// </summary>
        public string Details { get; private set; }

        /// <summary>
        ///     Returns crash message
        /// </summary>
        public string OverridingText { get; private set; }

        /// <summary>
        ///     Returns preference file path
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        ///     Returns initializes a new instance of the <see cref="CrashPromptArgs"/> class.
        /// </summary>
        /// <param name="details">Details of crash</param>
        /// <param name="overridingText">Crash message</param>
        /// <param name="filePath">Preferences file path</param>
        public CrashPromptArgs(string details, string overridingText = null, string filePath = null)
        {  
            if (details != null)
            {
                Details = details;
                Options |= DisplayOptions.HasDetails;
            }

            if (overridingText != null)
            {
                OverridingText = overridingText;
                Options |= DisplayOptions.IsDefaultTextOverridden;
            }

            if (filePath != null)
            {
                FilePath = filePath;
                Options |= DisplayOptions.HasFilePath;
            }
        }

        internal CrashPromptArgs(Exception e)
        {
            if (e != null)
            {
                var eDetails = new List<string>();
                GetDetailsFromException(e, eDetails);

                Details = string.Join($"\n\n-------- {nameof(Exception.InnerException)} --------\n\n", eDetails);
                Options |= DisplayOptions.HasDetails;
            }
        }

        private void GetDetailsFromException(Exception e, List<string> details)
        {
            details.Add(e.Message + "\n\n" + e.StackTrace);

            if (e.InnerException != null)
            {
                GetDetailsFromException(e.InnerException, details);
            }
        }

        internal bool IsDefaultTextOverridden()
        {
            return Options.HasFlag(DisplayOptions.IsDefaultTextOverridden);
        }

        internal bool HasDetails()
        {
            return Options.HasFlag(DisplayOptions.HasDetails);
        }

        internal bool IsFilePath()
        {
            return Options.HasFlag(DisplayOptions.HasFilePath);
        }
    }

    /// <summary>
    /// Event argument for CER (crash error reporting) tool.
    /// It contains options on what to send out with the crash report.
    /// </summary>
    public sealed class CrashErrorReportArgs : CrashPromptArgs
    {
        /// <summary>
        /// Constructs the options class to customize what CER will collect.
        /// </summary>
        /// <param name="e"></param>
        public CrashErrorReportArgs(Exception e) : base(e)
        {}

        /// <summary>
        /// Constructs the options class to customize what CER will collect.
        /// </summary>
        /// <param name="details">Crash details</param>
        internal CrashErrorReportArgs(string details) : base(details)
        { }

        /// <summary>
        /// Allow Dynamo to send the log file to the CER system.
        /// </summary>
        internal bool SendLogFile { get; set; } = true;

        /// <summary>
        /// Allow Dynamo to send the settings file to the CER system.
        /// </summary>
        internal bool SendSettingsFile { get; set; } = true;

        /// <summary>
        /// Allow Dynamo to send the recorded commands file to the CER system.
        /// </summary>
        public bool SendRecordedCommands { get; set; } = true;
    }
}
