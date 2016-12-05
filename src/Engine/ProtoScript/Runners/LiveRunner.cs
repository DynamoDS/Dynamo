using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
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
    /// <summary>
    /// A subtree represents a node in graph. It contains a list of AST node.
    /// </summary>
    public struct Subtree
    {
        /// <summary>
        /// The GUID of corresponding UI node.
        /// </summary>
        public Guid GUID;

        /// <summary>
        /// Specify if all ast nodes should be executed.
        /// </summary>
        public bool ForceExecution;

        /// <summary>
        /// Sepcify if the VM should do delta computation for these ASTs
        /// By default it is true.
        /// </summary>
        public bool DeltaComputation;

        public List<AssociativeNode> AstNodes;
        public List<AssociativeNode> ModifiedAstNodes;

        public Subtree(List<AssociativeNode> astNodes, System.Guid guid)
        {
            GUID = guid;
            AstNodes = astNodes;
            ForceExecution = false;
            DeltaComputation = true;
            ModifiedAstNodes = new List<AssociativeNode>();
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
        /// <summary>
        /// Session ID
        /// </summary>
        public Guid SessionID
        {
            get;
            private set;
        }

        /// <summary>
        /// Deleted sub trees.
        /// </summary>
        public List<Subtree> DeletedSubtrees
        {
            get;
            private set;
        }

        /// <summary>
        /// Added sub trees.
        /// </summary>
        public List<Subtree> AddedSubtrees
        {
            get;
            private set;
        }

        /// <summary>
        /// Modified sub trees.
        /// </summary>
        public List<Subtree> ModifiedSubtrees
        {
            get;
            private set;
        }

        /// <summary>
        /// Newly added nodes' IDs.
        /// </summary>
        public IEnumerable<Guid> AddedNodeIDs
        {
            get
            {
                return AddedSubtrees.Select(ts => ts.GUID);
            }
        }

        /// <summary>
        /// Modified nodes' IDs.
        /// </summary>
        public IEnumerable<Guid> ModifiedNodeIDs
        {
            get
            {
                return ModifiedSubtrees.Select(ts => ts.GUID);
            }
        }

        /// <summary>
        /// Deleted nodes' IDs.
        /// </summary>
        public IEnumerable<Guid> DeletedNodeIDs 
        {
            get
            {
                return DeletedSubtrees.Select(ts => ts.GUID);
            }
        }

        /// <summary>
        /// All node IDs in this graph sync data.
        /// </summary>
        public IEnumerable<Guid> NodeIDs
        {
            get
            {
                return AddedNodeIDs.Concat(ModifiedNodeIDs).Concat(DeletedNodeIDs);
            }
        }

        public GraphSyncData(List<Subtree> deleted, List<Subtree> added, List<Subtree> modified):
            this(Guid.Empty, deleted, added, modified)
        {
        }

        public GraphSyncData(Guid sessionID, List<Subtree> deleted, List<Subtree> added, List<Subtree> modified)
        {
            SessionID = sessionID;
            DeletedSubtrees = deleted ?? new List<Subtree>();
            AddedSubtrees = added ?? new List<Subtree>();
            ModifiedSubtrees = modified ?? new List<Subtree>();
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
        public bool ContainsDeltaAST = false;
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
        private RuntimeCore runtimeCore = null;

        public void Apply(ProtoCore.Core core, RuntimeCore runtimeCore, ChangeSetData changeSet)
        {
            Validity.Assert(null != changeSet);
            this.core = core;
            this.runtimeCore = runtimeCore;
            ApplyChangeSetDeleted(changeSet);
            ApplyChangeSetModified(changeSet);
            ApplyChangeSetForceExecute(changeSet);
        }

        private void ApplyChangeSetDeleted(ChangeSetData changeSet)
        {
            DeactivateGraphnodes(changeSet.DeletedBinaryExprASTNodes);
            UndefineFunctions(changeSet.DeletedFunctionDefASTNodes);
            ProtoCore.AssociativeEngine.Utils.MarkGraphNodesDirtyFromFunctionRedef(runtimeCore, changeSet.DeletedFunctionDefASTNodes);
        }

        private void ApplyChangeSetModified(ChangeSetData changeSet)
        {
            ClearModifiedNestedBlocks(changeSet.ModifiedNestedLangBlock);

            DeactivateGraphnodes(changeSet.RemovedBinaryNodesFromModification);

            // Undefine a function that was removed 
            UndefineFunctions(changeSet.RemovedFunctionDefNodesFromModification);

            // Mark all graphnodes dependent on the removed function as dirty
            ProtoCore.AssociativeEngine.Utils.MarkGraphNodesDirtyFromFunctionRedef(runtimeCore, changeSet.RemovedFunctionDefNodesFromModification);

            // Mark all graphnodes dependent on the modified functions as dirty
            ProtoCore.AssociativeEngine.Utils.MarkGraphNodesDirtyFromFunctionRedef(runtimeCore, changeSet.ModifiedFunctions);
        }


        private void ApplyChangeSetForceExecute(ChangeSetData changeSet)
        {
            // Check if there are nodes to force execute
            if (changeSet.ForceExecuteASTList.Count > 0)
            {
                // Mark all graphnodes dirty which are associated with the force exec ASTs
                ProtoCore.AssociativeGraph.GraphNode firstDirtyNode = ProtoCore.AssociativeEngine.Utils.MarkGraphNodesDirtyAtGlobalScope
(runtimeCore, changeSet.ForceExecuteASTList);
                Validity.Assert(firstDirtyNode != null);

                // If the only ASTs to execute are force exec, then set the entrypoint here.
                // Otherwise the entrypoint is set by the code generator when the new ASTs are compiled
                if (!changeSet.ContainsDeltaAST)
                {
                    runtimeCore.SetStartPC(firstDirtyNode.updateBlock.startpc);
                }
            }
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

            var workingStack = new Stack<AssociativeNode>(nodeList);
            var astIDs = new HashSet<int>();

            while (workingStack.Any())
            {
                var node = workingStack.Pop() as BinaryExpressionNode;
                if (node != null)
                {
                    astIDs.Add(node.OriginalAstID);
                    workingStack.Push(node.RightNode);
                }
            }

            if (!astIDs.Any())
            {
                return;
            }

            foreach (var gnode in core.DSExecutable.instrStreamList[0].dependencyGraph.GraphList)
            {
                if (astIDs.Contains(gnode.OriginalAstID))
                {
                    gnode.isActive = false;
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
        private ProtoCore.RuntimeCore runtimeCore = null;

        public ChangeSetData csData { get; private set; }

        public ChangeSetComputer(ProtoCore.Core core, ProtoCore.RuntimeCore runtimeCore)
        {
            this.core = core;
            this.runtimeCore = runtimeCore;
            currentSubTreeList = new Dictionary<Guid, Subtree>();
        }

        /// <summary>
        /// Deep clone the change set computer
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public ChangeSetComputer Clone()
        {
            ChangeSetComputer comp = new ChangeSetComputer(this.core, this.runtimeCore);

            comp.currentSubTreeList = new Dictionary<Guid, Subtree>();
            foreach (var subTreePairs in currentSubTreeList)
            {
                comp.currentSubTreeList.Add(subTreePairs.Key, subTreePairs.Value); 
            }

            comp.csData = new ChangeSetData();
            comp.csData.ContainsDeltaAST = csData.ContainsDeltaAST;
            comp.csData.DeletedBinaryExprASTNodes = new List<AssociativeNode>(csData.DeletedBinaryExprASTNodes);
            comp.csData.DeletedFunctionDefASTNodes = new List<AssociativeNode>(csData.DeletedFunctionDefASTNodes);
            comp.csData.RemovedBinaryNodesFromModification = new List<AssociativeNode>(csData.RemovedBinaryNodesFromModification);
            comp.csData.RemovedFunctionDefNodesFromModification = new List<AssociativeNode>(csData.RemovedFunctionDefNodesFromModification);
            comp.csData.ForceExecuteASTList = new List<AssociativeNode>(csData.ForceExecuteASTList);
            comp.csData.ModifiedFunctions = new List<AssociativeNode>(csData.ModifiedFunctions);
            comp.csData.ModifiedNestedLangBlock = new List<AssociativeNode>(csData.ModifiedNestedLangBlock);
            return comp;
        }

        /// <summary>
        /// Given deltaGraphNodes, estimate the reachable graphnodes from the live core
        /// </summary>
        /// <param name="liveCore"></param>
        /// <param name="deltaGraphNodes"></param>
        /// <returns></returns>
        private List<GraphNode> EstimateReachableGraphNodes(RuntimeCore rt, List<GraphNode> deltaGraphNodes)
        {
            List<GraphNode> reachableNodes = new List<GraphNode>();
            foreach (GraphNode executingNode in deltaGraphNodes)
            {
                reachableNodes.AddRange(ProtoCore.AssociativeEngine.Utils.UpdateDependencyGraph(
                    executingNode,
                    rt.CurrentExecutive.CurrentDSASMExec,
                    executingNode.exprUID,
                    executingNode.IsSSANode(),
                    true,
                    0,
                    true));
            }
            return reachableNodes;
        }

        /// <summary>
        /// Estimate the nodes that are affected by the changes in astList
        /// Returns a list of guids that map to the affected nodes
        /// </summary>
        /// <param name="astList"></param>
        /// <returns></returns>
        public List<Guid> EstimateNodesAffectedByASTList(List<AssociativeNode> astList)
        {
            List<Guid> cbnGuidList = new List<Guid>();

            // Get the VM graphnodes associated with the astList
            List<GraphNode> deltaGraphNodeList = ProtoCore.AssociativeEngine.Utils.GetGraphNodesFromAST(core.DSExecutable, astList);
            
            // Get the reachable VM graphnodes  given the modified graphnode list
            List<GraphNode> reachableNodes = EstimateReachableGraphNodes(runtimeCore, deltaGraphNodeList);

            // Append the modified nodes(deltaGraphNodeList) into the reachable list as they are also going to be executed when run
            reachableNodes.AddRange(deltaGraphNodeList);

            // Get the list of guid's of the ASTs
            foreach (GraphNode graphnode in reachableNodes)
            {
                if (!cbnGuidList.Contains(graphnode.guid))
                {
                    cbnGuidList.Add(graphnode.guid);
                }
            }
            return cbnGuidList;
        }

        private IEnumerable<AssociativeNode> GetDeltaAstListDeleted(IEnumerable<Subtree> deletedSubTrees)
        {
            var deltaAstList = new List<AssociativeNode>();
            csData.DeletedBinaryExprASTNodes = new List<AssociativeNode>();
            csData.DeletedFunctionDefASTNodes = new List<AssociativeNode>();

            if (deletedSubTrees == null || !deletedSubTrees.Any())
            {
                return deltaAstList;
            }

            foreach (var st in deletedSubTrees)
            {
                var deletedBinaryExpressions = new List<AssociativeNode>();

                if (st.AstNodes != null && st.AstNodes.Any())
                {
                    deletedBinaryExpressions.AddRange(st.AstNodes);
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
                            deletedBinaryExpressions.AddRange(removeSubTree.AstNodes);
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
                var nullNodes = BuildNullAssignments(deletedBinaryExpressions, st.GUID);
                deltaAstList.AddRange(nullNodes);

                core.BuildStatus.ClearWarningsForGraph(st.GUID);
                runtimeCore.RuntimeStatus.ClearWarningsForGraph(st.GUID);
                csData.DeletedBinaryExprASTNodes.AddRange(deletedBinaryExpressions);
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
                else if (rightNode is ProtoCore.AST.ImperativeAST.LanguageBlockNode)
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

        /// <summary>
        /// Update the cached ASTs in the subtree given the modified ASTs
        /// </summary>
        /// <param name="st"></param>
        /// <param name="modifiedASTList"></param>
        private void UpdateCachedASTList(Subtree st, List<AssociativeNode> modifiedASTList)
        {
            List<AssociativeNode> removedModifiedNodes = new List<AssociativeNode>();

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

                foreach (var removedAST in csData.RemovedBinaryNodesFromModification)
                {
                    core.BuildStatus.ClearWarningsForAst(removedAST.ID);
                    runtimeCore.RuntimeStatus.ClearWarningsForAst(removedAST.ID);
                }
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

                    // Update the cached AST to reflect the change

                    List<AssociativeNode> newCachedASTList = new List<AssociativeNode>();

                    // Get all the unomodified ASTs and append them to the cached ast list 
                    newCachedASTList.AddRange(GetUnmodifiedASTList(oldSubTree.AstNodes, st.AstNodes));

                    // Append all the modified ASTs to the cached ast list 
                    newCachedASTList.AddRange(modifiedASTList);

                    // ================================================================================
                    // Get a list of functions that were removed
                    // This is the list of functions that exist in oldSubTree.AstNodes and no longer exist in st.AstNodes
                    // This will passed to the changeset applier to handle removed functions in the VM
                    // ================================================================================
                    IEnumerable<AssociativeNode> removedFunctions = oldSubTree.AstNodes.Where(f => f is FunctionDefinitionNode && !st.AstNodes.Contains(f));
                    csData.RemovedFunctionDefNodesFromModification.AddRange(removedFunctions);

                    st.AstNodes.Clear();
                    st.AstNodes.AddRange(newCachedASTList);
                    currentSubTreeList[st.GUID] = st;
                }
            }
        }


        private IEnumerable<AssociativeNode> GetDeltaAstListModified(List<Subtree> modifiedSubTrees)
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

            for (int n = 0; n < modifiedSubTrees.Count(); ++n)
            {
                var modifiedSubTree = modifiedSubTrees[n];

                if (modifiedSubTree.AstNodes == null)
                {
                    continue;
                }

                if (modifiedSubTree.DeltaComputation)
                {
                    // Get modified statements
                    var modifiedASTList = GetModifiedNodes(modifiedSubTree);
                    modifiedSubTree.ModifiedAstNodes.Clear();
                    modifiedSubTree.ModifiedAstNodes.AddRange(modifiedASTList);
                    deltaAstList.AddRange(modifiedASTList);

                    foreach (AssociativeNode node in modifiedASTList)
                    {
                        var bnode = node as BinaryExpressionNode;
                        if (bnode != null)
                        {
                            bnode.guid = modifiedSubTrees[n].GUID;
                        }
                        SetNestedLanguageBlockASTGuids(modifiedSubTree.GUID, new List<ProtoCore.AST.Node>() { bnode });
                    }

                    UpdateCachedASTList(modifiedSubTree, modifiedSubTree.ModifiedAstNodes);
                }
                else
                {
                    // No delta computation. 
                    // Right now it is only disabled for code block node, but we may
                    // completely disable it. Details about why doing so:
                    // https://github.com/DynamoDS/Dynamo/pull/7282
                    Subtree oldSubTree;
                    List<AssociativeNode> deletedExpressions = new List<AssociativeNode>();
                    if (currentSubTreeList.TryGetValue(modifiedSubTree.GUID, out oldSubTree) && oldSubTree.AstNodes != null)
                    {
                        csData.RemovedFunctionDefNodesFromModification.AddRange(oldSubTree.AstNodes.Where(f => f is FunctionDefinitionNode));
                        deletedExpressions.AddRange(oldSubTree.AstNodes);
                        var nullNodes = BuildNullAssignments(deletedExpressions, modifiedSubTree.GUID);
                        deltaAstList.AddRange(nullNodes);
                    }
                    currentSubTreeList.Remove(modifiedSubTree.GUID);

                    core.BuildStatus.ClearWarningsForGraph(modifiedSubTree.GUID);
                    runtimeCore.RuntimeStatus.ClearWarningsForGraph(modifiedSubTree.GUID);
                    csData.DeletedBinaryExprASTNodes.AddRange(deletedExpressions);

                    currentSubTreeList.Add(modifiedSubTree.GUID, modifiedSubTree);
                    deltaAstList.AddRange(modifiedSubTree.AstNodes);

                    foreach (AssociativeNode node in modifiedSubTree.AstNodes)
                    {
                        var bnode = node as BinaryExpressionNode;
                        if (bnode != null)
                        {
                            bnode.guid = modifiedSubTree.GUID;
                        }

                        SetNestedLanguageBlockASTGuids(modifiedSubTree.GUID, new List<ProtoCore.AST.Node>() { bnode });
                    }

                    var modifiedFunctions = modifiedSubTree.AstNodes.Where(f => f is FunctionDefinitionNode);
                    csData.ModifiedFunctions.AddRange(modifiedFunctions);
                }
            }

            return deltaAstList;
        }


        public List<AssociativeNode> GetDeltaASTList(GraphSyncData syncData)
        {
            csData = new ChangeSetData();
            var finalDeltaAstList = new List<AssociativeNode>();

            var deletedDeltaAsts = GetDeltaAstListDeleted(syncData.DeletedSubtrees);
            finalDeltaAstList.AddRange(deletedDeltaAsts);

            var addedDeltaAsts = GetDeltaAstListAdded(syncData.AddedSubtrees);
            finalDeltaAstList.AddRange(addedDeltaAsts);

            var modifiedDeltaAsts = GetDeltaAstListModified(syncData.ModifiedSubtrees);
            finalDeltaAstList.AddRange(modifiedDeltaAsts);

            csData.ContainsDeltaAST = finalDeltaAstList.Any();
            return finalDeltaAstList;
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
                                    newNode.ExpressionUID = prevBinaryNode.ExpressionUID;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the only the modified nodes from the subtree by checking of the previous cached instance
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
                                                bnode.ExpressionUID = prevBinaryNode.ExpressionUID;
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
        /// Returns the ASTs from the previous list that no longer exist in the new list
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
        /// Returns the ASTs from the previous list that that still exist in the new list
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
        /// Creates a list of null assignment statements where the lhs is retrieved from an ast list
        /// </summary>
        /// <param name="astList"></param>
        /// <returns></returns>
        private List<BinaryExpressionNode> BuildNullAssignments(List<AssociativeNode> astList, Guid guid)
        {
            var astNodeList = new List<BinaryExpressionNode>();
            if (astList == null)
            {
                return astNodeList;
            }

            Stack<AssociativeNode> workingStack = new Stack<AssociativeNode>(astList);
            while (workingStack.Any())
            {
                var bNode = workingStack.Pop() as BinaryExpressionNode;
                if (bNode == null)
                {
                    continue;
                }

                IdentifierNode leftNode = bNode.LeftNode as IdentifierNode;
                if (leftNode == null || leftNode.ArrayDimensions != null)
                {
                    continue;
                }
                
                var nullAssignment = AstFactory.BuildAssignment(leftNode, AstFactory.BuildNullNode());
                nullAssignment.guid = guid;
                astNodeList.Add(nullAssignment);

                if (bNode.RightNode is BinaryExpressionNode)
                {
                    workingStack.Push(bNode.RightNode);
                }
            }

            return astNodeList;
        }
    }

    public interface ILiveRunner
    {
        ProtoCore.Core Core { get; }
        ProtoCore.RuntimeCore RuntimeCore { get; }

        #region Synchronous call
        void UpdateGraph(GraphSyncData syncData);
        List<Guid> PreviewGraph(GraphSyncData syncData);
        void UpdateCmdLineInterpreter(string code);
        ProtoCore.Mirror.RuntimeMirror InspectNodeValue(string nodeName);

        void UpdateGraph(AssociativeNode astNode);
        #endregion

        void ResetVMAndResyncGraph(IEnumerable<string> libraries);
        void ReInitializeLiveRunner();
        IDictionary<Guid, List<ProtoCore.Runtime.WarningEntry>> GetRuntimeWarnings();
        IDictionary<Guid, List<ProtoCore.BuildData.WarningEntry>> GetBuildWarnings();
        IEnumerable<Guid> GetExecutedAstGuids(Guid sessionID);
        void RemoveRecordedAstGuidsForSession(Guid SessionID);
    }

    public partial class LiveRunner : ILiveRunner, IDisposable
    {
        /// <summary>
        /// These are configuration parameters passed by host application to be 
        /// consumed by geometry library and persistent manager implementation. 
        /// </summary>
        public class Configuration
        {
            /// <summary>
            /// The configuration parameters that needs to be passed to
            /// different applications.
            /// </summary>
            private Dictionary<String, object> passThroughConfiguration;
            public IDictionary<string, object> PassThroughConfiguration
            {
                get
                {
                    return passThroughConfiguration;
                }
            }

            /// <summary>
            /// The path of the root graph/module
            /// </summary>
            public string RootModulePathName
            {
                get;
                set;
            }

            /// <summary>
            /// List of search directories to resolve any file reference
            /// </summary>
            private List<String> searchDirectories;
            public IList<string> SearchDirectories
            {
                get
                {
                    return searchDirectories;
                }
            }

            public Configuration()
            {
                passThroughConfiguration = new Dictionary<string, object>();
                searchDirectories = new List<string>();
                RootModulePathName = string.Empty;
            }
        }

        private ProtoScriptRunner runner;
        private Core runnerCore = null;
        public Core Core
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

        private ProtoCore.RuntimeCore runtimeCore = null;
        public ProtoCore.RuntimeCore RuntimeCore
        {
            get
            {
                return runtimeCore;
            }
            private set
            {
                runtimeCore = value;
            }
        }

        private Options coreOptions = null;
        private Configuration configuration = null;
        private int deltaSymbols = 0;
        private ProtoCore.CompileTime.Context staticContext = null;
        private readonly object mutexObject = new object();
        private ChangeSetComputer changeSetComputer;
        private ChangeSetApplier changeSetApplier;
        private Dictionary<Guid, List<Guid>> executedAstGuids = new Dictionary<Guid, List<Guid>>();

        public LiveRunner()
            : this(new Configuration())
        {
        }

        public LiveRunner(Configuration configuration)
        {
            this.configuration = configuration;

            runner = new ProtoScriptRunner();

            InitCore();

            staticContext = new ProtoCore.CompileTime.Context();

            changeSetComputer = new ChangeSetComputer(runnerCore, runtimeCore);
            changeSetApplier = new ChangeSetApplier();
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
                    runtimeCore.Cleanup();
                    runnerCore = null;
                }

                if (runtimeCore != null)
                {
                    runtimeCore.Cleanup();
                }
            }
        }

        private void InitCore()
        {
            coreOptions = new Options
            {
                IsDeltaExecution = true,
                BuildOptErrorAsWarning = true,
                ExecutionMode = ExecutionMode.Serial
            };

            runnerCore = new ProtoCore.Core(coreOptions);
            runnerCore.Compilers.Add(ProtoCore.Language.Associative, new ProtoAssociative.Compiler(runnerCore));
            runnerCore.Compilers.Add(ProtoCore.Language.Imperative, new ProtoImperative.Compiler(runnerCore));

            runnerCore.Options.RootModulePathName = configuration.RootModulePathName;
            runnerCore.Options.IncludeDirectories = configuration.SearchDirectories.ToList();
            foreach (var item in configuration.PassThroughConfiguration)
            {
                runnerCore.Configurations[item.Key] = item.Value;
            }

            CreateRuntimeCore();
        }

        /// <summary>
        /// Cretes a new instance of the RuntimeCore object
        /// </summary>
        private void CreateRuntimeCore()
        {
            runtimeCore = new ProtoCore.RuntimeCore(runnerCore.Heap, runnerCore.Options);
        }

        /// <summary>
        /// Setup the RuntimeCore for the next execution cycle
        /// </summary>
        private void SetupRuntimeCoreForExecution(bool isCodeCompiled)
        {
            // runnerCore.GlobOffset is the number of global symbols that need to be allocated on the stack
            // The argument to Reallocate is the number of ONLY THE NEW global symbols as the stack needs to accomodate this delta
            int globalStackFrameSize = runnerCore.GlobOffset - deltaSymbols;

            // If there are lesser symbols to allocate for this run, then it means nodes were deleted.
            // We just leave them in the global stack as no symbols point to this memory location in the stack anyway
            // This will be addressed when instruction cache is optimized
            runtimeCore.SetupForExecution(runnerCore, globalStackFrameSize);
            if (isCodeCompiled)
            {
                runtimeCore.SetupStartPC();
            }
          
            // Store the current number of global symbols
            deltaSymbols = runnerCore.GlobOffset;
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
            lock (mutexObject)
            {
                return Reflection.Reflect(nodeName, 0, runtimeCore, runnerCore);
            }
        }

        /// <summary>
        /// This API needs to be called for every delta AST preview
        /// </summary>
        /// <param name="syncData"></param>
        public List<Guid> PreviewGraph(GraphSyncData syncData)
        {
            lock (mutexObject)
            {
                return PreviewInternal(syncData);
            }
        }

        /// <summary>
        /// This API needs to be called for every delta AST execution
        /// </summary>
        /// <param name="syncData"></param>
        public void UpdateGraph(GraphSyncData syncData)
        {
            lock (mutexObject)
            {
                SynchronizeInternal(syncData);
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
            lock (mutexObject)
            {
                SynchronizeInternal(code);
            }
        }

        #region Internal Implementation

        private bool Compile(List<AssociativeNode> astList, Core targetCore)
        {
            bool succeeded = runner.CompileAndGenerateExe(astList, targetCore, new ProtoCore.CompileTime.Context());
            if (succeeded)
            {
                // Update the symbol tables
                // TODO Jun: Expand to accomoadate the list of symbols
                staticContext.symbolTable = targetCore.DSExecutable.runtimeSymbols[0];
            }
            return succeeded;
        }

        private void Execute(bool isCodeCompiled)
        {
            try
            {
                SetupRuntimeCoreForExecution(isCodeCompiled);
                if (isCodeCompiled)
                {
                    // If isCodeCompiled is false, nothing to execute but still
                    // bounce and push stack frame. Need to investigate why 
                    // previouslsy ExecuteLive() is called when isCodeCompiled is
                    // false.
                    runner.ExecuteLive(runnerCore, runtimeCore);
                }
            }
            catch (ProtoCore.Exceptions.ExecutionCancelledException)
            {
                runtimeCore.Cleanup();
                ReInitializeLiveRunner();
            }
        }

        private bool CompileAndExecute(string code)
        {
            // TODO Jun: Revisit all the Compile functions and remove the blockId out argument
            bool succeeded = runner.CompileAndGenerateExe(code, runnerCore, new ProtoCore.CompileTime.Context());
            if (succeeded)
            {
                Execute(!string.IsNullOrEmpty(code));
            }
            return succeeded;
        }

        private bool CompileAndExecute(List<AssociativeNode> astList)
        {
            bool succeeded = Compile(astList, runnerCore);
            if (succeeded)
            {
                Execute(astList.Count > 0);
            }
            return succeeded;
        }

        /// <summary>
        /// Functionalities applied to the VM after an execution cycle
        /// </summary>
        private void PostExecution()
        {
            ApplyUpdate();
            HandleWarnings();
        }

        /// <summary>
        /// Handle warnings that will be reported to the frontend
        /// </summary>
        private void HandleWarnings()
        {
            SuppressResovledUnboundVariableWarnings();
        }

        /// <summary>
        /// Removes all warnings that were initially unbound variables but were resolved at runtime
        /// </summary>
        private void SuppressResovledUnboundVariableWarnings()
        {
            runnerCore.BuildStatus.RemoveUnboundVariableWarnings(runnerCore.DSExecutable.UpdatedSymbols);

            runnerCore.DSExecutable.UpdatedSymbols.Clear();
        }

        private void ForceGC()
        {
            var gcRoots = runtimeCore.CurrentExecutive.CurrentDSASMExec.CollectGCRoots();
            runtimeCore.RuntimeMemory.Heap.FullGC(gcRoots, runtimeCore.CurrentExecutive.CurrentDSASMExec);
        }

        private void ApplyUpdate()
        {
            if (ProtoCore.AssociativeEngine.Utils.IsGlobalScopeDirty(runnerCore.DSExecutable))
            {
                ResetForDeltaExecution();
                runnerCore.Options.ApplyUpdate = true;
                Execute(true);

                // Execute() will push a stack frame in SetupAndBounceStackFrame().
                // In normal execution, that stack frame will pop in RETB. But in
                // ApplyUpdate(), there is no RETB instruciton, so need to manually
                // cleanup stack frame.
                StackValue restoreFramePointer = runtimeCore.RuntimeMemory.GetAtRelative(ProtoCore.DSASM.StackFrame.FrameIndexFramePointer);
                runtimeCore.RuntimeMemory.FramePointer = (int)restoreFramePointer.IntegerValue;
                runtimeCore.RuntimeMemory.PopFrame(ProtoCore.DSASM.StackFrame.StackFrameSize);

                runtimeCore.CurrentExecutive.CurrentDSASMExec.PopInterpreterProps();
            }
            ForceGC();
        }

        /// <summary>
        /// Resets few states in the core to prepare the core for a new
        /// delta code compilation and execution
        /// </summary>
        private void ResetForDeltaExecution()
        {
            runnerCore.ResetForDeltaExecution();
            runtimeCore.ResetForDeltaExecution();
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
            PostExecution();
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
            PostExecution();
        }

        private List<Guid> PreviewInternal(GraphSyncData syncData)
        {
            var previewChangeSetComputer = changeSetComputer.Clone();
            // Get the list of ASTs that will be affected by syncData
            var previewAstList = previewChangeSetComputer.GetDeltaASTList(syncData);

            // Get the list of guid's affected by the astlist
            List<Guid> cbnGuidList = previewChangeSetComputer.EstimateNodesAffectedByASTList(previewAstList);
            return cbnGuidList;
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
            bool anyForcedExecutedNodes = changeSetComputer.csData.ForceExecuteASTList.Any();
            changeSetApplier.Apply(runnerCore, runtimeCore, changeSetComputer.csData);
            if (finalDeltaAstList.Any() || anyForcedExecutedNodes)
            {
                CompileAndExecuteForDeltaExecution(finalDeltaAstList);
            }

            var guids = runtimeCore.ExecutedAstGuids.ToList();
            executedAstGuids[syncData.SessionID] = guids;
            runtimeCore.RemoveExecutedAstGuids();
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
            runner = new ProtoScriptRunner();
            deltaSymbols = 0;
            InitCore();
            staticContext = new ProtoCore.CompileTime.Context();
            changeSetComputer = new ChangeSetComputer(runnerCore, runtimeCore);
            CLRModuleType.ClearTypes();
        }

        /// <summary>
        /// This is called temporarily to reset the VM and recompile the entire graph with new import 
        /// statements whenever a node from a new library is added to the graph.
        /// TODO: It should not be needed once we have language support to insert import statements arbitrarily
        /// </summary>
        /// <param name="libraries"></param>
        /// <param name="syncData"></param>
        public void ResetVMAndResyncGraph(IEnumerable<string> libraries)
        {
            // Reset VM
            ReInitializeLiveRunner();

            if (!libraries.Any())
            {
                return;
            }

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
        /// Returns runtime warnings.
        /// </summary>
        /// <returns></returns>
        public IDictionary<Guid, List<ProtoCore.Runtime.WarningEntry>> GetRuntimeWarnings()
        {
            var ret = new Dictionary<Guid, List<ProtoCore.Runtime.WarningEntry>>();
            if (runtimeCore == null)
                return ret;

            // Group all warnings by their expression ids, and only keep the last
            // warning for each expression, and then group by GUID.  
            var warnings = runtimeCore.RuntimeStatus
                                     .Warnings
                                     .Where(w => !w.GraphNodeGuid.Equals(Guid.Empty))
                                     .OrderBy(w => w.GraphNodeGuid)
                                     .GroupBy(w => w.GraphNodeGuid);

            foreach (var warningGroup in warnings)
            {
                Guid guid = warningGroup.FirstOrDefault().GraphNodeGuid;
                // If there are two warnings in the same expression, take the first one.
                var trimmedWarnings = warningGroup.OrderBy(w => w.ExpressionID)
                                                  .GroupBy(w => w.ExpressionID)
                                                  .Select(g => g.FirstOrDefault());
                ret[guid] = new List<ProtoCore.Runtime.WarningEntry>(trimmedWarnings);
            }

            return ret;
        }

        /// <summary>
        /// Returns build warnings.
        /// </summary>
        /// <returns></returns>
        public IDictionary<Guid, List<ProtoCore.BuildData.WarningEntry>> GetBuildWarnings()
        {
            var ret = new Dictionary<Guid, List<ProtoCore.BuildData.WarningEntry>>();
            if (runnerCore == null)
                return ret;

            // Group all warnings by their expression ids, and only keep the last
            // warning for each expression, and then group by GUID.  
            var warnings = runnerCore.BuildStatus
                                     .Warnings
                                     .Where(w => !w.GraphNodeGuid.Equals(Guid.Empty))
                                     .OrderBy(w => w.GraphNodeGuid)
                                     .GroupBy(w => w.GraphNodeGuid);

            foreach (var w in warnings)
            {
                Guid guid = w.FirstOrDefault().GraphNodeGuid;
                ret[guid] = new List<ProtoCore.BuildData.WarningEntry>(w);
            }

            return ret;
        }

        public IEnumerable<Guid> GetExecutedAstGuids(Guid sessionID)
        {
            List<Guid> guids = null;
            if (executedAstGuids.TryGetValue(sessionID, out guids))
            {
                return guids;
            }
            else
            {
                return new List<Guid>();
            }
        }

        public void RemoveRecordedAstGuidsForSession(Guid SessionID)
        {
            executedAstGuids.Remove(SessionID);
        }
        #endregion
    }

}
