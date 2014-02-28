using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.Exceptions
{
    public class MethodResolutionException : Exception
    {
        public string MethodNotFound { get; private set; }

        public MethodResolutionException(string methodNotFound)
        {
            MethodNotFound = methodNotFound;
        }
    }
}
