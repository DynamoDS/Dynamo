namespace Dynamo.Linting.Interfaces
{
    public interface IRuleEvaluationResult
    {
        /// <summary>
        /// Id of the rule that create this result
        /// </summary>
        string RuleId { get; }

        /// <summary>
        /// Status of the evaluation (Passed or Failed)
        /// </summary>
        RuleEvaluationStatusEnum Status { get; }
    }
}
