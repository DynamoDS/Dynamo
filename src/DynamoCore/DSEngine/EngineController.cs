using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Models;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Mirror;
using ProtoScript.Runners;
using Dynamo.Nodes;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// A controller to coordinate the interactions between some DesignScript
    /// sub components like library managment, live runner and so on.
    /// </summary>
    public class EngineController: IAstNodeContainer
    {
        private LiveRunnerServices liveRunnerServices;
        private LibraryServices libraryServices;
        private AstBuilder astBuilder;
        private SyncDataManager syncDataManager;
        private Queue<GraphSyncData> graphSyncDataQueue = new Queue<GraphSyncData>();
        private int shortVarCounter = 0;

        /// <summary>
        /// libraries is a static property so we retain the loaded library information even after resetting EngineController 
        /// </summary>
        private static List<string> importedLibraries = new List<string>();

        internal EngineController(DynamoController controller, bool isReset)
        {
            GraphToDSCompiler.GraphUtilities.Reset();

            libraryServices = new LibraryServices();
            libraryServices.LibraryLoading += this.LibraryLoading;
            libraryServices.LibraryLoadFailed += this.LibraryLoadFailed;
            libraryServices.LibraryLoaded += this.LibraryLoaded;

            liveRunnerServices = new LiveRunnerServices(this);
            // If EngineController is reset, then load those imported libraries, otherwise
            // those imported libraries will be loaded in LoadLibraries().
            if (isReset)
            {
                liveRunnerServices.ReloadAllLibraries(libraryServices.BuiltinLibraries.Union(importedLibraries).ToList());
                GraphToDSCompiler.GraphUtilities.PreloadAssembly(importedLibraries);
            }
            else
            {
                liveRunnerServices.ReloadAllLibraries(libraryServices.BuiltinLibraries);
            }

            astBuilder = new AstBuilder(this);

            syncDataManager = new SyncDataManager();

            controller.DynamoModel.NodeDeleted += NodeDeleted;
        }

        /// <summary>
        /// Load builtin functions and libraries into Dynamo.
        /// </summary>
        public void LoadLibraries()
        {
            LoadFunctions(libraryServices[LibraryServices.Categories.BuiltIns]);
            LoadFunctions(libraryServices[LibraryServices.Categories.Operators]);

            foreach (var library in libraryServices.BuiltinLibraries)
            {
                LoadFunctions(libraryServices[library]);
            }

            var libs = new List<string>(importedLibraries);
            foreach (var lib in libs)
            {
                libraryServices.ImportLibrary(lib);
            }
        }

        /// <summary>
        /// Import a list of libraries.
        /// </summary>
        /// <param name="libraries"></param>
        public void ImportLibraries(List<string> libraries)
        {
            foreach (string library in libraries)
            {
                if (library.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) ||
                    library.EndsWith(".ds", StringComparison.InvariantCultureIgnoreCase))
                {
                    libraryServices.ImportLibrary(library);
                }
            }
        }

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
        /// Get runtime mirror for variable.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public RuntimeMirror GetMirror(string variableName)
        {
            RuntimeMirror mirror = null;
            try
            {
                mirror = liveRunnerServices.GetMirror(variableName);
            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log("Failed to get mirror for variable: " + variableName + "; reason: " + ex.Message);
            }

            return mirror;
        }

        public string ConvertNodesToCode(IEnumerable<NodeModel> nodes, out Dictionary<string,string> variableNames)
        {
            variableNames = new Dictionary<string, string>();
            if (!nodes.Any())
                return string.Empty;

            string code = Dynamo.DSEngine.NodeToCodeUtils.ConvertNodesToCode(nodes);
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
                    string thisVar = GraphToDSCompiler.GraphUtilities.ASTListToCode(new List<AssociativeNode> { node.AstIdentifierForPreview });
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
                        string inputVar = GraphToDSCompiler.GraphUtilities.ASTListToCode(new List<AssociativeNode> { inputNode.AstIdentifierForPreview });
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

        /// <summary>
        /// Get string representation of the value of variable.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public string GetStringValue(string variableName)
        {
            RuntimeMirror mirror = GetMirror(variableName);
            return null == mirror ? "null" : mirror.GetStringData();
        }

        /// <summary>
        /// Get a list of IGraphicItem of variable if it is a geometry object;
        /// otherwise returns null.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public List<IGraphicItem> GetGraphicItems(string variableName)
        {
            RuntimeMirror mirror = GetMirror(variableName);
            return null == mirror ? null : mirror.GetData().GetGraphicsItems();
        }

        /// <summary>
        /// Generate graph sync data based on the input Dynamo nodes. Return 
        /// false if all nodes are clean.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public bool GenerateGraphSyncData(IEnumerable<NodeModel> nodes)
        {
            IEnumerable<NodeModel> activeNodes = nodes.Where(n => ElementState.Active == n.State);
            if (!activeNodes.Any())
                return false;

            astBuilder.CompileToAstNodes(activeNodes, true);
            return VerifyGraphSyncData();
        }

        /// <summary>
        /// Generate graph sync data based on the input Dynamo custom node information.
        /// Return false if all nodes are clean.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nodes"></param>
        /// <param name="outputs"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool GenerateGraphSyncDataForCustomNode(
            Guid id,
            IEnumerable<NodeModel> nodes,
            List<AssociativeNode> outputs,
            IEnumerable<string> parameters)
        {
            astBuilder.CompileCustomNodeDefinition(id, nodes, outputs, parameters, true);
            return VerifyGraphSyncData();
        }

        private bool VerifyGraphSyncData()
        {
            GraphSyncData data = syncDataManager.GetSyncData();
            syncDataManager.ResetStates();

            if ((data.AddedSubtrees != null && data.AddedSubtrees.Count > 0) ||
                (data.ModifiedSubtrees != null && data.ModifiedSubtrees.Count > 0) ||
                (data.DeletedSubtrees != null && data.DeletedSubtrees.Count > 0))
            {
                graphSyncDataQueue.Enqueue(data);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update graph with graph sync data.
        /// </summary>
        public bool UpdateGraph()
        {
            if (graphSyncDataQueue.Count == 0)
            {
                return false;
            }
            GraphSyncData data = graphSyncDataQueue.Dequeue();

            try
            {
                liveRunnerServices.UpdateGraph(data);
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Update graph failed: " + e.Message);
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Get the corresponding FunctionItem based on mangled function name
        /// </summary>
        /// <param name="mangledFunctionName"></param>
        /// <returns></returns>
        public FunctionItem GetImportedFunction(string mangledFunctionName)
        {
            string searchName = mangledFunctionName.Split(new char[] { '@' })[0];

            List<FunctionItem> functionGroup;
            if (!dynSettings.Controller.DSImportedFunctions.TryGetValue(searchName, out functionGroup))
            {
                return null;
            }

            foreach (var item in functionGroup)
            {
                if (item.MangledName.Equals(mangledFunctionName))
                    return item;
            }

            if (functionGroup.Count > 0)
                return functionGroup[0];
            else
                return null;
        }

        private string GenerateShortVariable()
        {
            while (true)
            {
                shortVarCounter++;
                string var = AstBuilder.StringConstants.SHORT_VAR_PREFIX + shortVarCounter.ToString();

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

        /// <summary>
        /// Load DesignScript functions into Dynamo.
        /// </summary>
        /// <param name="functions"></param>
        private void LoadFunctions(List<FunctionItem> functions)
        {
            if (null == functions)
            {
                return;
            }

            var searchViewModel = dynSettings.Controller.SearchViewModel;
            var controller = dynSettings.Controller;

            foreach (var function in functions)
            {
                searchViewModel.Add(function);

                List<FunctionItem> functionGroup;
                if (!controller.DSImportedFunctions.TryGetValue(function.SearchName, out functionGroup))
                {
                    functionGroup = new List<FunctionItem>();
                    controller.DSImportedFunctions[function.SearchName] = functionGroup;
                }
                functionGroup.Add(function);
            }
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
            // Load all functions defined in that library.
            string newLibrary = e.LibraryPath;
            LoadFunctions(libraryServices[newLibrary]);

            // Reset the VM
            importedLibraries.Clear();
            importedLibraries.AddRange(libraryServices.ImportedLibraries);
            liveRunnerServices.ReloadAllLibraries(libraryServices.BuiltinLibraries.Union(importedLibraries).ToList());

            // Mark all nodes as dirty so that AST for the whole graph will be
            // regenerated.
            foreach (var node in dynSettings.Controller.DynamoViewModel.Model.HomeSpace.Nodes)
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
    }
}
