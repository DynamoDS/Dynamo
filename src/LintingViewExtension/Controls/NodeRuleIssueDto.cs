using System.Collections.ObjectModel;
using Dynamo.Core;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension.Controls
{
    public class NodeRuleIssueDto : NotificationObject
    {
        public string Id { get; }
        public NodeLinterRule Rule { get; }
        public ObservableCollection<NodeRuleEvaluationResult> Results { get; private set; }

        public NodeRuleIssueDto(string id, NodeLinterRule rule)
        {
            Id = id;
            Rule = rule;
            Results = new ObservableCollection<NodeRuleEvaluationResult>();
        }

        public void AddResult(NodeRuleEvaluationResult result)
        {
            Results.Add(result);
        }
    }
}
