

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
        public int GeneratedMacroblockCount {get; private set;}

        public MacroBlockGenerator()
        {
            GeneratedMacroblockCount = 0;
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
            bool hasNoDependency = graphnode.dependentList.Count == 0;
            bool hasMoreThanOneDependent = graphnode.dependentList.Count > 1;
            bool isEntryPoint = hasNoDependency || hasMoreThanOneDependent;
            return !graphnode.isReturn && isEntryPoint;
        }

        /// <summary>
        /// A node that diverges means that the node is connected to 2 or more nodes
        /// Here, 'a' diverges to 'b' and 'c'
        ///     a = 1
        ///     b = a
        ///     c = a
        ///     
        ///     a = 1 <- An input
        ///     b = a <- An input because it has a sibling 'c'
        ///     c = a <- An input because it has a sibling 'b'
        ///     
        /// </summary>
        /// <param name="?"></param>
        private void GenerateMacroblockForDivergingNodes(List<AssociativeGraph.GraphNode> programSnapshot, ref int macroblockID)
        {
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

                // graphNode.graphNodesToExecute are the children of graphNode
                // Where:
                //      a = 1
                //      b = a
                //      c = a
                //
                // The children of 'a = 1' are:
                //      b = a
                //      c = a
                if (graphNode.graphNodesToExecute.Count > 1)
                {
                    foreach (AssociativeGraph.GraphNode child in graphNode.graphNodesToExecute)
                    {
                        child.MacroblockID = macroblockID++;
                        child.Visited = true;
                        BuildMacroblock(child, programSnapshot);
                    }
                }
            }
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
            int macroblockID = 0;

            // First pass - Generate macroblocks for diverging nodes
            GenerateMacroblockForDivergingNodes(programSnapshot, ref macroblockID);

            // Second pass - Generate macroblocks for the rest of the unvisited nodes
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
                    graphNode.MacroblockID = macroblockID++;
                    graphNode.Visited = true;
                    BuildMacroblock(graphNode, programSnapshot);
                }
            }
            return macroblockID;
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

            GeneratedMacroblockCount = GenerateMacroblocksFromProgramSnapshot(programSnapshot);

            // Reinitialize the macroblock list
            RuntimeMacroBlockList = new List<Runtime.MacroBlock>();
            for (int n = 0; n < GeneratedMacroblockCount; ++n)
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