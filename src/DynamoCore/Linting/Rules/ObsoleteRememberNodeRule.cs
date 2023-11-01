using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Linting.Interfaces;
using Dynamo.Properties;

namespace Dynamo.Linting.Rules;

class ObsoleteRememberNodeRule : NodeLinterRule
{
    public override string Id => "3e863a77-d922-4470-8bdc-b47603fa7fcb";
    public override SeverityCodesEnum SeverityCode => SeverityCodesEnum.Warning;
    public override string Description => Resources.ObsoleteRememberNodeRule_Description;
    public override string CallToAction => Resources.ObsoleteRememberNodeRule_CallToAction;
    public override List<string> EvaluationTriggerEvents { get; }

    protected override RuleEvaluationStatusEnum EvaluateFunction(NodeModel nodeModel, string changedEvent)
    {
        if (nodeModel.IsSetAsInput)
        {
            // We use type name comparision here to avoid dependency on GD assemblies
            if (nodeModel.GetType().ToString() ==  "GenerativeDesign.Remember")
            {
                return RuleEvaluationStatusEnum.Failed;
            }
        }

        return RuleEvaluationStatusEnum.Passed;
    }

    protected override List<Tuple<RuleEvaluationStatusEnum, string>> InitFunction(WorkspaceModel workspaceModel)
    {
        var results = new List<Tuple<RuleEvaluationStatusEnum, string>>();
        foreach (var node in workspaceModel.Nodes)
        {
            var evaluationStatus = EvaluateFunction(node, "initialize");

            if (evaluationStatus == RuleEvaluationStatusEnum.Passed)
                continue;

            var valueTuple = Tuple.Create(evaluationStatus, node.GUID.ToString());
            results.Add(valueTuple);
        }

        return results;
    }
}
