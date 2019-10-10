using System;

namespace DesignScript.Builtin
{
    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException(string message)
            : base(message)
        {
        }
    }

    public class IndexOutOfRangeException : Exception
    {
        public IndexOutOfRangeException(string message)
            : base(message)
        {
        }
    }

    public class StringOverIndexingException : Exception
    {
        public StringOverIndexingException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Null reference exception thrown with null DS builtin types:
    /// lists, dictionaries and strings.
    /// </summary>
    public class BuiltinNullReferenceException : NullReferenceException
    {
        public BuiltinNullReferenceException(string message)
            : base(message)
        {
        }
    }
}
