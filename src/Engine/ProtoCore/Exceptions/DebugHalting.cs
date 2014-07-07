using System;

namespace ProtoCore.Exceptions
{
    public class DebugHalting : Exception
    {
    }

    public class EndOfScript : Exception
    {
    }

    public class ExecutionCancelledException : Exception
    {
        public override string Message
        {
            get
            {
                return "The execution has been cancelled!";
            }
        }
    }
}
