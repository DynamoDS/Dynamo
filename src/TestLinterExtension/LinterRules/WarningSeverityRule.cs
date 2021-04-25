using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;

namespace Dynamo.TestLinterExtension.LinterRules
{
    public class WarningSeverityRule : GraphLinterRule
    {
        private List<NodeModel> specialNodeNodes = new List<NodeModel>();
        public override string Id => "123";

        public override SeverityCodesEnum SeverityCode => SeverityCodesEnum.Warning;

        public override string Description => "There needs to be at least one node named SpecialNode";

        public override string CallToAction => "Make sure that the graph has at least one node that has been renamed to SpecialNode";

        public override List<string> EvaluationTriggerEvents => new List<string>
        {
            nameof(NodeModel.Name)
        };

        protected override Tuple<RuleEvaluationStatusEnum, HashSet<string>> EvaluateFunction(WorkspaceModel workspaceModel, NodeModel modifiedNode = null)
        {
            if (!(modifiedNode is null))
            {
                if (modifiedNode.Name == "SpecialNode" && !specialNodeNodes.Contains(modifiedNode))
                    specialNodeNodes.Add(modifiedNode);

                else if (specialNodeNodes.Contains(modifiedNode))
                    specialNodeNodes.Remove(modifiedNode);
            }

            var result = specialNodeNodes.Any() ?
                RuleEvaluationStatusEnum.Passed :
                RuleEvaluationStatusEnum.Failed;

            return Tuple.Create(result, new HashSet<string>());
        }

        protected override List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>> InitFunction(WorkspaceModel workspaceModel)
        {
            foreach (var node in workspaceModel.Nodes)
            {
                if (node.Name == "SpecialNode")
                    specialNodeNodes.Add(node);
            }

            return new List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>> { EvaluateFunction(workspaceModel) };
        }
    }
}
