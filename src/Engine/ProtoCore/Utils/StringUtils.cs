using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Utils;

namespace ProtoCore.Utils
{
    public static class StringUtils
    {
        public static int CompareString(StackValue s1, StackValue s2, Core core)
        {
            if (!StackUtils.IsString(s1) || !StackUtils.IsString(s2))
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            HeapElement he1 = ArrayUtils.GetHeapElement(s1, core);
            HeapElement he2 = ArrayUtils.GetHeapElement(s2, core);

            int len1 = he1.VisibleSize;
            int len2 = he2.VisibleSize;

            int len = len1 > len2 ? len2 : len1;
            int i = 0;
            for (; i < len; ++i)
            {
                if (he1.Stack[i].opdata != he2.Stack[i].opdata)
                {
                    return (he1.Stack[i].opdata > he2.Stack[i].opdata) ? 1 : -1;
                }
            }

            if (len1 > len2)
                return 1;
            else if (len1 == len2)
                return 0; 
            else
                return -1;
        }

        public static string GetStringValue(StackValue sv, Core core)
        {
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new DSASM.Mirror.ExecutionMirror(new ProtoCore.DSASM.Executive(core), core);
            return mirror.GetStringValue(sv, core.Heap, 0, true);
        }

        public static StackValue ConvertToString(StackValue sv, Core core, ProtoCore.Runtime.RuntimeMemory rmem)
        {
            StackValue returnSV;
            //TODO: Change Execution mirror class to have static methods, so that an instance does not have to be created
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new DSASM.Mirror.ExecutionMirror(new ProtoCore.DSASM.Executive(core), core);
            returnSV = ProtoCore.DSASM.StackValue.BuildString(mirror.GetStringValue(sv, core.Heap,0, true),core.Heap);
            return returnSV;
        }

        public static StackValue ConcatString(StackValue op1, StackValue op2, ProtoCore.Runtime.RuntimeMemory rmem)
        {
            StackValue[] v1 = (AddressType.String == op1.optype) ? rmem.GetArrayElements(op1) : new StackValue[] { op1 };
            StackValue[] v2 = (AddressType.String == op2.optype) ? rmem.GetArrayElements(op2) : new StackValue[] { op2 };
            StackValue tmp = rmem.BuildArray(v1.Concat(v2).ToArray());
            return StackValue.BuildString(tmp.opdata);
        }
    }
}
