using System;
using System.Runtime.Serialization;
using System.IO;

namespace DynamoServices
{
    public class Validity
    {
        public static void Assert(bool cond)
        {
            if (!cond)
                throw new NodeInternalException();
        }

        public static void Assert(bool cond, string message)
        {
            if (!cond)
                throw new NodeInternalException(message);
        }
    }


    [Serializable]
    public class NodeInternalException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NodeInternalException()
        {
        }

        public NodeInternalException(string message) : base(message)
        {
        }

        public NodeInternalException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Represents FileLoadException having HRESULT value of 0x80131515. 
    /// Throw this when we need to instruct the user to "unblock" the downloaded assembly.
    /// </summary>
    internal class AssemblyBlockedException : FileLoadException
    {
        public AssemblyBlockedException(string message) : base(message)
        {
        }
    }
}
