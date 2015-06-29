

#define __GENERATE_TEST_MACROBLOCKS

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

        public int UID { get; set; }
        public ExecuteState State { get; set; }
        public List<AssociativeGraph.GraphNode> GraphNodeList { get; set; }

        public MacroBlock()
        {
            State = ExecuteState.Ready;
            GraphNodeList = new List<AssociativeGraph.GraphNode>();
        }

        /// <summary>
        /// Generates and returns the entrypoint pc of the macroblock
        /// </summary>
        /// <returns></returns>
        public int GenerateEntryPoint()
        {
            int entryPoint = Constants.kInvalidPC;
            if (GraphNodeList.Count > 0)
            {
                entryPoint = GraphNodeList[0].updateBlock.startpc;
            }
            return entryPoint;
        }
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

        /// <summary>
        /// Generate test macroblocks
        /// A test macroblock start with a variable where the lhs is in the format "__aaa" to "__zzz"
        /// </summary>
        /// <param name="astList"></param>
        public static void GenerateTestMacroBlocks(List<AssociativeGraph.GraphNode> graphNodeList)
        {
        }

        /// <summary>
        /// Generates the macroblock groupings of the given list of graphnodes
        /// </summary>
        /// <param name="graphNodeList"></param>
        public static void GenerateMacroBlocks(List<AssociativeGraph.GraphNode> graphNodeList)
        {
#if __GENERATE_TEST_MACROBLOCKS
            GenerateTestMacroBlocks(graphNodeList);
#else
            // Implement the algorithm to generate macroblocks
#endif
        }

        public void GenerateMacroBlocks(List<AssociativeNode> astList)
        {
            // For now there are 2 macroblocks:
            //  0 - Macroblock for DS code processed as strings
            //  1 - global macroblock
            const int totalMacroBlocks = 2;

            // Initialize macroblocks
            int allocateSize = totalMacroBlocks - cachedMacroBlocks.Count;
            for (int n = 0; n < allocateSize; ++n)
            {
                cachedMacroBlocks.Add(new ProtoCore.CompileTime.MacroBlock());
            }


            // -------------------------------------------------
            // --------------------- Begin ---------------------
            // -------------------------------------------------

            // The number of macroblocks allocated for the entire program
            // This does not include the macroblock associated with strings
            int numMacroBlocks = 1;

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
                    // The macroBlockID is the index in which the macroblock is stored in core
                    int macroBlockID = mBlockID + 1;
                    Validity.Assert(cachedMacroBlocks[macroBlockID] != null);
                    cachedMacroBlocks[macroBlockID].AstList.Add(node);
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
        }

        /// <summary>
        /// Temporary support for DS code that need to be processed as strings
        /// </summary>
        /// <param name=></param>
        /// <returns></returns>
        public void GenerateDefaultMacroBlock()
        {
            // Initialize macroblocks
            if (cachedMacroBlocks.Count == 0)
            {
                cachedMacroBlocks.Add(new ProtoCore.CompileTime.MacroBlock());
                core.RuntimeMacroBlockList.Add(new ProtoCore.Runtime.MacroBlock());
            }

            core.MacroBlockList = cachedMacroBlocks;
        }

        public void GenerateMacroBlockIDForBinaryAST(List<AssociativeNode> astList, int macroBlockID)
        {
            // The default macroblock ID is at 1. 
            // 0 is reserved for macroblock that represent DS code processed as strings 
            foreach (AssociativeNode node in astList)
            {
                BinaryExpressionNode bnode = node as BinaryExpressionNode;
                if (bnode != null)
                {
                    bnode.MacroBlockID = macroBlockID;
                }
            }
        }
    }
}