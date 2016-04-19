using Dynamo.Interfaces;
using Dynamo.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Extensions
{
    /// <summary>
    ///  This class handles registration, lookup, and disposal of extensions.
    /// </summary>
    public class ExtensionManager: IExtensionManager, ILogSource
    {
        private readonly List<IExtension> extensions = new List<IExtension>();
        private readonly ExtensionLoader extensionLoader = new ExtensionLoader();

        /// <summary>
        /// Creates ExtensionManager.
        /// </summary>
        public ExtensionManager()
        {
            extensionLoader.MessageLogged += Log;
        }

        /// <summary>
        /// Adds an extension to the current session.
        /// </summary>
        /// <param name="extension">Extension</param>
        public void Add(IExtension extension)
        {
            var fullName = extension.Name + " (id: " + extension.UniqueId + ")";
            if (extensions.Find(ext => ext.UniqueId == extension.UniqueId) == null)
            {
                extensions.Add(extension);
                Log(fullName + " extension is added");

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

        /// <summary>
        /// Removes an extension from the current session.
        /// </summary>
        /// <param name="extension">Extension</param>
        public void Remove(IExtension extension)
        {
            var fullName = extension.Name + " (id: " + extension.UniqueId + ")";
            if (!extensions.Contains(extension))
            {
                Log("ExtensionManager does not contain " + fullName + " extension");
                return;
            }

            extensions.Remove(extension);
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

        /// <summary>
        /// Returns the collection of registered extensions
        /// </summary>
        public IEnumerable<IExtension> Extensions
        {
            get { return extensions; }
        }

        /// <summary>
        /// This event is fired when a new extension is added.
        /// </summary>
        public event Action<IExtension> ExtensionAdded;

        /// <summary>
        /// This event is fired when a new extension is removed,
        /// </summary>
        public event Action<IExtension> ExtensionRemoved;

        /// <summary>
        /// This loader loads extensions in Dynamo.
        /// </summary>
        public IExtensionLoader ExtensionLoader
        {
            get { return extensionLoader; }
        }

        /// <summary>
        /// Disposes all the loaded extensions.
        /// </summary>
        public void Dispose()
        {
            while (extensions.Count > 0)
            {
                Remove(extensions[0]);
            }

            extensionLoader.MessageLogged -= Log;
        }

        /// <summary>
        /// This event is fired when a message is logged.
        /// </summary>
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
    }
}
