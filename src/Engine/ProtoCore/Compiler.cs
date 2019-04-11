namespace ProtoCore
{
    public abstract class Compiler
	{
        protected Core core;

        public Compiler(Core core)
		{
            System.Diagnostics.Debug.Assert(core != null);
            this.core = core;
           
		}
        public abstract bool Compile(out int blockId, ProtoCore.DSASM.CodeBlock parentBlock, ProtoCore.LanguageCodeBlock codeblock, ProtoCore.CompileTime.Context callContext, ProtoCore.DebugServices.EventSink sink = null, ProtoCore.AST.Node codeBlockNode = null, ProtoCore.AssociativeGraph.GraphNode graphNode = null);
	}
}

 