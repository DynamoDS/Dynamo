using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension.Controls
{
    internal class NodeRuleIssue : IRuleIssue
    {
        public string Id { get; }
        public LinterRule Rule { get; }
        public ObservableCollection<NodeModel> AffectedNodes { get; private set; }

        public NodeRuleIssue(string id, NodeLinterRule rule)
        {
            Id = id;
            Rule = rule;
            AffectedNodes = new ObservableCollection<NodeModel>();
        }

        public void AddAffectedNodes(List<NodeModel> nodes)
        {
            // Node rules will always only have a single Id
            var nodeId = nodes.FirstOrDefault();
            if (nodeId is null)
                return;

            if (AffectedNodes.Contains(nodeId))
                return;

            AffectedNodes.Add(nodeId);
        }
    }
}
