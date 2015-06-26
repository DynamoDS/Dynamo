using Dynamo.Interfaces;
using Dynamo.Publish.Models;
using Dynamo.Publish.ViewModels;
using Dynamo.Publish.Views;
using Dynamo.Wpf.Extensions;
using System;

namespace Dynamo.Publish
{
    public class FloodPersistenceServiceExtension : IViewExtension, ILogSource
    {

        private PublishView publishWindow;
        private PublishViewModel publishVM;
        private PublishModel publishM;

        #region IViewExtension implementation

        public string UniqueId
        {
            get { return "BCABC211-D56B-4109-AF18-F434DFE48139"; }
        }

        public string Name
        {
            get { return "FloodPersistenceService"; }
        }

        public void Startup(ViewStartupParams param)
        {
            publishM = new PublishModel(param.AuthenticationManager);
            publishVM = new PublishViewModel(publishM);
        }

        public void Loaded(ViewLoadedParams p)
        {
            publishWindow = new PublishView(publishVM);
            publishWindow.Show();
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
