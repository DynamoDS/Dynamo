
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoAssociative
{

    public class Compiler : ProtoCore.Compiler
    {

        public Compiler(Core core)
            : base(core)
        {
        }

        public override bool Compile(out int blockId, ProtoCore.DSASM.CodeBlock parentBlock, ProtoCore.LanguageCodeBlock langBlock, ProtoCore.CompileTime.Context callContext, ProtoCore.DebugServices.EventSink sink = null, ProtoCore.AST.Node codeBlockNode = null, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            Validity.Assert(langBlock != null);
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

            bool buildSucceeded = false;

            try
            {
                ProtoCore.CodeGen oldCodegen = core.assocCodegen;

                if (ProtoCore.DSASM.InterpreterMode.Normal == core.Options.RunMode)
                {
                    if ((core.IsParsingPreloadedAssembly || core.IsParsingCodeBlockNode) && parentBlock == null)
                    {
                        if (core.CodeBlockList.Count == 0)
                        {
                            core.assocCodegen = new ProtoAssociative.CodeGen(core, callContext, parentBlock);
                        }
                        else
                        {
                            // We reuse the existing toplevel CodeBlockList's for the procedureTable's 
                            // by calling this overloaded constructor - pratapa
                            core.assocCodegen = new ProtoAssociative.CodeGen(core);
                        }
                    }
                    else
                        core.assocCodegen = new ProtoAssociative.CodeGen(core, callContext, parentBlock);
                }

                if (null != core.AssocNode)
                {
                    ProtoCore.AST.AssociativeAST.CodeBlockNode cnode = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
                    cnode.Body.Add(core.AssocNode as ProtoCore.AST.AssociativeAST.AssociativeNode);

                    core.assocCodegen.context = callContext;

                    blockId = core.assocCodegen.Emit((cnode as ProtoCore.AST.AssociativeAST.CodeBlockNode), graphNode);
                }
                else
                {
                    if (codeBlockNode == null)
                    {
                        var parseResult = ParserUtils.ParseWithCore(langBlock.Code, core);
                        // TODO Jun: Set this flag inside a persistent object
                        core.builtInsLoaded = true;
                        codeBlockNode = parseResult.CodeBlockNode;
                    }
                    else
                    {
                        //if codeBlockNode is not null, Compile has been called from DfsTraverse. No parsing is needed. 
                        if (!core.builtInsLoaded)
                        {
                            // Load the built-in methods manually
                            CoreUtils.InsertPredefinedAndBuiltinMethods(core, codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode);
                            core.builtInsLoaded = true;
                        }
                    }

                    core.assocCodegen.context = callContext;

                    //Temporarily change the code block for code gen to the current block, in the case it is an imperative block
                    //CodeGen for ProtoImperative is modified to passing in the core object.
                    ProtoCore.DSASM.CodeBlock oldCodeBlock = core.assocCodegen.codeBlock;
                    if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.Expression)
                    {
                        int tempBlockId = callContext.CurrentBlockId;
                        ProtoCore.DSASM.CodeBlock tempCodeBlock = ProtoCore.Utils.CoreUtils.GetCodeBlock(core.CodeBlockList, tempBlockId);
                        while (null != tempCodeBlock && tempCodeBlock.blockType != ProtoCore.DSASM.CodeBlockType.Language)
                        {
                            tempCodeBlock = tempCodeBlock.parent;
                        }
                        core.assocCodegen.codeBlock = tempCodeBlock;
                    }
                    core.assocCodegen.codeBlock.EventSink = sink;
                    if (core.BuildStatus.ErrorCount == 0) //if there is syntax error, no build needed
                    {
                        blockId = core.assocCodegen.Emit((codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode), graphNode);
                    }
                    if (core.Options.RunMode == ProtoCore.DSASM.InterpreterMode.Expression)
                    {
                        blockId = core.assocCodegen.codeBlock.codeBlockId;
                        //Restore the code block.
                        core.assocCodegen.codeBlock = oldCodeBlock;
                    }
                }

                // @keyu: we have to restore asscoCodegen here. It may be 
                // reused later on. Suppose for an inline expression 
                // "x = 1 == 2 ? 3 : 4", we dynamically create assocCodegen
                // to compile true and false expression in this inline 
                // expression, and if we don't restore assocCodegen, the pc
                // is totally messed up.
                //
                // But if directly replace with old assocCodegen, will it
                // replace some other useful information? Need to revisit it.
                //
                // Also refer to defect IDE-2120.
                if (oldCodegen != null && core.assocCodegen != oldCodegen)
                {
                    core.assocCodegen = oldCodegen;
                }
            }
            catch (ProtoCore.BuildHaltException)
            {
            }

            buildSucceeded = core.BuildStatus.BuildSucceeded;

            return buildSucceeded;

        }
    }
}

