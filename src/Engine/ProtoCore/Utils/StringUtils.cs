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
            if (!s1.IsString || !s2.IsString)
                return Constants.kInvalidIndex;

            if (s1.Equals(s2))
                return 0;

            RuntimeCore runtimeCore = core.__TempCoreHostForRefactoring;

            string str1 = runtimeCore.RuntimeMemory.Heap.GetString(s1);
            string str2 = runtimeCore.RuntimeMemory.Heap.GetString(s2);
            return string.Compare(str1, str2);
        }

        public static string GetStringValue(StackValue sv, Core core)
        {
            RuntimeCore runtimeCore = core.__TempCoreHostForRefactoring;
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new DSASM.Mirror.ExecutionMirror(new ProtoCore.DSASM.Executive(core), core);
            return mirror.GetStringValue(sv, runtimeCore.RuntimeMemory.Heap, 0, true);
        }

        public static StackValue ConvertToString(StackValue sv, Core core, ProtoCore.Runtime.RuntimeMemory rmem)
        {
            RuntimeCore runtimeCore = core.__TempCoreHostForRefactoring;
            StackValue returnSV;
            //TODO: Change Execution mirror class to have static methods, so that an instance does not have to be created
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new DSASM.Mirror.ExecutionMirror(new ProtoCore.DSASM.Executive(core), core);
            returnSV = ProtoCore.DSASM.StackValue.BuildString(mirror.GetStringValue(sv, runtimeCore.RuntimeMemory.Heap, 0, true), runtimeCore.RuntimeMemory.Heap);
            return returnSV;
        }

        public static StackValue ConcatString(StackValue op1, StackValue op2, ProtoCore.Core core)
        {
            RuntimeCore runtimeCore = core.__TempCoreHostForRefactoring;
            var v1 = runtimeCore.RuntimeMemory.Heap.GetString(op1);
            var v2 = runtimeCore.RuntimeMemory.Heap.GetString(op2);
            return StackValue.BuildString(v1 + v2, runtimeCore.RuntimeMemory.Heap);
        }
    }
}
