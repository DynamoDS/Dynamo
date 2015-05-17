using Autodesk.DesignScript.Interfaces;
using Dynamo.DSEngine.CodeCompletion;
using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Mirror;
using ProtoCore.Namespace;
using ProtoScript.Runners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using BuildWarning = ProtoCore.BuildData.WarningEntry;
using Constants = ProtoCore.DSASM.Constants;
using RuntimeWarning = ProtoCore.Runtime.WarningEntry;
using ProtoCore.Utils;

namespace Dynamo.DSEngine
{
    public delegate void AstBuiltEventHandler(object sender, AstBuilder.ASTBuiltEventArgs e);

    /// <summary>
    /// A controller to coordinate the interactions between some DesignScript
    /// sub components like library managment, live runner and so on.
    /// </summary>
    public class EngineController : LogSourceBase, IAstNodeContainer, IDisposable
    {
        public event AstBuiltEventHandler AstBuilt;

        public event Action<TraceReconciliationEventArgs> TraceReconcliationComplete;
        private void OnTraceReconciliationComplete(TraceReconciliationEventArgs e)
        {
            if (TraceReconcliationComplete != null)
            {
                TraceReconcliationComplete(e);
            }
        }

        private readonly LiveRunnerServices liveRunnerServices;
        private readonly LibraryServices libraryServices;
        private CodeCompletionServices codeCompletionServices;
        private readonly AstBuilder astBuilder;
        private readonly SyncDataManager syncDataManager;
        private readonly Queue<GraphSyncData> graphSyncDataQueue = new Queue<GraphSyncData>();
        private readonly Queue<List<Guid>> previewGraphQueue = new Queue<List<Guid>>();
        public bool VerboseLogging;

        private readonly Object macroMutex = new Object();

        public static CompilationServices CompilationServices;

        /// <summary>
        /// Get DesignScript core.
        /// </summary>
        public ProtoCore.Core LiveRunnerCore
        {
            get
            {
                return liveRunnerServices.Core;
            }
        }

        /// <summary>
        /// Get DesignScript runtime core.
        /// </summary>
        public ProtoCore.RuntimeCore LiveRunnerRuntimeCore
        {
            get
            {
                return liveRunnerServices.RuntimeCore;
            }
        }


        /// <summary>
        /// Return libary service instance.
        /// </summary>
        public LibraryServices LibraryServices
        {
            get { return libraryServices; }
        }

        public CodeCompletionServices CodeCompletionServices
        {
            get { return codeCompletionServices; }
        }

        /// <summary>
        /// A property defining whether the EngineController has been disposed or not.
        /// This is a conservative field, as there should only be one owner of a valid
        /// EngineController or not.
        /// </summary>
        public bool IsDisposed { get; private set; }

        public EngineController(LibraryServices libraryServices, string geometryFactoryFileName, bool verboseLogging)
        {
            this.libraryServices = libraryServices;
            libraryServices.LibraryLoaded += LibraryLoaded;
            CompilationServices = new CompilationServices(libraryServices.LibraryManagementCore);

            liveRunnerServices = new LiveRunnerServices(this, geometryFactoryFileName);

            liveRunnerServices.ReloadAllLibraries(libraryServices.ImportedLibraries);
            libraryServices.SetLiveCore(LiveRunnerCore);

            codeCompletionServices = new CodeCompletionServices(LiveRunnerCore);

            astBuilder = new AstBuilder(this);
            syncDataManager = new SyncDataManager();

            VerboseLogging = verboseLogging;
        }

        public void Dispose()
        {
            // This flag must be set immediately
            IsDisposed = true;

            libraryServices.LibraryLoaded -= LibraryLoaded;

            liveRunnerServices.Dispose();
            codeCompletionServices = null;
        }

        #region Function Groups

        /// <summary>
        /// Return all function groups.
        /// </summary>
        public IEnumerable<FunctionGroup> GetFunctionGroups()
        {
            return libraryServices.GetAllFunctionGroups();
        }

        /// <summary>
        /// Import library.
        /// </summary>
        /// <param name="library"></param>
        public void ImportLibrary(string library)
        {
            LibraryServices.ImportLibrary(library);
        }

