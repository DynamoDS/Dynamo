
/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoFFI;

namespace ProtoCore.Lang
{
    
    public class CSFFIActivationRecord
    {
        public string ModuleType;
        public string ModuleName;
        public bool IsDNI;
        public string FunctionName;
        public List<ProtoCore.Type> ParameterTypes;
        public ProtoCore.Type ReturnType;
        public JILActivationRecord JILRecord;
    }

    [Obsolete]
    public class CSFFIFunctionEndPoint : FunctionEndPoint
    {
        public static Dictionary<string, FFIHandler> FFIHandlers = new Dictionary<string,FFIHandler>();
        public CSFFIActivationRecord ActivationRecord;
        private FFIFunctionPointer mFunctionPointer;

        public CSFFIFunctionEndPoint(CSFFIActivationRecord record)
        {
            ActivationRecord = record;
        }
        public CSFFIFunctionEndPoint()
        {
            ActivationRecord = new CSFFIActivationRecord();
        }
        
        //Pending removal
        //[Obsolete]
        //public override ProtoCore.Lang.Obj Execute(Context c, List<ProtoCore.Lang.Obj> formalParameters, List<StackValue> activation, Core core)
        //{   
        //    ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core, true);

        //    ActivationRecord.JILRecord.globs = core.executable.runtimeSymbols[core.runningBlock].GetGlobalSize();

        //    // Params
        //    formalParameters.Reverse();
        //    for (int i = 0; i < formalParameters.Count; i++)
        //    {
        //        interpreter.Push(formalParameters[i].DsasmValue);
        //    }

        //    // Return addr
        //    interpreter.Push(ActivationRecord.JILRecord.retAddr);

        //    // Locals
        //    for (int n = 0; n < ActivationRecord.JILRecord.locals; ++n)
        //    {
        //        interpreter.Push(0);
        //    }

        //    // Members
        //    if (ProtoCore.DSASM.Constants.kInvalidIndex != ActivationRecord.JILRecord.classIndex)
        //    {
        //        DSASM.ClassTable classtable = core.executable.classTable;
        //        Validity.Assert(null != classtable);
        //        int members = classtable.list[ActivationRecord.JILRecord.classIndex].symbols.symbolList.Count;
        //        for (int n = 0; n < members; ++n)
        //        {
        //            interpreter.Push(0);
        //        }
        //    }

        //    interpreter.Push(ActivationRecord.JILRecord.funcIndex);
        //    interpreter.Push(ActivationRecord.JILRecord.classIndex);


        //    // Do the FFI call here
        //    object returnedData = mFunctionPointer.Execute(c, interpreter);

        //    // Build the result
        //    StackValue sv = StackValue.BuildInt((int)returnedData);

        //    return DSASM.Mirror.ExecutionMirror.Unpack(sv, core.heap, core);
            
        //}

        //[Obsolete]
        //public override bool DoesPredicateMatch(Context c, List<ProtoCore.Lang.Obj> formalParameters, List<ReplicationInstruction> ReplicationInstructions)
        //{
        //    //@TODO: FIXME
        //    return true;
        //    //throw new NotImplementedException();
        //}

        public override bool DoesPredicateMatch(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, List<ReplicationInstruction> replicationInstructions)
        {
            throw new NotImplementedException();
        }

        public override StackValue Execute(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, ProtoCore.DSASM.StackFrame stackFrame, Core core)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core, true);

            ActivationRecord.JILRecord.globs = core.DSExecutable.runtimeSymbols[core.RunningBlock].GetGlobalSize();

            // Params
            formalParameters.Reverse();
            for (int i = 0; i < formalParameters.Count; i++)
            {
                interpreter.Push(formalParameters[i]);
            }

            // Return addr
            interpreter.Push(ActivationRecord.JILRecord.retAddr);

            // Locals
            for (int n = 0; n < ActivationRecord.JILRecord.locals; ++n)
            {
                interpreter.Push(0);
            }

            // Members
            if (ProtoCore.DSASM.Constants.kInvalidIndex != ActivationRecord.JILRecord.classIndex)
            {
                DSASM.ClassTable classtable = core.DSExecutable.classTable;
                Validity.Assert(null != classtable);
                int members = classtable.list[ActivationRecord.JILRecord.classIndex].symbols.symbolList.Count;
                for (int n = 0; n < members; ++n)
                {
                    interpreter.Push(0);
                }
            }

            interpreter.Push(ActivationRecord.JILRecord.funcIndex);
            interpreter.Push(ActivationRecord.JILRecord.classIndex);


            // Do the FFI call here
            object returnedData = mFunctionPointer.Execute(c, interpreter);

            // Build the result
            StackValue sv = StackValue.BuildInt((int)returnedData);

            return sv; //DSASM.Mirror.ExecutionMirror.Unpack(sv, core.heap, core);

        }
    }
}*/
