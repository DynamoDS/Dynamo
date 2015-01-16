using System;

namespace Dynamo.Nodes
{
    public class UnresolvedFunctionException : Exception
    {
        public UnresolvedFunctionException(string functionName)
            : base(/*NXLT*/"Cannot find function: " + functionName)
        {
            FunctionName = functionName;
        }

        public string FunctionName { get; private set; }
    }
}