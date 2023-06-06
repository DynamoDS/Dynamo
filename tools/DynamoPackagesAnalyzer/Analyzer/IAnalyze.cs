using DynamoPackagesAnalyzer.Models;

namespace DynamoPackagesAnalyzer.Analyzer
{
    /// <summary>
    /// Allows to process a package
    /// </summary>
    internal interface IAnalyze
    {
        string Name { get; }
        Task<AnalyzedPackage> StartAnalysis();

        Task<AnalyzedPackage> GetAnalyzedPackage();
    }
}
