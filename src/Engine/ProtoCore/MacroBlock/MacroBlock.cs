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
        public List<AssociativeNode> AstList { get; set; }

        public MacroBlock()
        {
            AstList = new List<AssociativeNode>();
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
            const int numMacroBlocks = 1;

            // Initialize macroblocks
            int allocateSize = numMacroBlocks - cachedMacroBlocks.Count;
            for (int n = 0; n < allocateSize; ++n)
            {
                cachedMacroBlocks.Add(new ProtoCore.CompileTime.MacroBlock());
            }


            // -------------------------------------------------
            // --------------------- Begin ---------------------
            // -------------------------------------------------

            // Generate the macroblocks
            // Replace this logic with the real macroblock generator
            List<List<AssociativeNode>> generatedMacroBlockList = new List<List<AssociativeNode>>();
            for (int n = 0; n < numMacroBlocks; ++n)
            {
                List<AssociativeNode> singleMacroBlock = new List<AssociativeNode>(astList);
                generatedMacroBlockList.Add(singleMacroBlock);
            }

            // Cache the macroblocks 
            for (int mBlockID = 0; mBlockID < numMacroBlocks; ++mBlockID)
            {
                List<AssociativeNode> macroBlock = generatedMacroBlockList[mBlockID];
                foreach (AssociativeNode node in macroBlock)
                {
                    Validity.Assert(cachedMacroBlocks[mBlockID] != null);
                    cachedMacroBlocks[mBlockID].AstList.Add(node);
                }
            }

            core.MacroBlockList = cachedMacroBlocks;

            // -------------------------------------------------
            // --------------------- End -----------------------
            // -------------------------------------------------


            // Allocate space for runtime macroblock
            for (int n = 0; n < allocateSize; ++n)
            {
                core.RuntimeMacroBlockList.Add(new ProtoCore.Runtime.MacroBlock());
            }

            return cachedMacroBlocks;
        }

        public void GenerateMacroBlockIDForBinaryAST(List<AssociativeNode> astList)
        {
            foreach (AssociativeNode node in astList)
            {
                BinaryExpressionNode bnode = node as BinaryExpressionNode;
                if (bnode != null)
                {
                    bnode.MacroBlockID = 0;
                }
            }
        }
    }
}