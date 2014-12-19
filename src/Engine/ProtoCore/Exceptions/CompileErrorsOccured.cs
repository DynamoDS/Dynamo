using System;

namespace ProtoCore.Exceptions
{
    public class CompileErrorsOccured : Exception
    {
        public CompileErrorsOccured()
        { }
        public CompileErrorsOccured(string message) : base(/*NXLT*/"Compile exception. " + message) { }
    }

    public class RuntimeException : Exception
    {
        public RuntimeException() : base(/*NXLT*/"Runtime exception occured") { }

        public RuntimeException(string message) : base(/*NXLT*/"Runtime exception occured. " + message) { }
    }
}
