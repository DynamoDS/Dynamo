using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;

namespace ProtoCore.Utils
{
    public static class GCUtils
    {
        public static void GCRetain(StackValue sv, Core core)
        {
            if (core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
            {
                core.Heap.IncRefCount(sv);
            }
        }

        public static void GCRelease(StackValue sv, Core core)
        {
            if (null != core.CurrentExecutive.CurrentDSASMExec)
            {
                core.CurrentExecutive.CurrentDSASMExec.GCRelease(sv);
            }
        }
    }
}
