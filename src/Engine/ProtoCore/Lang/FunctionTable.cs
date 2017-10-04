
using System.Collections.Generic;
using ProtoCore.Utils;

namespace ProtoCore.Lang
{
    public class FunctionTable
    {

        //@FIXME (luke): This shouldn't be public, the whole thing needs better refactoring to support
        //DI test uses
        public FunctionTable()
        {
            FunctionList = new Dictionary<string, FunctionGroup>();
            GlobalFuncTable = new Dictionary<int, Dictionary<string, FunctionGroup>>();
        }

        /// <summary>
        /// Initialize the global function table entry for a class
        /// The argument is the index of the class of functions to initialize + 1, which is the index expected at callsite
        /// </summary>
        /// <param name="classIndexAtCallsite"></param>
        public void InitGlobalFunctionEntry(int classIndexAtCallsite)
        {
            Validity.Assert(null != GlobalFuncTable);
            Validity.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != classIndexAtCallsite);
            if (!GlobalFuncTable.ContainsKey(classIndexAtCallsite))
            {
                Dictionary<string, FunctionGroup> funcList = new Dictionary<string, FunctionGroup>();
                GlobalFuncTable.Add(classIndexAtCallsite, funcList);
            }
        }

        public Dictionary<string, FunctionGroup> FunctionList { get; private set; }
        public Dictionary<int, Dictionary<string, FunctionGroup> >GlobalFuncTable { get; private set; }
    }
}
