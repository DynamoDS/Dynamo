using Dynamo.Extensions;
using Dynamo.Interfaces;
using Dynamo.Logging;

using System;
using System.Collections.Generic;

namespace Dynamo.Wpf.Extensions
{
    internal class ViewExtensionManager : IViewExtensionManager, ILogSource
    {
        private readonly List<IViewExtension> viewExtensions = new List<IViewExtension>();
        private readonly ViewExtensionLoader viewExtensionLoader = new ViewExtensionLoader();

        public ViewExtensionManager()
        {
            viewExtensionLoader.MessageLogged += Log;
            this.ExtensionAdded += SubscribeViewExtension;
            this.ExtensionRemoved += UnsubscribeViewExtension;
        }

        private void requestAddViewExtensionHandler(dynamic viewExtension )
        {
            //this handler is used to cast to IViewExtension.    
            this.Add(viewExtension as IViewExtension);
        }

        private void UnsubscribeViewExtension(IViewExtension obj)
        {
            if (obj is IExtensionSource)
            {
                (obj as IExtensionSource).RequestLoadViewExtension -= this.ExtensionLoader.Load;
                (obj as IExtensionSource).RequestAddViewExtension -= this.requestAddViewExtensionHandler;
            }
        }

        private void SubscribeViewExtension(IViewExtension obj)
        {
            //if this extension could be a source of other extensions (like packageManagerExtension) then
            //lets handle those requests.
            if (obj is IExtensionSource)
            {
                (obj as IExtensionSource).RequestLoadViewExtension += this.ExtensionLoader.Load;
                (obj as IExtensionSource).RequestAddViewExtension += this.requestAddViewExtensionHandler;
            }

        }
        public ViewExtensionLoader ExtensionLoader
        {
            get { return viewExtensionLoader; }
        }

        #region IViewExtensionManager implementation

        public void Add(IViewExtension extension)
        {
            var fullName = extension.Name + " (id: " + extension.UniqueId + ")";
            if (viewExtensions.Find(ext => ext.UniqueId == extension.UniqueId) == null)
            {
                viewExtensions.Add(extension);
                Log(fullName + " view extension is added");

                if (ExtensionAdded != null)
                {
                    ExtensionAdded(extension);
                }
            }
            else
            {
                Log("Could not add a duplicate of " + fullName);
            }
        }

        public void Remove(IViewExtension extension)
        {
            var fullName = extension.Name + " (id: " + extension.UniqueId + ")";
            if (!viewExtensions.Contains(extension))
            {
                Log("ExtensionManager does not contain " + fullName + " view extension");
                return;
            }

            viewExtensions.Remove(extension);
            try
            {
                extension.Dispose();
            }
            catch (Exception ex)
            {
                Log(fullName + " extension cannot be disposed properly: " + ex.Message);
            }

            Log(fullName + " extension is removed");
            if (ExtensionRemoved != null)
            {
                ExtensionRemoved(extension);
            }
        }

        public IEnumerable<IViewExtension> ViewExtensions
        {
            get { return viewExtensions; }
        }

        public event Action<IViewExtension> ExtensionAdded;

        public event Action<IViewExtension> ExtensionRemoved;

        public void Dispose()
        {
            viewExtensions.Clear();
            viewExtensionLoader.MessageLogged -= Log;
        }

        #endregion

        #region ILogSource implementation

        public event Action<ILogMessage> MessageLogged;

        private void Log(ILogMessage obj)
        {
            if (MessageLogged != null)
            {
                MessageLogged(obj);
            }
        }

        private void Log(string message)
        {
            Log(LogMessage.Info(message));
        }

        #endregion
    }
}
