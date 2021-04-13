using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Core;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;
using Dynamo.Models;

namespace Dynamo.Linting
{
    public class LinterManager : NotificationObject
    {
        #region Private fields
        private readonly DynamoModel dynamoModel;
        private LinterExtensionDescriptor activeLinter;
        #endregion

        #region Public properties

        public WorkspaceModel CurrentWorkspace { get; private set; }

        /// <summary>
        /// Available linters
        /// </summary>
        public HashSet<LinterExtensionDescriptor> AvailableLinters { get; internal set; }

        internal bool IsExtensionActive(string uniqueId)
        {
            return ActiveLinter?.Id == uniqueId;
        }

        /// <summary>
        /// Results from evaluated rules
        /// </summary>
        public ObservableCollection<IRuleEvaluationResult> RuleEvaluationResults { get; set; }

        public LinterExtensionDescriptor ActiveLinter
        {
            get => activeLinter;
            set
            {
                if (activeLinter == value)
                    return;

                activeLinter = value;
            }
        }

        #endregion

        public LinterManager(DynamoModel dynamoModel)
        {
            this.dynamoModel = dynamoModel;
            CurrentWorkspace = this.dynamoModel.CurrentWorkspace;
            AvailableLinters = new HashSet<LinterExtensionDescriptor>();
            RuleEvaluationResults = new ObservableCollection<IRuleEvaluationResult>();
            this.dynamoModel.PropertyChanged += OnCurrentWorkspaceChanged;

            SubscribeLinterEvents();
        }


        internal event RequestNodeRuleEvaluationHandler RequestNodeRuleEvaluation;
        internal event RequestGraphRuleEvaluationHandler RequestGraphRuleEvaluation;

        internal void OnRequestNodeRuleEvaluation(NodeModel modifiedNode)
        {
            RequestNodeRuleEvaluation?.Invoke(modifiedNode);
        }

        internal void OnRequestGraphRuleEvaluation(NodeModel modifiedNode)
        {
            RequestGraphRuleEvaluation?.Invoke(modifiedNode);
        }

        /// <summary>
        /// Represents the method that will handle node rule requests related events.
        /// </summary>
        /// <param name="modifiedNode"></param>
        internal delegate void RequestNodeRuleEvaluationHandler(NodeModel modifiedNode);

        /// <summary>
        /// Represents the method that will handle node rule requests related events.
        /// </summary>
        /// <param name="modifiedNode"></param>
        internal delegate void RequestGraphRuleEvaluationHandler(NodeModel modifiedNode);


        #region Private methods

        private void SubscribeLinterEvents()
        {
            LinterExtensionBase.LinterExtensionReady += OnLinterExtensionReady;
            LinterRule.RuleEvaluated += OnRuleEvaluated;

        }

        private void OnLinterExtensionReady(LinterExtensionDescriptor extensionDescriptor)
        {
            if (AvailableLinters.Contains(extensionDescriptor))
                return;

            AvailableLinters.Add(extensionDescriptor);
        }

        private void OnCurrentWorkspaceChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DynamoModel.CurrentWorkspace))
            {
                if (this.CurrentWorkspace != null)
                    UnsubscribeGraphEvents(this.CurrentWorkspace);

                this.CurrentWorkspace = dynamoModel.CurrentWorkspace;
                this.SubscribeNodeEvents();
                this.SubscribeGraphEvents();
                
            }
        }

        private void SubscribeGraphEvents()
        {
            this.CurrentWorkspace.NodeRemoved += OnNodeRemoved;
            this.CurrentWorkspace.NodeAdded += OnNodeAdded;
        }


        private void SubscribeNodeEvents()
        {
            foreach (var node in CurrentWorkspace.Nodes)
            {
                node.PropertyChanged += OnNodePropertyChanged;
            }
        }

        private void UnsubscribeGraphEvents(WorkspaceModel workspaceModel)
        {
            workspaceModel.NodeRemoved -= OnNodeRemoved;
            workspaceModel.NodeAdded -= OnNodeAdded;
        }
        private void UnsubscribeNodeEvents(NodeModel node)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
        }

        private void OnNodeAdded(NodeModel node)
        {
            OnRequestGraphRuleEvaluation(node);
            node.PropertyChanged += OnNodePropertyChanged;
        }

        private void OnNodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NodeModel.Name):
                case nameof(NodeModel.State):
                    OnRequestNodeRuleEvaluation(sender as NodeModel);
                    return;

                case nameof(NodeModel.IsSetAsInput):
                case nameof(NodeModel.IsSetAsOutput):
                    OnRequestNodeRuleEvaluation(sender as NodeModel);
                    OnRequestGraphRuleEvaluation(sender as NodeModel);
                    return;

                default:
                    return;
            }
        }

        private void OnNodeRemoved(Graph.Nodes.NodeModel obj)
        {
            var nodeRuleEvaluations = RuleEvaluationResults.
                Where(x => x is NodeRuleEvaluationResult).
                Cast<NodeRuleEvaluationResult>().
                ToList().
                Where(x => x.NodeId == obj.GUID.ToString()).
                ToList();

            foreach (var item in nodeRuleEvaluations)
            {
                RuleEvaluationResults.Remove(item);
            }

            UnsubscribeNodeEvents(obj);
        }

        private void OnRuleEvaluated(IRuleEvaluationResult result)
        {
            if (result is null)
                return;

            if (result.Status == RuleEvaluationStatusEnum.Passed)
            {
                if (!RuleEvaluationResults.Contains(result))
                    return;
                RuleEvaluationResults.Remove(result);
            }

            else
            {
                if (RuleEvaluationResults.Contains(result))
                    return;
                RuleEvaluationResults.Add(result);
            }
        }


        #endregion
    }
}
