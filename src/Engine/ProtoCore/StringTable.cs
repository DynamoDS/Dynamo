using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    public class StringTable
    {
        public List<string> stringTable { get; set; }
        public StringTable()
        {
            stringTable = new List<string>();
        }
    }

    public class DynamicVariableTable
    {
        public List<DyanmicVariableNode> variableTable { get; set; }
        public DynamicVariableTable()
        {
            variableTable = new List<DyanmicVariableNode>();
        }
    }

    public struct DyanmicVariableNode
    {
        public string variableName;
        public int procIndex;
        public int classIndex;
        //public int codeBlockId;
        //public int symbolIndex;
        public DyanmicVariableNode(string varName, int procIndex = ProtoCore.DSASM.Constants.kGlobalScope, int classIndex = ProtoCore.DSASM.Constants.kGlobalScope)
        {
            variableName = varName;
            this.procIndex = procIndex;
            this.classIndex = classIndex;
            //classIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            //codeBlockId = DSASM.Constants.kInvalidIndex;
            //symbolIndex = DSASM.Constants.kInvalidIndex;
        }
    }
}
