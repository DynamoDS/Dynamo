using ProtoCore.Utils;

namespace ProtoImperative
{
    public class Compiler : ProtoCore.Compiler
	{
        public Compiler(ProtoCore.Core core)
            : base(core)
		{
		}

        public override bool Compile(out int blockId, ProtoCore.DSASM.CodeBlock parentBlock, ProtoCore.LanguageCodeBlock langBlock, ProtoCore.CompileTime.Context callContext, ProtoCore.DebugServices.EventSink sink, ProtoCore.AST.Node codeBlockNode, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            Validity.Assert(langBlock != null);
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

            bool buildSucceeded = false;
            try
            {
                ProtoImperative.CodeGen codegen = new ProtoImperative.CodeGen(core, callContext, parentBlock);

                codegen.context = callContext;
                codegen.codeBlock.EventSink = sink;
                blockId = codegen.Emit(codeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode, graphNode);
            }
            catch (ProtoCore.BuildHaltException)
            {
            }

            buildSucceeded = core.BuildStatus.BuildSucceeded;
            return buildSucceeded;
        }
    }
}

