namespace Dynamo.Wpf.Extensions
{
    /// <summary>
    /// Base class for View Extension.
    /// </summary>
    public abstract class ViewExtensionBase : IViewExtension
    {
        /// <summary>
        /// A unique id for this extension instance.  
        /// 
        /// There may be multiple instances of the same type, but the application 
        /// will *not* allow two instances to coexist with the same id.
        /// </summary>
        public abstract string UniqueId { get; }

        /// <summary>
        /// A name for the extension instance.  This is used for more user-readable logging.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Dispose method for the view extension.
        /// </summary>
        public abstract void Dispose();

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
        public virtual void Loaded(ViewLoadedParams viewLoadedParams)
        {

        }

        /// <summary>
        /// Action to be invoked when shutdown has begun.  This gives the UI a last minute chance to interact
        /// with the user.
        /// 
        /// This action is guaranteed to be invoked before the associated model layer method on IExtension.
        /// </summary>
        public virtual void Shutdown()
        {

        }

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
        public virtual void Startup(ViewStartupParams viewStartupParams)
        {

        }

        /// <summary>
        /// Action to be invoked when Dynamo starts up, to re-open the view extension which was open in the last session, if the preference setting
        /// to remember the last opened extensions was enabled.
        /// </summary>
        public virtual void ReOpen()
        {

        }

        /// <summary>
        /// Action to be invoked when the view extension is closed. 
        /// </summary>
        public virtual void Closed()
        {

        }
    }
}
