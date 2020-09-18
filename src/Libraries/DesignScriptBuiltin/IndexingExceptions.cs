using System;
using Autodesk.DesignScript.Runtime;

namespace DesignScript.Builtin
{
    [IsVisibleInDynamoLibrary(false)]
    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException(string message)
            : base(message)
        {
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class IndexOutOfRangeException : Exception
    {
        public IndexOutOfRangeException(string message)
            : base(message)
        {
        }
    }

    [IsVisibleInDynamoLibrary(false)]
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
    [IsVisibleInDynamoLibrary(false)]
    public class BuiltinNullReferenceException : NullReferenceException
    {
        public BuiltinNullReferenceException(string message)
            : base(message)
        {
        }
    }
}
