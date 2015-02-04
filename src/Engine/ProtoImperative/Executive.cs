
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

        public override bool Compile(out int blockId, ProtoCore.DSASM.CodeBlock parentBlock, ProtoCore.LanguageCodeBlock langBlock, ProtoCore.CompileTime.Context callContext, ProtoCore.DebugServices.EventSink sink, ProtoCore.AST.Node codeBlockNode, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            Validity.Assert(langBlock != null);
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

            bool buildSucceeded = false;
            bool isLanguageSignValid = isLanguageSignValid = core.Langverify.Verify(langBlock);

            if (isLanguageSignValid)
            {
                try
                {
                    ProtoImperative.CodeGen codegen = new ProtoImperative.CodeGen(core, callContext, parentBlock);

                    codegen.context = callContext;
                    codegen.codeBlock.EventSink = sink;
                    blockId = codegen.Emit(codeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode, graphNode);
                }
                catch (ProtoCore.BuildHaltException)
                {
#if DEBUG
                    //core.BuildStatus.LogSemanticError(e.errorMsg);
#endif
                }

                buildSucceeded = core.BuildStatus.BuildSucceeded;
            }
            return buildSucceeded;
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

