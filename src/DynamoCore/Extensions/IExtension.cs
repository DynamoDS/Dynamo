using Dynamo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dynamo.Extensions
{
    /// <summary>
    /// An extension to the model layer of Dynamo
    /// </summary>
    public interface IExtension : IDisposable
    {
        /// <summary>
        /// A unique id for this extension instance.  
        /// 
        /// There may be multiple instances of the same type, but the application 
        /// will *not* allow two instances to coexist with the same id.
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// A name for the Extension.  This is used for more user-readable logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Action to be invoked when Dynamo begins to start up. 
        /// 
        /// This action is *not* guaranteed to be invoked unless the extension is 
        /// already installed at startup. 
        /// 
        /// Exceptions thrown from this method will be caught by Dynamo and 
        /// logged.
        /// </summary>
        void Startup(StartupParams sp);

        /// <summary>
        /// Action to be invoked when the Dynamo has started up and is ready
        /// for user interaction.
        /// 
        /// This action is guaranteed to be called even if the extension is installed
        /// after startup.  
        /// 
        /// Exceptions thrown from this method will be caught by Dynamo and 
        /// logged.
        /// </summary>
        void Ready(ReadyParams sp);

        /// <summary>
        /// Action to be invoked when shutdown has begun.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Scan the PackagesDirectory for packages and attempt to load all of them.
        /// </summary>
        /// <param name="preferences">IPreferences instance, which contains 
        /// a list of packages used by the Package Manager to determine, 
        /// which packages are marked for deletion.</param>
        /// <param name="pathManager">IPathManager instance 
        /// with set of directories paths needed for Package Manager</param>
        void Load(IPreferences preferences, IPathManager pathManager);

        /// <summary>
        /// Event which is fired, when it needs to load an assembly with nodes
        /// </summary>
        event Action<Assembly> RequestLoadNodeLibrary;
    }
}
