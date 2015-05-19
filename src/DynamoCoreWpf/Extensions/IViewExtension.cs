using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Extensions;

namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// An extension to the UI layer of Dynamo.
    /// </summary>
    public interface IViewExtension : IDisposable
    {
        /// <summary>
        /// A unique id for this extension instance.  
        /// 
        /// There may be multiple instances of the same type, but the application 
        /// will *not* allow two instances to coexist with the same id.
        /// </summary>
        string UniqueId { get; }

        /// <summary>
        /// A name for the extension instance.  This is used for more user-readable logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Action to be invoked when DynamoView begins to start up.  This is guaranteed to happen
        /// after the DynamoModel has been created.
        /// 
        /// This method is *not* guaranteed to be invoked unless the extension is present
        /// at startup.
        /// 
        /// Exceptions thrown from this method will be caught by Dynamo and 
        /// displayed.
        /// </summary>
        void Startup(ViewStartupParams p);

        /// <summary>
        /// Action to be invoked when DynamoView is loaded.
        /// 
        /// This action is guaranteed to be invoked, even if the extension is not present 
        /// at startup.
        /// 
        /// Exceptions thrown from this method will be caught by Dynamo and 
        /// displayed.
        /// </summary>
        void Loaded(ViewLoadedParams p);

        /// <summary>
        /// Action to be invoked when shutdown has begun.  This gives the UI a last minute chance to interact
        /// with the user.
        /// 
        /// This action is guaranteed to be invoked before the associated model layer method on IExtension.
        /// </summary>
        void Shutdown();
    }
}