        #endregion

        #region Value queries

        /// <summary>
        /// Get runtime mirror for variable.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public RuntimeMirror GetMirror(string variableName)
        {
            lock (macroMutex)
            {
                RuntimeMirror mirror = null;
                try
                {
                    mirror = liveRunnerServices.GetMirror(variableName, VerboseLogging);
                }
                catch (SymbolNotFoundException)
                {
                    // The variable hasn't been defined yet. Just skip it. 
                }
                catch (Exception ex)
                {
                    Log(string.Format(Properties.Resources.FailedToGetMirrorVariable,variableName,
                        ex.Message));
                }

                return mirror;
            }
        }

        /// <summary>
        /// Get a list of IGraphicItem of variable if it is a geometry object;
        /// otherwise returns null.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public List<IGraphicItem> GetGraphicItems(string variableName)
        {
            lock (macroMutex)
            {
                RuntimeMirror mirror = GetMirror(variableName);
                return null == mirror ? null : mirror.GetData().GetGraphicsItems();
            }
        }

        #endregion

        /// <summary>
        /// Generate graph sync data based on the input Dynamo nodes. Return 
        /// false if all nodes are clean.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="verboseLogging"></param>
        /// <returns></returns>
        public bool GenerateGraphSyncData(ICollection<NodeModel> nodes, bool verboseLogging)
        {
            lock (macroMutex)
            {
                var activeNodes = nodes.Where(n => !n.IsInErrorState);

                if (activeNodes.Any())
                    astBuilder.CompileToAstNodes(activeNodes, AstBuilder.CompilationContext.DeltaExecution, verboseLogging);

                return VerifyGraphSyncData(nodes);
            }
        }

        /// <summary>
        /// This method is called on the main thread from UpdateGraphAsyncTask
        /// to generate GraphSyncData for a list of updated nodes.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="updatedNodes">The list of all updated nodes.</param>
        /// <param name="verboseLogging"></param>
        /// <returns>This method returns true if GraphSyncData is generated from 
        /// the list of updated nodes. If updatedNodes is empty or does not 
        /// result in any GraphSyncData, then this method returns false.</returns>
        internal GraphSyncData ComputeSyncData(IEnumerable<NodeModel> nodes, IEnumerable<NodeModel> updatedNodes, bool verboseLogging)
        {
            if (updatedNodes == null)
                return null;

            var activeNodes = updatedNodes.Where(n => !n.IsInErrorState);
            if (activeNodes.Any())
            {
                astBuilder.CompileToAstNodes(activeNodes, AstBuilder.CompilationContext.DeltaExecution, verboseLogging);
            }

            if (!VerifyGraphSyncData(nodes) || ((graphSyncDataQueue.Count <= 0)))
            {
                return null;
            }

            return graphSyncDataQueue.Dequeue();
        }


        /// <summary>
        ///  This is called on the main thread from PreviewGraphSyncData
        ///  to generate the list of node id's that will be executed on the next run
        /// </summary>
        /// <param name="updatedNodes">The updated nodes.</param>
        /// <returns>This method returns the list of all reachable node id's from the given
        /// updated nodes</returns>
        internal List<Guid> PreviewGraphSyncData(IEnumerable<NodeModel> updatedNodes, bool verboseLogging)
        {
            if (updatedNodes == null)
                return null;

            var activeNodes = updatedNodes.Where(n => n.State != ElementState.Error);
            if (activeNodes.Any())
            {
                astBuilder.CompileToAstNodes(activeNodes, AstBuilder.CompilationContext.DeltaExecution, verboseLogging);
            }

            GraphSyncData graphSyncdata = syncDataManager.GetSyncData();
            List<Guid> previewGraphData = this.liveRunnerServices.PreviewGraph(graphSyncdata, verboseLogging);

             lock (previewGraphQueue)
             {
                 previewGraphQueue.Enqueue(previewGraphData);
             }
            
            return previewGraphQueue.Dequeue();
        }

