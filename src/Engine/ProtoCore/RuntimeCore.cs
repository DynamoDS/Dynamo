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
        public RuntimeCore()
        {
        }

        public void SetProperties(Options runtimeOptions, Executable executable, DebugProperties debugProps = null, ProtoCore.Runtime.Context context = null)
        {
            this.context = context;
            this.DSExecutable = executable;
            this.RuntimeOptions = runtimeOptions;
            this.DebugProps = debugProps;
        }

        public Executable DSExecutable { get; private set; }
        public Options RuntimeOptions { get; private set; }
        public RuntimeStatus RuntimeStatus { get; set; }

        public RuntimeMemory RuntimeMemory { get; set; }
        public ProtoCore.Runtime.Context context { get; set; }
        public InterpreterMode ExecMode { get; set; }

        /// <summary>
        /// RuntimeExpressionUID is used by the associative engine at runtime to determine the current expression ID being executed
        /// </summary>
        public int RuntimeExpressionUID = 0;

        private Executive executiveRuntime;

#region DEBUGGER_PROPERTIES
        public DebugProperties DebugProps { get; set; }
        public List<Instruction> Breakpoints { get; set; }
#endregion 

    }
}
