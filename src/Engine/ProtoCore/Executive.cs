using System.Collections.Generic;
using ProtoCore.DSASM;

namespace ProtoCore
{
	public abstract class Executive
	{
        protected Core core; 

		public Executive (Core core)
		{
            System.Diagnostics.Debug.Assert(core != null);
            this.core = core;
           
		}

        public ProtoCore.DSASM.Executive CurrentDSASMExec { get; set; }
        public abstract StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, ProtoCore.DebugServices.EventSink sink = null);
        public abstract StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, List<Instruction> breakpoints, ProtoCore.DebugServices.EventSink sink = null, bool fepRun = false);

	}
}

 