        /// <summary>
        /// Return true if there are graph sync data in the queue waiting for
        /// being executed.
        /// </summary>
        /// <returns></returns>
        public bool HasPendingGraphSyncData
        {
            get
            {
                lock (macroMutex)
                {

                    lock (graphSyncDataQueue)
                    {
                        return graphSyncDataQueue.Count > 0;
                    }
                }
            }
        }

        private readonly Queue<GraphSyncData> pendingCustomNodeSyncData = new Queue<GraphSyncData>();

        /// <summary>
        /// Generate graph sync data based on the input Dynamo custom node information.
        /// Return false if all nodes are clean.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="definition"></param>
        /// <param name="verboseLogging"></param>
        /// <returns></returns>
        public bool GenerateGraphSyncDataForCustomNode(IEnumerable<NodeModel> nodes, CustomNodeDefinition definition, bool verboseLogging)
        {
            lock (macroMutex)
            {
                // Any graph updates through the scheduler no longer store their 
                // GraphSyncData in 'graphSyncDataQueue' (any such entry will be 
                // withdrawn from the queue and get associated with an AsyncTask.
                // This check is to ensure that such case does not exist.
                // 
                if (graphSyncDataQueue.Count > 0)
                {
                    throw new InvalidOperationException(
                        "'graphSyncDataQueue' is not empty");
                }

                astBuilder.CompileCustomNodeDefinition(
                    definition.FunctionId,
                    definition.ReturnKeys,
                    definition.FunctionName,
                    definition.FunctionBody,
                    definition.OutputNodes,
                    definition.Parameters,
                    verboseLogging);

                if (!VerifyGraphSyncData(nodes) || (graphSyncDataQueue.Count == 0))
                    return false;

                // GraphSyncData objects accumulated through the compilation above
                // will be stored in 'pendingCustomNodeSyncData'. Entries in this 
                // queue will be used to update custom node graph prior to updating
                // the graph for the home workspace.
                // 
                while (graphSyncDataQueue.Count > 0)
                {
                    var graphSyncData = graphSyncDataQueue.Dequeue();
                    pendingCustomNodeSyncData.Enqueue(graphSyncData);
                }

                return true;
            }
        }

        /// <summary>
        /// DynamoModel calls this method prior to scheduling a graph update for
        /// the home workspace. This method is called to schedule custom node 
        /// compilation since the home workspace update may depend on it. Any 
        /// updates to a CustomNodeDefinition will cause GraphSyncData to be added 
        /// to "pendingCustomNodeSyncData" queue.
        /// </summary>
        /// <param name="scheduler">The scheduler on which custom node compilation 
        /// task can be scheduled.</param>
        /// 
        internal void ProcessPendingCustomNodeSyncData(IScheduler scheduler)
        {
            while (pendingCustomNodeSyncData.Count > 0)
            {
                var initParams = new CompileCustomNodeParams
                {
                    SyncData = pendingCustomNodeSyncData.Dequeue(),
                    EngineController = this
                };

                var compileTask = new CompileCustomNodeAsyncTask(scheduler);
                if (compileTask.Initialize(initParams))
                    scheduler.ScheduleForExecution(compileTask);
            }
        }

