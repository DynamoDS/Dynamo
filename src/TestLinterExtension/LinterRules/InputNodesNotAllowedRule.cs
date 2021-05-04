using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;

namespace Dynamo.TestLinterExtension.LinterRules
{
    public class InputNodesNotAllowedRule : NodeLinterRule
    {
        public override string Id => "ebdabd96-4b7a-46bf-930f-6feca33c53b2";
        public override SeverityCodesEnum SeverityCode => SeverityCodesEnum.Error;
        public override string Description => "Nodes are not allowed to be set as input in this graph";
        public override string CallToAction => "Set the above nodes to not be inputs";

        public override List<string> EvaluationTriggerEvents => new List<string>
        {
            nameof(NodeModel.IsSetAsInput)
        };

        protected override RuleEvaluationStatusEnum EvaluateFunction(NodeModel nodeModel)
        {
            var status = nodeModel.IsSetAsInput ?
                RuleEvaluationStatusEnum.Failed :
                RuleEvaluationStatusEnum.Passed;

            return status;
        }

        protected override List<Tuple<RuleEvaluationStatusEnum, string>> InitFunction(WorkspaceModel workspaceModel)
        {
            var results = new List<Tuple<RuleEvaluationStatusEnum, string>>();
            foreach (var node in workspaceModel.Nodes)
            {
                var evaluationStatus = EvaluateFunction(node);

                if (evaluationStatus == RuleEvaluationStatusEnum.Passed)
                    continue;

                var valueTuple = Tuple.Create(evaluationStatus, node.GUID.ToString());
                results.Add(valueTuple);
            }
            return results;
        }
    }
}