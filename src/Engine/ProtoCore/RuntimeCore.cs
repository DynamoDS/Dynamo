using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using ProtoCore.AssociativeGraph;
using ProtoCore.AssociativeEngine;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.BuildData;
using ProtoCore.CodeModel;
using ProtoCore.DebugServices;
using ProtoCore.DSASM;
using ProtoCore.Lang;
using ProtoCore.Lang.Replication;
using ProtoCore.Runtime;
using ProtoCore.Utils;
using ProtoFFI;

using StackFrame = ProtoCore.DSASM.StackFrame;

namespace ProtoCore
{
    /// <summary>
    /// RuntimeCore is an object that is instantiated once across the lifecycle of the runtime
    /// This is the entry point of the runtime VM and its input is a DS Executable format. 
    /// There will only be one instance of RuntimeCore regardless of how many times instances of a DSASM.Executive (runtime VM) is instantiated.
    /// Its properties will be persistent and accessible across all instances of a DSASM.Executive
    /// </summary>
    public class RuntimeCore
    {
        public RuntimeCore(Options runtimeOptions, Executable executable, ProtoCore.Runtime.Context context)
        {
            this.context = context;
            this.DSExecutable = executable;
            this.RuntimeOptions = runtimeOptions;
            
            DebugProps = new DebugProperties();
        }

        public StackValue Bounce(
            int exeblock, 
            int entry, 
            Context context, 
            StackFrame stackFrame, 
            int locals = 0, 
            EventSink sink = null
            )
        {
            if (stackFrame != null)
            {
                StackValue svThisPtr = stackFrame.ThisPtr;
                int ci = stackFrame.ClassScope;
                int fi = stackFrame.FunctionScope;
                int returnAddr = stackFrame.ReturnPC;
                int blockDecl = stackFrame.FunctionBlock;
                int blockCaller = stackFrame.FunctionCallerBlock;
                StackFrameType callerFrameType = stackFrame.CallerStackFrameType;
                StackFrameType frameType = stackFrame.StackFrameType;
                Validity.Assert(frameType == StackFrameType.kTypeLanguage);

                int depth = stackFrame.Depth;
                int framePointer = stackFrame.FramePointer;
                List<StackValue> registers = stackFrame.GetRegisters();

                RuntimeMemory.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerFrameType, frameType, depth + 1, framePointer, registers, locals, 0);
            }

            StackValue sv = executiveRuntime.Execute(exeblock, entry, context, sink);
            return sv;
        }

        public StackValue Bounce(
            int exeblock, 
            int entry, 
            Context context, 
            List<Instruction> breakpoints, 
            StackFrame stackFrame, 
            int locals = 0,
            DSASM.Executive exec = null,
            EventSink sink = null, 
            bool fepRun = false
            )
        {
            if (stackFrame != null)
            {
                StackValue svThisPtr = stackFrame.ThisPtr;
                int ci = stackFrame.ClassScope;
                int fi = stackFrame.FunctionScope;
                int returnAddr = stackFrame.ReturnPC;
                int blockDecl = stackFrame.FunctionBlock;
                int blockCaller = stackFrame.FunctionCallerBlock;
                StackFrameType callerFrameType = stackFrame.CallerStackFrameType;
                StackFrameType frameType = stackFrame.StackFrameType;
                Validity.Assert(frameType == StackFrameType.kTypeLanguage);
                int depth = stackFrame.Depth;
                int framePointer = stackFrame.FramePointer;
                List<StackValue> registers = stackFrame.GetRegisters();

                DebugProps.SetUpBounce(exec, blockCaller, returnAddr);

                RuntimeMemory.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerFrameType, frameType, depth + 1, framePointer, registers, locals, 0);
            }

            StackValue sv = executiveRuntime.Execute(exeblock, entry, context, breakpoints, sink, fepRun);
            return sv;
        }

        public Executable DSExecutable { get; private set; }
        public Options RuntimeOptions { get; private set; }

        public RuntimeMemory RuntimeMemory { get; set; }
        private Options runtimeOptions;
        private ProtoCore.Runtime.Context context;
        private Executive executiveRuntime;

#region DEBUGGER_PROPERTIES
        public DebugProperties DebugProps { get; set; }
        public List<Instruction> Breakpoints { get; set; }
#endregion 

    }
}
