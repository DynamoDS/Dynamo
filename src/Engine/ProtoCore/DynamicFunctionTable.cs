using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    public class DynamicFunctionTable
    {
        public List<DynamicFunctionNode> functionTable { get; set; }
        public DynamicFunctionTable()
        {
            functionTable = new List<DynamicFunctionNode>();
        }
    }

    public class DynamicFunctionNode
    {
        public string functionName;
        public List<ProtoCore.Type> argList;
        public int classIndex;
        public int procedureIndex;
        public int pc;
        public DynamicFunctionNode(string pFunctionName, List<ProtoCore.Type> pArgList, int pClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            pc = ProtoCore.DSASM.Constants.kInvalidIndex;
            procedureIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            functionName = pFunctionName;
            argList = pArgList;
            classIndex = pClassIndex;
        }
    }
}
