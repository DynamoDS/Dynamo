using System;
using System.Collections.Generic;
using Dynamo.Linting.Interfaces;

namespace Dynamo.Linting.Rules
{
    /// <summary>
    /// Rule evaluation result for graph linter rules
    /// </summary>
    internal class GraphRuleEvaluationResult : IEquatable<GraphRuleEvaluationResult>, IRuleEvaluationResult
    {
        /// <summary>
        /// Id of the rule this evaluation result belongs to
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        /// Evaluation status
        /// </summary>
        public RuleEvaluationStatusEnum Status { get; }

        /// <summary>
        /// List of nodes involved in the evaluation of this rule
        /// </summary>
        internal HashSet<string> NodeIds { get; }

        internal GraphRuleEvaluationResult(string ruleId, RuleEvaluationStatusEnum status, HashSet<string> nodeIds)
        {
            RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
            Status = status;
            NodeIds = nodeIds;
        }

        public bool Equals(GraphRuleEvaluationResult other)
        {
            if (other is null)
                return false;

            return this.RuleId == other.RuleId && 
                this.NodeIds.SetEquals(other.NodeIds);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            return Equals(obj as GraphRuleEvaluationResult);
        }
    }
}
