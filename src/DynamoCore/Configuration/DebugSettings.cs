using Dynamo.Core;

namespace Dynamo.Configuration
{
    /// <summary>
    /// DebugSettings is used in Dynamo Logger and UI.
    /// </summary>
    public class DebugSettings : NotificationObject
    {
        private bool showDebugASTs = false;
        private bool verboseLogging = false;

        /// <summary>
        /// Enable verbose logging this is a lot of data
        /// </summary>
        internal bool VerboseLogging
        {
            get { return verboseLogging; }
            set
            {
                verboseLogging = value;
                RaisePropertyChanged("VerboseLogging");
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
                RaisePropertyChanged("ShowDebugASTs");
            }
        }
    }
}
