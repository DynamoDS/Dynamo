using ProtoCore.DSASM;

namespace ProtoCore
{
    public class Executive
	{
        protected RuntimeCore runtimeCore; 

		public Executive (RuntimeCore runtimeCore)
		{
            System.Diagnostics.Debug.Assert(runtimeCore != null);
            this.runtimeCore = runtimeCore;
           
		}

        public ProtoCore.DSASM.Executive CurrentDSASMExec { get; set; }

        public StackValue Execute(int codeblock, int entry, bool fepRun = false, System.Collections.Generic.List<Instruction> breakpoints = null)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(runtimeCore, fepRun);
            CurrentDSASMExec = interpreter.runtime;
            return interpreter.Run(codeblock, entry, CurrentDSASMExec.executingLanguage, breakpoints);
        }

	}
}

 