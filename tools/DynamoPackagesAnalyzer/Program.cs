using System.Collections.Concurrent;
using CommandLine;
using DynamoPackagesAnalyzer.Helper;
using DynamoPackagesAnalyzer.Models;
using DynamoPackagesAnalyzer.Models.CommandLine;
using DynamoPackagesAnalyzer.PackageSource;

internal class Program
{
    static ConcurrentBag<AnalyzedPackage> processed = new ConcurrentBag<AnalyzedPackage>();

    private static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<DirectoryOptions, DynamoPackagesOptions, ZipArchiveOptions>(args)
              .WithParsedAsync(async (obj) =>
              {
                  switch (obj)
                  {
                      case DirectoryOptions c:
                          processed.AddRange(await new DirectorySource(c).RunAnalysis());
                          break;
                      case DynamoPackagesOptions o:
                          processed.AddRange(await new DynamoPackagesSource(o).RunAnalysis());
                          break;
                      case ZipArchiveOptions z:
                          processed.AddRange(await new ZipArchiveSource(z).RunAnalysis());
                          break;
                  }
              });
        DateTime date = DateTime.Now;
        string results = await CsvHandler.WritePackagesCsv(processed.OrderBy(f => f.Index).ThenBy(f => f.Name).ToList(), date);
        LogHelper.Log("results", $"can be reviewed in '{results}'");

        List<DuplicatedPackage> duplicated = processed.Where(f => !string.IsNullOrEmpty(f.ArtifactName)).GroupBy(f => f.ArtifactName).Select(f => new DuplicatedPackage(f.Key, f.Count(), f.Select(g => g.Name).ToArray())).ToList();

        results = await CsvHandler.WriteDuplicatedCsv(duplicated.Where(f => f.Count > 1), date);
        LogHelper.Log("duplicated", $"can be reviewed in '{results}'");
    }
}
