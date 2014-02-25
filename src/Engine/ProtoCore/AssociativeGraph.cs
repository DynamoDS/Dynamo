using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.Utils;
using System;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;


namespace ProtoCore.AssociativeEngine
{
    public enum UpdateStatus
    {
        kNormalUpdate,
        kPropertyChangedUpdate
    }

    public class Utils
    {
        /// <summary>
        /// This function sets the modified temp graphnodes to the last graphnode in a statement
        /// </summary>
        /// <param name="graphnode"></param>
        public static void SetFinalGraphNodeRuntimeDependents(AssociativeGraph.GraphNode graphnode)
        {
            if (null != graphnode && graphnode.IsSSANode())
            {
                if (null != graphnode.lastGraphNode)
                {
                    graphnode.lastGraphNode.symbolListWithinExpression.Add(graphnode.updateNodeRefList[0].nodeList[0].symbol);
                }
            }
        }
    }
    public class ArrayUpdate
    {
        /// <summary>
        /// This function determines if the index into is part of a list of indices into
        /// This is an array element update method and must reside in the array update class
        /// </summary>
        /// <param name="indexList"></param>
        /// <param name="indexIntoList"></param>
        /// <returns></returns>
        public static bool IsIndexInElementUpdateList(int index, List<List<int>> indexIntoList)
        {
            //
            //  proc IsIndexInElementUpdateList(int index, List<List<int>> indexIntoList)
            //      foreach dimensionList in indexIntoList
            //          if index is equal to dimensionList[0]
            //              return true
            //          end
            //      end
            //      return false
            //  end
            //

            foreach (List<int> list in indexIntoList)
            {
                if (list.Count > 0 && list[0] == index)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<List<int>> UpdateIndexIntoList(int index, List<List<int>> indexIntoList)
        {
            //
            // proc UpdateIndexIntoList(int index, List<List<int>> indexIntoList)
            //    foreach dimensionList in indexIntoList
            //        if index is equal to dimensionList[0]
            //            return dimensionList.RemoveAt(0)
            //        end
            //    end
            //    return false
            // end
            //


            foreach (List<int> list in indexIntoList)
            {
                if (list.Count > 0 && list[0] == index)
                {
                    list.RemoveAt(0);
                }
            }
            return indexIntoList;
        }



        //proc UpdateSymbolArrayIndex(Symbol symbol, List<int> indices)
        //    if symbolIndexMap.DoesNotContain(symbol)
        //        symbolIndexMap.Push(symbol, indices)
        //    else
        //        symbolIndexMap[symbol].Assign(indices)
        //    end
        //end
        /// <summary>
        ///  This updates the symbol indices map with the latest indices associated with a symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="indices"></param>
        /// This method needs to be moved to the array update class
        public static void UpdateSymbolArrayIndex(string symbol, List<int> indices, Dictionary<string, List<int>> symbolArrayIndexMap)
        {
            if (!symbolArrayIndexMap.ContainsKey(symbol))
            {
                symbolArrayIndexMap.Add(symbol, indices);
            }
            else
            {
                symbolArrayIndexMap[symbol] = indices;
            }
        }
    }
}

namespace ProtoCore.AssociativeGraph
{
    public class UpdateBlock
    {
        public int startpc { get; set; }
        public int endpc { get; set; }

        public UpdateBlock()
        {
            startpc = ProtoCore.DSASM.Constants.kInvalidIndex;
            endpc = ProtoCore.DSASM.Constants.kInvalidIndex;
        }
    }


