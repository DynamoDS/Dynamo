using DynamoAnalyzer.Models;

namespace DynamoAnalyzer.Analyzer
{
    /// <summary>
    /// Allows to process a package
    /// </summary>
    internal interface IAnalyze
    {
        string Name { get; }
        Task<AnalyzedPackage> Process();

        Task<AnalyzedPackage> GetAnalyzedPackage();
    }
}
