using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Extensions
{
    /// <summary>
    /// This class handles registration, lookup, and disposal of extensions.  There should only 
    /// be one of these per application instance.
    /// </summary>
    public interface IExtensionManager : IDisposable
    {
        /// <summary>
        /// Add an extension to the current application session.
        /// </summary>
        void Add(IExtension extension);

        /// <summary>
        /// Remove an extension from the current application session.
        /// </summary>
        void Remove(IExtension extension);

        /// <summary>
        /// The collection of currently registered extensions
        /// </summary>
        IEnumerable<IExtension> Extensions { get; }

        /// <summary>
        /// Event raised when an extension is added
        /// </summary>
        event Action<IExtension> ExtensionAdded;

        /// <summary>
        /// Event raised when an extension is removed
        /// </summary>
        event Action<IExtension> ExtensionRemoved;

        IExtensionLoader ExtensionLoader { get; }
    }
}
