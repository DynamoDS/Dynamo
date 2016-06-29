using System;

namespace Dynamo.Graph.Nodes.ZeroTouch
{
    /// <summary>
    /// The exception that is thrown when definition for a custom node cannot be found
    /// </summary>
    public class UnresolvedFunctionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedFunctionException"/> class 
        /// with the name of the custom node that causes this exception.
        /// </summary>
        /// <param name="functionName">User friendly name of custom node on UI.</param>
        public UnresolvedFunctionException(string functionName)
            : base("Cannot find function: " + functionName)
        {
            FunctionName = functionName;
        }

        /// <summary>
        /// Returns user friendly name of not found custom node.
        /// </summary>
        public string FunctionName { get; private set; }
    }
}