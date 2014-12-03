using System;
using System.Collections.Generic;
using System.Linq;

using Dynamo.DSEngine;
using Dynamo.Library;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Search;
using Dynamo.Utilities;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo
{
    public class CustomNodeDefinition : IFunctionDescriptor
    {
        public CustomNodeDefinition(
            Guid functionId,
            string displayName,
            IList<NodeModel> nodeModels=null)
        {
            nodeModels = nodeModels ?? new List<NodeModel>();

            #region Find outputs

            // Find output elements for the node

            var outputs = nodeModels.OfType<Output>().ToList();

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
                IEnumerable<NodeModel> topMostNodes = nodeModels.Where(node => node.IsTopMostNode);

                var rtnPorts =
                    //Grab multiple returns from each node
                    topMostNodes.SelectMany(
                        topNode =>
                            //If the node is a recursive instance...
                            topNode is Function && (topNode as Function).Definition.FunctionId == functionId
                                // infinity output
                                ? new[] {new {portIndex = 0, node = topNode, name = "∞"}}
                                // otherwise, grab all ports with connected outputs and package necessary info
                                : topNode.OutPortData
                                    .Select(
                                        (port, i) =>
                                            new {portIndex = i, node = topNode, name = port.NickName})
                                    .Where(x => !topNode.HasOutput(x.portIndex)));

                foreach (var rtnAndIndex in rtnPorts.Select((rtn, i) => new {rtn, idx = i}))
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

            var returnKeys = new List<string>();
            foreach (var name in outNames)
            {
                int amt;
                if (nameDict.TryGetValue(name, out amt))
                {
                    nameDict[name] = amt - 1;
                    returnKeys.Add(name == "" ? amt + ">" : name + amt);
                }
                else
                    returnKeys.Add(name);
            }

            returnKeys.Reverse();

            #endregion

            #region Find inputs

            //Find function entry point, and then compile
            var inputNodes = nodeModels.OfType<Symbol>().ToList();
            var parameters =
                inputNodes.Select(x => new TypedParameter(x.GetAstIdentifierForOutputIndex(0).Value, "var[]..[]"));
            var displayParameters = inputNodes.Select(x => x.InputSymbol);

            #endregion

            FunctionBody = nodeModels.Where(node => !(node is Symbol));
            DisplayName = displayName;
            FunctionId = functionId;
            Parameters = parameters;
            ReturnKeys = returnKeys;
            DisplayParameters = displayParameters;
            OutputNodes = topMost.Select(x => x.Item2.GetAstIdentifierForOutputIndex(x.Item1));
            DirectDependencies = nodeModels
                .OfType<Function>()
                .Select(node => node.Definition)
                .Where(def => def.FunctionId != functionId)
                .Distinct();
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
        public Guid FunctionId { get; private set; }

        /// <summary>
        ///     User-friendly parameters
        /// </summary>
        public IEnumerable<string> DisplayParameters { get; private set; }

        /// <summary>
        ///     Function parameters.
        /// </summary>
        public IEnumerable<TypedParameter> Parameters { get; private set; } 

        /// <summary>
        ///     If the function returns a dictionary, this specifies all keys in
        ///     that dictionary.
        /// </summary>
        public IEnumerable<string> ReturnKeys { get; private set; }

        /// <summary>
        ///     NodeModels making up the body of the custom node.
        /// </summary>
        public IEnumerable<NodeModel> FunctionBody { get; private set; }

        /// <summary>
        ///     Identifiers associated with the outputs of the custom node.
        /// </summary>
        public IEnumerable<AssociativeNode> OutputNodes { get; private set; }

        /// <summary>
        ///    A definition is a proxy definition if we are not able to load
        ///    the corresponding .dyf file. All custom node instances that 
        ///    point to proxy custom node definition will be in error state 
        ///    until .dyf file loaded properly.
        /// </summary>
        [Obsolete("No longer supported.", true)]
        public bool IsProxy { get; set; }

        /// <summary>
        ///     User friendly name on UI.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        ///     If need to rebuild ast nodes.
        /// </summary>
        [Obsolete("No longer supported.", true)]
        public bool RequiresRecalc { get; internal set; }

        /// <summary>
        ///     Custom node definition's workspace model. 
        /// </summary>
        [Obsolete("Workspace is no longer stored on CustomNodeDefinition.", true)]
        public CustomNodeWorkspaceModel Workspace { get; internal set; }

        #region Dependencies

        public IEnumerable<CustomNodeDefinition> Dependencies
        {
            get { return FindAllDependencies(new HashSet<CustomNodeDefinition>()); }
        }

        public IEnumerable<CustomNodeDefinition> DirectDependencies { get; private set; //{
            //    return FindDirectDependencies();
            //}
        }

        [Obsolete("No longer supported.", true)]
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

        #endregion
        
        #region DS Compilation

        /// <summary>
        /// Compiles this custom node definition, updating all UI instances to match
        /// inputs and outputs and registering new definition with the EngineController.
        /// </summary>
        public void Compile(EngineController controller)
        {
            // If we are loading dyf file, dont compile it until all nodes are loaded
            // otherwise some intermediate function defintions will be created.
            // TODO: This is a hack, in reality we should be preventing this from being called at the Workspace.RequestSync() level --SJE
            if (IsBeingLoaded || IsProxy)
                return;

            #region Outputs and Inputs and UI updating

            #region Find outputs

            // Find output elements for the node
            List<Output> outputs = Workspace.Nodes.OfType<Output>().ToList();
 
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
                IEnumerable<NodeModel> topMostNodes = Workspace.GetTopMostNodes();

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
            var inputNodes = Workspace.Nodes.OfType<Symbol>().ToList();
            var parameters = inputNodes.Select(x => x.GetAstIdentifierForOutputIndex(0).Value);
            DisplayParameters = inputNodes.Select(x => x.InputSymbol);

            //Update existing function nodes which point to this function to match its changes
            OnUpdated();

            //Call OnSave for all saved elements
            foreach (var node in Workspace.Nodes)
                node.OnSave();

            #endregion

            var outputNodes = topMost.Select((x) =>
            {
                var n = x.Item2.GetAstIdentifierForOutputIndex(x.Item1);
                return n as AssociativeNode;
            });

            controller.GenerateGraphSyncDataForCustomNode(
                this,
                Workspace.Nodes.Where(x => !(x is Symbol)),
                outputNodes,
                parameters);

            // Not update graph until Run 
            // if (success)
            //    controller.UpdateGraph();
        }

        #endregion

        #region Custom Node Management

        [Obsolete("No longer supported", true)]
        public bool AddToSearch(SearchModel search)
        {
            return
                search.Add(new CustomNodeInfo(  FunctionId, 
                                                Workspace.Name,
                                                Workspace.Category,
                                                Workspace.Description,
                                                Workspace.FileName ));
        }

        [Obsolete("No longer supported", true)]
        public void UpdateCustomNodeManager(CustomNodeManager customNodeManager)
        {
            customNodeManager.SetNodeInfo(new CustomNodeInfo(   FunctionId,
                                                                Workspace.Name,
                                                                Workspace.Category,
                                                                Workspace.Description,
                                                                Workspace.FileName));
        }

        [Obsolete("No longer supported", true)]
        public bool SyncWithWorkspace(DynamoModel dynamoModel, bool addToSearch, bool compileFunction)
        {
            try
            {
                // Add function defininition
                dynamoModel.CustomNodeManager.AddFunctionDefinition(FunctionId, this);

                // search
                if (addToSearch)
                {
                    //AddToSearch(dynamoModel.SearchModel);
                }

                var info = new CustomNodeInfo(FunctionId, Workspace.Name, Workspace.Category,
                                              Workspace.Description, Workspace.FileName);

                dynamoModel.CustomNodeManager.SetNodeInfo(info);
                Compile(dynamoModel.EngineController);
            }
            catch (Exception e)
            {
                dynamoModel.Logger.Log("Error saving:" + e.GetType());
                dynamoModel.Logger.Log(e);
                return false;
            }

            return true;
        }

        #endregion
    }
    
    /// <summary>
    /// A simple class to keep track of custom nodes.
    /// </summary>
    public class CustomNodeInfo
    {
        public CustomNodeInfo(Guid guid, string name, string category, string description, string path)
        {
            Guid = guid;
            Name = name;
            Category = category;
            Description = description;
            Path = path;
        }

        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
    }
}
