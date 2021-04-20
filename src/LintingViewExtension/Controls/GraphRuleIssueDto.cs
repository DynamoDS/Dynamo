using System.Collections.ObjectModel;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension.Controls
{
    public class GraphRuleIssueDto
    {
        public string Id { get; }
        public GraphLinterRule Rule { get; }
        public ObservableCollection<GraphRuleEvaluationResult> Results { get; private set; }

        public GraphRuleIssueDto(string id, GraphLinterRule rule)
        {
            Id = id;
            Rule = rule;
            Results = new ObservableCollection<GraphRuleEvaluationResult>();
        }

        public void AddResult(GraphRuleEvaluationResult result)
        {
            Results.Add(result);
        }
    }
}