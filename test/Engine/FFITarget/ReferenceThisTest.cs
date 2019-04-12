using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

namespace FFITarget
{
    public class ReferenceThisItem 
    {
        private ReferenceThis refThis;

        public ReferenceThisItem(ReferenceThis refThis)
        {
            this.refThis = refThis;
        }

        public bool IsHostDisposed 
        {
            get { return refThis.Disposed; }
        }
    }

    public class ReferenceThis : IDisposable 
    {
        private bool _disposed = false;
        public bool Disposed
        {
            get { return _disposed; }
        }

        public ReferenceThis()
        {
            _disposed = false; 
        }

        [KeepReferenceThisAttribute]
        public IEnumerable<ReferenceThisItem> GetItems()
        {
            return new List<ReferenceThisItem> { new ReferenceThisItem(this), new ReferenceThisItem(this) };
        }

        public void Dispose()
        {
            _disposed = true;
        } 
    }
}
