using System;

namespace ProtoScript.Messages
{
    public class Error
    {
        public String Message { get; set; }

    }

    public class CompileError : Error
    {
    }

    /// <summary>
    /// Catch all for unknown exections. Emitting this represents a defect in the compiler
    /// </summary>
    public class FatalCompileError : CompileError
    {

    }

}
