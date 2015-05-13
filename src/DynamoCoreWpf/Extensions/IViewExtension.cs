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
        Guid Id { get; }

        /// <summary>
        /// A name for the extension instance.  This is used for more user-readable logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Action to be invoked when DynamoView begins to start up.  
        /// 
        /// Exceptions thrown from this method should be caught by Dynamo and 
        /// displayed.
        /// </summary>
        void Startup(ViewStartupParams p);

        /// <summary>
        /// Action to be invoked when DynamoView is loaded.
        /// 
        /// Exceptions thrown from this method should be caught by Dynamo and 
        /// displayed.
        /// </summary>
        void Loaded(ViewLoadedParams p);
    }
}
