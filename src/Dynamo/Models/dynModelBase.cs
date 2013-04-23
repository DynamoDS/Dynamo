using System;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    public abstract class dynModelBase : NotificationObject
    {
        public Guid GUID { get; set; }

        protected dynModelBase()
        {
            GUID = Guid.NewGuid();
        }

    }
}