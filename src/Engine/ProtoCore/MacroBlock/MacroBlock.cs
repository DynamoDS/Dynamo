

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

        public MacroBlock(int ID)
        {
            UID = ID;
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
        public List<ProtoCore.Runtime.MacroBlock> RuntimeMacroBlockList { get; set; }

        /// <summary>
        /// Increment this counter for every new macroblock
        /// </summary>
        private int generatedMacroblocks;

        public MacroBlockGenerator(ProtoCore.Core core)
        {
            this.core = core;
            cachedASTList = new List<AssociativeNode>();
            cachedMacroBlocks = new List<CompileTime.MacroBlock>();
            generatedMacroblocks = 0;
        }

        /// <summary>
        /// Check is the graphnode is an input node where the lhs is in the format "__aaa" to "__fff"
        /// </summary>
        /// <param name="graphnode"></param>
        /// <returns></returns>
        private bool __IsInputGraphNode_Deprecate(AssociativeGraph.GraphNode graphnode)
        {
            Validity.Assert(graphnode.updateNodeRefList[0].nodeList[0].symbol.name != null);

            string name = graphnode.updateNodeRefList[0].nodeList[0].symbol.name.Substring(0, 5);
            return name.Equals("__aaa")
                || name.Equals("__bbb")
                || name.Equals("__ccc")
                || name.Equals("__ddd")
                || name.Equals("__eee")
                || name.Equals("__fff");
        }

        /// <summary>
        /// Check if the graphnode is the start of a new macroblock
        /// </summary>
        /// <param name="graphnode"></param>
        /// <returns></returns>
        private bool IsMacroblockEntryPoint(AssociativeGraph.GraphNode graphnode)
        {
            Validity.Assert(graphnode != null);

            //
            // Determining if the graphnode is the start of a new block
            //
            //      NoDependent = graphnode.dependentList.Count == 0
            //      HasMoreThanOneDependent = graphnode.dependentList.Count > 1
            //      StartofNewBlock = NoDependent or HasMoreThanOneDependent
            //

            // The above condition is condensed by just checking if the graphnode has exactly one dependent
            return !graphnode.isReturn && !(graphnode.dependentList.Count == 1);
        }

        /// <summary>
        /// Generate test macroblocks
        /// A test macroblock starts with an input node
        /// The macroblock ID is set for each graphnode 
        /// </summary>
        /// <param name="programSnapshot"></param>
        public void __GenerateTestMacroBlocks_Deprecate(List<AssociativeGraph.GraphNode> programSnapshot, int macroBlockID = -1)
        {
            Validity.Assert(programSnapshot != null);
            foreach (AssociativeGraph.GraphNode graphnode in programSnapshot)
            {
                if (!graphnode.isActive)
                {
                    continue;
                }

                if (!graphnode.Visited)
                {
                    // Determine if it is the start of a new macroblock
                    bool isNewMacroblock = IsMacroblockEntryPoint(graphnode);
                    if (isNewMacroblock)
                    {
                        graphnode.Visited = true;
                        generatedMacroblocks++;
                        int newID = macroBlockID + 1;
                        graphnode.MacroblockID = newID;
                        __GenerateTestMacroBlocks_Deprecate(programSnapshot, newID);
                    }
                    else
                    {
                        foreach (AssociativeGraph.GraphNode connectedNode in graphnode.graphNodesToExecute)
                        {
                            connectedNode.MacroblockID = macroBlockID;
                            graphnode.Visited = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate test macroblocks
        /// A test macroblock starts with an input node
        /// The macroblock ID is set for each graphnode 
        /// </summary>
        /// <param name="programSnapshot"></param>
        public int GenerateMacroblocks(List<AssociativeGraph.GraphNode> programSnapshot)
        {
            Validity.Assert(programSnapshot != null);
            int macroBlockID = 0;
            foreach (AssociativeGraph.GraphNode graphnode in programSnapshot)
            {
                if (!graphnode.isActive)
                {
                    continue;
                }

                if (graphnode.Visited)
                {
                    continue;
                }

                if (IsMacroblockEntryPoint(graphnode))
                {
                    graphnode.MacroblockID = macroBlockID++;
                    graphnode.Visited = true;
                    BuildMacroblock(graphnode, programSnapshot);
                }
            }
            return macroBlockID;
        }

        private void BuildMacroblock(AssociativeGraph.GraphNode currentNode, List<AssociativeGraph.GraphNode> programSnapshot)
        {
            foreach (AssociativeGraph.GraphNode graphNode in programSnapshot)
            {
                AssociativeGraph.GraphNode depNode = null;
                if (graphNode.Visited)
                {
                    continue;
                }

                if (graphNode.DependsOn(currentNode.updateNodeRefList[0], ref depNode))
                {
                    graphNode.MacroblockID = currentNode.MacroblockID;
                    graphNode.Visited = true;
                    BuildMacroblock(graphNode, programSnapshot);
                }
            }
        }

        /// <summary>
        /// Generates the macroblock groupings of the given list of graphnodes (the program snapshot)
        /// </summary>
        /// <param name="programSnapshot"></param>
        public void GenerateMacroBlocks(List<AssociativeGraph.GraphNode> programSnapshot)
        {
            generatedMacroblocks = 0;
#if __GENERATE_TEST_MACROBLOCKS

            foreach (AssociativeGraph.GraphNode graphnode in programSnapshot)
            {
                graphnode.Visited = false;
            }
           // __GenerateTestMacroBlocks_Deprecate(programSnapshot);
            generatedMacroblocks = GenerateMacroblocks(programSnapshot);
#else
            // Implement the algorithm to generate macroblocks
#endif
            // Reinitialize the macroblock list
            RuntimeMacroBlockList = new List<Runtime.MacroBlock>();
            for (int n = 0; n < generatedMacroblocks; ++n)
            {
                RuntimeMacroBlockList.Add(new Runtime.MacroBlock(n));
            }

            foreach (AssociativeGraph.GraphNode graphNode in programSnapshot)
            {
                if (graphNode.MacroblockID != Constants.kInvalidIndex)
                {
                    RuntimeMacroBlockList[graphNode.MacroblockID].GraphNodeList.Add(graphNode);
                }
            }
            core.RuntimeMacroBlockList = RuntimeMacroBlockList;
        }
    }
}