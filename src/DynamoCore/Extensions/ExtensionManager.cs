using Dynamo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Extensions
{
    public class ExtensionManager: IExtensionManager, ILogSource
    {
        private readonly List<IExtension> extensions = new List<IExtension>();
        private readonly ExtensionLoader extensionLoader = new ExtensionLoader();

        public ExtensionManager()
        {
            extensionLoader.MessageLogged += Log;
        }

        public void Add(IExtension extension)
        {
            if (extensions.Find(ext => ext.UniqueId == extension.UniqueId) == null)
            {
                extensions.Add(extension);
                Log(extension.Name + " extension is added");

                if (ExtensionAdded != null)
                {
                    ExtensionAdded(extension);
                }
            }
            else
            {
                Log("Could not add a duplicate of " + extension.Name);
            }
        }

        public void Remove(IExtension extension)
        {
            if (!extensions.Contains(extension))
            {
                Log("ExtensionManager does not contain a specified extension");
                return;
            }

            extensions.Remove(extension);
            extension.Dispose();
            Log(extension.Name + " extension is removed");
            if (ExtensionRemoved != null)
            {
                ExtensionRemoved(extension);
            }
        }

        public IEnumerable<IExtension> Extensions
        {
            get { return extensions; }
        }

        public event Action<IExtension> ExtensionAdded;

        public event Action<IExtension> ExtensionRemoved;

        public IExtensionLoader ExtensionLoader
        {
            get { return extensionLoader; }
        }

        public void Dispose()
        {
            while (extensions.Count > 0)
            {
                Remove(extensions[0]);
            }

            extensionLoader.MessageLogged -= Log;
        }

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
