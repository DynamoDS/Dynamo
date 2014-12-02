using Autodesk.DesignScript.Interfaces;
using Dynamo.DSEngine.CodeCompletion;
using Dynamo.Core.Threading;
using Dynamo.Models;
using Dynamo.Nodes;
using DynamoUtilities;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Mirror;
using ProtoCore.Utils;
using ProtoScript.Runners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildWarning = ProtoCore.BuildData.WarningEntry;
using RuntimeWarning = ProtoCore.RuntimeData.WarningEntry;

namespace Dynamo.DSEngine
{
    public delegate void AstBuiltEventHandler(object sender, AstBuilder.ASTBuiltEventArgs e);

    /// <summary>
    /// A controller to coordinate the interactions between some DesignScript
    /// sub components like library managment, live runner and so on.
    /// </summary>
    public class EngineController : IAstNodeContainer, IDisposable
    {
        public event AstBuiltEventHandler AstBuilt;

        private readonly LiveRunnerServices liveRunnerServices;
        private readonly LibraryServices libraryServices;
        private CodeCompletionServices codeCompletionServices; 
        private readonly AstBuilder astBuilder;
        private readonly SyncDataManager syncDataManager;
        private readonly Queue<GraphSyncData> graphSyncDataQueue = new Queue<GraphSyncData>();
        private int shortVarCounter = 0;
        private readonly DynamoModel dynamoModel;
        private readonly ProtoCore.Core libraryCore;
        private readonly Object macroMutex = new Object();

        public EngineController(DynamoModel dynamoModel, string geometryFactoryFileName)
        {
            this.dynamoModel = dynamoModel;

            // Create a core which is used for parsing code and loading libraries
            libraryCore = new ProtoCore.Core(new Options()
            {
                RootCustomPropertyFilterPathName = string.Empty
            });
            libraryCore.Executives.Add(Language.kAssociative,new ProtoAssociative.Executive(libraryCore));
            libraryCore.Executives.Add(Language.kImperative, new ProtoImperative.Executive(libraryCore));
            libraryCore.ParsingMode = ParseMode.AllowNonAssignment;

            libraryServices = new LibraryServices(libraryCore);
            libraryServices.LibraryLoading += this.LibraryLoading;
            libraryServices.LibraryLoadFailed += this.LibraryLoadFailed;
            libraryServices.LibraryLoaded += this.LibraryLoaded;

            liveRunnerServices = new LiveRunnerServices(dynamoModel, this, geometryFactoryFileName);
            liveRunnerServices.ReloadAllLibraries(libraryServices.ImportedLibraries);

            codeCompletionServices = new CodeCompletionServices(LiveRunnerCore);

            astBuilder = new AstBuilder(dynamoModel, this);
            syncDataManager = new SyncDataManager();

            dynamoModel.NodeDeleted += NodeDeleted;
        }

        public void Dispose()
        {
            dynamoModel.NodeDeleted -= NodeDeleted;
            liveRunnerServices.Dispose();

            libraryServices.LibraryLoading -= this.LibraryLoading;
            libraryServices.LibraryLoadFailed -= this.LibraryLoadFailed;
            libraryServices.LibraryLoaded -= this.LibraryLoaded;

            // TODO: Find a better way to save loaded libraries. 
            if (!DynamoModel.IsTestMode)
            {
                foreach (var library in libraryServices.ImportedLibraries)
                {
                    DynamoPathManager.Instance.AddPreloadLibrary(library);
                }
            }

            libraryServices.Dispose();
            codeCompletionServices = null;

            libraryCore.Cleanup();
        }

        #region Function Groups

        /// <summary>
        /// Return all function groups.
        /// </summary>
        public IEnumerable<FunctionGroup> GetFunctionGroups()
        {
            return libraryServices.BuiltinFunctionGroups.Union(
                       libraryServices.ImportedLibraries.SelectMany(lib => libraryServices.GetFunctionGroups(lib)));
        }

        /// <summary>
        /// Import library.
        /// </summary>
        /// <param name="library"></param>
        public void ImportLibrary(string library)
        {
            libraryServices.ImportLibrary(library, this.dynamoModel.Logger);
        }

