using Dynamo.Interfaces;
using Dynamo.Wpf.Extensions;
using System;

namespace Dynamo.Publish
{
    public class DynamoPublishExtension : IViewExtension, ILogSource
    {

        #region IViewExtension implementation

        public string UniqueId
        {
            get { return "BCABC211-D56B-4109-AF18-F434DFE48139"; }
        }

        public string Name
        {
            get { return "DynamoPublishExtension"; }
        }

        public void Startup(ViewStartupParams p)
        {
            // Some start up stuff.
        }

        public void Loaded(ViewLoadedParams p)
        {
            // Some loaded stuff.            
        }

        public void Shutdown()
        {
            // Some shutdown stuff.
        }

        public void Dispose()
        {
            // Some dispose stuff.
        }

        #endregion

        #region ILogSource implementation

        public event Action<ILogMessage> MessageLogged;

        private void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                this.MessageLogged(msg);
            }
        }

        #endregion
    }
}
