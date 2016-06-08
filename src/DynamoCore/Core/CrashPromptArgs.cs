using System;

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

}
