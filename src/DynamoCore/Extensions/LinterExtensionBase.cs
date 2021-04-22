using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting;
using Dynamo.Linting.Rules;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Base class for all LinterExtensions
    /// </summary>
    public abstract class LinterExtensionBase : IExtension
    {
        private const string NODE_ADDED_PROPERTY = "NodeAdded";
        private const string NODE_REMOVED_PROPERTY = "NodeRemoved";

        #region Private/Internal properties
        private HashSet<LinterRule> linterRules = new HashSet<LinterRule>();
        private LinterManager linterManager;
        private WorkspaceModel currentWorkspace;

        internal ReadyParams ReadyParamsRef { get; set; }

        internal bool IsActive => this.linterManager?.IsExtensionActive(UniqueId) ?? false;
  
        internal LinterExtensionDescriptor ExtensionDescriptor { get; private set; }
        #endregion

        #region Public properties

        ///<inheritdoc/>
        public abstract string UniqueId { get; }

        ///<inheritdoc/>
        public abstract string Name { get; }

        /// <summary>
        /// Collection of the rules in this extension
        /// </summary>
        public HashSet<LinterRule> LinterRules => linterRules;

        #endregion

        #region Internal methods

        /// <summary>
        /// Add a LinterRule
        /// </summary>
        /// <param name="linterRule"></param>
        public void AddLinterRule(LinterRule linterRule)
        {
            linterRules.Add(linterRule);
        }

        /// <summary>
        /// Remove a LinterRule
        /// </summary>
        /// <param name="linterRule"></param>
        public void RemoveLinterRule(LinterRule linterRule)
        {
            linterRules.Remove(linterRule);
        }

        /// <summary>
        /// Activate this linter by subscribing the workspace and initializing its rules
        /// </summary>
        internal void Activate()
        {
            if (IsActive)
                return;

            ReadyParamsRef.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            OnCurrentWorkspaceChanged(ReadyParamsRef.CurrentWorkspaceModel);
            this.InitializeRules();
        }

        /// <summary>
        /// Deactivates this extension by unsubscribing all its events
        /// </summary>
        internal void Deactivate()
        {
            ReadyParamsRef.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            UnsubscribeGraphEvents(currentWorkspace);
        }

        #endregion

        internal void InitializeBase(LinterManager linterManager)
        {
            this.linterManager = linterManager;
        }

        private void EvaluateGraphRules(NodeModel modifiedNode, string changedProperty)
        {
            if (!IsActive)
                return;

            var graphRules = linterRules.
                Where(x => x is GraphLinterRule).
                Cast<GraphLinterRule>().
                ToList();

            if (graphRules is null)
                return;

            foreach (var rule in graphRules)
            {
                if (changedProperty != NODE_ADDED_PROPERTY && 
                    changedProperty != NODE_REMOVED_PROPERTY &&
                    !rule.EvaluationTriggerEvents.Contains(changedProperty))
                    continue;

                rule.Evaluate(currentWorkspace, modifiedNode);
            }
        }

        private void EvaluateNodeRules(NodeModel modifiedNode, string changedProperty)
        {
            if (!IsActive)
                return;

            var nodeRules = linterRules.
                Where(x => x is NodeLinterRule).
                Cast<NodeLinterRule>().
                ToList();

            if (nodeRules is null)
                return;

            foreach (var rule in nodeRules)
            {
                if (changedProperty != NODE_ADDED_PROPERTY && !rule.EvaluationTriggerEvents.Contains(changedProperty))
                    continue;

                rule.Evaluate(modifiedNode);
            }
        }

        private void InitializeRules()
        {
            if (!(ReadyParamsRef.CurrentWorkspaceModel is WorkspaceModel wm))
                return;

            foreach (var rule in LinterRules)
            {
                rule.InitializeBase(wm);
            }
        }

        #region Extension Lifecycle

        ///<inheritdoc/>
        public virtual void Ready(ReadyParams sp)
        {
            ReadyParamsRef = sp;
            if (IsActive)
                InitializeRules();
        }

        ///<inheritdoc/>
        public virtual void Startup(StartupParams sp)
        {
            ExtensionDescriptor = new LinterExtensionDescriptor(UniqueId, Name);
            OnLinterExtensionReady();
        }

        ///<inheritdoc/>
        public abstract void Shutdown();

        ///<inheritdoc/>
        public virtual void Dispose()
        {
            ReadyParamsRef.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            UnsubscribeGraphEvents(currentWorkspace);
        }

        #endregion

        #region Events

        /// <summary>
        /// Represents the method that will handle rule evaluated related events.
        /// </summary>
        internal delegate void LinterExtensionReadyHandler(LinterExtensionDescriptor descriptor);

        internal static event LinterExtensionReadyHandler LinterExtensionReady;

        private void OnLinterExtensionReady()
        {
            LinterExtensionReady?.Invoke(ExtensionDescriptor);
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel obj)
        {
            if (this.currentWorkspace != null)
                UnsubscribeGraphEvents(this.currentWorkspace);

            this.currentWorkspace = ReadyParamsRef.CurrentWorkspaceModel as WorkspaceModel;
            this.SubscribeNodeEvents();
            this.SubscribeGraphEvents();
        }

        private void SubscribeGraphEvents()
        {
            this.currentWorkspace.NodeRemoved += OnNodeRemoved;
            this.currentWorkspace.NodeAdded += OnNodeAdded;
        }


        private void SubscribeNodeEvents()
        {
            foreach (var node in currentWorkspace.Nodes)
            {
                node.PropertyChanged += OnNodePropertyChanged;
            }
        }

        private void UnsubscribeGraphEvents(WorkspaceModel workspaceModel)
        {
            workspaceModel.NodeRemoved -= OnNodeRemoved;
            workspaceModel.NodeAdded -= OnNodeAdded;
            workspaceModel.Nodes.
                ToList().
                ForEach(x => UnsubscribeNodeEvents(x));
        }

        private void UnsubscribeNodeEvents(NodeModel node)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
        }

        private void OnNodeAdded(NodeModel node)
        {
            EvaluateGraphRules(node, NODE_ADDED_PROPERTY);
            EvaluateNodeRules(node, NODE_ADDED_PROPERTY);
            node.PropertyChanged += OnNodePropertyChanged;
        }

        private void OnNodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            EvaluateNodeRules(sender as NodeModel, e.PropertyName);
            EvaluateGraphRules(sender as NodeModel, e.PropertyName);

        }

        private void OnNodeRemoved(Graph.Nodes.NodeModel node)
        {
            UnsubscribeNodeEvents(node);
            EvaluateGraphRules(node, NODE_REMOVED_PROPERTY);

            var nodeRules = LinterRules.
                Where(x => x is NodeLinterRule).
                Cast<NodeLinterRule>().
                ToList();

            if (nodeRules is null)
                return;

            foreach (var rule in nodeRules)
            {
                var result = new NodeRuleEvaluationResult(rule.Id, Linting.Interfaces.RuleEvaluationStatusEnum.Passed, node.GUID.ToString());
                rule.OnRuleEvaluated(result);
            }

        }
        #endregion
    }
}
