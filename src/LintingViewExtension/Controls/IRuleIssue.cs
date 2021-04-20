using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension
{
    public interface IRuleIssue
    {
        ObservableCollection<string> AffectedNodes { get; }
        string Id { get; }
        LinterRule Rule { get; }

        void AddResult(List<string> nodeIds);
    }
}