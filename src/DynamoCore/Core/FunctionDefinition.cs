using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;

namespace Dynamo
{
    public class FunctionDefinition
    {
        internal FunctionDefinition() : this(Guid.NewGuid()) { }

        internal FunctionDefinition(Guid id)
        {
            FunctionId = id;
            RequiresRecalc = true;
        }

        public Guid FunctionId { get; internal set; }
        public CustomNodeWorkspaceModel WorkspaceModel { get; internal set; }
        public List<Tuple<int, NodeModel>> OutPortMappings { get; internal set; }
        public List<Tuple<int, NodeModel>> InPortMappings { get; internal set; }

        public bool RequiresRecalc { get; internal set; }

        public IEnumerable<FunctionDefinition> Dependencies
        {
            get
            {
                return FindAllDependencies(new HashSet<FunctionDefinition>());
            }
        }

        public IEnumerable<FunctionDefinition> DirectDependencies
        {
            get
            {
                return FindDirectDependencies();
            }
        }

        private IEnumerable<FunctionDefinition> FindAllDependencies(HashSet<FunctionDefinition> dependencySet)
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

        private IEnumerable<FunctionDefinition> FindDirectDependencies()
        {
            return WorkspaceModel.Nodes
                            .OfType<Function>()
                            .Select(node => node.Definition)
                            .Where(def => def != this)
                            .Distinct();
        }

        public void CompileAndAddToEnvironment(ExecutionEnvironment env)
        {
            var compiledFunction = this.Compile();

            env.DefineSymbol( this.FunctionId.ToString(),compiledFunction);
        }

        public FScheme.Expression Compile()
        {
            IEnumerable<string> inputNames = null;
            IEnumerable<string> outputNames = null;

            // Get the internal nodes for the function
            WorkspaceModel functionWorkspace = this.WorkspaceModel;

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
            foreach (var ele in topMost)
            {
                ele.Item2.ValidateConnections();
            }

            //Find function entry point, and then compile
            var variables = functionWorkspace.Nodes.OfType<Symbol>().ToList();
            inputNames = variables.Select(x => x.InputSymbol);

            //Update existing function nodes which point to this function to match its changes
            dynSettings.Controller.DynamoModel.AllNodes
                .OfType<Function>()
                .Where(el => el.Definition != null && el.Definition.FunctionId == this.FunctionId)
                .ToList()
                .ForEach(node =>
                {
                    node.SetInputs(inputNames);
                    node.SetOutputs(outputNames);
                    node.RegisterAllPorts();
                });

            //Call OnSave for all saved elements
            functionWorkspace.Nodes.ToList().ForEach(x => x.onSave());

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

        public bool AddToSearch()
        {
            return
                dynSettings.Controller.SearchViewModel.Add(new CustomNodeInfo(  this.FunctionId, 
                                                                                this.WorkspaceModel.Name,
                                                                                this.WorkspaceModel.Category,
                                                                                this.WorkspaceModel.Description,
                                                                                this.WorkspaceModel.FileName ));
        }

        public void UpdateCustomNodeManager()
        {
            dynSettings.CustomNodeManager.SetNodeInfo(new CustomNodeInfo(   this.FunctionId,
                                                                            this.WorkspaceModel.Name,
                                                                            this.WorkspaceModel.Category,
                                                                            this.WorkspaceModel.Description,
                                                                            this.WorkspaceModel.FileName));
        }

        public bool SyncWithWorkspace(bool addToSearch, bool compileFunction)
        {

            // Get the internal nodes for the function
            var functionWorkspace = this.WorkspaceModel;

            try
            {
                // Add function defininition
                dynSettings.Controller.CustomNodeManager.AddFunctionDefinition(this.FunctionId, this);

                // search
                if (addToSearch)
                {
                    AddToSearch();
                }

                var info = new CustomNodeInfo(this.FunctionId, functionWorkspace.Name, functionWorkspace.Category,
                                              functionWorkspace.Description, this.WorkspaceModel.FileName);

                dynSettings.Controller.CustomNodeManager.SetNodeInfo(info);

                this.CompileAndAddToEnvironment(dynSettings.Controller.FSchemeEnvironment);
            }
            catch (Exception e)
            {
                DynamoLogger.Instance.Log("Error saving:" + e.GetType());
                DynamoLogger.Instance.Log(e);
                return false;
            }

            return true;
        }
    }
}
