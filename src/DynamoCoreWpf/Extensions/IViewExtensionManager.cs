using System;
using System.Collections.Generic;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// This class handles registration, lookup, and disposal of view layer extensions.  There should only 
    /// be one of these per application instance.
    /// </summary>
    public interface IViewExtensionManager : IDisposable
    {
        /// <summary>
        /// Add an extension to the current application session.
        /// </summary>
        void Add(IViewExtension extension);

        /// <summary>
        /// Remove an extension from the current application session.
        /// </summary>
        void Remove(IViewExtension extension);

        /// <summary>
        /// The collection of currently registered extensions
        /// </summary>
        IEnumerable<IViewExtension> ViewExtensions { get; }

        /// <summary>
        /// Event raised when an extension is added
        /// </summary>
        event Action<IViewExtension> ExtensionAdded;

        /// <summary>
        /// Event raised when an extension is removed
        /// </summary>
        event Action<IViewExtension> ExtensionRemoved;
    }
}
