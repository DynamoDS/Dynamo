﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using ProtoScript.Runners;
using Dynamo.Nodes;
using ProtoCore.DSASM.Mirror;

namespace Dynamo.DSEngine
{
    public delegate void AstBuiltEventHandler(object sender, AstBuilder.ASTBuiltEventArgs e);

    /// <summary>
    /// A controller to coordinate the interactions between some DesignScript
    /// sub components like library managment, live runner and so on.
    /// </summary>
    public class EngineController: IAstNodeContainer, IDisposable
    {
        public event AstBuiltEventHandler AstBuilt;

        private LiveRunnerServices liveRunnerServices;
        private LibraryServices libraryServices;
        private AstBuilder astBuilder;
        private SyncDataManager syncDataManager;
        private Queue<GraphSyncData> graphSyncDataQueue = new Queue<GraphSyncData>();
        private int shortVarCounter = 0;
        private readonly DynamoModel dynamoModel;

        private Object MacroMutex = new Object();

        public EngineController(DynamoModel dynamoModel, string geometryFactoryFileName)
        {
            this.dynamoModel = dynamoModel;

            libraryServices = LibraryServices.GetInstance();
            libraryServices.LibraryLoading += this.LibraryLoading;
            libraryServices.LibraryLoadFailed += this.LibraryLoadFailed;
            libraryServices.LibraryLoaded += this.LibraryLoaded;

            liveRunnerServices = new LiveRunnerServices(dynamoModel, this, geometryFactoryFileName);
            liveRunnerServices.ReloadAllLibraries(libraryServices.Libraries.ToList());

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
        }

        #region Function Groups

        /// <summary>
        /// Return all function groups.
        /// </summary>
        public IEnumerable<FunctionGroup> GetFunctionGroups() 
        {
            return libraryServices.BuiltinFunctionGroups.Union(
                       libraryServices.Libraries.SelectMany(lib => libraryServices.GetFunctionGroups(lib)));
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

        #region Value queries

        /// <summary>
        /// Get runtime mirror for variable.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public RuntimeMirror GetMirror(string variableName)
        {
            lock (MacroMutex)
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
        /// Get string representation of the value of variable.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public string GetStringValue(string variableName)
        {
            lock (MacroMutex)
            {
                RuntimeMirror mirror = GetMirror(variableName);
                return null == mirror ? "null" : mirror.GetStringData();
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
            lock (MacroMutex)
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
            lock (MacroMutex)
            {
                var activeNodes = nodes.Where(n => n.State != ElementState.Error);

                if (activeNodes.Any())
                    astBuilder.CompileToAstNodes(activeNodes, true);

                return VerifyGraphSyncData();
            }
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
                lock (MacroMutex)
                {

                    lock (graphSyncDataQueue)
                    {
                        return graphSyncDataQueue.Count > 0;
                    }
                }
            }
        }

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
            List<AssociativeNode> outputs,
            IEnumerable<string> parameters)
        {
            lock (MacroMutex)
            {
                astBuilder.CompileCustomNodeDefinition(def, nodes, outputs, parameters);
                return VerifyGraphSyncData();
            }
        }

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
            lock (MacroMutex)
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
                node.ClearError();
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
            liveRunnerServices.ReloadAllLibraries(libraryServices.Libraries.ToList());

            // Mark all nodes as dirty so that AST for the whole graph will be
            // regenerated.
            foreach (var node in dynamoModel.HomeSpace.Nodes)
            {
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
            ProtoCore.Core core = GraphToDSCompiler.GraphUtilities.GetCore();
            var cbs = core.CodeBlockList;
            if (cbs == null || cbs.Count > 0)
            {
                return false;
            }

            var idx = cbs[0].symbolTable.IndexOf(var, ProtoCore.DSASM.Constants.kGlobalScope, ProtoCore.DSASM.Constants.kGlobalScope);
            return idx == ProtoCore.DSASM.Constants.kInvalidIndex;
        }


        #endregion

    }
}
