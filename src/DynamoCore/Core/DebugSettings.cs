namespace Dynamo.Core
{
    public class DebugSettings : NotificationObject
    {
        private bool showDebugASTs = false;
        private bool verboseLogging = false;

        /// <summary>
        /// Enable verbose logging this is a lot of data
        /// </summary>
        public bool VerboseLogging
        {
            get { return verboseLogging; }
            set
            {
                verboseLogging = value;
                RaisePropertyChanged("VerboseLogging");
            }
        }

        public bool ShowDebugASTs
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
