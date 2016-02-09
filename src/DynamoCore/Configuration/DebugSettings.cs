using Dynamo.Core;

namespace Dynamo.Configuration
{
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
