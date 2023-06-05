using System.Collections.Concurrent;
using DynamoAnalyzer.Helper;
using DynamoAnalyzer.Models;
using DynamoAnalyzer.Models.CommandLine;
using DynamoAnalyzer.Models.Greg;
using DynamoPackagesAnalyzer.Helper;
using Microsoft.Extensions.Configuration;
using static DynamoAnalyzer.Helper.LogHelper;

namespace DynamoAnalyzer.PackageSource
{
    /// <summary>
    /// Provides method to analize package zip archives 
    /// </summary>
    internal class ZipArchiveSource
    {
        private readonly ZipArchiveOptions options;
        private readonly FileInfo[] files;
        private readonly DirectoryInfo workspace;
        private readonly IConfigurationRoot configuration;
        private PackageHeader packageHeader;

        public ZipArchiveSource(ZipArchiveOptions options, PackageHeader packageHeader = null, DirectoryInfo workspace = null)
        {
            this.options = options;
            files = options.Files.Select(f => new FileInfo(f)).ToArray();

            if (!files.All(f => f.Exists))
            {
                throw new InvalidOperationException($"{string.Join(", ", files.Where(f => !f.Exists))} does not exists");
            }

            if (!files.All(f => f.Extension.Equals(".zip")))
            {
                throw new InvalidOperationException($"Some files does not have a valid zip extension: {string.Join(", ", files.Where(f => !f.Extension.Equals(".zip")))}");
            }
            this.packageHeader = packageHeader;
            this.workspace = workspace ?? WorkspaceHelper.GetWorkspace();
            configuration = ConfigHelper.GetConfiguration();
        }

        /// <summary>
        /// Starts the analysis process
        /// </summary>
        /// <returns></returns>
        public async Task<List<AnalyzedPackage>> Run()
        {
            ConcurrentBag<AnalyzedPackage> packages = new ConcurrentBag<AnalyzedPackage>();

            switch (options.MultiThread)
            {
                case true:
                    int MaxDegreeOfParallelism = int.Parse(configuration["MaxDegreeOfParallelism"]);
                    await Parallel.ForEachAsync(files,
                        new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism },
                        async (item, cancellationToken) =>
                        {
                            packages.AddRange(await Process(item));
                        }
                    );
                    break;
                default:
                    foreach (FileInfo item in files)
                    {
                        packages.AddRange(await Process(item));
                    }
                    break;
            }

            return packages.ToList();
        }

        /// <summary>
        /// Unzips the file and delegates the analysis process to <see cref="DirectorySource"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<List<AnalyzedPackage>> Process(FileInfo item)
        {
            Log(item.Name, "Unzip file");
            List<AnalyzedPackage> result = new List<AnalyzedPackage>();
            DirectoryInfo unzipped = new DirectoryInfo(Path.Combine(item.Directory.FullName, Path.GetFileNameWithoutExtension(item.Name)));

            try
            {
                unzipped = ZipArchiveHelper.Unzip(item, unzipped);
                DirectorySource d = new DirectorySource(new DirectoryOptions()
                {
                    Path = unzipped.FullName,
                    ArchiveName = item.Name,
                    LookupDetails = options.LookupDetails
                }, packageHeader);
                result = await d.Run();
                unzipped.Delete(true);
                Log(item.Name, "Delete Unzip folder");
            }
            catch (Exception ex)
            {
                if (unzipped.Exists)
                {
                    unzipped.TryDelete(true);
                }
                result.Add(GetAnalyzedPackageError(item, ex));
            }

            return result;
        }

        /// <summary>
        /// When an unzipping operation fails this method returns an instance of <see cref="AnalyzedPackage"/> to track and be able to register the error of the package in the csv file
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private AnalyzedPackage GetAnalyzedPackageError(FileInfo item, Exception ex)
        {
            AnalyzedPackage analyzedPackage = packageHeader != null ?
                ClassConverterHelper.ToAnalyzedPackage(packageHeader)
                :
                new AnalyzedPackage()
                {
                    ArchiveName = item.Name,
                };

            analyzedPackage.HasAnalysisError = true;
            analyzedPackage.Result = new string[] { (ex.InnerException ?? ex).Message };

            //To distinct the packages that failed
            analyzedPackage.Index = -1;
            return analyzedPackage;
        }
    }
}
