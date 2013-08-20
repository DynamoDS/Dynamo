using System;
using System.Collections.Generic;

namespace DSRevitNodes
{
    public abstract class ObjectWithPropertyDictionary : IDisposable
    {
        private readonly Dictionary<string, object> _properties;
        public Dictionary<string, object> Properties
        {
            get { return _properties; }
        }

        protected ObjectWithPropertyDictionary()
        {
            _properties = new Dictionary<string, object> { { "GUID", Guid.NewGuid() } };
        }

        public abstract void Dispose();
    }
}
