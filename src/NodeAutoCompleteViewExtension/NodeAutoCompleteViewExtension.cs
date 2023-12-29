using System;
using Dynamo.Extensions;
using Dynamo.Logging;
using Dynamo.Wpf.Extensions;

namespace Dynamo.NodeAutoComplete
{
    /// <summary>
    /// This view extension tracks current clicked node port in Dynamo and 
    /// tries to suggest the next best node to connect that port.
    /// </summary>
    public class NodeAutoCompleteViewExtension : IViewExtension, ILogSource
    {
        private const String extensionName = "Node Auto Complete";

        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name
        {
            get
            {
                return extensionName;
            }
        }

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public string UniqueId
        {
            get
            {
                return "64F28473-0DCB-4E41-BB5B-A409FF6C90AD";
            }
        }

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {
            // Do nothing for now
        }

        public void Ready(ReadyParams readyParams)
        {
            // Do nothing for now
        }

        public void Shutdown()
        {
            // Do nothing for now
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            // Do nothing for now
        }

        public event Action<ILogMessage> MessageLogged;

        internal void OnMessageLogged(ILogMessage msg)
        {
            this.MessageLogged?.Invoke(msg);
        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            // Do nothing for now
        }
    }
}