    public class GraphNode
    {
        public int UID { get; set; }
        public int dependencyGraphListID { get; set; }
        public int exprUID { get; set; }
        public int modBlkUID { get; set; }
        public List<ProtoCore.AssociativeGraph.UpdateNode> dimensionNodeList { get; set; }
        public List<ProtoCore.AssociativeGraph.UpdateNodeRef> updateNodeRefList { get; set; }
        public bool isParent { get; set; }
        public bool isDirty { get; set; }
        public bool isReturn { get; set; }
        public int procIndex { get; set; }      // Function that this graph resides in
        public int classIndex { get; set; }     // Class index that this graph resides in
        public UpdateBlock updateBlock { get; set; }
        public List<GraphNode> dependentList { get; set; }
        public bool allowDependents { get; set; }
        public bool isIndexingLHS { get; set; }
        public bool isLHSNode { get; set; }
        public ProtoCore.DSASM.ProcedureNode firstProc { get; set; }
        public int firstProcRefIndex { get; set; }
        public bool isVisited { get; set; }
        public bool isCyclic { get; set; }
        public bool isInlineConditional { get; set; }
        public GraphNode cyclePoint { get; set; }
        public int dependencyTestRecursiveDepth { get; set; }
        public bool isAutoGenerated { get; set; }
        public bool isLanguageBlock { get; set; }
        public int languageBlockId { get; set; }
        public List<StackValue> updateDimensions { get; set; }
        public int counter { get; set; }
        public ReplicationControl replicationControl {get; set;}
        public bool propertyChanged { get; set; }       // The property of ffi object that created in this graph node is changed
        public bool forPropertyChanged { get; set; }    // The graph node is marked as dirty because of property changed event

        public GraphNode lastGraphNode { get; set; }    // This is the last graphnode of an SSA'd statement

        public List<ProtoCore.AssociativeGraph.UpdateNodeRef> updatedArguments { get; set; }


        /// <summary>
        /// This is the list of lhs symbols in the same expression ID
        /// It is applicable for expressions transformed to SSA where each ssa temp in the same expression is in this list
        /// This list is only populated on the last SSA assignment as such:
        ///     
        /// Given
        ///     a = b.c.d
        ///     
        ///     [0] t0 = b      -> List empty
        ///     [1] t1 = t0.b   -> List empty
        ///     [2] t2 = t1.c   -> List empty
        ///     [3] a = t2      -> This is the last SSA stmt, its graphnode contains a list of graphnodes {t0,t1,t2}
        ///     
        /// </summary>
        public List<SymbolNode> symbolListWithinExpression { get; set; }

        public bool reExecuteExpression { get; set; }
        /// <summary>
        /// Flag determines if a graph node is active or not. If inactive, the graph node is invalid
        /// this is especially used in the LiveRunner to mark modified/deleted nodes inactive so that they are not executed
        /// </summary>
        public bool isActive { get; set; }

        public int SSASubscript { get; set; }
        public bool IsLastNodeInSSA { get; set; }


        
#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
        public StackValue ArrayPointer { get; set; }
#endif

        public GraphNode()
        {
            UID = ProtoCore.DSASM.Constants.kInvalidIndex;
            dependencyGraphListID = ProtoCore.DSASM.Constants.kInvalidIndex;
            dimensionNodeList = new List<UpdateNode>();
            updateNodeRefList = new List<ProtoCore.AssociativeGraph.UpdateNodeRef>();
            isDirty = true;
            isParent = false;
            isReturn = false;
            procIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            classIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            updateBlock = new UpdateBlock();
            dependentList = new List<GraphNode>();
            allowDependents = true;
            isIndexingLHS = false;
            isLHSNode = false;
            firstProc = null;
            firstProcRefIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            isVisited = false;
            isCyclic = false;
            isInlineConditional = false;
            counter = 0;
            updatedArguments = new List<UpdateNodeRef>();
            dependencyTestRecursiveDepth = 0;
            isAutoGenerated = false;
            isLanguageBlock = false;
            languageBlockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            updateDimensions = new List<StackValue>();
            propertyChanged = false;
            forPropertyChanged = false;
            lastGraphNode = null;
            isActive = true;

#if __PROTOTYPE_ARRAYUPDATE_FUNCTIONCALL
            ArrayPointer = ProtoCore.DSASM.StackValue.Null;
#endif
            symbolListWithinExpression = new List<SymbolNode>();
            reExecuteExpression = false;
            SSASubscript = ProtoCore.DSASM.Constants.kInvalidIndex;
            IsLastNodeInSSA = false;
        }