        private bool VerifyGraphSyncData(IEnumerable<NodeModel> nodes)
        {
            GraphSyncData graphSyncdata = syncDataManager.GetSyncData();
            syncDataManager.ResetStates();

            var reExecuteNodesIds = new HashSet<Guid>(
                nodes.Where(n => n.NeedsForceExecution)
                     .Select(n => n.GUID));

            if (reExecuteNodesIds.Any() && graphSyncdata.ModifiedSubtrees != null)
            {
                for (int i = 0; i < graphSyncdata.ModifiedSubtrees.Count; ++i)
                {
                    var st = graphSyncdata.ModifiedSubtrees[i];
                    if (reExecuteNodesIds.Contains(st.GUID))
                    {
                        Subtree newSt = new Subtree(st.AstNodes, st.GUID);
                        newSt.ForceExecution = true;
                        graphSyncdata.ModifiedSubtrees[i] = newSt;
                    }
                }
            }

            if ((graphSyncdata.AddedSubtrees != null && graphSyncdata.AddedSubtrees.Count > 0) ||
                (graphSyncdata.ModifiedSubtrees != null && graphSyncdata.ModifiedSubtrees.Count > 0) ||
                (graphSyncdata.DeletedSubtrees != null && graphSyncdata.DeletedSubtrees.Count > 0))
            {
                lock (graphSyncDataQueue)
                {
                    graphSyncDataQueue.Enqueue(graphSyncdata);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method is called by UpdateGraphAsyncTask in the context of 
        /// ISchedulerThread to kick start an update through LiveRunner.
        /// </summary>
        /// <param name="graphSyncData">The GraphSyncData that was generated by 
        /// a prior call to ComputeSyncData at the time UpdateGraphAsyncTask was 
        /// scheduled.</param>
        /// 
        public void UpdateGraphImmediate(GraphSyncData graphSyncData)
        {
            // NOTE: We will not attempt to catch any unhandled exception from 
            // within the execution. Such exception, if any, will be caught by
            // DynamoScheduler.ProcessTaskInternal.

            liveRunnerServices.UpdateGraph(graphSyncData, VerboseLogging);
        }

        internal IDictionary<Guid, List<BuildWarning>> GetBuildWarnings()
        {
            return liveRunnerServices.GetBuildWarnings();
        }

        internal IDictionary<Guid, List<RuntimeWarning>> GetRuntimeWarnings()
        {
            return liveRunnerServices.GetRuntimeWarnings();
        }

        /// <summary>
        /// Update graph with graph sync data.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="fatalException">The exception that is not handled 
        /// anywhere within the LiveRunnerServices.UpdateGraph method. This 
        /// parameter will always be set to null if there is no unhandled 
        /// exception thrown from within the UpdateGraph call.</param>
        /// <returns>Returns true if any update has taken place, or false 
        /// otherwise.</returns>
        public bool UpdateGraph(ICollection<NodeModel> nodes, out Exception fatalException)
        {
            lock (macroMutex)
            {

                bool updated = false;
                fatalException = null;

                ClearWarnings(nodes);

                lock (graphSyncDataQueue)
                {
                    while (graphSyncDataQueue.Count > 0)
                    {
                        try
                        {
                            var data = graphSyncDataQueue.Dequeue();
                            liveRunnerServices.UpdateGraph(data, VerboseLogging);
                            updated = true;
                        }
                        catch (Exception e)
                        {
                            // The exception that is not handled within the UpdateGraph
                            // method is recorded here. The only thing for now is, we 
                            // are only interested in the first unhandled exception.
                            // This decision may change in the future if we decided to 
                            // clear up "graphSyncDataQueue" whenever there is a fatal 
                            // exception?
                            // 
                            if (fatalException == null)
                                fatalException = e;

                            Log("Update graph failed: " + e.Message);
                        }
                    }
                }

                if (updated)
                {
                    ShowBuildWarnings(nodes);
                    ShowRuntimeWarnings(nodes);
                }

                return updated;
            }
        }

        internal void ReconcileTraceDataAndNotify()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("EngineController");
            }

            var callsiteToOrphanMap = new Dictionary<Guid, List<ISerializable>>();
            foreach (var cs in liveRunnerServices.RuntimeCore.RuntimeData.CallsiteCache.Values)
            {
                var orphanedSerializables = cs.GetOrphanedSerializables().ToList();
                if (callsiteToOrphanMap.ContainsKey(cs.CallSiteID))
                {
                    callsiteToOrphanMap[cs.CallSiteID].AddRange(orphanedSerializables);
                }
                else
                {
                    callsiteToOrphanMap.Add(cs.CallSiteID, orphanedSerializables);
                }
            }

            OnTraceReconciliationComplete(new TraceReconciliationEventArgs(callsiteToOrphanMap));
        }

        private static void ClearWarnings(IEnumerable<NodeModel> nodes)
        {
            var warningNodes = nodes.Where(n => n.State == ElementState.Warning);

            foreach (var node in warningNodes)
            {
                node.ClearRuntimeError();
            }
        }

        private void ShowRuntimeWarnings(IEnumerable<NodeModel> nodes)
        {
            // Clear all previous warnings
            var warnings = liveRunnerServices.GetRuntimeWarnings();
            foreach (var item in warnings)
            {
                Guid guid = item.Key;
                var node = nodes.FirstOrDefault(n => n.GUID == guid);
                if (node != null)
                {
                    string warningMessage = string.Join("\n", item.Value.Select(w => w.Message));
                    node.Warning(warningMessage);
                }
            }
        }

        private void ShowBuildWarnings(IEnumerable<NodeModel> nodes)
        {
            // Clear all previous warnings
            var warnings = liveRunnerServices.GetBuildWarnings();
            foreach (var item in warnings)
            {
                Guid guid = item.Key;
                var node = nodes.FirstOrDefault(n => n.GUID == guid);
                if (node != null)
                {
                    string warningMessage = string.Join("\n", item.Value.Select(w => w.Message));
                    node.Warning(warningMessage);
                }
            }
        }

        /// <summary>
        ///     LibraryLoaded event handler.
        /// </summary>
        private void LibraryLoaded(object sender, LibraryServices.LibraryLoadedEventArgs e)
        {
            liveRunnerServices.ReloadAllLibraries(libraryServices.ImportedLibraries);

            // The LiveRunner core is newly instantiated whenever a new library is imported
            // due to which a new instance of CodeCompletionServices needs to be created with the new Core
            codeCompletionServices = new CodeCompletionServices(LiveRunnerCore);
            libraryServices.SetLiveCore(LiveRunnerCore);
        }

        #region Implement IAstNodeContainer interface

        public void OnAstNodeBuilding(Guid nodeGuid)
        {
            syncDataManager.MarkForAdding(nodeGuid);
        }

        public void OnAstNodeBuilt(Guid nodeGuid, IEnumerable<AssociativeNode> astNodes)
        {
            var associativeNodes = astNodes as IList<AssociativeNode> ?? astNodes.ToList();

            foreach (var astNode in associativeNodes)
                syncDataManager.AddNode(nodeGuid, astNode);

            if (AstBuilt != null)
                AstBuilt(this, new AstBuilder.ASTBuiltEventArgs(nodeGuid, associativeNodes));
        }

        #endregion

        /// <summary>
        /// NodeDeleted event handler.
        /// </summary>
        /// <param name="node"></param>
        public void NodeDeleted(NodeModel node)
        {
            syncDataManager.DeleteNodes(node.GUID);
        }

        #region Node2Code

        public NodeToCodeResult ConvertNodesToCode(IEnumerable<NodeModel> graph, IEnumerable<NodeModel> nodes)
        {
            return NodeToCodeUtils.NodeToCode(libraryServices.LibraryManagementCore, astBuilder, graph, nodes);
        }

        private bool HasVariableDefined(string var)
        {
            var cbs = libraryServices.LibraryManagementCore.CodeBlockList;
            if (cbs == null || cbs.Count > 0)
            {
                return false;
            }

            var idx = cbs[0].symbolTable.IndexOf(var, Constants.kGlobalScope, Constants.kGlobalScope);
            return idx == Constants.kInvalidIndex;
        }

        #endregion

    }

    public class CompilationServices
    {
        private  ProtoCore.Core compilationCore;

        public CompilationServices(ProtoCore.Core core)
        {
            compilationCore = core;
        }

        public bool PreCompileCodeBlock(ref ParseParam parseParams)
        {
            return CompilerUtils.PreCompileCodeBlock(compilationCore, ref parseParams);
        }
    }

    public class TraceReconciliationEventArgs : EventArgs
    {
        /// <summary>
        /// A list of ISerializable items.
        /// </summary>
        public Dictionary<Guid,List<ISerializable>> CallsiteToOrphanMap { get; private set; }

        public TraceReconciliationEventArgs(Dictionary<Guid, List<ISerializable>> callsiteToOrphanMap)
        {
            CallsiteToOrphanMap = callsiteToOrphanMap;
        }
    }

    public interface ITraceReconciliationProcessor
    {
        void PostTraceReconciliation(Dictionary<Guid, List<ISerializable>> orphanedSerializables);
    }
}
