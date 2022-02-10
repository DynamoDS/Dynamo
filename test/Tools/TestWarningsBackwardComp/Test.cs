using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.Runtime;
using System;
using System.Collections.Generic;

namespace TestWarningsBackwardComp
{
    public class Test
    {
        // This assembly references ProtoCore @ Dynamo 2.13
        public static void TestWarningBackwardsCompatibility()
        {
            RuntimeCore runtimeCore = new RuntimeCore(new Heap(), new Options());
            RuntimeStatus st = new RuntimeStatus(runtimeCore);
            st.LogWarning(WarningID.Default, "Test", "Test", 0, 0);


            IEnumerable<WarningEntry> warnings = st.Warnings;
            foreach (var item in warnings)
            {
                if (item.ID != WarningID.Default || item.Message != "Test")
                {
                    throw new Exception("Warning not found");
                }
            }
        } 
    }
}
