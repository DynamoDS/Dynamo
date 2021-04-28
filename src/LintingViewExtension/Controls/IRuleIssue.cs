using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamo.Graph.Nodes;
using Dynamo.Linting.Rules;

namespace Dynamo.LintingViewExtension
{
    public interface IRuleIssue
    {
        /// <summary>
        /// Collection of nodeIds affected by this rule issue
        /// </summary>
        ObservableCollection<NodeModel> AffectedNodes { get; }

        /// <summary>
        /// Id of the rule this issue comes from
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Rule this issue comes from
        /// </summary>
        LinterRule Rule { get; }

        /// <summary>
        /// Adds a list of affected nodes to this issue
        /// </summary>
        /// <param name="nodes"></param>
        void AddAffectedNodes(List<NodeModel> nodes);
    }
}