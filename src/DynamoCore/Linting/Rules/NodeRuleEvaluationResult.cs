using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Linting.Interfaces;

namespace Dynamo.Linting.Rules
{
    /// <summary>
    /// Rule evaluation result for node linter rules
    /// </summary>
    internal class NodeRuleEvaluationResult : IEquatable<NodeRuleEvaluationResult>, IRuleEvaluationResult
    {
        /// <summary>
        /// Id of the rule this evaluation result belongs to
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        /// Evaluation status
        /// </summary>
        public RuleEvaluationStatusEnum Status { get; }

        public SeverityCodesEnum SeverityCode { get; }

        /// <summary>
        /// Unique id of the node that has been evaluated
        /// </summary>
        internal string NodeId { get; }

        internal NodeRuleEvaluationResult(string ruleId, RuleEvaluationStatusEnum status, SeverityCodesEnum severityCode, string nodeId)
        {
            RuleId = ruleId;
            Status = status;
            SeverityCode = severityCode;
            NodeId = nodeId;
        }

        public bool Equals(NodeRuleEvaluationResult other)
        {
            if (other is null) return false;

            return this.NodeId == other.NodeId && this.RuleId == other.RuleId;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;

            return Equals(obj as NodeRuleEvaluationResult);
        }
    }
}
