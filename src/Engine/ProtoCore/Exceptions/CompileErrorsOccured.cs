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

    public class RunOutOfMemoryException : OutOfMemoryException 
    {
        public RunOutOfMemoryException(): base("Run out of memory") 
        {
        }

        public RunOutOfMemoryException(string message) : base("Run out of memory. " + message)
        {
        }
    }
}
