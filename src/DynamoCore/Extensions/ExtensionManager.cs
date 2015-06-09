using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Extensions
{
    public class ExtensionManager: IExtensionManager
    {
        List<IExtension> extensions = new List<IExtension>();
        IExtensionLoader extensionLoader = new ExtensionLoader();

        public void Add(IExtension extension)
        {
            if (extensions.Find(ext => ext.UniqueId == extension.UniqueId) == null)
            {
                extensions.Add(extension);

                if (ExtensionAdded != null)
                {
                    ExtensionAdded(extension);
                }
            }
        }

        public void Remove(IExtension extension)
        {
            extensions.Remove(extension);
            extension.Dispose();
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
        }
    }
}
