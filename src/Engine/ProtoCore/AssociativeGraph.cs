using System.Collections.Generic;
using ProtoCore.Utils;
using System;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using System.Linq;

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
        /// Builds the dependencies within the list of graphNodes
        /// </summary>
        /// <param name="graphNodeScopeToCheck"></param>
        public static void BuildGraphNodeDependencies(List<AssociativeGraph.GraphNode> graphNodesInScope)
        {
            if (graphNodesInScope == null)
            {
                return;
            }

            // Get the current graphnode to check against the list
            //  [a = 10]  -> this one
            //  c = 1
            //  b = a
            //  d = a
            for (int i = 0; i < graphNodesInScope.Count; ++i)
            {
                AssociativeGraph.GraphNode currentNode = graphNodesInScope[i];
                if (!currentNode.isActive)
                {
                    continue;
                }
                // Get the graphnode to check if it depends on nodeToCheckAgainstList
                //  a = 10 (currentnode)
                //  c = 1  (gnode) -> this is checked if it depends on [a]. If it does, add it to the dependency list of  a = 10 (currentnode)
                //  b = a  (gnode) -> next
                //  d = a  (gnode) -> next
                for (int j = 0; j < graphNodesInScope.Count; ++j)
                {
                    AssociativeGraph.GraphNode gnode = graphNodesInScope[j];
                    if (currentNode.UID != gnode.UID)
                    {
                        if (!gnode.isActive)
                        {
                            continue;
                        }

                        //
                        // Associative update within an expression only allows downstream update
                        //  Case 1
                        //  a = a + 1
                        //      [0] t0 = a
                        //      [1] t1 = t0 + 1
                        //      [2] a = t1  <- The final assignment to 'a' should not re-execute [0] as they are part of the same expression
                        //
                        //  Case 2
                        //  a = b + c
                        //  b = 2
                        //      [0] t0 = b
                        //      [1] t1 = c
                        //      [2] t2 = t0 + t1
                        //      [3] a = t2  
                        //      [3] b = 2   <- Modifying 'b' should re-execute [0], [2] and [3]
                        //

                        bool isUpdatableNode = currentNode.updateNodeRefList != null && currentNode.updateNodeRefList.Count > 0;
                        if (!isUpdatableNode)
                        {
                            continue;
                        }

                        bool nodesAreSelfModifying = AreNodesSelfModifyingAndEqualLHS(currentNode, gnode);
                        if (nodesAreSelfModifying)
                        {
                            continue;
                        }

                        bool nodesAreSelfModifyingIdentList = AreNodesSelfModifyingAndEqualIdentList(currentNode, gnode);
                        if (nodesAreSelfModifyingIdentList)
                        {
                            continue;
                        }

                        bool canUpdate = DoesExecutingNodeAffectOtherNode(currentNode, gnode);
                        if (!canUpdate)
                        {
                            continue;
                        }

                        // No update for auto generated temps
                        bool isTempVarUpdate = IsTempVarLHS(currentNode);
                        if (isTempVarUpdate)
                        {
                            continue;
                        }


                        bool equalIdentList = AreLHSEqualIdentList(currentNode, gnode);
                        if (equalIdentList)
                        {
                            continue;
                        }

                        currentNode.PushGraphNodeToExecute(gnode);
                    }
                }
            }
        }

        public static bool IsTempVarLHS(AssociativeGraph.GraphNode graphNode)
        {
            Validity.Assert(graphNode != null);
            if(graphNode.updateNodeRefList.Count > 0)
            {
                return graphNode.updateNodeRefList[0].nodeList[0].symbol.name.Equals(Constants.kTempVar);
            }
            return false;
        }

        /// <summary>
        /// Check if executing 'execNode' will cause re-execution 'otherNode'
        /// </summary>
        /// <param name="executingNode"></param>
        /// <param name="otherNode"></param>
        private static bool DoesExecutingNodeAffectOtherNode(AssociativeGraph.GraphNode execNode, AssociativeGraph.GraphNode otherNode)
        {
            bool isWithinSSAExpression = execNode.ssaExpressionUID == otherNode.ssaExpressionUID;
            bool isDownstreamUpdate = isWithinSSAExpression && execNode.UID < otherNode.UID;
            bool isUpdatable = !isWithinSSAExpression || isDownstreamUpdate;
            if (!isUpdatable)
            {
                return false;
            }

            AssociativeGraph.GraphNode dependent = null;
            bool doesOtherNodeDependOnExecNode = false; 
            foreach (AssociativeGraph.UpdateNodeRef nodeRef in execNode.updateNodeRefList)
            {
                if (otherNode.DependsOn(nodeRef, ref dependent))
                {
                    doesOtherNodeDependOnExecNode = true;
                    break;
                }
            }

            // Other conditions can go here

            return doesOtherNodeDependOnExecNode;
        }


        /// <summary>
        /// Check if both nodes are self modifying and have equal lhs
        /// Example cases:
        ///     x = x + 1 is equal to x = x + 1
        ///     x = x + 1 is equal to x = a + x + 1
        /// </summary>
        /// <param name="varAssignNode"></param>
        /// <param name="inspectNode"></param>
        /// <returns></returns>
        private static bool AreNodesSelfModifyingAndEqualLHS(AssociativeGraph.GraphNode varAssignNode, AssociativeGraph.GraphNode inspectNode)
        {
            // Check if self modifying
            bool areNodesSelfModifying = varAssignNode.IsModifier && inspectNode.IsModifier;
            if (!areNodesSelfModifying)
            {
                return false;
            }

            // Check if varAssignNode is indeed the final assignment node
            //  x = x + 1
            //      [0] t0 = x
            //      [1] t1 = t0 + 1;
            //      [2] x = t1  <- This is the varAssignNode
            if (!varAssignNode.IsLastNodeInSSA)
            {
                return false; 
            }

            bool canInspectNodes =
                varAssignNode != null && inspectNode != null
                && varAssignNode.updateNodeRefList.Count > 0 && inspectNode.updateNodeRefList.Count > 0;
            if (!canInspectNodes)
            {
                return false;
            }

            // Check if they are from different expressions
            bool isWithinSSAExpression = varAssignNode.ssaExpressionUID == inspectNode.ssaExpressionUID;
            if (isWithinSSAExpression)
            {
                return false;
            }

            // Check for equal LHS
            AssociativeGraph.GraphNode assignNode = inspectNode.lastGraphNode;
            bool isValidAssignNode = assignNode != null && assignNode.updateNodeRefList.Count > 0;
            if (!isValidAssignNode)
            {
                return false;
            }

            bool areLHSEqual = AreLHSEqual(varAssignNode, assignNode);  
            return areLHSEqual;
        }


        /// <summary>
        /// Check if both nodes are self modifying and have equal lhs
        /// Example cases:
        ///     x.y = x.y + 1 is equal to x.y = x.y + 1
        ///     x.y = x.y + 1 is equal to x.y = a + x.y + 1
        /// </summary>
        /// <param name="varAssignNode"></param>
        /// <param name="inspectNode"></param>
        /// <returns></returns>
        private static bool AreNodesSelfModifyingAndEqualIdentList(AssociativeGraph.GraphNode varAssignNode, AssociativeGraph.GraphNode inspectNode)
        {
            AssociativeGraph.GraphNode node1 = varAssignNode.lastGraphNode;
            AssociativeGraph.GraphNode node2 = inspectNode.lastGraphNode;

            bool isUpdateable = node1 != null && node2 != null;
            if (!isUpdateable)
            {
                return false;
            }

            // Check if their lhs are equal identlists
            return AreLHSEqualIdentList(node1, node2);
        }

        /// <summary>
        /// Checks if both nodes are LHS identlists and that their identlists are equal
        /// </summary>
        /// <param name="executingNode"></param>
        /// <param name="dependentNode"></param>
        /// <returns></returns>
        public static bool AreLHSEqualIdentList(AssociativeGraph.GraphNode node, AssociativeGraph.GraphNode otherNode)
        {
            bool areBothLHSIdentList = node.IsLHSIdentList && otherNode.IsLHSIdentList;
            if (!areBothLHSIdentList)
            {
                return false;
            }

            for (int n = 0; n < node.updateNodeRefList.Count; ++n)
            {
                // Only check for identlists where the nodeList > 1 
                // nodeList contains all the symbols in an identlist 
                // a.b.c -> nodeList.Count == 3
                if (node.updateNodeRefList[n].nodeList.Count > 1)
                {
                    if (!node.updateNodeRefList[n].Equals(otherNode.updateNodeRefList[n]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Checks if the lhs (updateNodeRefList) are equal for both graphnodes
        /// </summary>
        /// <param name="node"></param>
        /// <param name="otherNode"></param>
        /// <returns></returns>
        private static bool AreLHSEqual(AssociativeGraph.GraphNode node, AssociativeGraph.GraphNode otherNode)
        {
            Validity.Assert(node != null && otherNode != null);
            Validity.Assert(node.updateNodeRefList.Count > 0);

            // Check for same number of noderefs
            if (node.updateNodeRefList.Count != otherNode.updateNodeRefList.Count)
            {
                return false;
            }

            return node.updateNodeRefList.SequenceEqual(otherNode.updateNodeRefList);
        }

        /// <summary>
        /// Returns the VM Graphnodes associated with the input ASTs
        /// </summary>
        /// <param name="core"></param>
        /// <param name="astList"></param>
        /// <returns></returns>
        public static List<AssociativeGraph.GraphNode> GetGraphNodesFromAST(ProtoCore.DSASM.Executable exe, List<AST.AssociativeAST.AssociativeNode> astList)
        {
            List<AssociativeGraph.GraphNode> deltaGraphNodes = new List<AssociativeGraph.GraphNode>();

            // Get nodes at global scope
            int classIndex = Constants.kInvalidIndex;
            int procIndex = Constants.kGlobalScope;
            int blockScope = (int)Executable.OffsetConstants.kInstrStreamGlobalScope;
            AssociativeGraph.DependencyGraph dependencyGraph = exe.instrStreamList[blockScope].dependencyGraph;
            List<AssociativeGraph.GraphNode> graphNodesInScope = dependencyGraph.GetGraphNodesAtScope(classIndex, procIndex);


            // Get a list of AST guids 
            List<Guid> astGuidList = new List<Guid>();
            foreach (AST.AssociativeAST.AssociativeNode ast in astList)
            {
                AST.AssociativeAST.BinaryExpressionNode bnode = ast as AST.AssociativeAST.BinaryExpressionNode;
                if (!astGuidList.Contains(bnode.guid))
                {
                    astGuidList.Add(bnode.guid);
                }
            }

            // For every graphnode in scope, find the graphodes that have the same guid as the guids in astList
            foreach (AssociativeGraph.GraphNode graphNode in graphNodesInScope)
            {
                foreach (Guid guid in astGuidList)
                {
                    if (graphNode.guid == guid)
                    {
                        deltaGraphNodes.Add(graphNode);
                    }
                }
            }
            return deltaGraphNodes;
        }

        /// <summary>
        /// Determines if at least one graphnode in the glboal scope is dirty
        /// </summary>
        /// <param name="exe"></param>
        /// <returns></returns>
        public static bool IsGlobalScopeDirty(Executable exe)
        {
            Validity.Assert(exe != null);
            var graph = exe.instrStreamList[0].dependencyGraph;
            var graphNodes = graph.GetGraphNodesAtScope(Constants.kInvalidIndex, Constants.kGlobalScope);
            if (graphNodes != null)
            {
                foreach (AssociativeGraph.GraphNode graphNode in graphNodes)
                {
                    if (graphNode.isDirty)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Find and return all graphnodes that can be reached by executingGraphNode
        /// </summary>
        /// <param name="executingGraphNode"></param>
        /// <param name="executive"></param>
        /// <param name="exprUID"></param>
        /// <param name="modBlkId"></param>
        /// <param name="isSSAAssign"></param>
        /// <param name="executeSSA"></param>
        /// <param name="languageBlockID"></param>
        /// <param name="propertyChanged"></param>
        /// <returns></returns>
        public static List<AssociativeGraph.GraphNode> UpdateDependencyGraph(
            AssociativeGraph.GraphNode executingGraphNode,
            DSASM.Executive executive,
            int exprUID,
            int modBlkId,
            bool isSSAAssign,
            bool executeSSA,
            int languageBlockID,
            bool recursiveSearch,
            bool propertyChanged = false)
        {
            AssociativeGraph.DependencyGraph dependencyGraph = executive.exe.instrStreamList[languageBlockID].dependencyGraph;
            List<AssociativeGraph.GraphNode> reachableGraphNodes = new List<AssociativeGraph.GraphNode>();

            if (executingGraphNode == null)
            {
                return reachableGraphNodes;
            }

            int classIndex = executingGraphNode.classIndex;
            int procIndex = executingGraphNode.procIndex;

            var graph = dependencyGraph;
            var graphNodes = graph.GetGraphNodesAtScope(classIndex, procIndex);
            if (graphNodes == null)
            {
                return reachableGraphNodes;
            }

            //foreach (var graphNode in graphNodes)
            for (int i = 0; i < graphNodes.Count; ++i)
            {
                var graphNode = graphNodes[i];

                // If the graphnode is inactive then it is no longer executed
                if (!graphNode.isActive)
                {
                    continue;
                }

                //
                // Comment Jun: 
                //      This is clarifying the intention that if the graphnode is within the same SSA expression, we still allow update
                //
                bool allowUpdateWithinSSA = false;
                if (executeSSA)
                {
                    allowUpdateWithinSSA = true;
                    isSSAAssign = false; // Remove references to this when ssa flag is removed

                    // Do not update if its a property change and the current graphnode is the same expression
                    if (propertyChanged && graphNode.exprUID == executingGraphNode.exprUID)
                    {
                        continue;
                    }
                }
                else
                {
                    // TODO Jun: Remove this code immediatley after enabling SSA
                    bool withinSSAStatement = graphNode.UID == executingGraphNode.UID;
                    allowUpdateWithinSSA = !withinSSAStatement;
                }

                if (!allowUpdateWithinSSA || (propertyChanged && graphNode == executingGraphNode))
                {
                    continue;
                }

                foreach (var noderef in executingGraphNode.updateNodeRefList)
                {
                    // If this dirty graphnode is an associative lang block, 
                    // then find all that nodes in that lang block and mark them dirty
                    if (graphNode.isLanguageBlock)
                    {
                        List<AssociativeGraph.GraphNode> subGraphNodes = ProtoCore.AssociativeEngine.Utils.UpdateDependencyGraph(
                            executingGraphNode, executive, exprUID, modBlkId, isSSAAssign, executeSSA, graphNode.languageBlockId, recursiveSearch);
                        if (subGraphNodes.Count > 0)
                        {
                            reachableGraphNodes.Add(graphNode);
                        }
                    }

                    AssociativeGraph.GraphNode matchingNode = null;
                    if (!graphNode.DependsOn(noderef, ref matchingNode))
                    {
                        continue;
                    }

                    // @keyu: if we are modifying an object's property, e.g.,
                    // 
                    //    foo.id = 42;
                    //
                    // both dependent list and update list of the corresponding 
                    // graph node contains "foo" and "id", so if property "id"
                    // is changed, this graph node will be re-executed and the
                    // value of "id" is incorrectly set back to old value.
                    if (propertyChanged)
                    {
                        var depUpdateNodeRef = graphNode.dependentList[0].updateNodeRefList[0];
                        if (graphNode.updateNodeRefList.Count == 1)
                        {
                            var updateNodeRef = graphNode.updateNodeRefList[0];
                            if (depUpdateNodeRef.Equals(updateNodeRef))
                            {
                                continue;
                            }
                        }
                    }

                    //
                    // Comment Jun: We dont want to cycle between such statements:
                    //
                    // a1.a = 1;
                    // a1.a = 10;
                    //

                    Validity.Assert(null != matchingNode);
                    bool isLHSModification = matchingNode.isLHSNode;
                    bool isUpdateable = matchingNode.IsUpdateableBy(noderef);

                    // isSSAAssign means this is the graphnode of the final SSA assignment
                    // Overrride this if allowing within SSA update
                    // TODO Jun: Remove this code when SSA is completely enabled
                    bool allowSSADownstream = false;
                    if (executeSSA)
                    {
                        // Check if we allow downstream update
                        if (exprUID == graphNode.exprUID)
                        {
                            allowSSADownstream = graphNode.AstID > executingGraphNode.AstID;
                        }
                    }


                    // Comment Jun: 
                    //      If the triggered dependent graphnode is LHS 
                    //          and... 
                    //      the triggering node (executing graphnode)
                    if (isLHSModification && !isUpdateable)
                    {
                        break;
                    }

                    // TODO Jun: Optimization - Reimplement update delta evaluation using registers
                    //if (IsNodeModified(EX, FX))
                    bool isLastSSAAssignment = (exprUID == graphNode.exprUID) && graphNode.IsLastNodeInSSA && !graphNode.isReturn;
                    if (exprUID != graphNode.exprUID && modBlkId != graphNode.modBlkUID)
                    {
                        UpdateModifierBlockDependencyGraph(graphNode, dependencyGraph.GraphList);
                    }
                    else if (allowSSADownstream
                                || isSSAAssign
                                || isLastSSAAssignment
                                || (exprUID != graphNode.exprUID
                                    && modBlkId == Constants.kInvalidIndex
                                    && graphNode.modBlkUID == Constants.kInvalidIndex)
                        )
                    {
                        if (graphNode.isCyclic)
                        {
                            // If the graphnode is cyclic, mark it as not dirst so it wont get executed 
                            // Sets its cyclePoint graphnode to be not dirty so it also doesnt execute.
                            // The cyclepoint is the other graphNode that the current node cycles with
                            graphNode.isDirty = false;
                            if (null != graphNode.cyclePoint)
                            {
                                graphNode.cyclePoint.isDirty = false;
                                graphNode.cyclePoint.isCyclic = true;
                            }
                        }
                        else if (!graphNode.isDirty)
                        {
                            graphNode.forPropertyChanged = propertyChanged;
                            reachableGraphNodes.Add(graphNode);

                            // On debug mode:
                            //      we want to mark all ssa statements dirty for an if the lhs pointer is a new instance.
                            //      In this case, the entire line must be re-executed
                            //      
                            //  Given:
                            //      x = 1
                            //      p = p.f(x) 
                            //      x = 2
                            //
                            //  To SSA:
                            //
                            //      x = 1
                            //      t0 = p -> we want to execute from here of member function 'f' returned a new instance of 'p'
                            //      t1 = x
                            //      t2 = t0.f(t1)
                            //      p = t2
                            //      x = 2
                            if (null != executingGraphNode.lastGraphNode && executingGraphNode.lastGraphNode.reExecuteExpression)
                            {
                                executingGraphNode.lastGraphNode.reExecuteExpression = false;
                                // TODO Jun: Perform reachability analysis at compile time so the first node can  be determined statically at compile time
                                var firstGraphNode = AssociativeEngine.Utils.GetFirstSSAGraphnode(i - 1, graphNodes);
                                reachableGraphNodes.Add(firstGraphNode);
                                
                            }

                            // When a graphnode is dirty, recursively search of other graphnodes that may be affected
                            // Recursive search is only done statically 
                            if (recursiveSearch)
                            {
                                List<AssociativeGraph.GraphNode> subGraphNodes = ProtoCore.AssociativeEngine.Utils.UpdateDependencyGraph(
                                    graphNode,
                                    executive,
                                    graphNode.exprUID,
                                    graphNode.modBlkUID,
                                    graphNode.IsSSANode(),
                                    executeSSA,
                                    graphNode.languageBlockId,
                                    recursiveSearch);
                                if (subGraphNodes.Count > 0)
                                {
                                    reachableGraphNodes.AddRange(subGraphNodes);
                                }
                            }
                        }
                    }
                }
            }
            return reachableGraphNodes;
        }

        //
        // Comment Jun: Revised 
        //
        //  proc UpdateGraphNodeDependency(execnode)
        //      foreach node in graphnodelist 
        //          if execnode.lhs is equal to node.lhs
        //              if execnode.HasDependents() 
        //                  if execnode.Dependents() is not equal to node.Dependents()
        //                      node.RemoveDependents()
        //                  end
        //              end
        //          end
        //      end
        //  end
        //

        /// <summary>
        /// Determines if a graphnode was redefined by executingNode
        /// Given:
        ///     a = b;
        ///     a = 1; 
        ///  Where: 'a = b' has been redefined by 'a = 1'
        ///  
        /// </summary>
        /// <param name="gnode"></param>
        /// <param name="executingNode"></param>
        /// <returns></returns>
        public static bool IsGraphNodeRedefined(AssociativeGraph.GraphNode gnode, AssociativeGraph.GraphNode executingNode)
        {
            if (gnode.UID >= executingNode.UID // for previous graphnodes
                || gnode.updateNodeRefList.Count == 0
                || gnode.updateNodeRefList.Count != executingNode.updateNodeRefList.Count
                || gnode.isAutoGenerated)
            {
                return false;
            }

            // Check if the updateNodeRefList is equal for both graphnodes
            // In code form, it checks if the LHS symbols of an assignment stmt are equal
            if (!gnode.updateNodeRefList.SequenceEqual(executingNode.updateNodeRefList))
            {
                return false;
            }

            return true;
        }

        public static void UpdateModifierBlockDependencyGraph(AssociativeGraph.GraphNode graphNode, List<AssociativeGraph.GraphNode> graphNodeList)
        {
            int modBlkUID = graphNode.modBlkUID;
            int index = graphNode.UID;
            bool setModifierNode = true;
            if (graphNode.isCyclic)
            {
                // If the graphnode is cyclic, mark it as not first so it wont get executed 
                // Sets its cyclePoint graphnode to be not dirty so it also doesnt execute.
                // The cyclepoint is the other graphNode that the current node cycles with
                graphNode.isDirty = false;
                if (null != graphNode.cyclePoint)
                {
                    graphNode.cyclePoint.isDirty = false;
                    graphNode.cyclePoint.isCyclic = true;
                }
                setModifierNode = false;
            }

            if (modBlkUID != Constants.kInvalidIndex)
            {
                for (int i = index; i < graphNodeList.Count; ++i)
                {
                    AssociativeGraph.GraphNode node = graphNodeList[i];
                    if (node.modBlkUID == modBlkUID)
                    {
                        node.isDirty = setModifierNode;
                    }
                }
            }
            else
            {
                graphNode.isDirty = true;
            }
        }

        /// <summary>
        /// GetRedefinedGraphNodes will return a list of graphnodes that have been redefined by executingGraphNode
        /// 
        /// Given:
        ///     [1] a = b + c
        ///     [2] a = d
        /// Statement [1] has been redefined by statment [2]    
        /// Return true if this has occured
        /// 
        /// </summary>
        /// <param name="executingGraphNode"></param>
        /// <param name="classScope"></param>
        /// <param name="functionScope"></param>
        public static List<AssociativeGraph.GraphNode> GetRedefinedGraphNodes(RuntimeCore runtimeCore, AssociativeGraph.GraphNode executingGraphNode, List<AssociativeGraph.GraphNode> nodesInScope, int classScope, int functionScope)
        {
            List<AssociativeGraph.GraphNode> redefinedNodes = new List<AssociativeGraph.GraphNode>();
            if (executingGraphNode != null)
            {
                // Remove this condition when full SSA is enabled
                bool isssa = (!executingGraphNode.IsSSANode() && executingGraphNode.DependsOnTempSSA());

                if (runtimeCore.Options.ExecuteSSA)
                {
                    isssa = executingGraphNode.IsSSANode();
                }
                if (!isssa)
                {
                    foreach (AssociativeGraph.GraphNode graphNode in nodesInScope)
                    {
                        bool allowRedefine = true;

                        SymbolNode symbol = executingGraphNode.updateNodeRefList[0].nodeList[0].symbol;
                        bool isMember = symbol.classScope != Constants.kInvalidIndex
                            && symbol.functionIndex == Constants.kInvalidIndex;

                        if (isMember)
                        {
                            // For member vars, do not allow if not in the same scope
                            if (symbol.classScope != graphNode.classIndex || symbol.functionIndex != graphNode.procIndex)
                            {
                                allowRedefine = false;
                            }
                        }

                        if (allowRedefine)
                        {
                            // Check if graphnode was redefined by executingGraphNode
                            if (AssociativeEngine.Utils.IsGraphNodeRedefined(graphNode, executingGraphNode))
                            {
                                redefinedNodes.Add(graphNode);
                            }
                        }
                    }
                }
            }
            return redefinedNodes;
        }


        /// <summary>
        /// Find the first dirty node of the graphnode residing at indexOfDirtyNode
        /// </summary>
        /// <param name="indexOfDirtyNode"></param>
        /// <param name="nodesInScope"></param>
        /// <returns></returns>
        public static ProtoCore.AssociativeGraph.GraphNode GetFirstSSAGraphnode(int indexOfDirtyNode, List<AssociativeGraph.GraphNode> nodesInScope)
        {
            while (nodesInScope[indexOfDirtyNode].IsSSANode())
            {
                --indexOfDirtyNode;
                if (indexOfDirtyNode < 0)
                {
                    // In this case, the first SSA statemnt is the first graphnode
                    break;
                }

                Validity.Assert(indexOfDirtyNode >= 0);
            }
            return nodesInScope[indexOfDirtyNode + 1];
        }

       


        /// <summary>
        ///  Finds all graphnodes associated with each AST and marks them dirty. Returns the first dirty node
        /// </summary>
        /// <param name="core"></param>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public static AssociativeGraph.GraphNode MarkGraphNodesDirtyAtGlobalScope
(RuntimeCore core, IEnumerable<AST.AssociativeAST.AssociativeNode> nodeList)
        {
            if (nodeList == null)
            {
                return null;
            }

            AssociativeGraph.GraphNode firstDirtyNode = null;
            foreach (var node in nodeList)
            {
                var bNode = node as AST.AssociativeAST.BinaryExpressionNode;
                if (bNode == null)
                {
                    continue;
                }

                foreach (var gnode in core.DSExecutable.instrStreamList[0].dependencyGraph.GetGraphNodesAtScope(Constants.kInvalidIndex, Constants.kGlobalScope))
                {
                    if (gnode.isActive && gnode.OriginalAstID == bNode.OriginalAstID)
                    {
                        
                        gnode.isDirty = true;
                        gnode.isActive = true;
                        if (gnode.updateBlock.updateRegisterStartPC != Constants.kInvalidIndex)
                        {
                            gnode.updateBlock.startpc = gnode.updateBlock.updateRegisterStartPC;
                        }
                        if (firstDirtyNode == null)
                        {
                            firstDirtyNode = gnode;
                        }
                    }
                }
            }
            return firstDirtyNode;
        }

        public static void MarkGraphNodesDirtyFromFunctionRedef(RuntimeCore runtimeCore, List<AST.AssociativeAST.AssociativeNode> fnodeList)
        {
            bool entrypointSet = false;
            foreach (var node in fnodeList)
            {
                var fnode = node as AST.AssociativeAST.FunctionDefinitionNode;
                if (null == fnode)
                {
                    continue;
                }

                int exprId = Constants.kInvalidIndex;
                foreach (var gnode in runtimeCore.DSExecutable.instrStreamList[0].dependencyGraph.GraphList)
                {
                    if (gnode.isActive)
                    {
                        if (null != gnode.firstProc)
                        {
                            if (fnode.Name == gnode.firstProc.name && fnode.Signature.Arguments.Count == gnode.firstProc.argInfoList.Count)
                            {
                                if (Constants.kInvalidIndex == exprId)
                                {
                                    exprId = gnode.exprUID;
                                    if (!entrypointSet)
                                    {
                                        runtimeCore.SetStartPC(gnode.updateBlock.startpc);
                                        entrypointSet = true;
                                    }
                                }
                                gnode.isDirty = true;
                            }
                        }
                        else if (Constants.kInvalidIndex != exprId)
                        {
                            if (gnode.exprUID == exprId)
                            {
                                gnode.isDirty = true;
                                if (gnode.IsLastNodeInSSA)
                                {
                                    exprId = Constants.kInvalidIndex;
                                }
                            }
                        }
                    }
                }
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
        public int updateRegisterStartPC { get; set; }

        public UpdateBlock()
        {
            startpc = Constants.kInvalidIndex;
            endpc = Constants.kInvalidIndex;
            updateRegisterStartPC = Constants.kInvalidIndex;
        }
    }

    public class GraphNode
    {
        public int ssaExpressionUID { get; set; }
        public bool IsModifier { get; set; }    // Flags if a graphnode is part of a statement that performs self assignment (the LHS appears on the RHS)
        public int UID { get; set; }
        public Guid guid {get; set;}
        public int dependencyGraphListID { get; set; }
        public int AstID { get; set; }
        public int OriginalAstID { get; set; }    // The original AST that this graphnode is associated with
        public int exprUID { get; set; }
        public int ssaExprID { get; set; }
        public int modBlkUID { get; set; }
        public string CallsiteIdentifier { get; set; }
        public List<UpdateNode> dimensionNodeList { get; set; }
        public List<UpdateNodeRef> updateNodeRefList { get; set; }
        public bool isDirty { get; set; }
        public bool isDeferred { get; set; }
        public bool isReturn { get; set; }
        public int procIndex { get; set; }              // Function that this graph resides in
        public int classIndex { get; set; }             // Class index that this graph resides in
        public bool ProcedureOwned { get; set; }       // This graphnode's immediate scope is within a function (as opposed to languageblock or construct)
        public UpdateBlock updateBlock { get; set; }
        public List<GraphNode> dependentList { get; set; }
        public List<GraphNode> graphNodesToExecute { get; set; }
        public bool allowDependents { get; set; }
        public bool isIndexingLHS { get; set; }
        public bool isLHSNode { get; set; }
        public bool IsLHSIdentList { get; set; }
        public ProcedureNode firstProc { get; set; }
        public int firstProcRefIndex { get; set; }
        public bool isCyclic { get; set; }
        public bool isInlineConditional { get; set; }
        public GraphNode cyclePoint { get; set; }
        public bool isAutoGenerated { get; set; }
        public bool isLanguageBlock { get; set; }
        public int languageBlockId { get; set; }
        public List<StackValue> updateDimensions { get; set; }
        public int counter { get; set; }
        public ReplicationControl replicationControl {get; set;}
        public bool propertyChanged { get; set; }       // The property of ffi object that created in this graph node is changed
        public bool forPropertyChanged { get; set; }    // The graph node is marked as dirty because of property changed event

        public GraphNode lastGraphNode { get; set; }    // This is the last graphnode of an SSA'd statement

        public List<UpdateNodeRef> updatedArguments { get; set; }


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

        public GraphNode()
        {
            IsModifier = false;
            UID = Constants.kInvalidIndex;
            AstID = Constants.kInvalidIndex;
            dependencyGraphListID = Constants.kInvalidIndex;
            CallsiteIdentifier = string.Empty;
            dimensionNodeList = new List<UpdateNode>();
            updateNodeRefList = new List<UpdateNodeRef>();
            isDirty = true;
            isDeferred = false;
            isReturn = false;
            procIndex = Constants.kGlobalScope;
            classIndex = Constants.kInvalidIndex;
            updateBlock = new UpdateBlock();
            dependentList = new List<GraphNode>();
            graphNodesToExecute = new List<GraphNode>();
            allowDependents = true;
            isIndexingLHS = false;
            isLHSNode = false;
            IsLHSIdentList = false;
            firstProc = null;
            firstProcRefIndex = Constants.kInvalidIndex;
            isCyclic = false;
            isInlineConditional = false;
            counter = 0;
            updatedArguments = new List<UpdateNodeRef>();
            isAutoGenerated = false;
            isLanguageBlock = false;
            languageBlockId = Constants.kInvalidIndex;
            updateDimensions = new List<StackValue>();
            propertyChanged = false;
            forPropertyChanged = false;
            lastGraphNode = null;
            isActive = true;
            symbolListWithinExpression = new List<SymbolNode>();
            reExecuteExpression = false;
            SSASubscript = Constants.kInvalidIndex;
            IsLastNodeInSSA = false;
        }


        public void PushGraphNodeToExecute(GraphNode dependent)
        {
            // Do not add if it already contains this dependent
            foreach (GraphNode node in graphNodesToExecute)
            {
                if (node.UID == dependent.UID)
                {
                    return;
                }
            }
            graphNodesToExecute.Add(dependent);
        }

        public void PushDependent(GraphNode dependent)
        {
            if (!allowDependents)
            {
                return;
            }

            bool exists = false;
            foreach (GraphNode gnode in dependentList)
            {
                if (dependent.updateNodeRefList[0].nodeList[0].Equals(gnode.updateNodeRefList[0].nodeList[0]))
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                if (dependent.UID != Constants.kInvalidIndex)
                {
                    dependent.UID = dependentList.Count;
                }
                dependentList.Add(dependent);
            }
        }

        public void ResolveLHSArrayIndex()
        {
            if (dimensionNodeList.Count > 0)
            {
                int last = updateNodeRefList[0].nodeList.Count - 1;
                updateNodeRefList[0].nodeList[last].dimensionNodeList.AddRange(dimensionNodeList);
            }
        }

        public void PushSymbolReference(SymbolNode symbol, UpdateNodeType type = UpdateNodeType.kSymbol)
        {
            Validity.Assert(null != symbol);
            UpdateNode updateNode = new UpdateNode();
            updateNode.symbol = symbol;
            updateNode.nodeType = type;

            UpdateNodeRef nodeRef = new UpdateNodeRef();
            nodeRef.PushUpdateNode(updateNode);

            updateNodeRefList.Add(nodeRef);
        }

        public void PushSymbolReference(SymbolNode symbol)
        {
            Validity.Assert(null != symbol);
            UpdateNode updateNode = new UpdateNode();
            updateNode.symbol = symbol;
            updateNode.nodeType = UpdateNodeType.kSymbol;

            UpdateNodeRef nodeRef = new UpdateNodeRef();
            nodeRef.block = symbol.runtimeTableIndex;
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
                    UpdateNode updateNode = modifiedRef.nodeList[n];
                    if (!updateNode.Equals(updateNodeRefList[0].nodeList[n]))
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

            foreach (var dependent in dependentList)
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

            foreach (var dependent in dependentList)
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

        public bool DependsOn(UpdateNodeRef modifiedRef, ref GraphNode dependentNode)
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
                                        if (modifiedRef.nodeList[0].symbol.functionIndex == Constants.kInvalidIndex)
                                        {
                                            inImperative = inImperative
                                                && (depNodeRef.nodeList[m].symbol.functionIndex == Constants.kInvalidIndex)
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
                                        if (modifiedRef.nodeList[m].symbol.functionIndex == Constants.kInvalidIndex)
                                        {
                                            inImperative = inImperative
                                                && (depNodeRef.nodeList[m].symbol.functionIndex == Constants.kInvalidIndex)
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
                        bothSymbolsMatch = modifiedRef.nodeList[0].symbol.Equals(depNodeRef.nodeList[0].symbol);


                        bothSymbolsStatic =
                            modifiedRef.nodeList[0].symbol.memregion == MemoryRegion.kMemStatic
                            && depNodeRef.nodeList[0].symbol.memregion == MemoryRegion.kMemStatic
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
                                        bothSymbolsMatch = modDimNode.symbol.Equals(depDimNode.symbol);
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
                            bothSymbolsMatch = modifiedRef.nodeList[m].symbol.Equals(depNodeRef.nodeList[m].symbol);
                            bothSymbolsStatic =
                                modifiedRef.nodeList[m].symbol.memregion == MemoryRegion.kMemStatic
                                && depNodeRef.nodeList[m].symbol.memregion == MemoryRegion.kMemStatic
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
            foreach (GraphNode dnode in dependentList)
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

            return CoreUtils.IsSSATemp(updateNodeRefList[0].nodeList[0].symbol.name);
        }
    }

    public class DependencyGraph
    {
        private readonly Core core;
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

        /// <summary>
        /// Marks all graphnodes in scope as dirty
        /// </summary>
        /// <param name="block"></param>
        /// <param name="classIndex"></param>
        /// <param name="procIndex"></param>
        public void MarkAllGraphNodesDirty(int block, int classIndex, int procIndex)
        {
            List<GraphNode> gnodeList = GetGraphNodesAtScope(classIndex, procIndex);
            if (gnodeList != null)
            {
                foreach (GraphNode gnode in gnodeList)
                {
                    if (gnode.languageBlockId == block)
                    {
                        gnode.isDirty = true;
                    }
                }
            }
        }


        /// <summary>
        /// Gets the graphnode of the given pc and scope
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="classIndex"></param>
        /// <param name="procIndex"></param>
        /// <returns></returns>
        public GraphNode GetGraphNode(int pc, int classIndex, int procIndex)
        {
            List<GraphNode> gnodeList = GetGraphNodesAtScope(classIndex, procIndex);
            if (gnodeList != null && gnodeList.Count > 0)
            {
                foreach (GraphNode gnode in gnodeList)
                {
                    if (gnode.isActive && gnode.isDirty && gnode.updateBlock.startpc == pc)
                    {
                        return gnode;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Gets the first dirty graphnode starting from the given pc
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="classIndex"></param>
        /// <param name="procIndex"></param>
        /// <returns></returns>
        public GraphNode GetFirstDirtyGraphNode(int pc, int classIndex, int procIndex)
        {
            List<GraphNode> gnodeList = GetGraphNodesAtScope(classIndex, procIndex);
            if (gnodeList != null && gnodeList.Count > 0)
            {
                foreach (GraphNode gnode in gnodeList)
                {
                    if (gnode.isActive && gnode.isDirty && gnode.updateBlock.startpc >= pc)
                    {
                        return gnode;
                    }
                }
            }
            return null;
        }


        private ulong GetGraphNodeKey(int classIndex, int procIndex)
        {
            uint ci = (uint)classIndex;
            uint pi = (uint)procIndex;
            return (((ulong)ci) << 32) | pi;
        }

        public DependencyGraph(Core core)
        {
            this.core = core;
            graphList = new List<GraphNode>();
            graphNodeMap = new Dictionary<ulong, List<GraphNode>>();
        }

        public List<GraphNode> GetGraphNodesAtScope(int classIndex, int procIndex)
        {
            List<GraphNode> nodes;
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
            List<GraphNode> nodes;
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
        kSymbol,
        kMethod
    };

    public class UpdateNode
    {
        public SymbolNode symbol;
        public ProcedureNode procNode;
        public UpdateNodeType nodeType;

        // This is the list of nodes represting every indexed dimension
        public List<UpdateNode> dimensionNodeList { get; set; }

        public UpdateNode()
        {
            dimensionNodeList = new List<UpdateNode>();
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as UpdateNode;
            if (rhs == null)
            {
                return false;
            }

            if (nodeType != rhs.nodeType)
            {
                return false;
            }

            if (nodeType == UpdateNodeType.kSymbol || nodeType == UpdateNodeType.kLiteral)
            {
                return symbol.Equals(rhs.symbol);
            }
            else if (nodeType == UpdateNodeType.kMethod)
            {
                return procNode.Equals(rhs.procNode);
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
            if (null != rhs && null != rhs.nodeList)
            {
                nodeList = new List<UpdateNode>(rhs.nodeList);
            }
            else
            {
                nodeList = new List<UpdateNode>();
            }
        }

        public void PushUpdateNode(UpdateNode node)
        {
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

        public override bool Equals(object obj)
        {
            var rhs = obj as UpdateNodeRef;
            if (rhs == null)
            {
                return false;
            }

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

                if (!nodeList[n].Equals(rhs.nodeList[n]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
