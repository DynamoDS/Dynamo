using System.Collections.Concurrent;
using AutoMapper;
using DynamoAnalyzer;
using DynamoAnalyzer.Analyzer;
using DynamoAnalyzer.Extensions;
using DynamoAnalyzer.Helper;
using DynamoAnalyzer.Models;

internal class Program
{
    static List<PackageHeaderCustom> response;
    static ConcurrentBag<AnalyzedPackage> processed = new ConcurrentBag<AnalyzedPackage>();

    private static async Task Main(string[] args)
    {
        int threads = Convert.ToInt32(ConfigHelper.GetConfiguration()["MaxDegreeOfParallelism"]);

        int _index = 0;
        IMapper _mapper = AutomapperHelper.GetMapper();

        response = (await GregHelper.GetPackages()).Select(f => _mapper.Map<PackageHeaderCustom>(f)).ToList();

        response.ForEach(f => f.Index = _index++);

        //Allows to analyze multiple packages at the same time
        await Parallel.ForEachAsync(response
            , new ParallelOptions { MaxDegreeOfParallelism = threads },
            async (packageHeader, cancellationToken) =>
            {
                await Process(packageHeader);
            }
        );

        LogHelper.Log("", $"Process finised for {response.Count} packages");
        DateTime date = DateTime.Now;
        string results = await CsvHandler.WritePackagesCsv(processed.OrderBy(f => f.Index).ToList(), date);
        LogHelper.Log("", $"results can be reviewed in '{results}'");

        List<DuplicatedPackage> duplicated = processed.Where(f => !string.IsNullOrEmpty(f.ArtifactName)).GroupBy(f => f.ArtifactName).Select(f => new DuplicatedPackage(f.Key, f.Count(), f.Select(g => g.Name).ToArray())).ToList();

        results = await CsvHandler.WriteDuplicatedCsv(duplicated.Where(f => f.Count > 1), date);
        LogHelper.Log("", $"duplicated can be reviewed in '{results}'");

        Console.ReadKey();
    }

    /// <summary>
    /// Download, extract and start the upgrade-assistant process
    /// </summary>
    /// <param name="packageHeader"></param>
    /// <returns></returns>
    private static async Task Process(PackageHeaderCustom packageHeader)
    {
        List<AnalyzedPackage> dllAnalysisResult = new List<AnalyzedPackage>();
        List<IAnalyze> packagesToAnalyze = new List<IAnalyze>();

        AnalyzedPackage analyzedPackage = await GregHelper.DownloadAndExtract(packageHeader);
        analyzedPackage.Index = packageHeader.Index;
        if (analyzedPackage.HasAnalysisError)
        {
            LogHelper.Log(packageHeader.name, $"Analysis failed {packageHeader.Index}");
            processed.Add(analyzedPackage);
            await GregHelper.DeletePackage(packageHeader);
            return;
        }

        LogHelper.Log(packageHeader.name, $"Analysis start {packageHeader.Index}");

        #region Add To Binary Analyzer
        switch (analyzedPackage.DLLs.Any())
        {
            case true:
                analyzedPackage.HasBinaries = true;

                foreach (FileInfo dll in analyzedPackage.DLLs)
                {
                    packagesToAnalyze.Add(new BinaryAnalyzer(dll, analyzedPackage, Path.Combine(AnalyzeEnvironment.GetWorkspace().FullName, Path.GetFileNameWithoutExtension(analyzedPackage.ArchiveName))));
                }
                break;
            default:
                analyzedPackage.HasBinaries = false;
                break;
        }
        #endregion

        foreach (IAnalyze f in packagesToAnalyze)
        {
            LogHelper.Log(packageHeader.name, $"{f.Name} -> start");
            AnalyzedPackage result = await f.Process();

            if (result.HasAnalysisError)
            {
                LogHelper.Log(packageHeader.name, $"{f.Name} -> end with error: {result.Result.First()}");
            }
            else
            {
                LogHelper.Log(packageHeader.name, $"{f.Name} -> end");
            }

            dllAnalysisResult.Add(result);
        }

        if (analyzedPackage.DLLs.Any())
        {
            processed.AddRange(dllAnalysisResult);
        }
        else
        {
            processed.Add(analyzedPackage);
        }

        LogHelper.Log(packageHeader.name, $"Analysis end {packageHeader.Index}");
        await GregHelper.DeletePackage(packageHeader);
    }
}
