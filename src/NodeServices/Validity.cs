using System;
using System.Runtime.Serialization;

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

        protected NodeInternalException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

}
