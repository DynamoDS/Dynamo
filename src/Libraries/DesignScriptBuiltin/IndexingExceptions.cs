using System;
using Autodesk.DesignScript.Runtime;

namespace DesignScript.Builtin
{
    [SupressImportIntoVM]
    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException(string message)
            : base(message)
        {
        }
    }

    [SupressImportIntoVM]
    public class IndexOutOfRangeException : Exception
    {
        public IndexOutOfRangeException(string message)
            : base(message)
        {
        }
    }

    [SupressImportIntoVM]
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
    [SupressImportIntoVM]
    public class BuiltinNullReferenceException : NullReferenceException
    {
        public BuiltinNullReferenceException(string message)
            : base(message)
        {
        }
    }
}
