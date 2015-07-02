using Dynamo.Interfaces;
using Dynamo.Publish.Models;
using Dynamo.Publish.ViewModels;
using Dynamo.Publish.Views;
using Dynamo.Wpf.Extensions;
using System;

namespace Dynamo.Publish
{
    public class DynamoPublishExtension : IViewExtension, ILogSource
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
            get { return "DynamoPublishExtension"; }
        }

        public void Startup(ViewStartupParams p)
        {
            publishM = new PublishModel();
            publishVM = new PublishViewModel(publishM);
        }

        public void Loaded(ViewLoadedParams p)
        {
            publishVM.WorkSpaces = p.WorkSpaces;
            publishWindow = new PublishView(publishVM);

            // This should be called when user clicks on menu item.
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