        public bool PushDependent(GraphNode dependent)
        {
            Validity.Assert(null != dependentList);

            if (allowDependents)
            {
                //if (!dependent.updateNodeRefList[0].nodeList[0].symbol.name.StartsWith(ProtoCore.DSASM.Constants.kTempVar))
                {
                    bool exists = false;
                    foreach (ProtoCore.AssociativeGraph.GraphNode gnode in dependentList)
                    {
                        if (dependent.updateNodeRefList[0].nodeList[0].IsEqual(gnode.updateNodeRefList[0].nodeList[0]))
                        {
                            exists = true;
                        }
                    }

                    if (!exists)
                    {
                        if (dependent.UID != ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            dependent.UID = dependentList.Count;
                        }
                        dependentList.Add(dependent);
                    }
                }
            }
            return true;
        }

        public void ResolveLHSArrayIndex()
        {
            if (dimensionNodeList.Count > 0)
            {
                int last = updateNodeRefList[0].nodeList.Count - 1;
                updateNodeRefList[0].nodeList[last].dimensionNodeList.AddRange(dimensionNodeList);
            }
        }

        public void PushSymbolReference(ProtoCore.DSASM.SymbolNode symbol, ProtoCore.AssociativeGraph.UpdateNodeType type = UpdateNodeType.kSymbol)
        {
            Validity.Assert(null != symbol);
            Validity.Assert(null != updateNodeRefList);
            UpdateNode updateNode = new UpdateNode();
            updateNode.symbol = symbol;
            updateNode.nodeType = type;

            UpdateNodeRef nodeRef = new UpdateNodeRef();
            nodeRef.PushUpdateNode(updateNode);

            updateNodeRefList.Add(nodeRef);
        }

        public void PushSymbolReference(ProtoCore.DSASM.SymbolNode symbol)
        {
            Validity.Assert(null != symbol);
            Validity.Assert(null != updateNodeRefList);
            UpdateNode updateNode = new UpdateNode();
            updateNode.symbol = symbol;
            updateNode.nodeType = UpdateNodeType.kSymbol;

            UpdateNodeRef nodeRef = new UpdateNodeRef();
            nodeRef.block = symbol.runtimeTableIndex;
            nodeRef.PushUpdateNode(updateNode);

            updateNodeRefList.Add(nodeRef);
        }

        public void PushProcReference(ProtoCore.DSASM.ProcedureNode proc)
        {
            Validity.Assert(null != proc);
            Validity.Assert(null != updateNodeRefList);
            UpdateNode updateNode = new UpdateNode();
            updateNode.procNode = proc;
            updateNode.nodeType = UpdateNodeType.kMethod;

            UpdateNodeRef nodeRef = new UpdateNodeRef();
            nodeRef.PushUpdateNode(updateNode);

            updateNodeRefList.Add(nodeRef);
        }

        public bool IsUpdateableBy(UpdateNodeRef modifiedRef)
        {
            // Function to check if the current graphnode can be modified by the modified reference
            bool isUpdateable = false;
            if (modifiedRef.nodeList.Count < updateNodeRefList[0].nodeList.Count)
            {
                isUpdateable = true;
                for (int n = 0; n < modifiedRef.nodeList.Count; ++n)
                {
                    ProtoCore.AssociativeGraph.UpdateNode updateNode = modifiedRef.nodeList[n];
                    if (!updateNode.IsEqual(updateNodeRefList[0].nodeList[n]))
                    {
                        isUpdateable = false;
                        break;
                    }
                }
            }
            return isUpdateable;
        }

