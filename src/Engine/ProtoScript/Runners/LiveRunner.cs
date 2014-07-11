using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using GraphToDSCompiler;
using ProtoCore.DSASM.Mirror;
using System.Diagnostics;
using ProtoCore.Utils;
using ProtoCore;
using System.ComponentModel;
using System.Threading;
using ProtoFFI;
using ProtoCore.AssociativeGraph;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using System.Linq;
using ProtoCore.DSASM;

namespace ProtoScript.Runners
{
    public enum EventStatus
    {
        OK,
        Error,
        Warning
    }

    /// <summary>
    /// A subtree represents a node in graph. It contains a list of AST node.
    /// </summary>
    public struct Subtree
    {
        public Guid GUID;
        public List<AssociativeNode> AstNodes;
        public bool ForceExecution;

        public Subtree(List<AssociativeNode> astNodes, System.Guid guid)
        {
            GUID = guid;
            AstNodes = astNodes;
            ForceExecution = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GUID.ToString() + " " + ForceExecution + " ");
            if (AstNodes != null)
                AstNodes.ForEach((a) => sb.AppendLine(a.ToString()));
            else
                sb.AppendLine("AstNodes: null");
            return sb.ToString();

        }
    }

    /// <summary>
    /// GraphSyncData contains three lists: Subtrees that are added, modified
    /// and deleted in a session.
    /// </summary>
    public class GraphSyncData
    {
        public List<Subtree> DeletedSubtrees
        {
            get;
            private set;
        }

        public List<Subtree> AddedSubtrees
        {
            get;
            private set;
        }

        public List<Subtree> ModifiedSubtrees
        {
            get;
            private set;
        }

        public GraphSyncData(List<Subtree> deleted, List<Subtree> added, List<Subtree> modified)
        {
            DeletedSubtrees = deleted;
            AddedSubtrees = added;
            ModifiedSubtrees = modified;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SyncData");
            sb.AppendLine("Deleted Subtrees: " + DeletedSubtrees.Count);
            DeletedSubtrees.ForEach((t) => sb.AppendLine("\t" + t.ToString()));

            sb.AppendLine("Added Subtrees: " + AddedSubtrees.Count);
            AddedSubtrees.ForEach((t) => sb.AppendLine("\t" + t.ToString()));

            sb.AppendLine("Modified Subtrees: " + ModifiedSubtrees.Count);
            ModifiedSubtrees.ForEach((t) => sb.AppendLine("\t" + t.ToString()));

            return sb.ToString();
        }
    }

    /// <summary>
    /// This is the data returned by ChangeSetComputer and consumed by ChangeSetApplier
    /// </summary>
    public class ChangeSetData
    {
        public ChangeSetData() { }
        public List<AssociativeNode> DeletedBinaryExprASTNodes;
        public List<AssociativeNode> DeletedFunctionDefASTNodes;
        public List<AssociativeNode> RemovedBinaryNodesFromModification;
        public List<AssociativeNode> RemovedFunctionDefNodesFromModification;
        public List<AssociativeNode> ForceExecuteASTList;
        public List<AssociativeNode> ModifiedFunctions;
        public List<AssociativeNode> ModifiedNestedLangBlock;
    }

    /// <summary>
    /// ChangeSetApplier modifes the VM state given the changes computed from a ChangeSetComputer instance
    /// </summary>
    public class ChangeSetApplier
    {
        private ProtoCore.Core core = null;
        public void Apply(ProtoCore.Core core, ChangeSetData changeSet)
        {
            Validity.Assert(null != changeSet);
            this.core = core;
            ApplyChangeSetDeleted(changeSet);
            ApplyChangeSetModified(changeSet);
            ApplyChangeSetForceExecute(changeSet);
        }

        private void ApplyChangeSetDeleted(ChangeSetData changeSet)
        {
            DeactivateGraphnodes(changeSet.DeletedBinaryExprASTNodes);
            UndefineFunctions(changeSet.DeletedFunctionDefASTNodes);
        }

        private void ApplyChangeSetModified(ChangeSetData changeSet)
        {
            ClearModifiedNestedBlocks(changeSet.ModifiedNestedLangBlock);
            DeactivateGraphnodes(changeSet.RemovedBinaryNodesFromModification);
            UndefineFunctions(changeSet.RemovedFunctionDefNodesFromModification);
            // Mark all graphnodes dependent on the modified functions as dirty
            ProtoCore.AssociativeEngine.Utils.MarkGraphNodesDirtyFromFunctionRedef(core, changeSet.ModifiedFunctions);
        }


        private void ApplyChangeSetForceExecute(ChangeSetData changeSet)
        {
            // Mark all graphnodes dirty which are associated with the force exec ASTs
            ProtoCore.AssociativeEngine.Utils.MarkGraphNodesDirty(core, changeSet.ForceExecuteASTList);
        }
        

        /// <summary>
        /// Deactivate a single graphnode regardless of its associated dependencies
        /// </summary>
        /// <param name="nodeList"></param>
        private void DeactivateGraphnodes(List<AssociativeNode> nodeList)
        {
            if (null == nodeList)
            {
                return;
            }

            foreach (var node in nodeList)
            {
                BinaryExpressionNode bNode = node as BinaryExpressionNode;
                if (bNode != null)
                {
                    foreach (var gnode in core.DSExecutable.instrStreamList[0].dependencyGraph.GraphList)
                    {
                        if (gnode.OriginalAstID == bNode.OriginalAstID)
                        {
                            gnode.isActive = false;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This method updates a redefined function
        /// </summary>
        /// <param name="subtree"></param>
        /// <returns></returns>
        private void UndefineFunctions(IEnumerable<AssociativeNode> functionDefintions)
        {
            foreach (var funcDef in functionDefintions)
            {
                core.SetFunctionInactive(funcDef as FunctionDefinitionNode);
            }
        }

        /// <summary>
        /// Removes the modified nested block from the VM codegens in preparation for the next run
        /// </summary>
        /// <param name="modifuedGuids"></param>
        private void ClearModifiedNestedBlocks(List<AssociativeNode> astNodes)
        {
            BinaryExpressionNode bnode = null;
            foreach (AssociativeNode node in astNodes)
            {
                bnode = node as BinaryExpressionNode;
                if (bnode.RightNode is LanguageBlockNode)
                {
                    if (core.CodeBlockList[0].children != null)
                    {
                        core.CodeBlockList[0].children.RemoveAll(x => x.guid == bnode.guid);
                    }

                    // Remove from the global codeblocks
                    core.CodeBlockList.RemoveAll(x => x.guid == bnode.guid);// && x.AstID == bnode.OriginalAstID);
                    // Remove from the runtime codeblocks
                    core.CompleteCodeBlockList.RemoveAll(x => x.guid == bnode.guid);// && x.AstID == bnode.OriginalAstID);
                }
            }
        }
    }

    /// <summary>
    /// ChangeSetComputer handles delta computation of AST's
    /// </summary>
    public class ChangeSetComputer
    {
        private Dictionary<System.Guid, Subtree> currentSubTreeList = null;
        private ProtoCore.Core core = null;
        private Dictionary<Guid, List<ProtoCore.AST.Node>> astCache = null;

        public ChangeSetData csData { get; private set; }

        public ChangeSetComputer(ProtoCore.Core core)
        {
            this.core = core;
            currentSubTreeList = new Dictionary<Guid, Subtree>();
            astCache = new Dictionary<Guid, List<ProtoCore.AST.Node>>();
        }


        private IEnumerable<AssociativeNode> GetDeltaAstListDeleted(IEnumerable<Subtree> deletedSubTrees)
        {
            var deltaAstList = new List<AssociativeNode>();

            csData.DeletedBinaryExprASTNodes = new List<AssociativeNode>();
            csData.DeletedFunctionDefASTNodes = new List<AssociativeNode>();

            if (deletedSubTrees != null)
            {
                foreach (var st in deletedSubTrees)
                {
                    if (st.AstNodes != null && st.AstNodes.Count > 0)
                    {
                        csData.DeletedBinaryExprASTNodes.AddRange(st.AstNodes);
                    }
                    else
                    {
                        // Handle the case where only the GUID of the deleted subtree was provided
                        // Get the cached subtree that is now being deleted
                        Subtree removeSubTree = new Subtree();
                        if (currentSubTreeList.TryGetValue(st.GUID, out removeSubTree))
                        {
                            if (removeSubTree.AstNodes != null)
                            {
                                csData.DeletedBinaryExprASTNodes.AddRange(removeSubTree.AstNodes);
                            }
                        }
                    }

                    // Cache removed function definitions
                    Subtree oldSubTree;
                    if (currentSubTreeList.TryGetValue(st.GUID, out oldSubTree))
                    {
                        if (oldSubTree.AstNodes != null)
                        {
                            csData.DeletedFunctionDefASTNodes.AddRange(oldSubTree.AstNodes.Where(n => n is FunctionDefinitionNode));
                        }
                        currentSubTreeList.Remove(st.GUID);
                    }

                    // Build the nullify ASTs
                    var nullNodes = BuildNullAssignments(csData.DeletedBinaryExprASTNodes);
                    deltaAstList.AddRange(nullNodes);

                    foreach (AssociativeNode node in deltaAstList)
                    {
                        var bnode = node as BinaryExpressionNode;
                        if (bnode != null)
                        {
                            bnode.guid = st.GUID;
                        }
                    }

                    core.RuntimeStatus.ClearWarningsForGraph(st.GUID);
                }
            }
            return deltaAstList;
        }

        private IEnumerable<AssociativeNode> GetDeltaAstListAdded(IEnumerable<Subtree> addedSubTrees)
        {
            var deltaAstList = new List<AssociativeNode>();
            if (addedSubTrees != null)
            {
                foreach (var st in addedSubTrees)
                {
                    currentSubTreeList.Add(st.GUID, st);

                    if (st.AstNodes != null)
                    {
                        deltaAstList.AddRange(st.AstNodes);

                        foreach (AssociativeNode node in st.AstNodes)
                        {
                            var bnode = node as BinaryExpressionNode;
                            if (bnode != null)
                            {
                                bnode.guid = st.GUID;
                            }

                            SetNestedLanguageBlockASTGuids(st.GUID, new List<ProtoCore.AST.Node>() { bnode });
                        }
                    }
                }
            }

            return deltaAstList;
        }

        /// <summary>
        /// Traverse the list of ASTs and set the guid of the nested binary expressions
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="astList"></param>
        private void SetNestedLanguageBlockASTGuids(Guid guid, List<ProtoCore.AST.Node> astList)
        {
            foreach (ProtoCore.AST.Node node in astList)
            {
                ProtoCore.AST.Node rightNode = null;
                if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                {
                    (node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).guid = guid;
                    rightNode = (node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).RightNode;
                }
                else if (node is ProtoCore.AST.ImperativeAST.BinaryExpressionNode)
                {
                    (node as ProtoCore.AST.ImperativeAST.BinaryExpressionNode).guid = guid;
                    rightNode = (node as ProtoCore.AST.ImperativeAST.BinaryExpressionNode).RightNode;
                }

                ProtoCore.AST.Node langblock = null;
                List<ProtoCore.AST.Node> nextAstList = new List<ProtoCore.AST.Node>();
                if (rightNode is ProtoCore.AST.AssociativeAST.LanguageBlockNode)
                {
                    langblock = (rightNode as ProtoCore.AST.AssociativeAST.LanguageBlockNode).CodeBlockNode;
                }
                else if  (rightNode is ProtoCore.AST.ImperativeAST.LanguageBlockNode)
                {
                    langblock = (rightNode as ProtoCore.AST.ImperativeAST.LanguageBlockNode).CodeBlockNode;
                }

                if (langblock != null)
                {
                    if (langblock is ProtoCore.AST.AssociativeAST.CodeBlockNode)
                    {
                        ProtoCore.AST.AssociativeAST.CodeBlockNode codeBlock = langblock as ProtoCore.AST.AssociativeAST.CodeBlockNode;
                        foreach (ProtoCore.AST.AssociativeAST.AssociativeNode assocNode in codeBlock.Body)
                        {
                            nextAstList.Add(assocNode as ProtoCore.AST.Node);
                        }
                    }
                    else if (langblock is ProtoCore.AST.ImperativeAST.CodeBlockNode)
                    {
                        ProtoCore.AST.ImperativeAST.CodeBlockNode codeBlock = langblock as ProtoCore.AST.ImperativeAST.CodeBlockNode;
                        foreach (ProtoCore.AST.ImperativeAST.ImperativeNode imperativeNode in codeBlock.Body)
                        {
                            nextAstList.Add(imperativeNode as ProtoCore.AST.Node);
                        }
                    }
                }
                SetNestedLanguageBlockASTGuids(guid, nextAstList);
            }
        }


        private IEnumerable<AssociativeNode> GetDeltaAstListModified(IEnumerable<Subtree> modifiedSubTrees)
        {
            var deltaAstList = new List<AssociativeNode>();
            csData.RemovedBinaryNodesFromModification = new List<AssociativeNode>();
            csData.RemovedFunctionDefNodesFromModification = new List<AssociativeNode>();
            csData.ModifiedFunctions = new List<AssociativeNode>();
            csData.ForceExecuteASTList = new List<AssociativeNode>();
            csData.ModifiedNestedLangBlock = new List<AssociativeNode>();

            if (modifiedSubTrees == null)
            {
                return deltaAstList;
            }

            foreach (var st in modifiedSubTrees)
            {
                if (st.AstNodes == null)
                {
                    continue;
                }

                // Handle modified statements
                var modifiedASTList = GetModifiedNodes(st);
                if (null != modifiedASTList && modifiedASTList.Count > 0)
                {
                    deltaAstList.AddRange(modifiedASTList);
                }

                var modifiedExprIDs = modifiedASTList.Where(n => n is BinaryExpressionNode)
                                                     .Select(n => (n as BinaryExpressionNode).exprUID);

                // Disable removed nodes from the cache
                Subtree oldSubTree;
                bool cachedTreeExists = currentSubTreeList.TryGetValue(st.GUID, out oldSubTree);

                if (cachedTreeExists && oldSubTree.AstNodes != null)
                {
                    List<AssociativeNode> removedNodes = null;
                    if (!st.ForceExecution)
                    {
                        removedNodes = GetInactiveASTList(oldSubTree.AstNodes, st.AstNodes);
                        // We only need the removed binary ASTs
                        // Function definitions are handled in ChangeSetData.RemovedFunctionDefNodesFromModification
                        csData.RemovedBinaryNodesFromModification.AddRange(removedNodes.Where(n => n is BinaryExpressionNode));
                    }

                    // There is a bug in DeactivateGraphNodes(), otherwise we
                    // could remove all warnings generated from removedNodes in
                    // DeactivateGraphnodes(). 
                    // Right now just simply remove all related warnings.
                    core.RuntimeStatus.ClearWarningsForGraph(st.GUID);
                }

                // Cache the modifed functions
                //var modifiedFunctions = st.AstNodes.Where(n => n is FunctionDefinitionNode);
                var modifiedFunctions = modifiedASTList.Where(n => n is FunctionDefinitionNode);
                csData.ModifiedFunctions.AddRange(modifiedFunctions);

                // Handle cached subtree
                if (!cachedTreeExists)
                {
                    // Cache the subtree if it does not exist yet
                    // This scenario is possible if a subtree was deleted and the same subtree was added again as a modified subtree
                    currentSubTreeList.Add(st.GUID, st);
                }
                else
                {
                    if (null == oldSubTree.AstNodes)
                    {
                        // The ast list for this subtree is null
                        // This is due to the liverunner being passed an empty astlist, such as a codeblock with no content
                        // Populate this subtree with the current ast contents
                        oldSubTree.AstNodes = modifiedASTList;
                        currentSubTreeList[st.GUID] = oldSubTree;
                    }
                    else
                    {
                        if (st.ForceExecution)
                        {
                            // Get the cached AST and append it to the changeSet
                            csData.ForceExecuteASTList.AddRange(GetUnmodifiedASTList(oldSubTree.AstNodes, st.AstNodes));
                        }
                        else
                        {
                            // Only update the cached ASTs if it is not ForceExecution

                            // Cache the functions that were re-defined
                            // The changeSetApplier will remove the previous definition of these functions given the function signature
                            csData.RemovedFunctionDefNodesFromModification.AddRange(modifiedFunctions.Where(n => n is FunctionDefinitionNode));

                            // Update the current subtree list
                            List<AssociativeNode> newCachedASTList = new List<AssociativeNode>();
                            newCachedASTList.AddRange(GetUnmodifiedASTList(oldSubTree.AstNodes, st.AstNodes));
                            newCachedASTList.AddRange(modifiedASTList);

                            st.AstNodes.Clear();
                            st.AstNodes.AddRange(newCachedASTList);
                            currentSubTreeList[st.GUID] = st;
                        }
                    }
                }

                foreach (AssociativeNode node in deltaAstList)
                {
                    var bnode = node as BinaryExpressionNode;
                    if (bnode != null)
                    {
                        bnode.guid = st.GUID;
                    }

                    SetNestedLanguageBlockASTGuids(st.GUID, new List<ProtoCore.AST.Node>() { bnode });
                }
            }
            return deltaAstList;
        }


        public List<AssociativeNode> GetDeltaASTList(GraphSyncData syncData)
        {
            UpdateAstCache(syncData);
            csData = new ChangeSetData();
            List<AssociativeNode> finalDeltaAstList = new List<AssociativeNode>();
            finalDeltaAstList.AddRange(GetDeltaAstListDeleted(syncData.DeletedSubtrees));
            finalDeltaAstList.AddRange(GetDeltaAstListAdded(syncData.AddedSubtrees));
            finalDeltaAstList.AddRange(GetDeltaAstListModified(syncData.ModifiedSubtrees));

            return finalDeltaAstList;
        }

        private List<AssociativeNode> GetASTNodesDependentOnFunctionList(FunctionDefinitionNode functionNode)
        {
            // Determine if the modified function was used in any of the current nodes
            List<AssociativeNode> modifiedNodes = new List<AssociativeNode>();

            // Iterate through the vm graphnodes at the global scope that contain a function call
            //foreach (ProtoCore.AssociativeGraph.GraphNode gnode in runnerCore.DSExecutable.instrStreamList[0].dependencyGraph.GraphList)
            Validity.Assert(null != core.GraphNodeCallList);
            foreach (ProtoCore.AssociativeGraph.GraphNode gnode in core.GraphNodeCallList)
            {
                if (gnode.isActive)
                {
                    // Iterate through the current ast nodes 
                    foreach (KeyValuePair<System.Guid, Subtree> kvp in currentSubTreeList)
                    {
                        foreach (AssociativeNode assocNode in kvp.Value.AstNodes)
                        {
                            if (assocNode is BinaryExpressionNode)
                            {
                                if (gnode.exprUID == (assocNode as BinaryExpressionNode).exprUID)
                                {
                                    // Check if the procedure associatied with this graphnode matches thename and arg count of the modified proc
                                    if (null != gnode.firstProc)
                                    {
                                        if (gnode.firstProc.name == functionNode.Name
                                            && gnode.firstProc.argInfoList.Count == functionNode.Signature.Arguments.Count)
                                        {
                                            // If it does, create a new ast tree for this graphnode and append it to deltaAstList
                                            modifiedNodes.Add(assocNode);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return modifiedNodes;
        }


        /// <summary>
        ///             
        /// Handle instances of redefining the lhs of an expression
        /// Given:
        ///      a = p.x -> b = p.x
        /// In such a scenario, the new expression 'b = p.x' must inherit the previous expression id of a = p.x
        /// </summary>
        /// <param name="newNode"></param>
        /// <param name="cachedASTList"></param>
        private void HandleRedefinedLHS(BinaryExpressionNode newNode, List<AssociativeNode> cachedASTList)
        {
            //
            // Note that after SSA is applied, the expression:
            //      a = p.x 
            // transforms to: 
            //      t0 = p
            //      t1 = t0.x
            //      a = t1
            //      
            // And the expression:
            //      b = p.x 
            // transforms to: 
            //      t0 = p
            //      t1 = t0.x
            //      b = t1
            //
            // As such we only need to update the expression id of 'b = t1' to inherit the expression id of 'a = t1'
            //
            if (null != newNode)
            {
                IdentifierNode rnode = newNode.RightNode as IdentifierNode;
                if (null != rnode)
                {
                    foreach (AssociativeNode prevNode in cachedASTList)
                    {
                        BinaryExpressionNode prevBinaryNode = prevNode as BinaryExpressionNode;
                        if (null != prevBinaryNode)
                        {
                            IdentifierNode prevIdent = prevBinaryNode.LeftNode as IdentifierNode;
                            if (null != prevIdent)
                            {
                                if (prevIdent.Equals(rnode))
                                {
                                    newNode.InheritID(prevBinaryNode.ID);
                                    newNode.exprUID = prevBinaryNode.exprUID;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the only the modified nodes from the subtree by checking of the previous cached instance
        /// </summary>
        /// <param name="subtree"></param>
        /// <returns></returns>
        private List<AssociativeNode> GetModifiedNodes(Subtree subtree)
        {
            Subtree st;
            if (!currentSubTreeList.TryGetValue(subtree.GUID, out st) || st.AstNodes == null)
            {
                // If the subtree was not cached, it means the cache was delted
                // This means the current subtree is all modified
                return subtree.AstNodes;
            }

            // We want to process only modified statements
            // If the AST is identical to an existing AST in the same GUID, it means it was not modified
            List<AssociativeNode> modifiedASTList = new List<AssociativeNode>();
            foreach (AssociativeNode node in subtree.AstNodes)
            {
                // Check if node exists in the prev AST list
                bool nodeFound = false;
                foreach (AssociativeNode prevNode in st.AstNodes)
                {
                    if (prevNode.Equals(node))
                    {
                        nodeFound = true;
                        break;
                    }
                }
                if (!nodeFound)
                {
                    // At this point, the ast was determined to have been modified
                    // It can then be handled normally regardless of its ForceExecution state
                    subtree.ForceExecution = false;

                    // node is modifed as it does not match any existing
                    modifiedASTList.Add(node);

                    BinaryExpressionNode bnode = node as BinaryExpressionNode;
                    if (null != bnode)
                    {
                        if (bnode.RightNode is LanguageBlockNode)
                        {
                            csData.ModifiedNestedLangBlock.Add(bnode);
                        }
                        else if (bnode.LeftNode is IdentifierNode)
                        {
                            string lhsName = (bnode.LeftNode as IdentifierNode).Name;
                            Validity.Assert(null != lhsName && string.Empty != lhsName);
                            if (CoreUtils.IsSSATemp(lhsName))
                            {
                                // If the lhs of this binary expression is an SSA temp, and it existed in the lhs of any cached nodes, 
                                // this means that it was a modified variable within the previous expression.
                                // Inherit its expression ID 
                                foreach (AssociativeNode prevNode in st.AstNodes)
                                {
                                    BinaryExpressionNode prevBinaryNode = prevNode as BinaryExpressionNode;
                                    if (null != prevBinaryNode)
                                    {
                                        IdentifierNode prevIdent = prevBinaryNode.LeftNode as IdentifierNode;
                                        if (null != prevIdent)
                                        {
                                            if (prevIdent.Equals(bnode.LeftNode as IdentifierNode))
                                            {
                                                bnode.InheritID(prevBinaryNode.ID);
                                                bnode.exprUID = prevBinaryNode.exprUID;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Handle re-defined lhs expressions
                                HandleRedefinedLHS(bnode, st.AstNodes);
                            }
                        }
                    }
                }
            }
            return modifiedASTList;
        }


        /// <summary>
        /// Get the ASTs from the previous list that no longer exist in the new list
        /// </summary>
        /// <param name="prevASTList"></param>
        /// <param name="newASTList"></param>
        /// <returns></returns>
        private List<AssociativeNode> GetInactiveASTList(List<AssociativeNode> prevASTList, List<AssociativeNode> newASTList)
        {
            List<AssociativeNode> removedList = new List<AssociativeNode>();
            foreach (AssociativeNode prevNode in prevASTList)
            {
                bool prevNodeFoundInNewList = false;
                foreach (AssociativeNode newNode in newASTList)
                {
                    if (prevNode.Equals(newNode))
                    {
                        // prev node still exists in the new list
                        prevNodeFoundInNewList = true;
                        break;
                    }
                }

                if (!prevNodeFoundInNewList)
                {
                    removedList.Add(prevNode);
                }
            }
            return removedList;
        }

        /// <summary>
        /// Get the ASTs from the previous list that that still exist in the new list
        /// </summary>
        /// <param name="prevASTList"></param>
        /// <param name="newASTList"></param>
        /// <returns></returns>
        private List<AssociativeNode> GetUnmodifiedASTList(List<AssociativeNode> prevASTList, List<AssociativeNode> newASTList)
        {
            List<AssociativeNode> existingList = new List<AssociativeNode>();
            foreach (AssociativeNode prevNode in prevASTList)
            {
                foreach (AssociativeNode newNode in newASTList)
                {
                    if (prevNode.Equals(newNode))
                    {
                        existingList.Add(prevNode);
                        break;
                    }
                }
            }
            return existingList;
        }

        /// <summary>
        /// Updates the cached AST's if they were modified
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="modifiedNodes"></param>
        private void UpdateCachedSubtree(Guid guid, List<AssociativeNode> modifiedNodes)
        {
            if (null != modifiedNodes && modifiedNodes.Count > 0)
            {
                List<AssociativeNode> cachedASTList = currentSubTreeList[guid].AstNodes;
                foreach (AssociativeNode node in modifiedNodes)
                {
                    // Remove ast from cachedASTList if the current node matches an exprID
                    // cachedASTList.RemoveUnmodified()

                    if (node is BinaryExpressionNode)
                    {
                        cachedASTList.Add(node);
                    }
                }
            }
        }

        /// <summary>
        /// Update the map from graph UI node to a list of ast nodes. Each
        /// ast node is in SSA form. 
        /// </summary>
        /// <param name="syncData"></param>
        private void UpdateAstCache(GraphSyncData syncData)
        {
            if (syncData.ModifiedSubtrees != null && astCache.Count > 0)
            {
                foreach (var t in syncData.ModifiedSubtrees)
                {
                    if (astCache.ContainsKey(t.GUID))
                    {
                        astCache[t.GUID].Clear();
                        if (t.AstNodes != null)
                        {
                            astCache[t.GUID].AddRange(t.AstNodes);
                        }
                    }
                }
            }

            if (syncData.DeletedSubtrees != null)
            {
                syncData.DeletedSubtrees.ForEach(t => astCache.Remove(t.GUID));
            }

            if (syncData.AddedSubtrees != null)
            {
                foreach (var t in syncData.AddedSubtrees)
                {
                    var astNodes = new List<ProtoCore.AST.Node>();
                    if (t.AstNodes != null)
                    {
                        astNodes.AddRange(t.AstNodes);
                    }
                    astCache[t.GUID] = astNodes;
                }
            }
        }

        private bool CompileToSSA(Guid guid, List<AssociativeNode> astList, out List<AssociativeNode> ssaAstList)
        {
            core.Options.GenerateSSA = true;
            core.ResetSSASubscript(guid, 0);
            ProtoAssociative.CodeGen codegen = new ProtoAssociative.CodeGen(core, null);
            ssaAstList = new List<AssociativeNode>();
            ssaAstList = codegen.EmitSSA(astList);
            return true;
        }


        /// <summary>
        ///  Compiles all ASTs within the syncData to SSA
        /// </summary>
        /// <param name="syncData"></param>
        private void CompileToSSA(GraphSyncData syncData)
        {
            List<AssociativeNode> newASTList = null;
            if (null != syncData.AddedSubtrees)
            {
                foreach (Subtree st in syncData.AddedSubtrees)
                {
                    if (null != st.AstNodes)
                    {
                        CompileToSSA(st.GUID, st.AstNodes, out newASTList);
                        st.AstNodes.Clear();
                        st.AstNodes.AddRange(newASTList);
                    }
                }
            }

            if (null != syncData.ModifiedSubtrees)
            {
                foreach (Subtree st in syncData.ModifiedSubtrees)
                {
                    if (null != st.AstNodes)
                    {
                        CompileToSSA(st.GUID, st.AstNodes, out newASTList);
                        st.AstNodes.Clear();
                        st.AstNodes.AddRange(newASTList);
                    }
                }
            }

            if (null != syncData.DeletedSubtrees)
            {
                foreach (Subtree st in syncData.DeletedSubtrees)
                {
                    if (null != st.AstNodes)
                    {
                        CompileToSSA(st.GUID, st.AstNodes, out newASTList);
                        st.AstNodes.Clear();
                        st.AstNodes.AddRange(newASTList);
                    }
                }
            }
        }


        /// <summary>
        /// Get ast nodes for graph UI node. The returned ast nodes are in 
        /// SSA form.
        /// </summary>
        /// <param name="nodeGuid"></param>
        /// <returns></returns>
        public IEnumerable<ProtoCore.AST.Node> GetSSANodes(Guid nodeGuid)
        {
            List<ProtoCore.AST.Node> nodes = null;
            astCache.TryGetValue(nodeGuid, out nodes);
            return nodes;
        }


        /// <summary>
        /// Creates a list of null assignment statements where the lhs is retrieved from an ast list
        /// </summary>
        /// <param name="astList"></param>
        /// <returns></returns>
        private List<AssociativeNode> BuildNullAssignments(List<AssociativeNode> astList)
        {
            var astNodeList = new List<AssociativeNode>();
            if (null != astList)
            {
                foreach (var node in astList)
                {
                    BinaryExpressionNode bNode = node as BinaryExpressionNode;
                    if (bNode != null)
                    {
                        BinaryExpressionNode newBNode = new BinaryExpressionNode(bNode.LeftNode, new NullNode(), ProtoCore.DSASM.Operator.assign);
                        astNodeList.Add(newBNode);
                    }
                }
            }
            return astNodeList;
        }
    }

    public interface ILiveRunner
    {
        ProtoCore.Core Core { get; }

        #region Synchronous call
        void UpdateGraph(GraphSyncData syncData);
        void UpdateCmdLineInterpreter(string code);
        ProtoCore.Mirror.RuntimeMirror QueryNodeValue(Guid nodeId);
        ProtoCore.Mirror.RuntimeMirror InspectNodeValue(string nodeName);

        void UpdateGraph(AssociativeNode astNode);
        #endregion

        #region Asynchronous call
        void BeginUpdateGraph(GraphSyncData syncData);
        void BeginConvertNodesToCode(List<Subtree> subtrees);
        void BeginQueryNodeValue(Guid nodeGuid);
        void BeginQueryNodeValues(List<Guid> nodeGuid);
        #endregion

        string GetCoreDump();
        void ResetVMAndResyncGraph(List<string> libraries);
        List<LibraryMirror> ResetVMAndImportLibrary(List<string> libraries);
        void ReInitializeLiveRunner();
        Dictionary<Guid, List<ProtoCore.RuntimeData.WarningEntry>> GetRuntimeWarnings();

        // Event handlers for the notification from asynchronous call
        event NodeValueReadyEventHandler NodeValueReady;
        event GraphUpdateReadyEventHandler GraphUpdateReady;
        event NodesToCodeCompletedEventHandler NodesToCodeCompleted;

    }

    public partial class LiveRunner : ILiveRunner, IDisposable
    {
        /// <summary>
        ///  These are configuration parameters passed by host application to be consumed by geometry library and persistent manager implementation. 
        /// </summary>
        public class Options
        {
            /// <summary>
            /// The configuration parameters that needs to be passed to
            /// different applications.
            /// </summary>
            public Dictionary<string, object> PassThroughConfiguration;

            /// <summary>
            /// The path of the root graph/module
            /// </summary>
            public string RootModulePathName;

            /// <summary>
            /// List of search directories to resolve any file reference
            /// </summary>
            public List<string> SearchDirectories;

            /// <summary>
            /// If the Interpreter mode is true, the LiveRunner takes in code statements as input strings
            /// and not SyncData
            /// </summary>
            public bool InterpreterMode = false;
        }

        private void ResetModifiedSymbols()
        {
            this.runnerCore.Rmem.ResetModifedSymbols();
        }

        private ProtoScriptTestRunner runner;
        private ProtoRunner.ProtoVMState vmState;
        private GraphToDSCompiler.GraphCompiler graphCompiler;
        private ProtoCore.Core runnerCore = null;
        public ProtoCore.Core Core
        {
            get
            {
                return runnerCore;
            }
            private set
            {
                runnerCore = value;
            }
        }

        private ProtoCore.Options coreOptions = null;
        private Options executionOptions = null;
        private int deltaSymbols = 0;
        private ProtoCore.CompileTime.Context staticContext = null;


        private readonly Object operationsMutex = new object();

        private Queue<Task> taskQueue;

        private Thread workerThread;

        private bool terminating;

        private ChangeSetComputer changeSetComputer;
        private ChangeSetApplier changeSetApplier;



        public LiveRunner()
        {
            InitRunner(new Options());
        }

        public LiveRunner(Options options)
        {
            InitRunner(options);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (runnerCore != null)
                {
                    runnerCore.FFIPropertyChangedMonitor.FFIPropertyChangedEventHandler -= FFIPropertyChanged;
                    runnerCore.Cleanup();
                }

                terminating = true;

                lock (taskQueue)
                {
                    taskQueue.Clear();
                }

                // waiting for thread to finish
                if (workerThread.IsAlive)
                {
                    workerThread.Join();
                }
            }
        }

        private void InitRunner(Options options)
        {
            graphCompiler = GraphToDSCompiler.GraphCompiler.CreateInstance();
            graphCompiler.SetCore(GraphUtilities.GetCore());
            runner = new ProtoScriptTestRunner();

            executionOptions = options;
            InitOptions();
            InitCore();


            taskQueue = new Queue<Task>();

            workerThread = new Thread(new ThreadStart(TaskExecMethod));


            workerThread.IsBackground = true;
            workerThread.Start();

            staticContext = new ProtoCore.CompileTime.Context();

            terminating = false;
            changeSetComputer = new ChangeSetComputer(runnerCore);
            changeSetApplier = new ChangeSetApplier();
        }

        private void InitOptions()
        {

            // Build the options required by the core
            Validity.Assert(coreOptions == null);
            coreOptions = new ProtoCore.Options();
            coreOptions.GenerateExprID = true;
            coreOptions.IsDeltaExecution = true;
            coreOptions.BuildOptErrorAsWarning = true;

            coreOptions.WebRunner = false;
            coreOptions.ExecutionMode = ProtoCore.ExecutionMode.Serial;


            // This should have been set in the consturctor
            Validity.Assert(executionOptions != null);
        }

        private void InitCore()
        {
            Validity.Assert(coreOptions != null);

            // Comment Jun:
            // It must be guaranteed that in delta exeuction, expression id's must not be autogerated
            // expression Id's must be propagated from the graphcompiler to the DS codegenerators
            //Validity.Assert(coreOptions.IsDeltaExecution && !coreOptions.GenerateExprID);

            runnerCore = new ProtoCore.Core(coreOptions);

            SyncCoreConfigurations(runnerCore, executionOptions);


            runnerCore.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(runnerCore));
            runnerCore.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(runnerCore));
            runnerCore.FFIPropertyChangedMonitor.FFIPropertyChangedEventHandler += FFIPropertyChanged;
            vmState = null;
        }

        private void FFIPropertyChanged(FFIPropertyChangedEventArgs arg)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(new PropertyChangedTask(this, arg.hostGraphNode));
            }
        }

        private static void SyncCoreConfigurations(ProtoCore.Core core, Options options)
        {
            if (null == options)
                return;
            //update the root module path name, if set.
            if (!string.IsNullOrEmpty(options.RootModulePathName))
                core.Options.RootModulePathName = options.RootModulePathName;
            //then update the search path, if set.
            if (null != options.SearchDirectories)
                core.Options.IncludeDirectories = options.SearchDirectories;

            //Finally update the pass thru configuration values
            if (null == options.PassThroughConfiguration)
                return;
            foreach (var item in options.PassThroughConfiguration)
            {
                core.Configurations[item.Key] = item.Value;
            }
        }


        public void SetOptions(Options options)
        {
            executionOptions = options;
        }


        #region Public Live Runner Events

        public event NodeValueReadyEventHandler NodeValueReady = null;
        public event GraphUpdateReadyEventHandler GraphUpdateReady = null;
        public event NodesToCodeCompletedEventHandler NodesToCodeCompleted = null;

        #endregion

        public void BeginUpdateGraph(GraphSyncData syncData)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(new UpdateGraphTask(syncData, this));
            }
        }

        /// <summary>
        /// Async call from command-line interpreter to LiveRunner
        /// </summary>
        /// <param name="cmdLineString"></param>
        public void BeginUpdateCmdLineInterpreter(string cmdLineString)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(
                    new UpdateCmdLineInterpreterTask(cmdLineString, this));
            }
        }

        public void BeginConvertNodesToCode(List<Subtree> subtrees)
        {
            if (null == subtrees || (subtrees.Count <= 0))
                return; // Do nothing, there's no nodes to be converted.

            lock (taskQueue)
            {
                taskQueue.Enqueue(new ConvertNodesToCodeTask(subtrees, this));
            }
        }

        public void BeginQueryNodeValue(Guid nodeGuid)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(
                    new NodeValueRequestTask(nodeGuid, this));
            }
        }

        public void BeginQueryNodeValues(List<Guid> nodeGuids)
        {
            lock (taskQueue)
            {
                foreach (Guid nodeGuid in nodeGuids)
                {
                    taskQueue.Enqueue(
                        new NodeValueRequestTask(nodeGuid, this));
                }
            }
        }

        /// <summary>
        /// Query for a node value given its UID. This will block until the value is available.
        /// This uses the expression interpreter to evaluate a node variable's value.
        /// It will only serviced when all ASync calls have been completed
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public ProtoCore.Mirror.RuntimeMirror QueryNodeValue(Guid nodeGuid)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {

                        //No entries and we have the lock
                        //Synchronous query to get the node

                        return InternalGetNodeValue(nodeGuid);
                    }
                }
                Thread.Sleep(0);
            }

        }



        /// <summary>
        /// Inspects the VM for the value of a node given its variable name. 
        /// As opposed to QueryNodeValue, this does not use the Expression Interpreter
        /// This will block until the value is available.
        /// It will only serviced when all ASync calls have been completed
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        ///
        public ProtoCore.Mirror.RuntimeMirror InspectNodeValue(string nodeName)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {
                        //return GetWatchValue(nodeName);
                        const int blockID = 0;
                        ProtoCore.Mirror.RuntimeMirror runtimeMirror = ProtoCore.Mirror.Reflection.Reflect(nodeName, blockID, runnerCore);
                        return runtimeMirror;
                    }
                }
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// VM Debugging API for general Debugging purposes 
        /// temporarily used by Cmmand Line REPL
        /// </summary>
        /// <returns></returns>
        public string GetCoreDump()
        {
            // Prints out the final Value of every symbol in the program
            // Traverse order:
            //  Exelist, Globals symbols

            ProtoCore.DSASM.Executive exec = runnerCore.CurrentExecutive.CurrentDSASMExec;
            ProtoCore.DSASM.Mirror.ExecutionMirror execMirror = new ProtoCore.DSASM.Mirror.ExecutionMirror(exec, runnerCore);
            ProtoCore.DSASM.Executable exe = exec.rmem.Executable;

            // Only display symbols defined in the default top-most langauge block;
            // Otherwise garbage information may be displayed.
            string formattedString = string.Empty;
            if (exe.runtimeSymbols.Length > 0)
            {
                int blockId = 0;

                ProtoCore.DSASM.SymbolTable symbolTable = exe.runtimeSymbols[blockId];

                for (int i = 0; i < symbolTable.symbolList.Count; ++i)
                {
                    //int n = symbolTable.symbolList.Count - 1;
                    //formatParams.ResetOutputDepth();
                    ProtoCore.DSASM.SymbolNode symbolNode = symbolTable.symbolList[i];

                    bool isLocal = ProtoCore.DSASM.Constants.kGlobalScope != symbolNode.functionIndex;
                    bool isStatic = (symbolNode.classScope != ProtoCore.DSASM.Constants.kInvalidIndex && symbolNode.isStatic);
                    if (symbolNode.isArgument || isLocal || isStatic || symbolNode.isTemp)
                    {
                        // These have gone out of scope, their values no longer exist
                        //return ((null == globaltrace) ? string.Empty : globaltrace.ToString());
                        continue;
                    }

                    ProtoCore.Runtime.RuntimeMemory rmem = exec.rmem;
                    StackValue sv = rmem.GetStackData(blockId, i, ProtoCore.DSASM.Constants.kGlobalScope);
                    formattedString = formattedString + string.Format("{0} = {1}\n", symbolNode.name, execMirror.GetStringValue(sv, rmem.Heap, blockId));

                    //if (null != globaltrace)
                    //{
                    //    int maxLength = 1020;
                    //    while (formattedString.Length > maxLength)
                    //    {
                    //        globaltrace.AppendLine(formattedString.Substring(0, maxLength));
                    //        formattedString = formattedString.Remove(0, maxLength);
                    //    }

                    //    globaltrace.AppendLine(formattedString);
                    //}
                }

                //formatParams.ResetOutputDepth();
            }

            //return ((null == globaltrace) ? string.Empty : globaltrace.ToString());
            return formattedString;
        }

        /// <summary>
        /// This API needs to be called for every delta AST execution
        /// </summary>
        /// <param name="syncData"></param>
        public void UpdateGraph(GraphSyncData syncData)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    if (taskQueue.Count == 0)
                    {
                        SynchronizeInternal(syncData);
                        return;
                    }
                }
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Called for delta execution of AST node input
        /// </summary>
        /// <param name="astNode"></param>
        public void UpdateGraph(AssociativeNode astNode)
        {
            CodeBlockNode cNode = astNode as CodeBlockNode;
            if (cNode != null)
            {
                List<AssociativeNode> astList = cNode.Body;
                List<Subtree> addedList = new List<Subtree>();
                addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
                GraphSyncData syncData = new GraphSyncData(null, addedList, null);

                UpdateGraph(syncData);
            }
            else if (astNode is AssociativeNode)
            {
                List<AssociativeNode> astList = new List<AssociativeNode>();
                astList.Add(astNode);
                List<Subtree> addedList = new List<Subtree>();
                addedList.Add(new Subtree(astList, System.Guid.NewGuid()));
                GraphSyncData syncData = new GraphSyncData(null, addedList, null);

                UpdateGraph(syncData);
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        /// <summary>
        /// This api needs to be called by a command line REPL for each DS command/expression entered to be executed
        /// </summary>
        /// <param name="code"></param>
        public void UpdateCmdLineInterpreter(string code)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {
                        SynchronizeInternal(code);
                        return;
                    }
                }
                Thread.Sleep(0);
            }
        }

        //Secondary thread
        private void TaskExecMethod()
        {
            while (!terminating)
            {
                Task task = null;

                lock (taskQueue)
                {
                    if (taskQueue.Count > 0)
                        task = taskQueue.Dequeue();

                    //The task has to be executed inside the critical region
                    //otherwise it will race with the sync based on the taskQueue count

                    //TODO: This should be seperated into two seperate mutexes, one of the
                    //queue and the other protecting execution
                    if (task != null)
                    {
                        task.Execute();
                        continue;

                    }
                }
                Thread.Sleep(10);
            }
        }



        #region Internal Implementation

        private ProtoCore.Mirror.RuntimeMirror GetWatchValue(string varname)
        {
            runnerCore.Options.IsDeltaCompile = true;
            CompileAndExecuteForDeltaExecution(GraphUtilities.GetWatchExpression(varname));

            const int blockID = 0;
            ProtoCore.Mirror.RuntimeMirror runtimeMirror = ProtoCore.Mirror.Reflection.Reflect(ProtoCore.DSASM.Constants.kWatchResultVar, blockID, runnerCore);
            return runtimeMirror;

        }


        /// <summary>
        /// This is being called currently as it uses the Expression interpreter which does not
        /// work well with delta execution. Instead we are currently inspecting into the VM using Mirrors
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        private ProtoCore.Mirror.RuntimeMirror InternalGetNodeValue(string varname)
        {
            Validity.Assert(null != vmState);

            // Comment Jun: all symbols are in the global block as there is no notion of scoping the the graphUI yet.
            const int blockID = 0;

            return vmState.LookupName(varname, blockID);
        }

        private ProtoCore.Mirror.RuntimeMirror InternalGetNodeValue(Guid nodeGuid)
        {
            throw new NotImplementedException();
        }


        private bool Compile(string code, out int blockId)
        {
            Dictionary<string, bool> execFlagList = null;
            if (graphCompiler != null)
                execFlagList = graphCompiler.ExecutionFlagList;

            staticContext.SetData(code, new Dictionary<string, object>(), execFlagList);

            bool succeeded = runner.Compile(staticContext, runnerCore, out blockId);
            if (succeeded)
            {
                // Regenerate the DS executable
                runnerCore.GenerateExecutable();

                // Update the symbol tables
                // TODO Jun: Expand to accomoadate the list of symbols
                //staticContext.symbolTable = runnerCore.DSExecutable.runtimeSymbols[0];
            }
            return succeeded;
        }

        private bool Compile(List<AssociativeNode> astList, out int blockId)
        {
            // The ASTs have already been transformed to SSA
            //runnerCore.Options.GenerateSSA = false;

            bool succeeded = runner.Compile(astList, runnerCore, out blockId);
            if (succeeded)
            {
                // Regenerate the DS executable
                runnerCore.GenerateExecutable();

                // Update the symbol tables
                // TODO Jun: Expand to accomoadate the list of symbols
                staticContext.symbolTable = runnerCore.DSExecutable.runtimeSymbols[0];
            }
            return succeeded;
        }

        private ProtoRunner.ProtoVMState Execute()
        {
            // runnerCore.GlobOffset is the number of global symbols that need to be allocated on the stack
            // The argument to Reallocate is the number of ONLY THE NEW global symbols as the stack needs to accomodate this delta
            int newSymbols = runnerCore.GlobOffset - deltaSymbols;

            // If there are lesser symbols to allocate for this run, then it means nodes were deleted.
            // TODO Jun: Determine if it is safe to just leave them in the global stack 
            //           as no symbols point to this memory location in the stack anyway
            if (newSymbols >= 0)
            {
                runnerCore.Rmem.ReAllocateMemory(newSymbols);
            }

            // Store the current number of global symbols
            deltaSymbols = runnerCore.GlobOffset;

            // Initialize the runtime context and pass it the execution delta list from the graph compiler
            ProtoCore.Runtime.Context runtimeContext = new ProtoCore.Runtime.Context();

            if (graphCompiler != null)
                runtimeContext.execFlagList = graphCompiler.ExecutionFlagList;

            runner.Execute(runnerCore, runtimeContext);

            // ExecutionMirror mirror = new ExecutionMirror(runnerCore.CurrentExecutive.CurrentDSASMExec, runnerCore);

            return new ProtoRunner.ProtoVMState(runnerCore);
        }

        private bool CompileAndExecute(string code)
        {
            // TODO Jun: Revisit all the Compile functions and remove the blockId out argument
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(code, out blockId);
            if (succeeded)
            {
                runnerCore.RunningBlock = blockId;
                vmState = Execute();
            }
            return succeeded;
        }

        private bool CompileAndExecute(List<AssociativeNode> astList)
        {
            // TODO Jun: Revisit all the Compile functions and remove the blockId out argument
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool succeeded = Compile(astList, out blockId);
            if (succeeded)
            {
                runnerCore.RunningBlock = blockId;
                vmState = Execute();
            }
            return succeeded;
        }

        /// <summary>
        /// Resets few states in the core to prepare the core for a new
        /// delta code compilation and execution
        /// </summary>
        private void ResetForDeltaExecution()
        {
            runnerCore.ResetForDeltaExecution();
        }

        /// <summary>
        /// Compiles and executes input script in delta execution mode
        /// </summary>
        /// <param name="code"></param>
        private void CompileAndExecuteForDeltaExecution(string code)
        {
            if (coreOptions.Verbose)
            {
                System.Diagnostics.Debug.WriteLine("SyncInternal => " + code);
            }

            ResetForDeltaExecution();
            CompileAndExecute(code);
        }

        private void CompileAndExecuteForDeltaExecution(List<AssociativeNode> astList)
        {
            // Make a copy of the ASTs to be executed
            // We dont want the compiler to modify the ASTs cached in the liverunner
            List<AssociativeNode> dispatchASTList = new List<AssociativeNode>();
            foreach (AssociativeNode astNode in astList)
            {
                AssociativeNode newNode = NodeUtils.Clone(astNode);
                dispatchASTList.Add(newNode);
            }

            if (coreOptions.Verbose)
            {
                string code = DebugCodeEmittedForDeltaAst(dispatchASTList);
                System.Diagnostics.Debug.WriteLine(code);
            }

            ResetForDeltaExecution();
            CompileAndExecute(dispatchASTList);
        }


        private void SynchronizeInternal(GraphSyncData syncData)
        {
            runnerCore.Options.IsDeltaCompile = true;

            if (syncData == null)
            {
                ResetForDeltaExecution();
                return;
            }

            // Get AST list that need to be executed
            var finalDeltaAstList = changeSetComputer.GetDeltaASTList(syncData);

            // Prior to execution, apply state modifications to the VM given the delta AST's
            changeSetApplier.Apply(runnerCore, changeSetComputer.csData);

            CompileAndExecuteForDeltaExecution(finalDeltaAstList);

#if DEBUG // Debug preproc the function here as we dont want it to perform additional calls on release
            ProtoCore.Utils.HeapUtils.VerifyHeap(runnerCore);
#endif
        }


        private void SynchronizeInternal(string code)
        {
            runnerCore.Options.IsDeltaCompile = true;

            if (string.IsNullOrEmpty(code))
            {
                ResetForDeltaExecution();
                return;
            }
            else
            {
                CompileAndExecuteForDeltaExecution(code);
            }
        }

        /// <summary>
        /// This is to be used for debugging only to check code emitted from delta AST input
        /// </summary>
        /// <param name="deltaAstList"></param>
        /// <returns></returns>
        private string DebugCodeEmittedForDeltaAst(List<AssociativeNode> deltaAstList)
        {
            var codeGen = new ProtoCore.CodeGenDS(deltaAstList);
            var code = codeGen.GenerateCode();
            return code;
        }

        /// <summary>
        /// Re-initializes the LiveRunner to reset the VM 
        /// Used temporarily when importing libraries on-demand during delta execution
        /// Will be deprecated once this is supported by the core language
        /// </summary>
        public void ReInitializeLiveRunner()
        {
            runner = new ProtoScriptTestRunner();

            executionOptions = new Options();
            deltaSymbols = 0;

            coreOptions = null;
            InitOptions();
            InitCore();

            staticContext = new ProtoCore.CompileTime.Context();

            changeSetComputer = new ChangeSetComputer(runnerCore);

            CLRModuleType.ClearTypes();
        }

        /// <summary>
        /// This is called temporarily to reset the VM and recompile the entire graph with new import 
        /// statements whenever a node from a new library is added to the graph.
        /// TODO: It should not be needed once we have language support to insert import statements arbitrarily
        /// </summary>
        /// <param name="libraries"></param>
        /// <param name="syncData"></param>
        public void ResetVMAndResyncGraph(List<string> libraries)
        {
            // Reset VM
            ReInitializeLiveRunner();

            // generate import node for each library in input list
            List<AssociativeNode> importNodes = new List<AssociativeNode>();
            foreach (string lib in libraries)
            {
                ProtoCore.AST.AssociativeAST.ImportNode importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
                importNode.ModuleName = lib;

                importNodes.Add(importNode);
            }
            ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(importNodes);
            string code = codeGen.GenerateCode();

            UpdateCmdLineInterpreter(code);
        }

        /// <summary>
        /// Resets the VM whenever a new library is imported and re-imports them
        /// Returns the list of new Library Mirrors for reflection
        /// TODO: It should not be needed once we have language support to insert import statements arbitrarily
        /// </summary>
        /// <param name="libraries"></param>
        /// <returns></returns>
        public List<LibraryMirror> ResetVMAndImportLibrary(List<string> libraries)
        {
            List<LibraryMirror> libs = new List<LibraryMirror>();

            // Reset VM
            ReInitializeLiveRunner();

            // generate import node for each library in input list
            List<AssociativeNode> importNodes = null;
            foreach (string lib in libraries)
            {
                importNodes = new List<AssociativeNode>();

                ProtoCore.AST.AssociativeAST.ImportNode importNode = new ProtoCore.AST.AssociativeAST.ImportNode();
                importNode.ModuleName = lib;

                importNodes.Add(importNode);

                ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(importNodes);
                string code = codeGen.GenerateCode();

                int currentCI = runnerCore.ClassTable.ClassNodes.Count;

                UpdateCmdLineInterpreter(code);

                int postCI = runnerCore.ClassTable.ClassNodes.Count;

                IList<ProtoCore.DSASM.ClassNode> classNodes = new List<ProtoCore.DSASM.ClassNode>();
                for (int i = currentCI; i < postCI; ++i)
                {
                    classNodes.Add(runnerCore.ClassTable.ClassNodes[i]);
                }

                ProtoCore.Mirror.LibraryMirror libraryMirror = ProtoCore.Mirror.Reflection.Reflect(lib, classNodes, runnerCore);
                libs.Add(libraryMirror);
            }

            return libs;
        }

        /// <summary>
        /// Returns runtime warnings.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, List<ProtoCore.RuntimeData.WarningEntry>> GetRuntimeWarnings()
        {
            // Group all warnings by their expression ids, and only keep the last
            // warning for each expression, and then group by GUID.  
            var warnings = runnerCore.RuntimeStatus
                                     .Warnings
                                     .Where(w => !w.GraphNodeGuid.Equals(Guid.Empty))
                                     .OrderBy(w => w.GraphNodeGuid)
                                     .GroupBy(w => w.GraphNodeGuid);

            var ret = new Dictionary<Guid, List<ProtoCore.RuntimeData.WarningEntry>>();
            foreach (var w in warnings)
            {
                Guid guid = w.FirstOrDefault().GraphNodeGuid;
                ret[guid] = new List<ProtoCore.RuntimeData.WarningEntry>(w);
            }

            return ret;
        }


        #endregion
    }

}
