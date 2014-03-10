using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dynamo.DSEngine;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
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

        public string Name
        {
            get { return FunctionId.ToString(); }
        }

        public string FunctionName
        {
            get { return AstBuilder.StringConstants.FunctionPrefix + Name.Replace("-", string.Empty); }
        }

        public Guid FunctionId { get; internal set; }
        public CustomNodeWorkspaceModel WorkspaceModel { get; internal set; }
        public IEnumerable<string> Parameters { get; internal set; }
        public IEnumerable<string> ReturnKeys { get; internal set; }

        public string DisplayName
        {
            get { return WorkspaceModel.Name; }
        }

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

        #region FScheme Compilation
        
        public bool RequiresRecalc { get; internal set; }

        public void CompileAndAddToEnvironment(ExecutionEnvironment env)
        {
            var compiledFunction = _Compile();

            env.DefineSymbol( FunctionId.ToString(),compiledFunction);
        }

        private FScheme.Expression _Compile()
        {
            IEnumerable<string> inputNames = null;
            IEnumerable<string> outputNames = null;

            // Get the internal nodes for the function
            WorkspaceModel functionWorkspace = WorkspaceModel;

            #region Find outputs

            // Find output elements for the node
            List<Output> outputs = functionWorkspace.Nodes.OfType<Output>().ToList();

            var topMost = new List<Tuple<int, NodeModel>>();

            // if we found output nodes, add select their inputs
            // these will serve as the function output
            if (outputs.Any())
            {
                topMost.AddRange(
                    outputs.Where(x => x.HasInput(0)).Select(x => x.Inputs[0]));

                outputNames = outputs.Select(x => x.Symbol);
            }
            else
            {
                // if there are no explicitly defined output nodes
                // get the top most nodes and set THEM as the output
                IEnumerable<NodeModel> topMostNodes = functionWorkspace.GetTopMostNodes();

                NodeModel infinite = null;

                var outNames = new List<string>();

                foreach (NodeModel topNode in topMostNodes)
                {
                    if (topNode is Function && (topNode as Function).Definition == this)
                    {
                        infinite = topNode;
                        continue;
                    }

                    foreach (int output in Enumerable.Range(0, topNode.OutPortData.Count))
                    {
                        if (!topNode.HasOutput(output))
                        {
                            topMost.Add(Tuple.Create(output, topNode));
                            outNames.Add(topNode.OutPortData[output].NickName);
                        }
                    }
                }

                if (infinite != null && outNames.Count == 0)
                {
                    topMost.Add(Tuple.Create(0, infinite));
                    outNames.Add("∞");
                }

                outputNames = outNames;
            }

            #endregion

            // color the node to define its connectivity
            //foreach (var ele in topMost)
            //{
            //    ele.Item2.ValidateConnections();
            //}

            //Find function entry point, and then compile
            var variables = functionWorkspace.Nodes.OfType<Symbol>().ToList();
            inputNames = variables.Select(x => x.InputSymbol);

            //Update existing function nodes which point to this function to match its changes
            var customNodeInstances = dynSettings.Controller.DynamoModel.AllNodes
                .OfType<Function>()
                .Where(el => el.Definition != null && el.Definition.FunctionId == FunctionId);

            foreach (var node in customNodeInstances)    
            {
                node.DisableReporting();

                node.SetInputs(inputNames);
                node.SetOutputs(outputNames);
                node.RegisterAllPorts();

                node.EnableReporting();
            }

            //Call OnSave for all saved elements
            foreach (NodeModel node in functionWorkspace.Nodes)
                node.OnSave();

            INode top;
            var buildDict = new Dictionary<NodeModel, Dictionary<int, INode>>();

            if (topMost.Count > 1)
            {
                InputNode node = new ExternalFunctionNode(FScheme.Value.NewList);

                int i = 0;
                foreach (var topNode in topMost)
                {
                    string inputName = i.ToString(CultureInfo.InvariantCulture);
                    node.AddInput(inputName);
                    node.ConnectInput(inputName, new BeginNode());
                    try
                    {
                        var exp = topNode.Item2.Build(buildDict, topNode.Item1);
                        node.ConnectInput(inputName, exp);
                    }
                    catch
                    {

                    }

                    i++;
                }

                top = node;
            }
            else if (topMost.Count == 1)
            {
                top = topMost[0].Item2.Build(buildDict, topMost[0].Item1);
            }
            else
            {
                // if the custom node is empty, it will initially be an empty begin
                top = new BeginNode();
            }

            // if the node has any outputs, we create a BeginNode in order to evaluate all of them
            // sequentially (begin evaluates a list of expressions)
            if (outputs.Any())
            {
                var beginNode = new BeginNode();
                List<NodeModel> hangingNodes = functionWorkspace.GetHangingNodes().ToList();

                foreach (var tNode in hangingNodes.Select((x, index) => new { Index = index, Node = x }))
                {
                    beginNode.AddInput(tNode.Index.ToString(CultureInfo.InvariantCulture));
                    beginNode.ConnectInput(
                        tNode.Index.ToString(CultureInfo.InvariantCulture),
                        tNode.Node.Build(buildDict, 0));
                }

                beginNode.AddInput(hangingNodes.Count.ToString(CultureInfo.InvariantCulture));
                beginNode.ConnectInput(hangingNodes.Count.ToString(CultureInfo.InvariantCulture), top);

                top = beginNode;
            }

            // make the anonymous function
            FScheme.Expression expression = Utils.MakeAnon(
                variables.Select(x => x.GUID.ToString()),
                top.Compile());

            return expression;

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
            if (IsBeingLoaded)
                return;

            #region Outputs and Inputs and UI updating

            #region Find outputs

            // Find output elements for the node
            List<Output> outputs = WorkspaceModel.Nodes.OfType<Output>().ToList();
 
            var topMost = new List<Tuple<int, NodeModel>>();
            
            // if we found output nodes, add select their inputs
            // these will serve as the function output
            if (outputs.Any())
            {
                topMost.AddRange(
                    outputs.Where(x => x.HasInput(0)).Select(x => new Tuple<int, NodeModel>(0, x)));
                ReturnKeys =
                    outputs.Select(
                        (x, i) => !string.IsNullOrEmpty(x.Symbol) ? x.Symbol : i.ToString());
            }
            else
            {
                // if there are no explicitly defined output nodes
                // get the top most nodes and set THEM as the output
                IEnumerable<NodeModel> topMostNodes = WorkspaceModel.GetTopMostNodes();

                var outNames = new List<string>();

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
                    outNames.Add(
                        rtnAndIndex.rtn.name != null
                            ? rtnAndIndex.rtn.name + rtnAndIndex.idx
                            : rtnAndIndex.idx.ToString());
                }

                ReturnKeys = outNames;
            }

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
                DynamoLogger.Instance.Log("Error saving:" + e.GetType());
                DynamoLogger.Instance.Log(e);
                return false;
            }

            return true;
        }

        #endregion
    }
}
