
using System.Collections.Generic;

namespace ProtoCore
{
    namespace Runtime
    {
        public class Context
        {
            public bool IsReplicating { get; set; }
            public bool IsImplicitCall { get; set; }
            
#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
            #region __ARRAY_UPDATE
            public StackValue ArrayPointer { get; set; }
            public List<List<int>> IndicesIntoArgMap { get; set; }
            #endregion
#endif

            public Dictionary<string, bool> execFlagList { get; set; }

            public Context()
            {
                IsReplicating = false;
                IsImplicitCall = false;
                execFlagList = null;

#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
                IndicesIntoArgMap = new List<List<int>>();
                ArrayPointer = ProtoCore.DSASM.StackValue.Null;
#endif
            }
        }
    }
}