using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;

namespace Dynamo.TestLinterExtension.LinterRules
{
    public class GraphNeedsOutputNodesRule : GraphLinterRule
    {
        public override string Id => "a9ecd7fc-ac33-4a2d-b1c5-cf225e740d47";

        public override SeverityCodesEnum SeverityCode => SeverityCodesEnum.Error;

        public override string Description => "There are currently no nodes in the graph set as output";

        public override string CallToAction => "Set an appropriate node as 'IsOutput'";

        protected override Tuple<RuleEvaluationStatusEnum, HashSet<string>> EvalualteFunction(WorkspaceModel workspaceModel, NodeModel nodeModel = null)
        {
            var result = workspaceModel.Nodes.Any(x => x.IsSetAsOutput) ?
                RuleEvaluationStatusEnum.Passed :
                RuleEvaluationStatusEnum.Failed;

            return Tuple.Create(result, new HashSet<string>());
        }

        protected override List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>> InitFunction(WorkspaceModel workspaceModel)
        {
            return new List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>> { EvalualteFunction(workspaceModel) };
        }
    }
}
