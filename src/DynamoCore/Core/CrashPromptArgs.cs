﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Core
{
    public class CrashPromptArgs : EventArgs
    {
        [Flags]
        public enum DisplayOptions
        {
            IsDefaultTextOverridden = 0x00000001,
            HasDetails = 0x00000002,
            HasFilePath = 0x00000004
        }

        public DisplayOptions Options { get; private set; }
        public string Details { get; private set; }
        public string OverridingText { get; private set; }
        public string FilePath { get; private set; }

        // Default Crash Prompt
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

        public bool IsDefaultTextOverridden()
        {
            return Options.HasFlag(DisplayOptions.IsDefaultTextOverridden);
        }

        public bool HasDetails()
        {
            return Options.HasFlag(DisplayOptions.HasDetails);
        }

        public bool IsFilePath()
        {
            return Options.HasFlag(DisplayOptions.HasFilePath);
        }
    }

}
