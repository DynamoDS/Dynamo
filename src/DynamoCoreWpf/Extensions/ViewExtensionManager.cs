using System;
using System.Collections.Generic;
using Dynamo.Extensions;
using Dynamo.Logging;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// An object which may request a layout specification to be applied to the current library.
    /// </summary>
    internal interface ILayoutSpecSource
    {
        /// <summary>
        /// Event that is raised when the LayoutSpecSource requests a LayoutSpec to be applied.
        /// The string parameter here should be the layout spec json to merge into the existing spec.
        /// </summary>
        event Action<string> RequestApplyLayoutSpec;

        /// <summary>
        /// Event that is raised when LayoutSpecSource requests a LayoutSpec
        /// </summary>
        event Func<LayoutSpecification> RequestLayoutSpec;
    }

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
        private readonly List<IExtensionStorageAccess> storageAccessViewExtensions = new List<IExtensionStorageAccess>();
        private readonly ViewExtensionLoader viewExtensionLoader = new ViewExtensionLoader();
        private IExtensionManager extensionManager;

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

        /// <summary>
        /// Creates ViewExtensionManager with directories which require package certificate verification and access to the ExtensionManager.
        /// </summary>
        internal ViewExtensionManager(IExtensionManager manager,IEnumerable<string> directoriesToVerify) : this(directoriesToVerify)
        {
            extensionManager = manager;
        }

        private void RequestAddViewExtensionHandler(IViewExtension viewExtension)
        {
            if (viewExtension is IViewExtension)
            {
                this.Add(viewExtension);
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

        private void RequestApplyLayoutSpecHandler(string specJSON)
        {
            Log($"an extension requested application of {specJSON} layout spec");
              try
                {
                //try to combine the layout specs.
                    var customizationService = extensionManager.Service<ILibraryViewCustomization>();
                //TODO if the layoutspec is empty, we're calling this too early, we can retry after x seconds?
                    var originalLayoutSpec = customizationService.GetSpecification();
                    var requestedLayoutSpec = LayoutSpecification.FromJSONString(specJSON);
                    var merged = LayoutSpecification.MergeLayoutSpecs(originalLayoutSpec, requestedLayoutSpec);
                    //update the library with the merged spec.
                    customizationService.SetSpecification(merged);
                }

                catch (Exception ex)
                {
                    Log(ex.ToString());
                }
        }

        private void RequsetLayoutSpeckHandler(string speckJSON)
        {
            var test = speckJSON;
            var customizationService = extensionManager.Service<ILibraryViewCustomization>();
            var originalLayoutSpec = customizationService.GetSpecification();
        }

        private void UnsubscribeViewExtension(IViewExtension obj)
        {
            if (obj is IViewExtensionSource)
            {
                (obj as IViewExtensionSource).RequestLoadExtension -= RequestLoadViewExtensionHandler;
                (obj as IViewExtensionSource).RequestAddExtension -= RequestAddViewExtensionHandler ;
            }
            if (obj is ILayoutSpecSource ls)
            {
                ls.RequestApplyLayoutSpec -= RequestApplyLayoutSpecHandler;
                ls.RequestLayoutSpec -= RequsetLayoutSpeckHandler;
            }
        }

        private LayoutSpecification RequsetLayoutSpeckHandler()
        {
            var customizationService = extensionManager.Service<ILibraryViewCustomization>();
            var originalLayoutSpec = customizationService.GetSpecification();

            return originalLayoutSpec;
        }

        private void SubscribeViewExtension(IViewExtension obj)
        {
            if (obj is IViewExtensionSource)
            {
                (obj as IViewExtensionSource).RequestLoadExtension += RequestLoadViewExtensionHandler;
                (obj as IViewExtensionSource).RequestAddExtension += RequestAddViewExtensionHandler;
            }
            if (obj is ILayoutSpecSource ls)
            {
                ls.RequestApplyLayoutSpec += RequestApplyLayoutSpecHandler;
                ls.RequestLayoutSpec += RequsetLayoutSpeckHandler;
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
                // Inform view extension author and consumer that the view extension does not come 
                // with a consistent UniqueId. This may result in unexpected Dynamo behavior.
                if (extension.UniqueId != extension.UniqueId)
                {
                    Log("Inconsistent UniqueId for " + extension.Name + " view extension. This may result in unexpected Dynamo behavior.");
                }

                if (ExtensionAdded != null)
                {
                    ExtensionAdded(extension);
                }

                if (extension is IExtensionStorageAccess storageAccess &&
                    storageAccessViewExtensions.Find(x=> (x as IViewExtension).UniqueId == extension.UniqueId) is null)
                {
                    storageAccessViewExtensions.Add(storageAccess);
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

            if (extension is IExtensionStorageAccess storageAccess &&
                storageAccessViewExtensions.Contains(storageAccess))
            {
                storageAccessViewExtensions.Remove(storageAccess);
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

        /// <summary>
        /// Returns the collection of registered extensions implementing IExtensionStorageAccess
        /// </summary>
        public IEnumerable<IExtensionStorageAccess> StorageAccessViewExtensions
        {
            get { return storageAccessViewExtensions; }
        }

        public event Action<IViewExtension> ExtensionAdded;

        public event Action<IViewExtension> ExtensionRemoved;


        public void Dispose()
        {
            foreach (var ext in ViewExtensions)
            {
                try
                {
                    ext.Dispose();
                }
                catch (Exception exc)
                {
                    Log($"{ext.Name} :  {exc.Message} during dispose");
                }
            }
            viewExtensions.Clear();
            storageAccessViewExtensions.Clear();
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
