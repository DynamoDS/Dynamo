
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoImperative
{
	public class Executive : ProtoCore.Executive
	{
		public Executive(ProtoCore.Core core) : base(core)
		{
		}

        public override StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, ProtoCore.DebugServices.EventSink sink)
        {
            if (!core.Options.CompileToLib)
            {
                ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
                CurrentDSASMExec = interpreter.runtime;
                var sv = interpreter.Run(codeblock, entry, ProtoCore.Language.kImperative);
                return sv;
            }
            else
            {
                return StackValue.Null;
            }
        }


        public override StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, List<Instruction> breakpoints, ProtoCore.DebugServices.EventSink sink, bool fepRun = false)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
            CurrentDSASMExec = interpreter.runtime;
            return interpreter.Run(breakpoints, codeblock, entry, ProtoCore.Language.kImperative);
        }

	}
}

