using System.Collections.Generic;

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

        public Dictionary<string, FunctionGroup> FunctionList { get; private set; }
        public Dictionary<int, Dictionary<string, FunctionGroup> >GlobalFuncTable { get; private set; }
    }
}
