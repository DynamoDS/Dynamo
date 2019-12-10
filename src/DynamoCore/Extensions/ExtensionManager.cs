using System;
using System.Collections.Generic;
using Dynamo.Logging;

namespace Dynamo.Extensions
{

    /// <summary>
    /// An object which may request extensions to be loaded and added to the extensionsManager.
    /// </summary>
    public interface IExtensionSource
    {
        /// <summary>
        /// Event that is raised when the ExtensionSource requests an Extension be loaded.
        /// </summary>
        event Func<string, IExtension> RequestLoadExtension;

        /// <summary>
        /// Event that is raised when ExtensionSource requests an Extension to be added to 
        /// list of currently loaded extensions.
        /// </summary>
        event Action<IExtension> RequestAddExtension;

        /// <summary>
        /// Collection of Extensions this ExtensionSource has requested be loaded.
        /// </summary>
        IEnumerable<IExtension> RequestedExtensions { get;}
    }

    /// <summary>
    ///  This class handles registration, lookup, and disposal of extensions.
    /// </summary>
    public class ExtensionManager: IExtensionManager, ILogSource
    {
        private readonly List<IExtension> extensions = new List<IExtension>();
        private readonly ExtensionLoader extensionLoader = new ExtensionLoader();
        private readonly Dictionary<Type, KeyValuePair<string, object>> services = new Dictionary<Type, KeyValuePair<string, object>>();

        /// <summary>
        /// Creates ExtensionManager.
        /// </summary>
        public ExtensionManager()
        {
            extensionLoader.MessageLogged += Log;
            this.extensionLoader.ExtensionLoading += SubscribeExtension;
            this.ExtensionRemoved += UnsubscribeExtension;
        }

        /// <summary>
        /// Creates ExtensionManager with directories which require package certificate verification.
        /// </summary>
        public ExtensionManager(IEnumerable<string> directoriesToVerify) :this()
        {
            this.extensionLoader.DirectoriesToVerifyCertificates.AddRange(directoriesToVerify);
        }

        private void RequestAddExtensionHandler(dynamic extension)
        {
            if(extension is IExtension)
            {
                this.Add(extension as IExtension);
            }
           
        }
        private IExtension RequestLoadExtensionHandler(string extensionPath)
        {
            // If the path is not a viewExtension - load it.
            if (!(extensionPath.Contains("_ViewExtension")))
            {
               return this.extensionLoader.Load(extensionPath);
            }
            return null;
        }

        private void UnsubscribeExtension(IExtension obj)
        {
            if (obj is IExtensionSource)
            {
                (obj as IExtensionSource).RequestLoadExtension -= RequestLoadExtensionHandler;
                (obj as IExtensionSource).RequestAddExtension -= RequestAddExtensionHandler;
            }
        }

        private void SubscribeExtension(IExtension obj)
        {
            //if this extension could be a source of other extensions (like packageManagerExtension) then
            //lets handle those requests.
            if(obj is IExtensionSource)
            {
                (obj as IExtensionSource).RequestLoadExtension += RequestLoadExtensionHandler;
                (obj as IExtensionSource).RequestAddExtension += RequestAddExtensionHandler;
            }

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

        /// <summary>
        /// Allows extension applications to register some specific service by its type.
        /// Only one service of a given type can be registered.
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="service">The service object to register</param>
        /// <returns>A key for the registered service if registeration is 
        /// successful else null.</returns>
        public string RegisterService<T>(T service)
        {
            //If the input service is null, throw ArgumentNullException.
            if (service == null) throw new ArgumentNullException("service");

            //If there is a service already registered, return null
            if (Service<T>() != null) return null;

            var id = Guid.NewGuid().ToString();
            services.Add(typeof(T), new KeyValuePair<string, object>(id, service));
            return id;
        }

        /// <summary>
        /// Unregisters a service of given type registered with given key. 
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <param name="serviceKey">The service key to ensure that only authorized
        /// client is unregistering this service type.</param>
        /// <returns></returns>
        public bool UnregisterService<T>(string serviceKey)
        {
            var type = typeof(T);
            KeyValuePair<string, object> pair;
            if(services.TryGetValue(type, out pair) && pair.Key.Equals(serviceKey))
            {
                return services.Remove(type);
            }

            return false;
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the service</typeparam>
        /// <returns>The service object if registered else null</returns>
        public T Service<T>()
        {
            return (T)GetService(typeof(T));
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">Type of the service</param>
        /// <returns>The service object if registered else null</returns>
        public object GetService(Type serviceType)
        {
            KeyValuePair<string, object> pair;
            if (services.TryGetValue(serviceType, out pair)) return pair.Value;

            return null;
        }
    }
}
