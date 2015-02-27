using System;

namespace ProtoCore.Exceptions
{
    public class CompileErrorsOccured : Exception
    {
        public CompileErrorsOccured()
        { }
        public CompileErrorsOccured(string message) : base("Compile exception. " + message) { }
    }

    public class RuntimeException : Exception
    {
        public RuntimeException() : base("Runtime exception occured") { }

        public RuntimeException(string message) : base("Runtime exception occured. " + message) { }
    }
}
