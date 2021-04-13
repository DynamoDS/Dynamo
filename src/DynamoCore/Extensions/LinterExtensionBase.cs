using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting;
using Dynamo.Linting.Rules;

namespace Dynamo.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class LinterExtensionBase : IExtension
    {
        #region Private/Internal properties
        private HashSet<LinterRule> linterRules = new HashSet<LinterRule>();
        private LinterManager linterManager;

        internal bool IsActive => this.linterManager?.IsExtensionActive(UniqueId) ?? false;
  
        internal LinterExtensionDescriptor ExtensionDescriptor { get; private set; }
        #endregion

        #region Public properties

        public ReadyParams ReadyParamsRef { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract string UniqueId { get; }

        /// <summary>
        /// 
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        public HashSet<LinterRule> LinterRules => linterRules;

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linterRule"></param>
        public void AddLinterRule(LinterRule linterRule)
        {
            linterRules.Add(linterRule);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linterRule"></param>
        public void RemoveLinterRule(LinterRule linterRule)
        {
            if (linterRules.Contains(linterRule))
                linterRules.Remove(linterRule);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Activate()
        {
            this.linterManager.ActiveLinter = this.ExtensionDescriptor;
            this.InitializeRules();
        }

        #endregion

        internal void InitializeBase(LinterManager linterManager)
        {
            this.linterManager = linterManager;
            this.linterManager.RequestNodeRuleEvaluation += OnRequestNodeRuleEvaluation;
            this.linterManager.RequestGraphRuleEvaluation += OnRequestGraphRuleEvaluation;
        }

        private void OnRequestGraphRuleEvaluation(NodeModel modifiedNode)
        {
            if (!IsActive ||
                !(ReadyParamsRef.CurrentWorkspaceModel is WorkspaceModel ws))
                return;

            linterRules.
                Where(x => x is GraphLinterRule).
                Cast<GraphLinterRule>().
                ToList().
                ForEach(x => x.Evaluate(ws, modifiedNode));
        }

        private void OnRequestNodeRuleEvaluation(NodeModel modifiedNode)
        {
            if (!IsActive)
                return;

            linterRules.
                Where(x => x is NodeLinterRule).
                Cast<NodeLinterRule>().
                ToList().
                ForEach(x => x.Evaluate(modifiedNode));
        }

        #region Extension Lifecycle
       
        public virtual void Ready(ReadyParams sp)
        {
            ReadyParamsRef = sp;
            if (IsActive)
                InitializeRules();
        }


        public virtual void Startup(StartupParams sp)
        {
            ExtensionDescriptor = new LinterExtensionDescriptor(UniqueId, Name);
            OnLinterExtensionReady();
        }

        public abstract void Shutdown();

        public abstract void Dispose();

        #endregion

        #region Events

        /// <summary>
        /// Represents the method that will handle rule evaluated related events.
        /// </summary>
        internal delegate void LinterExtensionReadyHandler(LinterExtensionDescriptor descriptor);

        internal static event LinterExtensionReadyHandler LinterExtensionReady;

        public void OnLinterExtensionReady()
        {
            LinterExtensionReady?.Invoke(ExtensionDescriptor);
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

        internal void OnGraphModified(WorkspaceModel ws)
        {
            if (!IsActive)
                return;

            LinterRules.Where(x => x is GraphLinterRule)
                .Select(x => x as GraphLinterRule)
                .ToList()
                .ForEach(rule => rule.Evaluate(ws));
        }

        internal void OnNodeModified(NodeModel node)
        {
            if (!IsActive)
                return;

            LinterRules.Where(x => x is NodeLinterRule)
                .Select(x => x as NodeLinterRule)
                .ToList()
                .ForEach(rule => rule.Evaluate(node));
        }

        #endregion
    }
}
