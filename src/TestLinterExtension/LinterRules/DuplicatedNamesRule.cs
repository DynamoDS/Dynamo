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
    public class DuplicatedNamesRule : GraphLinterRule
    {
        private Dictionary<string, HashSet<string>> duplicatedNodes = new Dictionary<string, HashSet<string>>(); 
        public override string Id => "1234";

        public override SeverityCodesEnum SeverityCode => SeverityCodesEnum.Error;

        public override string Description => "Graph contains nodes with duplicated names";

        public override string CallToAction => "Make sure all nodes in the graph has unique names";

        public override List<string> EvaluationTriggerEvents => new List<string>
        {
            nameof(NodeModel.Name)
        };


        protected override Tuple<RuleEvaluationStatusEnum, HashSet<string>> EvaluateFunction(WorkspaceModel workspaceModel, NodeModel modifiedNode = null)
        {

            // First we check if the id was already used on another node
            // and it's being renamed
            var previousName = duplicatedNodes
                .Where(kpv => kpv.Value.Contains(modifiedNode.GUID.ToString()))
                .FirstOrDefault()
                .Key;

            if (!string.IsNullOrEmpty(previousName) && previousName != modifiedNode.Name)
            {
                var previousIds = duplicatedNodes[previousName];
                previousIds.Remove(modifiedNode.GUID.ToString());

                //if (previousIds.Count > 1)
                //    return Tuple.Create(RuleEvaluationStatusEnum.Failed, duplicatedNodes.SelectMany(x=>x.Value).ToHashSet());
            }

            // Now we check the name is not being used yet
            // and we add it to the dictionary
            if (!duplicatedNodes.TryGetValue(modifiedNode.Name, out HashSet<string> ids))
            {
                ids = new HashSet<string> { modifiedNode.GUID.ToString() };
                duplicatedNodes[modifiedNode.Name] = ids;
            }

            ids.Add(modifiedNode.GUID.ToString());

            var issues = duplicatedNodes
                .Where(x => x.Value.Count > 1)
                .SelectMany(x => x.Value)
                .ToHashSet();

            var result = issues.Any() ?
                RuleEvaluationStatusEnum.Failed :
                RuleEvaluationStatusEnum.Passed;

            return Tuple.Create(result, issues);
        }

        protected override List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>> InitFunction(WorkspaceModel workspaceModel)
        {
            var results = new List<Tuple<RuleEvaluationStatusEnum, HashSet<string>>>();
            foreach (var node in workspaceModel.Nodes)
            {
                results.Add(EvaluateFunction(workspaceModel, node));
            }
            return results;
        }
    }
}
