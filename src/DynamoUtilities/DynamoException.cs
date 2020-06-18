using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoUtilities
{
    /// <summary>
    /// Represents an error raised by Dynamo
    /// </summary>
    [Serializable]
    public class DynamoException : Exception
    {
        public DynamoException() { }
        public DynamoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
