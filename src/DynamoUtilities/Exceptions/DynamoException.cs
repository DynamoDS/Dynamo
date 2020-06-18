using System;

namespace Dynamo.Utilities.Exceptions
{
    /// <summary>
    /// Represents an error raised by Dynamo
    /// </summary>
    [Serializable]
    public class DynamoException : Exception
    {
        public DynamoException() { }

        public DynamoException(string message) : base(message)
        {
        }

        public DynamoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
