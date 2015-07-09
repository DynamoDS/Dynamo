using Dynamo.Interfaces;
using Dynamo.Publish.Configurations;
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

            p.AddMenuItem(MenuBarType.File, GenerateMenuItem());
        }

        public void Shutdown()
        {
            publishWindow.Close();
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
                    if (publishWindow == null)
                    {
                        publishWindow = new PublishView(publishVM);
                        publishWindow.Closed += (s, arg) =>
                        {
                            publishWindow = null;
                        };
                    }

                    publishWindow.ShowDialog();
                };

            return item;
        }

        #endregion
    }
}
