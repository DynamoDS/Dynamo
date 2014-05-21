using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.Core
{
    public class DebugSettings : NotificationObject
    {
        private bool _showDebugASTs = false;
        private bool _verboseLogging = false;

        /// <summary>
        /// Enable verbose logging this is a lot of data
        /// </summary>
        public bool VerboseLogging
        {
            get { return _verboseLogging; }
            set
            {
                _verboseLogging = value;
                RaisePropertyChanged("VerboseLogging");
            }
        }

        public bool ShowDebugASTs
        {
            get { return _showDebugASTs; }
            set
            {
                _showDebugASTs = value;
                RaisePropertyChanged("ShowDebugASTs");
            }
        }
    }
}
