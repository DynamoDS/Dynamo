using System.Collections.Generic;
using ProtoCore.DSASM;

namespace ProtoCore
{
	public class Executive
	{
        protected Core core; 

		public Executive (Core core)
		{
            System.Diagnostics.Debug.Assert(core != null);
            this.core = core;
           
		}

        public ProtoCore.DSASM.Executive CurrentDSASMExec { get; set; }

        public StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, ProtoCore.DebugServices.EventSink sink)
        {
            if (!core.Options.CompileToLib)
            {
                ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
                CurrentDSASMExec = interpreter.runtime;
                StackValue sv = interpreter.Run(codeblock, entry, CurrentDSASMExec.executingLanguage);
                return sv;
            }
            else
            {
                return StackValue.Null;
            }
        }

        public StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, System.Collections.Generic.List<Instruction> breakpoints, ProtoCore.DebugServices.EventSink sink = null, bool fepRun = false)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core, fepRun);
            CurrentDSASMExec = interpreter.runtime;
            StackValue sv = interpreter.Run(breakpoints, codeblock, entry, CurrentDSASMExec.executingLanguage);
            return sv;
        }

	}
}

 