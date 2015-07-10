using Dynamo.Interfaces;
using Dynamo.Publish.Models;
using Dynamo.Publish.ViewModels;
using Dynamo.Publish.Views;
using Dynamo.Wpf.Extensions;
using System;
using System.Windows.Controls;

namespace Dynamo.Publish
{
    public class DynamoPublishExtension : IViewExtension, ILogSource
    {

        private PublishViewModel publishViewModel;
        private PublishModel publishModel;

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
            publishModel = new PublishModel();
            publishViewModel = new PublishViewModel(publishModel);
        }

        public void Loaded(ViewLoadedParams p)
        {
            publishViewModel.Workspaces = p.WorkspaceViewModels;

            p.AddMenuItem(MenuBarType.File, GenerateMenuItem());
        }

        public void Shutdown()
        {
            
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

        #region Helpers

        private MenuItem GenerateMenuItem()
        {
            MenuItem item = new MenuItem();
            item.Header = Resource.DynamoViewMenuItemPublishTitle;

            item.Click += (sender, args) =>
                {
                    PublishView publishWindow = new PublishView(publishViewModel);
                    publishWindow.ShowDialog();
                };

            return item;
        }

        #endregion
    }
}
