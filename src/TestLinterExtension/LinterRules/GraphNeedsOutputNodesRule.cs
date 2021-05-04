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
        private List<NodeModel> outputNodes = new List<NodeModel>();

        public override string Id => "a9ecd7fc-ac33-4a2d-b1c5-cf225e740d47";

        public override SeverityCodesEnum SeverityCode => SeverityCodesEnum.Error;

        public override string Description => "There are currently no nodes in the graph set as output";

        public override string CallToAction => "Set an appropriate node as 'IsOutput'";


        public override List<string> EvaluationTriggerEvents => new List<string>
        {
            nameof(NodeModel.IsSetAsOutput)
        };

        protected override Tuple<RuleEvaluationStatusEnum, HashSet<string>> EvaluateFunction(WorkspaceModel workspaceModel, NodeModel nodeModel = null)
        {
            if (!(nodeModel is null))
            {
                if (nodeModel.IsSetAsOutput && !outputNodes.Contains(nodeModel))
                    outputNodes.Add(nodeModel);

                else if (outputNodes.Contains(nodeModel))
                    outputNodes.Remove(nodeModel);
            }

            var result = outputNodes.Any() ?
                RuleEvaluationStatusEnum.Passed :
                RuleEvaluationStatusEnum.Failed;

            return Tuple.Create(result, new HashSet<string>());
        }

        protected override List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>> InitFunction(WorkspaceModel workspaceModel)
        {
            foreach (var node in workspaceModel.Nodes)
            {
                if (node.IsSetAsOutput)
                    outputNodes.Add(node);
            }

            return new List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>> { EvaluateFunction(workspaceModel) };
        }
    }
}