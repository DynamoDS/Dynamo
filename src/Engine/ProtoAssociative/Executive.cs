
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoAssociative
{

	public class Executive : ProtoCore.Executive
	{

		public Executive (Core core) : base(core)
		{
		}

        public override StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, ProtoCore.DebugServices.EventSink sink)
        {
            if (!core.Options.CompileToLib)
            {
                ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
                CurrentDSASMExec = interpreter.runtime;
                StackValue sv = interpreter.Run(codeblock, entry, Language.kAssociative);
                return sv;
            }
            else
            {
                return StackValue.Null;
            }
        }

        public override StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, System.Collections.Generic.List<Instruction> breakpoints, ProtoCore.DebugServices.EventSink sink = null, bool fepRun = false)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core, fepRun);
            CurrentDSASMExec = interpreter.runtime;
            StackValue sv = interpreter.Run(breakpoints, codeblock, entry, Language.kAssociative);
            return sv;
        }

	}
}

