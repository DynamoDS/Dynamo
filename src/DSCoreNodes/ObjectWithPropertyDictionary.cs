using System;
using System.Collections.Generic;

namespace DSCoreNodes
{
    /// <summary>
    /// Base class for all node objects.
    /// </summary>
    public abstract class ObjectWithPropertyDictionary : IDisposable
    {
        private readonly Dictionary<string, object> _properties;
        
        /// <summary>
        /// A Dictionary of object properties.
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get { return _properties; }
        }

        protected ObjectWithPropertyDictionary()
        {
            _properties = new Dictionary<string, object> { { "GUID", Guid.NewGuid() } };
        }

        /// <summary>
        /// Override in derived classes.
        /// </summary>
        public abstract void Dispose();
    }
}
