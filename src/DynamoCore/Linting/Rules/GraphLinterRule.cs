using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting.Interfaces;

namespace Dynamo.Linting.Rules
{
    /// <summary>
    /// Base class for creating Graph related linter rules
    /// </summary>
    public abstract class GraphLinterRule : LinterRule
    {
        /// <summary>
        /// Method to call when this rule needs to be evaluated.
        /// This will use <see cref="EvaluateFunction"/> to evaluate the rule.       
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <param name="changedEvent"></param>
        /// <param name="modifiedNode"></param>
        internal void Evaluate(WorkspaceModel workspaceModel, string changedEvent, NodeModel modifiedNode = null)
        {
            var pair = EvaluateFunction(workspaceModel, changedEvent, modifiedNode);
            if (pair is null) return;

            var result = new GraphRuleEvaluationResult(this.Id, pair.Item1, this.SeverityCode, pair.Item2);
            OnRuleEvaluated(result);
        }

        /// <summary>
        /// Function used to evaluate this rule
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <param name="changedEvent"></param>
        /// <param name="modifiedNode"></param>
        /// <returns></returns>
        protected abstract Tuple<RuleEvaluationStatusEnum, HashSet<string>> EvaluateFunction(WorkspaceModel workspaceModel, string changedEvent, NodeModel modifiedNode = null);

        /// <summary>
        /// The init function is used when the Linter extension implementing this Rule is initialized.
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <returns></returns>
        protected abstract List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>> InitFunction(WorkspaceModel workspaceModel);

        private protected sealed override List<IRuleEvaluationResult> InitializeRule(WorkspaceModel workspaceModel)
        {
            var pairs = this.InitFunction(workspaceModel);

            var results = new List<IRuleEvaluationResult>();
            foreach (var pair in pairs)
            {
                if (pair.Item1 == RuleEvaluationStatusEnum.Passed) continue;

                var result = new GraphRuleEvaluationResult(this.Id, pair.Item1, this.SeverityCode, pair.Item2);
                results.Add(result);
            }
            return results;
        }
    }
}