        #endregion


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
                    mirror = liveRunnerServices.GetMirror(variableName);
                }
                catch (SymbolNotFoundException)
                {
                    // The variable hasn't been defined yet. Just skip it. 
                }
                catch (Exception ex)
                {
                    dynamoModel.Logger.Log("Failed to get mirror for variable: " + variableName + "; reason: " +
                                                 ex.Message);
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
        /// <returns></returns>
        public bool GenerateGraphSyncData(IEnumerable<NodeModel> nodes)
        {
            lock (macroMutex)
            {
                var activeNodes = nodes.Where(n => !n.IsInErrorState);

                if (activeNodes.Any())
                    astBuilder.CompileToAstNodes(activeNodes, true);

                return VerifyGraphSyncData();
            }
        }


#if ENABLE_DYNAMO_SCHEDULER

        /// <summary>
        /// This method is called on the main thread from UpdateGraphAsyncTask
        /// to generate GraphSyncData for a list of updated nodes.
        /// </summary>
        /// <param name="updatedNodes">The list of all updated nodes.</param>
        /// <returns>This method returns true if GraphSyncData is generated from 
        /// the list of updated nodes. If updatedNodes is empty or does not 
        /// result in any GraphSyncData, then this method returns false.</returns>
        /// 
        internal GraphSyncData ComputeSyncData(IEnumerable<NodeModel> updatedNodes)
        {
            if (updatedNodes == null)
                return null;

            var activeNodes = updatedNodes.Where(n => !n.IsInErrorState);
            if (activeNodes.Any())
            {
                astBuilder.CompileToAstNodes(activeNodes, true);
            }

            if (!VerifyGraphSyncData() || ((graphSyncDataQueue.Count <= 0)))
            {
                return null;
            }

            return graphSyncDataQueue.Dequeue();
        }

#endif

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

#if !ENABLE_DYNAMO_SCHEDULER

        /// <summary>
        /// Generate graph sync data based on the input Dynamo custom node information.
        /// Return false if all nodes are clean.
        /// </summary>
        /// <param name="def"></param>
        /// <param name="nodes"></param>
        /// <param name="outputs"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool GenerateGraphSyncDataForCustomNode(
            CustomNodeDefinition def,
            IEnumerable<NodeModel> nodes,
            IEnumerable<AssociativeNode> outputs,
            IEnumerable<string> parameters)
        {
            lock (macroMutex)
            {
                astBuilder.CompileCustomNodeDefinition(def, nodes, outputs, parameters);
                return VerifyGraphSyncData();
            }
        }

#else

        private Queue<GraphSyncData> pendingCustomNodeSyncData = new Queue<GraphSyncData>();

        /// <summary>
        /// Generate graph sync data based on the input Dynamo custom node information.
        /// Return false if all nodes are clean.
        /// </summary>
        /// <param name="def"></param>
        /// <param name="nodes"></param>
        /// <param name="outputs"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool GenerateGraphSyncDataForCustomNode(
            CustomNodeDefinition def,
            IEnumerable<NodeModel> nodes,
            IEnumerable<AssociativeNode> outputs,
            IEnumerable<string> parameters)
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

                astBuilder.CompileCustomNodeDefinition(def, nodes, outputs, parameters);
                if (!VerifyGraphSyncData() || (graphSyncDataQueue.Count == 0))
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
        internal void ProcessPendingCustomNodeSyncData(DynamoScheduler scheduler)
        {
            while (pendingCustomNodeSyncData.Count > 0)
            {
                var initParams = new CompileCustomNodeParams()
                {
                    SyncData = pendingCustomNodeSyncData.Dequeue(),
                    EngineController = this
                };

                var compileTask = new CompileCustomNodeAsyncTask(scheduler);
                if (compileTask.Initialize(initParams))
                    scheduler.ScheduleForExecution(compileTask);
            }
        }

#endif

        private bool VerifyGraphSyncData()
        {
            GraphSyncData data = syncDataManager.GetSyncData();
            syncDataManager.ResetStates();

            var reExecuteNodesIds = dynamoModel.HomeSpace.Nodes
                .Where(n => n.ForceReExecuteOfNode)
                .Select(n => n.GUID);
            if (reExecuteNodesIds.Any() && data.ModifiedSubtrees != null)
            {
                for (int i = 0; i < data.ModifiedSubtrees.Count; ++i)
                {
                    var st = data.ModifiedSubtrees[i];
                    if (reExecuteNodesIds.Contains(st.GUID))
                    {
                        Subtree newSt = new Subtree(st.AstNodes, st.GUID);
                        newSt.ForceExecution = true;
                        data.ModifiedSubtrees[i] = newSt;
                    }
                }
            }

            if ((data.AddedSubtrees != null && data.AddedSubtrees.Count > 0) ||
                (data.ModifiedSubtrees != null && data.ModifiedSubtrees.Count > 0) ||
                (data.DeletedSubtrees != null && data.DeletedSubtrees.Count > 0))
            {
                lock (graphSyncDataQueue)
                {
                    graphSyncDataQueue.Enqueue(data);
                }
                return true;
            }

            return false;
        }

