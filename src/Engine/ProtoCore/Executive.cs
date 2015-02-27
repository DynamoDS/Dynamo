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

        public StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, bool fepRun = false, System.Collections.Generic.List<Instruction> breakpoints = null)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core, fepRun);
            CurrentDSASMExec = interpreter.runtime;
            return interpreter.Run(codeblock, entry, CurrentDSASMExec.executingLanguage, breakpoints);
        }

	}
}

 