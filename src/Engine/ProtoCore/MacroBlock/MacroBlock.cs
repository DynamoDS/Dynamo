using System;
using System.Text;
using System.Collections.Generic;
using ProtoCore.Utils;
using ProtoCore.DSASM;
using ProtoCore.AST.AssociativeAST;


namespace ProtoCore.CompileTime
{
    public class MacroBlock
    {
        public int UID { get; set; }
        public List<BinaryExpressionNode> AstList { get; set; }

        public MacroBlock()
        {
            AstList = new List<BinaryExpressionNode>();
        }
    }
}

namespace ProtoCore.Runtime 
{
    public class MacroBlock
    {
        public enum ExecuteState
        {
            Ready,
            Executing,
            Paused
        }

        public MacroBlock()
        {
            State = ExecuteState.Ready;
            GraphNodeList = new List<AssociativeGraph.GraphNode>();
        }

        public int UID { get; set; }
        public ExecuteState State { get; set; }
        public List<AssociativeGraph.GraphNode> GraphNodeList { get; set; }
    }
}

namespace ProtoCore
{
    /// <summary>
    /// Generates macroblocks from a list of ASTs
    /// </summary>
    public class MacroBlockGenerator
    {
        ProtoCore.Core core = null;
        private List<AssociativeNode> cachedASTList = null;
        private List<ProtoCore.CompileTime.MacroBlock> cachedMacroBlocks = null;

        public MacroBlockGenerator(ProtoCore.Core core)
        {
            this.core = core;
            cachedASTList = new List<AssociativeNode>();
            cachedMacroBlocks = new List<CompileTime.MacroBlock>(); 
        }

        public List<ProtoCore.CompileTime.MacroBlock> GenerateMacroBlocks(List<AssociativeNode> astList)
        {
            // For now, generate 1 macroblock
            const int generatedMacroBlocks = 1;

            // Initialize macroblocks
            for (int n = 0; n < generatedMacroBlocks; ++n)
            {
                cachedMacroBlocks.Add(new ProtoCore.CompileTime.MacroBlock());
            }


            // Populate the macroblocks

            // --------------------- Begin ---------------------
            foreach (BinaryExpressionNode bnode in astList)
            {
                Validity.Assert(cachedMacroBlocks[bnode.MacroBlockID] != null);
                cachedMacroBlocks[bnode.MacroBlockID].AstList.Add(bnode);
            }
            // --------------------- End ---------------------


            // Generate macroblocks for compilation
            core.MacroBlockList = cachedMacroBlocks;

            // Allocate space for runtime macroblock
            int allocateSize = core.MacroBlockList.Count - core.RuntimeMacroBlockList.Count;
            for (int n = 0; n < allocateSize; ++n)
            {
                core.RuntimeMacroBlockList.Add(new ProtoCore.Runtime.MacroBlock());
            }

            return cachedMacroBlocks;
        }

        public void GenerateMacroBlockIDForAST(List<AssociativeNode> astList)
        {
            foreach (BinaryExpressionNode bnode in astList)
            {
                bnode.MacroBlockID = 0;
            }
        }
    }
}