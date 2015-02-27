
using System;

namespace ProtoCore.Exceptions
{
    public class ReplicationCaseNotCurrentlySupported : Exception
    {
        public ReplicationCaseNotCurrentlySupported(string message)
            : base(message) 
        { 
        }

    }
}
