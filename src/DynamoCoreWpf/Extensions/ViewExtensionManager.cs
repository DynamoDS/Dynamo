using System;
using System.Collections.Generic;
using Dynamo.Logging;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// An object which may request ViewExtensions to be loaded and added to the ViewExtensionsManager.
    /// </summary>
    public interface IViewExtensionSource
    {
        /// <summary>
        /// Event that is raised when the ViewExtensionSource requests a ViewExtension be loaded.
        /// </summary>
        event Func<string, IViewExtension> RequestLoadExtension;

        /// <summary>
        /// Event that is raised when ViewExtensionSource requests a ViewExtension to be added to 
        /// list of currently loaded ViewExtensions.
        /// </summary>
        event Action<IViewExtension> RequestAddExtension;

        /// <summary>
        /// Collection of ViewExtensions the ViewExtensionSource requested be loaded.
        /// </summary>
        IEnumerable<IViewExtension> RequestedExtensions { get; }
    }

    internal class ViewExtensionManager : IViewExtensionManager, ILogSource
    {
        private readonly List<IViewExtension> viewExtensions = new List<IViewExtension>();
        private readonly ViewExtensionLoader viewExtensionLoader = new ViewExtensionLoader();

        public ViewExtensionManager()
        {
            viewExtensionLoader.MessageLogged += Log;
            this.ExtensionLoader.ExtensionLoading += SubscribeViewExtension;
            this.ExtensionRemoved += UnsubscribeViewExtension;
        }

        /// <summary>
        /// Creates ViewExtensionManager with directories which require package certificate verification.
        /// </summary>
        public ViewExtensionManager(IEnumerable<string> directoriesToVerify) : this()
        {
            this.viewExtensionLoader.DirectoriesToVerifyCertificates.AddRange(directoriesToVerify);
        }

        private void RequestAddViewExtensionHandler(IViewExtension viewExtension)
        {
            if (viewExtension is IViewExtension)
            {
                this.Add(viewExtension as IViewExtension);
            }
        }
        private IViewExtension RequestLoadViewExtensionHandler(string extensionPath)
        {
            // If the path is a viewExtension - load it.
            if ((extensionPath.Contains("_ViewExtensionDefinition")))
            {
                return this.ExtensionLoader.Load(extensionPath);
            }
            return null;
        }

        private void UnsubscribeViewExtension(IViewExtension obj)
        {
            if (obj is IViewExtensionSource)
            {
                (obj as IViewExtensionSource).RequestLoadExtension -= RequestLoadViewExtensionHandler;
                (obj as IViewExtensionSource).RequestAddExtension -= RequestAddViewExtensionHandler ;
            }
        }

        private void SubscribeViewExtension(IViewExtension obj)
        {
            if (obj is IViewExtensionSource)
            {
                (obj as IViewExtensionSource).RequestLoadExtension += RequestLoadViewExtensionHandler;
                (obj as IViewExtensionSource).RequestAddExtension += RequestAddViewExtensionHandler;
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
