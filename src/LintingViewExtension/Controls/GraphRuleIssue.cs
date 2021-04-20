using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension.Controls
{
    internal class GraphRuleIssue : IRuleIssue
    {
        public string Id { get; }
        public LinterRule Rule { get; }
        public ObservableCollection<string> AffectedNodes { get; private set; }

        public GraphRuleIssue(string id, GraphLinterRule rule)
        {
            Id = id;
            Rule = rule;
            AffectedNodes = new ObservableCollection<string>();
        }

        public void AddAffectedNodes(List<string> nodeIds)
        {
            nodeIds.ForEach(x => AffectedNodes.Add(x));
        }
    }
}