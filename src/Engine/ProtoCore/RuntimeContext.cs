
using System.Collections.Generic;

namespace ProtoCore
{
    namespace Runtime
    {
        public class Context
        {
            public bool IsReplicating { get; set; }
            public bool IsImplicitCall { get; set; }
            public Dictionary<string, bool> execFlagList { get; set; }

            public Context()
            {
                IsReplicating = false;
                IsImplicitCall = false;
                execFlagList = null;
            }
        }
    }
}