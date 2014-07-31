using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo
{
    public class CustomNodeDefinition
    {
        internal CustomNodeDefinition() : this(Guid.NewGuid()) { }

        internal CustomNodeDefinition(Guid id)
        {
            FunctionId = id;
        }

        /// <summary>
        ///     Function name.
        /// </summary>
        public string FunctionName
        {
            get { return AstBuilder.StringConstants.FunctionPrefix + 
                         FunctionId.ToString().Replace("-", string.Empty); }
        }

        /// <summary>
        ///     Function unique ID.
        /// </summary>
        public Guid FunctionId { get; internal set; }

        /// <summary>
        ///     Custom node definition's workspace model. 
        /// </summary>
        public CustomNodeWorkspaceModel WorkspaceModel { get; internal set; }

        /// <summary>
        ///     Function parameters
        /// </summary>
        public IEnumerable<string> Parameters { get; internal set; }

        /// <summary>
        ///     If the function returns a dictionary, it specifies all keys in
        ///     that dictionary.
        /// </summary>
        public IEnumerable<string> ReturnKeys { get; internal set; }

        /// <summary>
        ///    A definition is a proxy definition if we are not able to load
        ///    the corresponding .dyf file. All custom node instances that 
        ///    point to proxy custom node definition will be in error state 
        ///    until .dyf file loaded properly.
        /// </summary>
        public bool IsProxy { get; set; }

        /// <summary>
        ///     User friendly name on UI.
        /// </summary>
        public string DisplayName
        {
            get { return WorkspaceModel.Name; }
        }

        /// <summary>
        ///     If need to rebuild ast nodes.
        /// </summary>
        public bool RequiresRecalc { get; internal set; }

        #region Dependencies

        public IEnumerable<CustomNodeDefinition> Dependencies
        {
            get
            {
                return FindAllDependencies(new HashSet<CustomNodeDefinition>());
            }
        }

        public IEnumerable<CustomNodeDefinition> DirectDependencies
        {
            get
            {
                return FindDirectDependencies();
            }
        }

        public bool IsBeingLoaded { get; set; }

        private IEnumerable<CustomNodeDefinition> FindAllDependencies(HashSet<CustomNodeDefinition> dependencySet)
        {
            var query = DirectDependencies.Where(def => !dependencySet.Contains(def));

            foreach (var definition in query)
            {
                yield return definition;
                dependencySet.Add(definition);
                foreach (var def in definition.FindAllDependencies(dependencySet))
                    yield return def;
            }
        }

        private IEnumerable<CustomNodeDefinition> FindDirectDependencies()
        {
            return WorkspaceModel.Nodes
                            .OfType<Function>()
                            .Select(node => node.Definition)
                            .Where(def => def != this)
                            .Distinct();
        }

        #endregion
        
        #region DS Compilation

        /// <summary>
        /// Compiles this custom node definition, updating all UI instances to match
        /// inputs and outputs and registering new definition with the EngineController.
        /// </summary>
        /// <param name="controller"></param>
        public void Compile(EngineController controller)
        {
            // If we are loading dyf file, dont compile it until all nodes are loaded
            // otherwise some intermediate function defintions will be created.
            // TODO: This is a hack, in reality we should be preventing this from being called at the Workspace.Modified() level --SJE
            if (IsBeingLoaded || IsProxy)
                return;

            #region Outputs and Inputs and UI updating

            #region Find outputs

            // Find output elements for the node
            List<Output> outputs = WorkspaceModel.Nodes.OfType<Output>().ToList();
 
            var topMost = new List<Tuple<int, NodeModel>>();

            List<string> outNames;

            // if we found output nodes, add select their inputs
            // these will serve as the function output
            if (outputs.Any())
            {
                topMost.AddRange(
                    outputs.Where(x => x.HasInput(0)).Select(x => Tuple.Create(0, x as NodeModel)));
                outNames = outputs.Select(x => x.Symbol).ToList();
            }
            else
            {
                outNames = new List<string>();

                // if there are no explicitly defined output nodes
                // get the top most nodes and set THEM as the output
                IEnumerable<NodeModel> topMostNodes = WorkspaceModel.GetTopMostNodes();

                var rtnPorts =
                    //Grab multiple returns from each node
                    topMostNodes.SelectMany(
                        topNode =>
                            //If the node is a recursive instance...
                            topNode is Function && (topNode as Function).Definition == this
                                // infinity output
                                ? new[] { new { portIndex = 0, node = topNode, name = "∞" } }
                                // otherwise, grab all ports with connected outputs and package necessary info
                                : topNode.OutPortData
                                    .Select(
                                        (port, i) =>
                                            new { portIndex = i, node = topNode, name = port.NickName })
                                    .Where(x => !topNode.HasOutput(x.portIndex)));

                foreach (var rtnAndIndex in rtnPorts.Select((rtn, i) => new { rtn, idx = i }))
                {
                    topMost.Add(Tuple.Create(rtnAndIndex.rtn.portIndex, rtnAndIndex.rtn.node));
                    outNames.Add(rtnAndIndex.rtn.name ?? rtnAndIndex.idx.ToString());
                }
            }

            var nameDict = new Dictionary<string, int>();
            foreach (var name in outNames)
            {
                if (nameDict.ContainsKey(name))
                    nameDict[name]++;
                else
                    nameDict[name] = 0;
            }

            nameDict = nameDict.Where(x => x.Value != 0).ToDictionary(x => x.Key, x => x.Value);

            outNames.Reverse();

            var keys = new List<string>();
            foreach (var name in outNames)
            {
                int amt;
                if (nameDict.TryGetValue(name, out amt))
                {
                    nameDict[name] = amt - 1;
                    keys.Add(name == "" ? amt + ">" : name + amt);
                }
                else
                    keys.Add(name);
            }

            keys.Reverse();

            ReturnKeys = keys;

            #endregion

            //Find function entry point, and then compile
            var inputNodes = WorkspaceModel.Nodes.OfType<Symbol>().ToList();
            var parameters = inputNodes.Select(x => string.IsNullOrEmpty(x.InputSymbol) ? x.AstIdentifierForPreview.Value: x.InputSymbol);
            Parameters = inputNodes.Select(x => x.InputSymbol);

            //Update existing function nodes which point to this function to match its changes
            var customNodeInstances = dynSettings.Controller.DynamoModel.AllNodes
                        .OfType<Function>()
                        .Where(el => el.Definition != null && el.Definition == this);
            
            foreach (var node in customNodeInstances)
                node.ResyncWithDefinition();

            //Call OnSave for all saved elements
            foreach (var node in WorkspaceModel.Nodes)
                node.OnSave();

            #endregion

            controller.GenerateGraphSyncDataForCustomNode(
                this,
                WorkspaceModel.Nodes.Where(x => !(x is Symbol)),
                topMost.Select(x => x.Item2.GetAstIdentifierForOutputIndex(x.Item1) as AssociativeNode).ToList(),
                parameters);

            // Not update graph until Run 
            // if (success)
            //    controller.UpdateGraph();
        }

        #endregion

        #region Custom Node Management

        public bool AddToSearch()
        {
            return
                dynSettings.Controller.SearchViewModel.Add(new CustomNodeInfo(  FunctionId, 
                                                                                WorkspaceModel.Name,
                                                                                WorkspaceModel.Category,
                                                                                WorkspaceModel.Description,
                                                                                WorkspaceModel.FileName ));
        }

        public void UpdateCustomNodeManager()
        {
            dynSettings.CustomNodeManager.SetNodeInfo(new CustomNodeInfo(   FunctionId,
                                                                            WorkspaceModel.Name,
                                                                            WorkspaceModel.Category,
                                                                            WorkspaceModel.Description,
                                                                            WorkspaceModel.FileName));
        }

        public bool SyncWithWorkspace(bool addToSearch, bool compileFunction)
        {

            // Get the internal nodes for the function
            var functionWorkspace = WorkspaceModel;

            try
            {
                // Add function defininition
                dynSettings.Controller.CustomNodeManager.AddFunctionDefinition(FunctionId, this);

                // search
                if (addToSearch)
                {
                    AddToSearch();
                }

                var info = new CustomNodeInfo(FunctionId, functionWorkspace.Name, functionWorkspace.Category,
                                              functionWorkspace.Description, WorkspaceModel.FileName);

                dynSettings.Controller.CustomNodeManager.SetNodeInfo(info);

#if USE_DSENGINE
                Compile(dynSettings.Controller.EngineController);
#else
                CompileAndAddToEnvironment(dynSettings.Controller.FSchemeEnvironment);
#endif
            }
            catch (Exception e)
            {
                dynSettings.DynamoLogger.Log("Error saving:" + e.GetType());
                dynSettings.DynamoLogger.Log(e);
                return false;
            }

            return true;
        }

        #endregion
    }
}
