using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;

namespace Dynamo.TestLinterExtension.LinterRules
{
    public class NodesCantBeNamedFooRule : NodeLinterRule
    {
        public override string Id => "01963dcb-4cbe-41f8-adb0-cc101c5dc2e5";

        public override SeverityCodesEnum SeverityCode => SeverityCodesEnum.Error;

        public override string Description => "Nodes are not allowed to be named 'Foo'";

        public override string CallToAction => "Rename the nodes listed above to something else than 'Foo'";

        public override List<string> EvaluationTriggerEvents { get => new List<string> { nameof(NodeModel.Name) }; }

        protected override RuleEvaluationStatusEnum EvaluateFunction(NodeModel nodeModel)
        {
            var status = nodeModel.Name != "Foo" ?
                RuleEvaluationStatusEnum.Passed :
                RuleEvaluationStatusEnum.Failed;

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
