using DynamoAnalyzer.Models;

namespace DynamoAnalyzer.Analyzer
{
    /// <summary>
    /// Allows to process a package
    /// </summary>
    public interface IAnalyze
    {
        string Name { get; }
        Task<AnalyzedPackage> Process();
    }
}
