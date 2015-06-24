using Dynamo.Interfaces;
using Dynamo.Wpf.Extensions;
using System;

namespace Dynamo.Publish
{
    public class FloodPersistenceServiceExtension : IViewExtension, ILogSource
    {

        #region IViewExtension implementation

        public string UniqueId
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { return "FloodPersistenceService"; }
        }

        public void Startup(ViewStartupParams p)
        {
            throw new NotImplementedException();
        }

        public void Loaded(ViewLoadedParams p)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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