#if ENABLE_DYNAMO_SCHEDULER

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

            liveRunnerServices.UpdateGraph(graphSyncData);
        }

        internal IDictionary<Guid, List<BuildWarning>> GetBuildWarnings()
        {
            return liveRunnerServices.GetBuildWarnings();
        }

        internal IDictionary<Guid, List<RuntimeWarning>> GetRuntimeWarnings()
        {
            return liveRunnerServices.GetRuntimeWarnings();
        }

#endif

        /// <summary>
        /// Update graph with graph sync data.
        /// </summary>
        /// <param name="fatalException">The exception that is not handled 
        /// anywhere within the LiveRunnerServices.UpdateGraph method. This 
        /// parameter will always be set to null if there is no unhandled 
        /// exception thrown from within the UpdateGraph call.</param>
        /// <returns>Returns true if any update has taken place, or false 
        /// otherwise.</returns>
        /// 
        public bool UpdateGraph(ref Exception fatalException)
        {
            lock (macroMutex)
            {

                bool updated = false;
                fatalException = null;

                ClearWarnings();

                lock (graphSyncDataQueue)
                {
                    while (graphSyncDataQueue.Count > 0)
                    {
                        try
                        {
                            var data = graphSyncDataQueue.Dequeue();
                            liveRunnerServices.UpdateGraph(data);
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

                            dynamoModel.Logger.Log("Update graph failed: " + e.Message);
                        }
                    }
                }

                if (updated)
                {
                    ShowBuildWarnings();
                    ShowRuntimeWarnings();
                }

                return updated;
            }
        }

        private void ClearWarnings()
        {
            var warningNodes = dynamoModel.HomeSpace.Nodes.Where(n => n.State == ElementState.Warning);

            foreach (var node in warningNodes)
            {
                node.ClearRuntimeError();
            }
        }

        private void ShowRuntimeWarnings()
        {
            // Clear all previous warnings
            var warnings = liveRunnerServices.GetRuntimeWarnings();
            foreach (var item in warnings)
            {
                Guid guid = item.Key;
                var node = dynamoModel.HomeSpace.Nodes.FirstOrDefault(n => n.GUID == guid);
                if (node != null)
                {
                    string warningMessage = string.Join("\n", item.Value.Select(w => w.Message));
                    node.Warning(warningMessage);
                }
            }
        }

        private void ShowBuildWarnings()
        {
            // Clear all previous warnings
            var warnings = liveRunnerServices.GetBuildWarnings();
            foreach (var item in warnings)
            {
                Guid guid = item.Key;
                var node = dynamoModel.HomeSpace.Nodes.FirstOrDefault(n => n.GUID == guid);
                if (node != null)
                {
                    string warningMessage = string.Join("\n", item.Value.Select(w => w.Message));
                    node.Warning(warningMessage);
                }
            }
        }

        /// <summary>
        /// Get function descriptor from managed function name.
        /// </summary>
        /// <param name="mangledFunctionName"></param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string library, string managledName)
        {
            return libraryServices.GetFunctionDescriptor(library, managledName);
        }

        /// <summary>
        /// Get function descriptor from managed function name.
        /// </summary>
        /// <param name="managledName"></param>
        /// <returns></returns>
        public FunctionDescriptor GetFunctionDescriptor(string managledName)
        {
            return libraryServices.GetFunctionDescriptor(managledName);
        }

        /// <summary>
        /// LibraryLoading event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibraryLoading(object sender, LibraryServices.LibraryLoadingEventArgs e)
        {
        }

        /// <summary>
        /// LibraryLoadFailed event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibraryLoadFailed(object sender, LibraryServices.LibraryLoadFailedEventArgs e)
        {
        }

        /// <summary>
        /// LibraryLoaded event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibraryLoaded(object sender, LibraryServices.LibraryLoadedEventArgs e)
        {
            string newLibrary = e.LibraryPath;

            // Load all functions defined in that library.
            dynamoModel.SearchModel.Add(libraryServices.GetFunctionGroups(newLibrary));

            // Reset the VM
            liveRunnerServices.ReloadAllLibraries(libraryServices.ImportedLibraries);

            // The LiveRunner core is newly instantiated whenever a new library is imported
            // due to which a new instance of CodeCompletionServices needs to be created with the new Core
            codeCompletionServices = new CodeCompletionServices(LiveRunnerCore);

            
            foreach (var node in dynamoModel.HomeSpace.Nodes)
            {
                // All CBN's need to be pre-compiled again after a new library is loaded
                // to warn for any new namespace conflicts that may arise.
                CodeBlockNodeModel codeBlockNode = node as CodeBlockNodeModel;
                if (codeBlockNode != null)
                {
                    codeBlockNode.ProcessCodeDirect();
                }

                // Mark all nodes as dirty so that AST for the whole graph will be
                // regenerated.
                node.RequiresRecalc = true;
            }
        }

        #region Implement IAstNodeContainer interface

        public void OnAstNodeBuilding(Guid nodeGuid)
        {
            syncDataManager.MarkForAdding(nodeGuid);
        }

        public void OnAstNodeBuilt(Guid nodeGuid, IEnumerable<AssociativeNode> astNodes)
        {
            foreach (var astNode in astNodes)
            {
                syncDataManager.AddNode(nodeGuid, astNode);
            }

            if (AstBuilt != null)
            {
                if (dynamoModel.NodeMap.ContainsKey(nodeGuid))
                {
                    AstBuilt(this, new AstBuilder.ASTBuiltEventArgs(dynamoModel.NodeMap[nodeGuid], astNodes));
                }
            }
        }

        #endregion

        /// <summary>
        /// NodeDeleted event handler.
        /// </summary>
        /// <param name="node"></param>
        private void NodeDeleted(NodeModel node)
        {
            syncDataManager.DeleteNodes(node.GUID);
        }

        #region N2C

        public string ConvertNodesToCode(IEnumerable<NodeModel> nodes, out Dictionary<string, string> variableNames)
        {
            variableNames = new Dictionary<string, string>();
            if (!nodes.Any())
                return string.Empty;

            string code = Dynamo.DSEngine.NodeToCodeUtils.ConvertNodesToCode(this.dynamoModel, nodes);
            if (string.IsNullOrEmpty(code))
                return code;

            StringBuilder sb = new StringBuilder(code);
            string newVar;
            foreach (var node in nodes)
            {
                if (node is CodeBlockNodeModel)
                {
                    var tempVars = (node as CodeBlockNodeModel).TempVariables;
                    foreach (var tempVar in tempVars)
                    {
                        newVar = GenerateShortVariable();
                        sb = sb.Replace(tempVar, newVar);
                        variableNames.Add(tempVar, newVar);
                    }
                }
                else
                {
                    string thisVar = node.AstIdentifierForPreview.ToString();
                    newVar = GenerateShortVariable();
                    sb = sb.Replace(thisVar, newVar);
                    variableNames.Add(thisVar, newVar);
                }

                //get the names of inputs as well and replace them with simpler names
                foreach (var inport in node.InPorts)
                {
                    if (inport.Connectors.Count == 0)
                        continue;
                    var inputNode = inport.Connectors[0].Start.Owner;
                    if (nodes.Contains(inputNode))
                        continue;
                    if (!(inputNode is CodeBlockNodeModel))
                    {
                        string inputVar = inputNode.AstIdentifierForPreview.ToString();
                        if (!variableNames.ContainsKey(inputVar))
                        {
                            newVar = GenerateShortVariable();
                            variableNames.Add(inputVar, newVar);
                            sb = sb.Replace(inputVar, newVar);
                        }
                    }
                    else
                    {
                        var cbn = inputNode as CodeBlockNodeModel;
                        int portIndex = cbn.OutPorts.IndexOf(inport.Connectors[0].Start);
                        string inputVar = cbn.GetAstIdentifierForOutputIndex(portIndex).Value;
                        if (cbn.TempVariables.Contains(inputVar))
                        {
                            if (!variableNames.ContainsKey(inputVar))
                            {
                                newVar = GenerateShortVariable();
                                variableNames.Add(inputVar, newVar);
                                sb = sb.Replace(inputVar, newVar);
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }


        private string GenerateShortVariable()
        {
            while (true)
            {
                shortVarCounter++;
                string var = AstBuilder.StringConstants.ShortVarPrefix + shortVarCounter.ToString();

                if (!HasVariableDefined(var))
                    return var;
            }
        }

        private bool HasVariableDefined(string var)
        {
            var cbs = libraryCore.CodeBlockList;
            if (cbs == null || cbs.Count > 0)
            {
                return false;
            }

            var idx = cbs[0].symbolTable.IndexOf(var, ProtoCore.DSASM.Constants.kGlobalScope, ProtoCore.DSASM.Constants.kGlobalScope);
            return idx == ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        #endregion

        public bool TryParseCode(ref ParseParam parseParam)
        {
            return CompilerUtils.PreCompileCodeBlock(libraryCore, ref parseParam);
        }
    }
}
