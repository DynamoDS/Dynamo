using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting.Interfaces;

namespace Dynamo.Linting.Rules
{
    /// <summary>
    /// Base class for all linting rules
    /// </summary>
    public abstract class LinterRule
    {
        /// <summary>
        /// Unique id of this rule
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// Severity code of this rule.
        /// This code will define how the rule is displayed in the UI
        /// </summary>
        public abstract SeverityCodesEnum SeverityCode { get; }

        /// <summary>
        /// Description of the rule
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Description of how to solve issues related to this rule
        /// </summary>
        public abstract string CallToAction { get; }
       
        /// <summary>
        /// Uses the init function to evaluate this rule and return a collection of evaluation results
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <returns></returns>
        protected abstract List<IRuleEvaluationResult> InitializeRule(WorkspaceModel workspaceModel);

        /// <summary>
        /// Initializes this rule using the <see cref="InitFunction(WorkspaceModel)"/>
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <returns></returns>
        internal void InitializeBase(WorkspaceModel workspaceModel)
        {
            var initResults = InitializeRule(workspaceModel);

            foreach (var result in initResults)
            {
                OnRuleEvaluated(result);
            }
        }

        /// <summary>
        /// Represents the method that will handle rule evaluated related events.
        /// </summary>
        /// <param name="result"></param>
        internal delegate void RuleEvaluatedHandler(IRuleEvaluationResult result);

        internal static event RuleEvaluatedHandler RuleEvaluated;

        public void OnRuleEvaluated(IRuleEvaluationResult result)
        {
            RuleEvaluated?.Invoke(result);
        }

        public virtual void Dispose() { }
    }
}
