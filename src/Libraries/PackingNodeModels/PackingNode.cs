using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Newtonsoft.Json;
using PackingNodeModels.Properties;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using VMDataBridge;

namespace PackingNodeModels
{
    public abstract class PackingNode : NodeModel
    {
        protected string TypeDefinitionPortName = Resource.TypePortName;

        public event Action<Action> RequestScheduledTask;

        private string _cachedTypeDefinition;
        private TypeDefinition _typeDefinition;

        [JsonProperty("TypeDefinition")]
        public TypeDefinition TypeDefinition
        {
            get
            {
                return _typeDefinition;
            }
            protected set
            {
                if (value == null)
                    _cachedTypeDefinition = null;
                _typeDefinition = value;
                ClearErrorsAndWarnings();
                RequestScheduledTask?.Invoke(RefreshTypeDefinitionPorts);
            }
        }

        [JsonIgnore]
        public bool IsInValidState
        {
            get
            {
                return !IsInErrorState && State != ElementState.Warning && State != ElementState.PersistentWarning;
            }
        }

        public PackingNode()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData(TypeDefinitionPortName, Resource.TypePortTooltip)));

            RegisterAllPorts();
        }

        protected PackingNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(inPorts, outPorts) { }

        protected override void OnBuilt()
        {
            base.OnBuilt();
            DataBridge.Instance.RegisterCallback(GUID.ToString(), OnEvaluationComplete);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new List<AssociativeNode>() {
                AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes ?? new List<AssociativeNode>())))
            };
        }

        protected virtual void ValidateInputs(List<object> values) { }

        protected abstract void RefreshTypeDefinitionPorts();

        private void OnEvaluationComplete(object obj)
        {
            if (obj is ArrayList inputValues)
            {
                CheckTypeDefinition(inputValues);
                ValidateInputs(inputValues.Cast<object>().ToList());
            }
        }

        private void CheckTypeDefinition(ArrayList inputValues)
        {
            if (InputNodes.Count == 0 || !InputNodes.ContainsKey(0) || InputNodes[0] == null)
                TypeDefinition = null;
            else
            {
                if (inputValues[0] is string typeDef)
                {
                    if (typeDef != _cachedTypeDefinition)
                    {
                        try
                        {
                            TypeDefinition = TypeDefinitionParser.ParseType(typeDef);
                            _cachedTypeDefinition = typeDef;
                        }
                        catch (Sprache.ParseException e)
                        {
                            Warning(e.Message, true);
                        }
                    }
                }
                else
                    Warning(Resource.TypePortWarning, true);
            }
        }

        protected bool IsValidInputState(List<AssociativeNode> inputAstNodes)
        {
            return InPorts[0].Connectors.Any() && inputAstNodes.Count > 1 && IsInValidState && !inputAstNodes.Exists(node => node is NullNode);
        }

        public override void Dispose()
        {
            base.Dispose();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }
    }

    public class PackingNodeView : INodeViewCustomization<PackingNode>
    {
        private DynamoModel dynamoModel;
        private DynamoViewModel dynamoViewModel;
        private DispatcherSynchronizationContext syncContext;
        private PackingNode node;

        public void CustomizeView(PackingNode nodeModel, NodeView nodeView)
        {
            dynamoModel = nodeView.ViewModel.DynamoViewModel.Model;
            dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            syncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);
            node = nodeModel;
            node.RequestScheduledTask += OnRequestScheduledTask;
        }

        private void OnRequestScheduledTask(Action action)
        {
            var s = dynamoViewModel.Model.Scheduler;

            var t = new DelegateBasedAsyncTask(s, () =>
            {
            });

            t.ThenSend((_) =>
            {
                action();
            }, syncContext);

            s.ScheduleForExecution(t);
        }

        public void Dispose()
        {
        }
    }
}
