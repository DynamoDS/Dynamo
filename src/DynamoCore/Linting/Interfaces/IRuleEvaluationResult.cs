namespace Dynamo.Linting.Interfaces
{
    internal interface IRuleEvaluationResult
    {
        /// <summary>
        /// Id of the rule that created this result
        /// </summary>
        string RuleId { get; }

        /// <summary>
        /// Severity code of the rule that created this result
        /// </summary>
        SeverityCodesEnum SeverityCode { get; }

        /// <summary>
        /// Status of the evaluation (Passed or Failed)
        /// </summary>
        RuleEvaluationStatusEnum Status { get; }
    }
}
