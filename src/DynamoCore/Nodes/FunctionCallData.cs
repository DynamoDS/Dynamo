using Dynamo.DSEngine;

namespace Dynamo.Nodes
{
    public class FunctionCallData
    {
        public IFunctionDescriptor Definition { get; private set; }

        public FunctionCallData(IFunctionDescriptor def)
        {
            Definition = def;
        }
    }
}