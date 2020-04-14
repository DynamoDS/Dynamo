using Dynamo.Core;

namespace Dynamo.Configuration
{
    /// <summary>
    /// This class is used for setting debug settings through Dynamo UI.
    /// E.g. turn on/off logging; show/hide compiled node values.
    /// </summary>
    public class DebugSettings : NotificationObject
    {
        private bool showDebugASTs = false;
        private bool verboseLogging = false;
        private bool showPythonEngineSwitcher = false;

        /// <summary>
        /// Enable verbose logging this is a lot of data
        /// </summary>
        internal bool VerboseLogging
        {
            get { return verboseLogging; }
            set
            {
                verboseLogging = value;
                RaisePropertyChanged(nameof(VerboseLogging));
            }
        }

        /// <summary>
        /// Shows near node view its AST compiled node.
        /// </summary>
        internal bool ShowDebugASTs
        {
            get { return showDebugASTs; }
            set
            {
                showDebugASTs = value;
                RaisePropertyChanged(nameof(ShowDebugASTs));
            }
        }

        /// <summary>
        /// Enable Python Engine Switcher in Dynamo
        /// </summary>
        internal bool ShowPythonEngineSwitcher
        {
            get { return showPythonEngineSwitcher; }
            set
            {
                showPythonEngineSwitcher = value;
                RaisePropertyChanged(nameof(ShowPythonEngineSwitcher));
            }
        }
    }
}
