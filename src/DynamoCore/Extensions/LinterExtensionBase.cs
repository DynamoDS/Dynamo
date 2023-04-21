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
using Dynamo.Models;

namespace Dynamo.Extensions
{
    /// <summary>
    /// Base class for all LinterExtensions
    /// </summary>
    public abstract class LinterExtensionBase : IExtension
    {
        private const string NODE_ADDED_PROPERTY = "NodeAdded";
        private const string NODE_REMOVED_PROPERTY = "NodeRemoved";
        private const string NODE_MODIFIED_PROPERTY = "Modified";

        #region Private/Internal properties
        private HashSet<LinterRule> linterRules = new HashSet<LinterRule>();
        private LinterManager linterManager;
        private HomeWorkspaceModel currentWorkspace;

        public ReadyParams ReadyParamsRef { get; private set; }

        /// <summary>
        /// Is this linter currently active for the active workspace.
        /// </summary>
        public bool IsActive => this.linterManager?.IsExtensionActive(UniqueId) ?? false;

        internal bool SetupComplete { get; set; } = false;
  
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
        internal void Activate(bool linkToWorkspace = true)
        {
            //Nothing left to do if initial setup is complete and we are already linked to application events
            if (SetupComplete)
            {
                return;
            }

            UnlinkFromCurrentWorkspace();

            ReadyParamsRef.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;
            ReadyParamsRef.CurrentWorkspaceClearingStarted += OnCurrentWorkspaceClosing;
            ReadyParamsRef.CurrentWorkspaceRemoveStarted += OnCurrentWorkspaceClosing;

            if (linkToWorkspace)
            {
                LinkToWorkspace(ReadyParamsRef.CurrentWorkspaceModel as HomeWorkspaceModel);
            }            

            SetupComplete = true;

            OnActivated();
        }

        /// <summary>
        /// Deactivates this extension by unsubscribing all its events
        /// </summary>
        internal void Deactivate()
        {
            //Nothing to deactivate if we have not setup everything properly
            //yet or we already deactivated this linter extension
            if (!SetupComplete)
            {
                return;
            }

            ReadyParamsRef.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            ReadyParamsRef.CurrentWorkspaceClearingStarted -= OnCurrentWorkspaceClosing;
            ReadyParamsRef.CurrentWorkspaceRemoveStarted -= OnCurrentWorkspaceClosing;

            UnlinkFromCurrentWorkspace();

            SetupComplete = false;

            OnDeactivated();
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

                rule.Evaluate(currentWorkspace, changedProperty, modifiedNode);
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

                rule.Evaluate(modifiedNode, changedProperty);
            }
        }

        private void InitializeRules()
        {
            if (currentWorkspace is null)
            {
                return;
            }

            foreach (var rule in LinterRules)
            {
                rule.InitializeBase(currentWorkspace);
            }
        }

        #region Extension Lifecycle

        ///<inheritdoc/>
        public virtual void Ready(ReadyParams sp)
        {
            currentWorkspace = null;
            ReadyParamsRef = sp;
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
            //TODO - move to ShutDown once the coresponding PR is merged
            //ReadyParamsRef.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;
            //UnsubscribeGraphEvents(currentWorkspace);
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

        private void LinkToWorkspace(HomeWorkspaceModel workspace)
        {
            if (workspace != null)
            {
                currentWorkspace = workspace;
                SubscribeNodeEvents();
                SubscribeGraphEvents();
                InitializeRules();

                OnLink();
            }
        }

        private void UnlinkFromCurrentWorkspace()
        {
            if (currentWorkspace != null)
            {
                UnsubscribeGraphEvents(currentWorkspace);
                DynamoModel.OnRequestDispatcherInvoke(() => { this.linterManager.RuleEvaluationResults.Clear(); });
                currentWorkspace = null;

                OnUnlink();
            }
        }

        private void OnCurrentWorkspaceClosing(IWorkspaceModel obj)
        {
            HomeWorkspaceModel workspaceAboutToClose = obj as HomeWorkspaceModel;
            //It can be a CustomNodeWorkspaceModel which we are not interested in processing
            if (workspaceAboutToClose is null) 
            {
                return;
            }

            //we only care about the workspace we are linked to
            if (workspaceAboutToClose != currentWorkspace)
            {
                return;
            }

            foreach (var rule in linterRules)
            {
                rule?.CleanupRule(workspaceAboutToClose);
            }

            UnlinkFromCurrentWorkspace();
        }

        private void OnCurrentWorkspaceChanged(IWorkspaceModel obj)
        {
            HomeWorkspaceModel incomingWorkspace = obj as HomeWorkspaceModel;

            //If incoming workspace is not of expected type we will unlink from current workspace.
            //This will happen when you edit a custom node for example.
            //Other than that we do not expect to have a change event for the same workspace,
            //we assume something went bad in this case and we also unlink.
            //If the incoming workspace is default home screen workspace we also unlink.
            if (incomingWorkspace is null || 
                currentWorkspace == incomingWorkspace || 
                String.IsNullOrEmpty(incomingWorkspace.FileName))
            {
                UnlinkFromCurrentWorkspace();
                return;
            }

            LinkToWorkspace(incomingWorkspace);
        }

        private void SubscribeGraphEvents()
        {
            currentWorkspace.NodeRemoved += OnNodeRemoved;
            currentWorkspace.NodeAdded += OnNodeAdded;
        }


        private void SubscribeNodeEvents()
        {
            foreach (var node in currentWorkspace.Nodes)
            {
                node.PropertyChanged += OnNodePropertyChanged;
                node.Modified += OnNodeModified;
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
            node.Modified -= OnNodeModified;
        }

        private void OnNodeAdded(NodeModel node)
        {
            EvaluateGraphRules(node, NODE_ADDED_PROPERTY);
            EvaluateNodeRules(node, NODE_ADDED_PROPERTY);
            node.PropertyChanged += OnNodePropertyChanged;
            node.Modified += OnNodeModified;
        }

        private void OnNodeModified(NodeModel node)
        {
            EvaluateGraphRules(node, NODE_MODIFIED_PROPERTY);
            EvaluateNodeRules(node, NODE_MODIFIED_PROPERTY);
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
                var result = new NodeRuleEvaluationResult(rule.Id, Linting.Interfaces.RuleEvaluationStatusEnum.Passed, rule.SeverityCode, node.GUID.ToString());
                rule.OnRuleEvaluated(result);
            }

        }

        internal event Action OnLinterExtensionActivated;

        private void OnActivated()
        {
            OnLinterExtensionActivated?.Invoke();
        }

        internal event Action OnLinterExtensionDeactivated;

        private void OnDeactivated()
        {
            OnLinterExtensionDeactivated?.Invoke();
        }

        internal event Action OnLinterUnlinkFromWorkspace;

        private void OnUnlink()
        {
            OnLinterUnlinkFromWorkspace?.Invoke();
        }

        internal event Action OnLinterLinkToWorkspace;

        private void OnLink()
        {
            OnLinterLinkToWorkspace?.Invoke();
        }

        #endregion
    }
}
