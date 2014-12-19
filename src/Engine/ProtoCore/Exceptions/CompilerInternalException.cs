using System;

namespace ProtoCore.Exceptions
{
    class CompilerInternalException : Exception
    {
        public CompilerInternalException(String message)
        {

        }


    }

    /// <summary>
    /// Exception handler for all error cases in the DS heap
    /// </summary>
    public class HeapCorruptionException : Exception
    {
        public HeapCorruptionException(String message)
            : base(/*NXLT*/"Heap Corruption Exception: " + message)
        {
        }
    }
}
