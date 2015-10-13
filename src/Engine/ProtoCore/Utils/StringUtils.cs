﻿using ProtoCore.DSASM;

namespace ProtoCore.Utils
{
    public static class StringUtils
    {
        public static int CompareString(StackValue s1, StackValue s2, RuntimeCore runtimeCore)
        {
            if (!s1.IsString || !s2.IsString)
                return Constants.kInvalidIndex;

            if (s1.Equals(s2))
                return 0;

            string str1 = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(s1).Value;
            string str2 = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(s2).Value;
            return string.Compare(str1, str2);
        }

        public static string GetStringValue(StackValue sv, RuntimeCore runtimeCore)
        {
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new DSASM.Mirror.ExecutionMirror(new ProtoCore.DSASM.Executive(runtimeCore), runtimeCore);
            return mirror.GetStringValue(sv, runtimeCore.RuntimeMemory.Heap, 0, true);
        }

        public static StackValue ConvertToString(StackValue sv, RuntimeCore runtimeCore, ProtoCore.Runtime.RuntimeMemory rmem)
        {
            StackValue returnSV;
            //TODO: Change Execution mirror class to have static methods, so that an instance does not have to be created
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new DSASM.Mirror.ExecutionMirror(new ProtoCore.DSASM.Executive(runtimeCore), runtimeCore);
            returnSV = ProtoCore.DSASM.StackValue.BuildString(mirror.GetStringValue(sv, runtimeCore.RuntimeMemory.Heap, 0, true), runtimeCore.RuntimeMemory.Heap);
            return returnSV;
        }

        public static StackValue ConcatString(StackValue op1, StackValue op2, RuntimeCore runtimeCore)
        {
            var v1 = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(op1).Value;
            var v2 = runtimeCore.RuntimeMemory.Heap.ToHeapObject<DSString>(op2).Value;
            return StackValue.BuildString(v1 + v2, runtimeCore.RuntimeMemory.Heap);
        }
    }
}
