using System;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo
{
    public abstract class dynViewModelBase : NotificationObject
    {
        Guid guid;
        public Guid GUID
        {
            get { return guid; }
            set { guid = value; }
        }

        public dynViewModelBase()
        {
        }
    }
}
