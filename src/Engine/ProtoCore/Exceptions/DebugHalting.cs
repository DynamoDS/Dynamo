using System;

namespace ProtoCore.Exceptions
{
    public class DebugHalting : Exception
    {
    }

    public class EndOfScript : Exception
    {
    }

    public class CancelExecution : Exception
    {
        public override string Message
        {
            get
            {
                return string.Format("The execution has been cancelled!");
            }
        }
    }
}
