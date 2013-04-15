using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo
{
    public abstract class dynModelBase
    {
        public Guid Guid { get; private set; }

        protected dynModelBase()
        {
            Guid = Guid.NewGuid();
        }

    }
}