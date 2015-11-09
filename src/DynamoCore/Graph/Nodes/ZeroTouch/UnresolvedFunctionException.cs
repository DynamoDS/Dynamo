using System;

namespace Dynamo.Graph.Nodes.ZeroTouch
{
    public class UnresolvedFunctionException : Exception
    {
        public UnresolvedFunctionException(string functionName)
            : base("Cannot find function: " + functionName)
        {
            FunctionName = functionName;
        }

        public string FunctionName { get; private set; }
    }
}