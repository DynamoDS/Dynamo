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
    /// Base class for creating Node related linter rules
    /// </summary>
    public abstract class NodeLinterRule : LinterRule
    {

        /// <summary>
        /// Method to call when this rule needs to be evaluated.
        /// This will use <see cref="EvaluateFunction(NodeModel)"/> to evaluate the rule.
        /// </summary>
        /// <param name="nodeModel"></param>
        /// <param name="changedEvent"></param>
        internal void Evaluate(NodeModel nodeModel, string changedEvent)
        {
            var status = EvaluateFunction(nodeModel, changedEvent);
            var result = new NodeRuleEvaluationResult(this.Id, status, this.SeverityCode, nodeModel.GUID.ToString());
            OnRuleEvaluated(result);
        }

        /// <summary>
        /// Function used to evaluate this rule
        /// </summary>
        /// <param name="nodeModel">Node to evaluate</param>
        /// <param name="changedEvent"></param>
        /// <returns></returns>
        protected abstract RuleEvaluationStatusEnum EvaluateFunction(NodeModel nodeModel, string changedEvent);

        /// <summary>
        /// The init function is used when the Linter extension implementing this Rule is initialized.
        /// </summary>
        /// <param name="workspaceModel"></param>
        /// <returns></returns>
        protected abstract List<Tuple<RuleEvaluationStatusEnum, string>> InitFunction(WorkspaceModel workspaceModel);

        private protected sealed override List<IRuleEvaluationResult> InitializeRule(WorkspaceModel workspaceModel)
        {
            var pairs = InitFunction(workspaceModel);

            var results = new List<IRuleEvaluationResult>();
            foreach (var pair in pairs)
            {
                if (pair.Item1 == RuleEvaluationStatusEnum.Passed) continue;

                var result = new NodeRuleEvaluationResult(this.Id, pair.Item1, this.SeverityCode, pair.Item2);
                results.Add(result);
            }
            return results;
        }

    }
}
