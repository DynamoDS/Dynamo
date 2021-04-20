using System.Collections.ObjectModel;
using Dynamo.Linting.Interfaces;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension.Controls
{
    public interface IRuleIssueDto<TRuleArgType, TResultType> where TResultType : IRuleEvaluationResult
    {
        string Id { get; }
        ObservableCollection<TResultType> Results { get; }
        LinterRule Rule { get; }

        void AddResult(TResultType result);
    }
}