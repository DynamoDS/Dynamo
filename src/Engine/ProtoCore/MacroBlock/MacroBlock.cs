

using System;
using System.Text;
using System.Collections.Generic;
using ProtoCore.Utils;
using ProtoCore.DSASM;
using ProtoCore.AST.AssociativeAST;

namespace ProtoCore.Runtime 
{
    public class MacroBlock
    {
        public enum ExecuteState
        {
            NotReady,
            Ready,
            Executing,
            Paused
        }

        public int UID { get; set; }
        public ExecuteState State { get; set; }
        public AssociativeGraph.GraphNode InputGraphNode { get; set; }
        public List<AssociativeGraph.GraphNode> GraphNodeList { get; set; }

        public MacroBlock(int ID)
        {
            UID = ID;
            State = ExecuteState.NotReady;
            InputGraphNode = null;
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
        private enum MacroblockGeneratorType
        {
            Default,
            NumTypes
        }

        public List<ProtoCore.Runtime.MacroBlock> RuntimeMacroBlockList { get; set; }

        /// <summary>
        /// Increment this counter for every new macroblock
        /// </summary>
        private int generatedMacroblocks;

        public MacroBlockGenerator()
        {
            generatedMacroblocks = 0;
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
            // Determining if the graphnode is an entrypoint (the start of a new macroblock)
            //      * A graphnode that has no dependency (a constant assignment)
            //      * A graphnode that has at least 2 dependencies (a = b + c)
            //
            //      NoDependent = graphnode.dependentList.Count == 0
            //      HasMoreThanOneDependent = graphnode.dependentList.Count > 1
            //      IsEntryPoint = NoDependent or HasMoreThanOneDependent
            //
            // The above condition is condensed by just checking if the graphnode has exactly one dependent
            return !graphnode.isReturn && !(graphnode.dependentList.Count == 1);
        }

        /// <summary>
        /// Generate macroblocks using the default method
        /// A macroblock starts with an input node by checking IsMacroblockEntryPoint
        /// The macroblock ID is set for each graphnode 
        /// </summary>
        /// <param name="programSnapshot"></param>
        private int GenerateDefaultMacroblocks(List<AssociativeGraph.GraphNode> programSnapshot)
        {
            Validity.Assert(programSnapshot != null);
            int macroBlockID = 0;
            foreach (AssociativeGraph.GraphNode graphNode in programSnapshot)
            {
                if (!graphNode.isActive)
                {
                    continue;
                }

                if (graphNode.Visited)
                {
                    continue;
                }

                if (IsMacroblockEntryPoint(graphNode))
                {
                    graphNode.MacroblockID = macroBlockID++;
                    graphNode.Visited = true;
                    BuildMacroblock(graphNode, programSnapshot);
                }
            }
            return macroBlockID;
        }

        /// <summary>
        /// Builds the macroblock grouping starting from the given currentNode
        /// Graphnodes are grouped into a macroblock by setting their macroblockID property
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="programSnapshot"></param>
        private void BuildMacroblock(AssociativeGraph.GraphNode currentNode, List<AssociativeGraph.GraphNode> programSnapshot)
        {
            foreach (AssociativeGraph.GraphNode graphNode in programSnapshot)
            {
                AssociativeGraph.GraphNode depNode = null;
                if (graphNode.Visited)
                {
                    continue;
                }

                // Does graphNode node depend on currentNode and it is not an input node
                bool isInputNode = IsMacroblockEntryPoint(graphNode);
                if (!isInputNode && graphNode.DependsOn(currentNode.updateNodeRefList[0], ref depNode))
                {
                    graphNode.MacroblockID = currentNode.MacroblockID;
                    graphNode.Visited = true;
                    BuildMacroblock(graphNode, programSnapshot);
                }
            }
        }

        /// <summary>
        /// Analyze the program snapshot and return the optimal macroblock generator type
        /// </summary>
        /// <param name="programSnapshot"></param>
        /// <returns></returns>
        private MacroblockGeneratorType GetMacroblockTypeFromSnapshot(List<AssociativeGraph.GraphNode> programSnapshot)
        {
            // Perform analysis of program snapshot
            // Extend this implementation to support static analyzers of the snapshot
            return MacroblockGeneratorType.Default;
        }

        /// <summary>
        /// Analyze the program snapshot and generate the optimal macroblock
        /// </summary>
        /// <param name="programSnapshot"></param>
        private int GenerateMacroblocksFromProgramSnapshot(List<AssociativeGraph.GraphNode> programSnapshot)
        {
            int generatedBlocks = Constants.kInvalidIndex;

            MacroblockGeneratorType type = GetMacroblockTypeFromSnapshot(programSnapshot);
            if (type == MacroblockGeneratorType.Default)
            {
                generatedBlocks = GenerateDefaultMacroblocks(programSnapshot);
            }
            else
            {
                throw new NotImplementedException();
            }
            return generatedBlocks;
        }


        /// <summary>
        /// Generates the macroblock groupings of the given list of graphnodes (the program snapshot)
        /// </summary>
        /// <param name="programSnapshot"></param>
        /// <returns></returns>
        public List<ProtoCore.Runtime.MacroBlock> GenerateMacroblocks(List<AssociativeGraph.GraphNode> programSnapshot)
        {
            // Reset the graphnode states
            foreach (AssociativeGraph.GraphNode graphnode in programSnapshot)
            {
                graphnode.Visited = false;
            }

            generatedMacroblocks = GenerateMacroblocksFromProgramSnapshot(programSnapshot);

            // Reinitialize the macroblock list
            RuntimeMacroBlockList = new List<Runtime.MacroBlock>();
            for (int n = 0; n < generatedMacroblocks; ++n)
            {
                RuntimeMacroBlockList.Add(new Runtime.MacroBlock(n));
            }

            // Cache the macroblocks
            foreach (AssociativeGraph.GraphNode graphNode in programSnapshot)
            {
                if (graphNode.MacroblockID != Constants.kInvalidIndex)
                {
                    RuntimeMacroBlockList[graphNode.MacroblockID].GraphNodeList.Add(graphNode);
                    if (IsMacroblockEntryPoint(graphNode))
                    {
                        RuntimeMacroBlockList[graphNode.MacroblockID].InputGraphNode = graphNode;
                    }
                }
            }
            return RuntimeMacroBlockList;
        }
    }
}