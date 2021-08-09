using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Graph.Nodes;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension.Controls
{
    internal class GraphRuleIssue : IRuleIssue
    {
        public string Id { get; }
        public LinterRule Rule { get; }
        public ObservableCollection<NodeModel> AffectedNodes { get; private set; }

        public GraphRuleIssue(string id, GraphLinterRule rule)
        {
            Id = id;
            Rule = rule;
            AffectedNodes = new ObservableCollection<NodeModel>();
        }

        public void AddAffectedNodes(List<NodeModel> nodes)
        {
            nodes.ForEach(x => AffectedNodes.Add(x));
        }
    }
}