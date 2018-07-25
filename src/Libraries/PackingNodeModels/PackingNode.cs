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
    /// <summary>
    /// Base class for nodes that utilize the dynamic-like functionality of taking in a TypeDefinition as an input and redefining InPorts or OutPorts according to this definition.
    /// Exposes two methods that should be overriden by extending classes :
    ///     -ValidateInputs - which will receive InPorts' data.
    ///     -RefreshTypeDefinitionPorts - which will be called after the TypeDefinition changes.
    /// </summary>
    public abstract class PackingNode : NodeModel
    {
        protected string TypeDefinitionPortName = Resource.TypePortName;

        /// <summary>
        /// Event made to communicate with the NodeViewCustomization and request a scheduled action.
        /// </summary>
        public event Action<Action> RequestScheduledTask;

        private string _cachedTypeDefinition;
        private TypeDefinition _typeDefinition;

        /// <summary>
        /// A TypeDefinition that will be used by extending classes to define OutPorts and InPorts
        /// Modifying the TypeDefinition implies clearing errors and warnings and calling RefreshTypeDefinitionPorts
        /// </summary>
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
                {
                    _cachedTypeDefinition = null;
                }

                _typeDefinition = value;
                ClearErrorsAndWarnings();
                RequestScheduledTask?.Invoke(RefreshTypeDefinitionPorts);
            }
        }

        /// <summary>
        /// Returns whether the node is in a valid state, that is if it does not have errors or warnings.
        /// </summary>
        [JsonIgnore]
        public bool IsInValidState
        {
            get
            {
                return !IsInErrorState && State != ElementState.Warning && State != ElementState.PersistentWarning;
            }
        }

        /// <summary>
        /// Base constructor used to define the constant Type InPort.
        /// </summary>
        public PackingNode()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData(TypeDefinitionPortName, Resource.TypePortTooltip)));

            RegisterAllPorts();
        }

        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        protected PackingNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(inPorts, outPorts) { }

        protected override void OnBuilt()
        {
            base.OnBuilt();
            DataBridge.Instance.RegisterCallback(GUID.ToString(), OnEvaluationComplete);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new List<AssociativeNode>()
            {
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

                if (TypeDefinition != null)
                {
                    ValidateInputs(inputValues.Cast<object>().ToList());
                }
            }
        }

        private void CheckTypeDefinition(ArrayList inputValues)
        {
            if (InputNodes.Count == 0 || !InputNodes.ContainsKey(0) || InputNodes[0] == null)
            {
                TypeDefinition = null;
            }
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
                            TypeDefinition = null;
                            Warning(e.Message, true);
                        }
                    }
                }
                else
                {
                    Warning(Resource.TypePortWarning, true);
                }
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

    /// <summary>
    /// ViewCustomization for PackingNodes whose purpose is being able to schedule actions from the PackingNode.
    /// </summary>
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