        public bool DependsOnProperty(string propertyName)
        {
            string getter = Constants.kGetterPrefix + propertyName;

            foreach (var dependent in this.dependentList)
            {
                foreach (var updateNodeRef in dependent.updateNodeRefList)
                {
                    foreach (var node in updateNodeRef.nodeList)
                    {
                        if (node.procNode != null && node.procNode.name == getter)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// For a list of update node like x.y.z, for specified property name
        /// "y", return x.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public UpdateNode GetUpdateNodeForGetter(string propertyName)
        {
            string getter = Constants.kGetterPrefix + propertyName;

            foreach (var dependent in this.dependentList)
            {
                foreach (var updateNodeRef in dependent.updateNodeRefList)
                {
                    for (int i = 0; i < updateNodeRef.nodeList.Count; ++i)
                    {
                        if (updateNodeRef.nodeList[i].procNode.name == getter && i == 1)
                        {
                            return updateNodeRef.nodeList[0];
                        }
                    }
                }
            }

            return null;
        }

        public bool DependsOn(ProtoCore.AssociativeGraph.UpdateNodeRef modifiedRef, ref GraphNode dependentNode)
        {
            bool match = false;

            foreach (GraphNode depNode in dependentList)
            {

                Validity.Assert(1 == depNode.updateNodeRefList.Count);
                //foreach (UpdateNodeRef depNodeRef in depNode.updateNodeRefList)
                //{
                UpdateNodeRef depNodeRef = depNode.updateNodeRefList[0];
                bool bothSymbolsMatch = false;
                bool bothSymbolsStatic = false;
                bool inImperativeMatch = false;
                bool inImperative = false;
                if (depNodeRef != null)
                    if (depNodeRef.nodeList != null && modifiedRef.nodeList != null && depNodeRef.nodeList.Count > 0 && modifiedRef.nodeList.Count > 0)
                    {
                        if (depNodeRef.nodeList.Count > modifiedRef.nodeList.Count)
                        {
                            for (int m = 0; m < depNodeRef.nodeList.Count; m++)
                            {

                                if (depNodeRef.nodeList[m] != null && modifiedRef.nodeList[0] != null && depNodeRef.nodeList[m].symbol != null && modifiedRef.nodeList[0].symbol != null)
                                {
                                    if (modifiedRef.nodeList[0].symbol.forArrayName != null && !modifiedRef.nodeList[0].symbol.forArrayName.Equals(""))
                                    {
                                        inImperative = true;
                                        if (modifiedRef.nodeList[0].symbol.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                                        {
                                            inImperative = inImperative
                                                && (depNodeRef.nodeList[m].symbol.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                                                && (modifiedRef.nodeList[0].symbol.codeBlockId == depNodeRef.nodeList[m].symbol.codeBlockId);
                                        }

                                        if (inImperative && modifiedRef.nodeList[0].symbol.functionIndex == depNodeRef.nodeList[m].symbol.functionIndex && (modifiedRef.nodeList[0].symbol.name == depNodeRef.nodeList[m].symbol.name || modifiedRef.nodeList[0].symbol.forArrayName == depNodeRef.nodeList[m].symbol.name))
                                        {
                                            inImperativeMatch = true;
                                        }

                                    }
                                }
                            }
                        }
                        else if (depNodeRef.nodeList.Count == modifiedRef.nodeList.Count)
                        {
                            for (int m = 0; m < depNodeRef.nodeList.Count && m < modifiedRef.nodeList.Count; m++)
                            {

                                if (depNodeRef.nodeList[m] != null && modifiedRef.nodeList[m] != null && depNodeRef.nodeList[m].symbol != null && modifiedRef.nodeList[m].symbol != null)
                                {
                                    if (modifiedRef.nodeList[0].symbol.forArrayName != null && !modifiedRef.nodeList[0].symbol.forArrayName.Equals(""))
                                    {
                                        inImperative = true;
                                        if (modifiedRef.nodeList[m].symbol.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                                        {
                                            inImperative = inImperative
                                                && (depNodeRef.nodeList[m].symbol.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                                                && (modifiedRef.nodeList[m].symbol.codeBlockId == depNodeRef.nodeList[m].symbol.codeBlockId);
                                        }
                                        if (inImperative && modifiedRef.nodeList[m].symbol.functionIndex == depNodeRef.nodeList[m].symbol.functionIndex && modifiedRef.nodeList[m].symbol.name == depNodeRef.nodeList[m].symbol.name )
                                        {
                                            inImperativeMatch = true;
                                        }

                                    }
                                }
                            }
                        }
                    }


                if (!inImperativeMatch)
                {
                    // Does first symbol match


                    if (null != modifiedRef.nodeList[0].symbol && null != depNodeRef.nodeList[0].symbol)
                    {
                        bothSymbolsMatch = modifiedRef.nodeList[0].symbol.IsEqualAtScope(depNodeRef.nodeList[0].symbol);


                        bothSymbolsStatic =
                            modifiedRef.nodeList[0].symbol.memregion == DSASM.MemoryRegion.kMemStatic
                            && depNodeRef.nodeList[0].symbol.memregion == DSASM.MemoryRegion.kMemStatic
                            && modifiedRef.nodeList[0].symbol.name == depNodeRef.nodeList[0].symbol.name;

                        // Check further if their array index match in literal values
                        if (bothSymbolsMatch)
                        {
                            // Are the indices the same number
                            bool areIndicesMatching = modifiedRef.nodeList[0].dimensionNodeList.Count >= depNodeRef.nodeList[0].dimensionNodeList.Count;
                            if (areIndicesMatching && depNodeRef.nodeList[0].dimensionNodeList.Count > 0)
                            {
                                for (int n = 0; n < depNodeRef.nodeList[0].dimensionNodeList.Count; ++n)
                                {
                                    // Is either a non-literal
                                    UpdateNode modDimNode =modifiedRef.nodeList[0].dimensionNodeList[n];
                                    UpdateNode depDimNode = depNodeRef.nodeList[0].dimensionNodeList[n];

                                    if (modDimNode.nodeType != depDimNode.nodeType)
                                    {
                                        bothSymbolsMatch = false;
                                    }
                                    else if (modDimNode.nodeType == UpdateNodeType.kLiteral)
                                    {
                                        bothSymbolsMatch = modDimNode.symbol.name.CompareTo(depDimNode.symbol.name) == 0;
                                    }
                                    else if (modDimNode.nodeType == UpdateNodeType.kSymbol)
                                    {
                                        bothSymbolsMatch = modDimNode.symbol.IsEqualAtScope(depDimNode.symbol);
                                    }
                                    else
                                    {
                                        bothSymbolsMatch = false;
                                    }

                                    if (!bothSymbolsMatch)
                                    { 
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (bothSymbolsMatch || bothSymbolsStatic)
                    {
                        match = true;

                        // If it is static, then all symbols must match
                        if (bothSymbolsStatic)
                        {
                            // The number of symbols in the modifed reference... 
                            //  ...must match
                            // The number of symbols in the current dependency noderef
                            if (modifiedRef.nodeList.Count == depNodeRef.nodeList.Count)
                            {
                                for (int n = 1; n < modifiedRef.nodeList.Count; ++n)
                                {
                                    //Validity.Assert(!modifiedRef.nodeList[n].isMethod);
                                    //Validity.Assert(!depNodeRef.nodeList[n].isMethod);

                                    if (UpdateNodeType.kMethod == modifiedRef.nodeList[n].nodeType || UpdateNodeType.kMethod == depNodeRef.nodeList[n].nodeType)
                                    {
                                        match = false;
                                        break;
                                    }

                                    if (modifiedRef.nodeList[n].symbol.index != depNodeRef.nodeList[n].symbol.index)
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                match = false;
                            }
                        }
                        else
                        {
                            if (modifiedRef.nodeList.Count >= depNodeRef.nodeList.Count)
                            {
                                //
                                // The modifed reference is either the same nodelist length or more than the current dependent
                                // a.x.y is being compared to a.x
                                //
                                for (int n = 1; n < modifiedRef.nodeList.Count; ++n)
                                {
                                    if (modifiedRef.nodeList.Count != depNodeRef.nodeList.Count)
                                    {
                                        if (n >= depNodeRef.nodeList.Count)
                                        {
                                            match = false;
                                            break;
                                        }
                                    }

                                    if (UpdateNodeType.kMethod == modifiedRef.nodeList[n].nodeType || UpdateNodeType.kMethod == depNodeRef.nodeList[n].nodeType)
                                    {
                                        match = false;
                                        break;
                                    }

                                    if (modifiedRef.nodeList[n].symbol.name != depNodeRef.nodeList[n].symbol.name)
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                //
                                // The modifed reference nodelist is less than than the current dependent nodelist
                                // a.x is being compared to a.x.y 
                                //
                                for (int n = 1; n < depNodeRef.nodeList.Count; ++n)
                                {

                                    if (n >= modifiedRef.nodeList.Count)
                                    {
                                        break;
                                    }

                                    if (UpdateNodeType.kMethod == modifiedRef.nodeList[n].nodeType || UpdateNodeType.kMethod == depNodeRef.nodeList[n].nodeType)
                                    {
                                        match = false;
                                        break;
                                    }

                                    if (modifiedRef.nodeList[n].symbol.name != depNodeRef.nodeList[n].symbol.name)
                                    {
                                        match = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    dependentNode = depNode;
                    if (match)
                    {
                        break;
                    }

                }
                else
                {
                    for (int m = 0; m < depNodeRef.nodeList.Count && m < modifiedRef.nodeList.Count; m++)
                    {
                        // Does first symbol match

                        if (null != modifiedRef.nodeList[m].symbol && null != depNodeRef.nodeList[m].symbol)
                        {
                            bothSymbolsMatch = modifiedRef.nodeList[m].symbol.IsEqualAtScope(depNodeRef.nodeList[m].symbol);
                            bothSymbolsStatic =
                                modifiedRef.nodeList[m].symbol.memregion == DSASM.MemoryRegion.kMemStatic
                                && depNodeRef.nodeList[m].symbol.memregion == DSASM.MemoryRegion.kMemStatic
                                && modifiedRef.nodeList[m].symbol.name == depNodeRef.nodeList[m].symbol.name;

                            // Check further if their array index match in literal values
                            if (bothSymbolsMatch)
                            {
                                // Are the indices the same number
                                bool areIndicesMatching = modifiedRef.nodeList[m].dimensionNodeList.Count == depNodeRef.nodeList[m].dimensionNodeList.Count;
                                if (areIndicesMatching && modifiedRef.nodeList[m].dimensionNodeList.Count > 0)
                                {
                                    for (int n = 0; n < modifiedRef.nodeList[m].dimensionNodeList.Count; ++n)
                                    {
                                        // Is either a non-literal
                                        bool isEitherNonLiteral = modifiedRef.nodeList[m].dimensionNodeList[n].nodeType != UpdateNodeType.kLiteral
                                            || depNodeRef.nodeList[m].dimensionNodeList[n].nodeType != UpdateNodeType.kLiteral;
                                        if (isEitherNonLiteral)
                                        {
                                            bothSymbolsMatch = false;
                                            break;
                                        }

                                        // They are both literal, now check for their literal values
                                        if (0 != modifiedRef.nodeList[m].dimensionNodeList[n].symbol.name.CompareTo(depNodeRef.nodeList[m].dimensionNodeList[n].symbol.name))
                                        {
                                            // They are not the same
                                            bothSymbolsMatch = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (bothSymbolsMatch || bothSymbolsStatic || inImperativeMatch)
                        {
                            match = true;

                            // If it is static, then all symbols must match
                            if (bothSymbolsStatic)
                            {
                                // The number of symbols in the modifed reference... 
                                //  ...must match
                                // The number of symbols in the current dependency noderef
                                if (modifiedRef.nodeList.Count == depNodeRef.nodeList.Count)
                                {
                                    for (int n = 1; n < modifiedRef.nodeList.Count; ++n)
                                    {
                                        //Validity.Assert(!modifiedRef.nodeList[n].isMethod);
                                        //Validity.Assert(!depNodeRef.nodeList[n].isMethod);

                                        if (UpdateNodeType.kMethod == modifiedRef.nodeList[n].nodeType || UpdateNodeType.kMethod == depNodeRef.nodeList[n].nodeType)
                                        {
                                            match = false;
                                            break;
                                        }

                                        if (modifiedRef.nodeList[n].symbol.index != depNodeRef.nodeList[n].symbol.index)
                                        {
                                            match = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    match = false;
                                }
                            }
                            else
                            {
                                if (modifiedRef.nodeList.Count >= depNodeRef.nodeList.Count)
                                {
                                    //
                                    // The modifed reference is either the same nodelist length or more than the current dependent
                                    // a.x.y is being compared to a.x
                                    //
                                    for (int n = 1; n < modifiedRef.nodeList.Count; ++n)
                                    {
                                        if (modifiedRef.nodeList.Count != depNodeRef.nodeList.Count)
                                        {
                                            if (n >= depNodeRef.nodeList.Count)
                                            {
                                                match = false;
                                                break;
                                            }
                                        }

                                        if (UpdateNodeType.kMethod == modifiedRef.nodeList[n].nodeType || UpdateNodeType.kMethod == depNodeRef.nodeList[n].nodeType)
                                        {
                                            match = false;
                                            break;
                                        }

                                        if (modifiedRef.nodeList[n].symbol.name != depNodeRef.nodeList[n].symbol.name)
                                        {
                                            match = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //
                                    // The modifed reference nodelist is less than than the current dependent nodelist
                                    // a.x is being compared to a.x.y 
                                    //
                                    for (int n = 1; n < depNodeRef.nodeList.Count; ++n)
                                    {

                                        if (n >= modifiedRef.nodeList.Count)
                                        {
                                            break;
                                        }

                                        if (UpdateNodeType.kMethod == modifiedRef.nodeList[n].nodeType || UpdateNodeType.kMethod == depNodeRef.nodeList[n].nodeType)
                                        {
                                            match = false;
                                            break;
                                        }

                                        if (modifiedRef.nodeList[n].symbol.name != depNodeRef.nodeList[n].symbol.name)
                                        {
                                            match = false;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    dependentNode = depNode;
                    if (match)
                    {
                        break;
                    }
                }
                //}
            }
            return match;
        }

        public bool DependsOnTempSSA()
        {
            foreach (ProtoCore.AssociativeGraph.GraphNode dnode in dependentList)
            {
                if (dnode.IsSSANode())
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSSANode()
        {
            if (updateNodeRefList.Count == 0)
            {
                return false;
            }

            if (ProtoCore.Utils.CoreUtils.IsSSATemp(updateNodeRefList[0].nodeList[0].symbol.name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class DependencyGraph
    {
        private readonly ProtoCore.Core core;
        private List<GraphNode> graphList;

        // For quickly get a list of graph nodes at some scope. 
        private Dictionary<ulong, List<GraphNode>> graphNodeMap;

        public List<GraphNode> GraphList
        {
            get
            {
                return graphList;
            }
        }

        private ulong GetGraphNodeKey(int classIndex, int procIndex)
        {
            uint ci = (uint)classIndex;
            uint pi = (uint)procIndex;
            return (((ulong)ci) << 32) | pi;
        }

        public DependencyGraph(ProtoCore.Core core)
        {
            this.core = core;
            graphList = new List<GraphNode>();
            graphNodeMap = new Dictionary<ulong, List<GraphNode>>();
        }

        public List<GraphNode> GetGraphNodesAtScope(int classIndex, int procIndex)
        {
            List<GraphNode> nodes = null;
            graphNodeMap.TryGetValue(GetGraphNodeKey(classIndex, procIndex), out nodes);
            return nodes;
        }

        public List<bool> GetExecutionStatesAtScope(int classIndex, int procIndex)
        {
            List<GraphNode> nodes = GetGraphNodesAtScope(classIndex, procIndex);

            List<bool> execStates = new List<bool>();
            if (null != nodes && nodes.Count > 0)
            {
                for (int n = 0; n < nodes.Count; ++n)
                {
                    execStates.Add(nodes[n].isDirty);
                }
            }
            return execStates;
        }

        public void RemoveNodesFromScope(int classIndex, int procIndex)
        {
            ulong removeKey = GetGraphNodeKey(classIndex, procIndex);
            graphNodeMap.Remove(removeKey);
        }

        public void Push(GraphNode node)
        {
            Validity.Assert(null != core);
            Validity.Assert(core.GraphNodeUID >= 0);
            node.UID = core.GraphNodeUID++;
            node.dependencyGraphListID = graphList.Count;
            graphList.Add(node);

            ulong key = GetGraphNodeKey(node.classIndex, node.procIndex);
            List<GraphNode> nodes = null;
            if (graphNodeMap.TryGetValue(key, out nodes))
            {
                nodes.Add(node);
            }
            else
            {
                nodes = new List<GraphNode> {node};
                graphNodeMap[key] = nodes;
            }
        }
    }

    public enum UpdateNodeType
    {
        kLiteral,
        kClass,
        kSymbol,
        kMethod
    };

    public class UpdateNode
    {
        public ProtoCore.DSASM.SymbolNode symbol;
        public ProtoCore.DSASM.ProcedureNode procNode;
        public UpdateNodeType nodeType;

        // This is the list of nodes represting every indexed dimension
        public List<UpdateNode> dimensionNodeList { get; set; }

        public UpdateNode()
        {
            dimensionNodeList = new List<UpdateNode>();
        }

        public bool IsEqual(UpdateNode rhs)
        {
            if (nodeType == rhs.nodeType)
            {
                if (nodeType == UpdateNodeType.kSymbol || nodeType == UpdateNodeType.kLiteral)
                {
                    return symbol.IsEqualAtScope(rhs.symbol);
                }
                else if (nodeType == UpdateNodeType.kMethod)
                {
                    return procNode.IsEqual(rhs.procNode);
                }
            }
            return false;
        }
    }


    // An update node reference is an entity in a graphnode that represents 
    // the LHS of an identifer or one of the RHS identifiers of an expression
    public class UpdateNodeRef
    {
        public int block { get; set; }
        public List<UpdateNode> nodeList { get; set; }
        public StackValue symbolData { get; set; }

        public UpdateNodeRef()
        {
            nodeList = new List<UpdateNode>();
        }

        public UpdateNodeRef(UpdateNodeRef rhs)
        {
            nodeList = new List<UpdateNode>();
            if (null != rhs && null != rhs.nodeList)
            {
                foreach (UpdateNode node in rhs.nodeList)
                {
                    PushUpdateNode(node);
                }
            }
        }


        public void PushUpdateNode(UpdateNode node)
        {
            Validity.Assert(null != nodeList);
            nodeList.Add(node);
        }

        public void PushUpdateNodeRef(UpdateNodeRef nodeRef)
        {
            Validity.Assert(null != nodeList);
            foreach (UpdateNode node in nodeRef.nodeList)
            {
                nodeList.Add(node);
            }
        }

        public UpdateNodeRef GetUntilFirstProc()
        {
            Validity.Assert(null != nodeList);
            UpdateNodeRef newRef = new UpdateNodeRef();
            foreach (UpdateNode node in nodeList)
            {
                if (node.nodeType != UpdateNodeType.kMethod)
                {
                    newRef.nodeList.Add(node);
                }
            }
            return newRef;
        }

        public bool IsEqual(UpdateNodeRef rhs)
        {
            if (nodeList.Count != rhs.nodeList.Count)
            {
                return false;
            }

            for (int n = 0; n < nodeList.Count; ++n)
            {
                if (nodeList[n].dimensionNodeList.Count != rhs.nodeList[n].dimensionNodeList.Count)
                {
                    return false;
                }
                else if (nodeList[n].dimensionNodeList.Count != 0)
                {
                    for (int m = 0; m < nodeList[n].dimensionNodeList.Count; m++)
                    {
                        if (nodeList[n].dimensionNodeList[m].symbol.name != rhs.nodeList[n].dimensionNodeList[m].symbol.name)
                        {
                            return false;
                        }
                    }
                }
                if (!nodeList[n].IsEqual(rhs.nodeList[n]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
