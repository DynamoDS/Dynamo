using System.Collections.Concurrent;
using System.Web;
using DynamoPackagesAnalyzer.Analyzer;
using DynamoPackagesAnalyzer.Helper;
using DynamoPackagesAnalyzer.Models;
using DynamoPackagesAnalyzer.Models.CommandLine;
using DynamoPackagesAnalyzer.Models.DirectorySource;
using DynamoPackagesAnalyzer.Models.Greg;
using Microsoft.Extensions.Configuration;
using static DynamoPackagesAnalyzer.Helper.LogHelper;

namespace DynamoPackagesAnalyzer.PackageSource
{
    /// <summary>
    /// provides methods to analyze package directories 
    /// </summary>
    internal class DirectorySource
    {
        private readonly DirectoryOptions options;
        private readonly DirectoryInfo workspace;
        private readonly IConfigurationRoot configuration;
        private readonly PackageHeaderCustom packageHeader;

        internal DirectorySource(DirectoryOptions options, PackageHeaderCustom packageHeader = null)
        {
            this.options = options;

            this.packageHeader = packageHeader;
            DirectoryInfo directoryToAnalyze = new DirectoryInfo(this.options.Path);
            if (!directoryToAnalyze.Exists)
            {
                throw new DirectoryNotFoundException(this.options.Path);
            }
            workspace = directoryToAnalyze;
            configuration = ConfigHelper.GetConfiguration();
        }

        /// <summary>
        /// Starts the analysis process verifying the <see cref="DirectoryOptions.HasZipArchives"/> flag and processing the folder accordingly
        /// </summary>
        /// <returns></returns>
        internal async Task<List<AnalyzedPackage>> RunAnalysis()
        {
            if (options.HasZipArchives)
            {
                return await ProcessAsZipArchiveAsync();
            }
            return await ProcessAsDirectoryAsync();
        }

        /// <summary>
        /// if <see cref="DirectoryOptions.HasZipArchives"/> in <see cref="options"/> is true, then search for zip files in the provided directory, and delegate the zip files to <see cref="ZipArchiveSource"/>
        /// </summary>
        /// <returns></returns>
        private async Task<List<AnalyzedPackage>> ProcessAsZipArchiveAsync()
        {
            FileInfo[] zipFiles = await GetZipFiles(workspace);

            if (!zipFiles.Any())
            {
                return new List<AnalyzedPackage>();
            }

            ZipArchiveSource zipArchiveSource = new ZipArchiveSource(new ZipArchiveOptions()
            {
                Files = zipFiles.Select(f => f.FullName),
                MultiThread = options.MultiThread,
                LookupDetails = options.LookupDetails
            });

            return await zipArchiveSource.RunAnalysis();
        }

        /// <summary>
        /// Tries to resolve the package information using the pkg.json file, optionally if <see cref="DirectoryOptions.LookupDetails"/> is true, try to get the information from the package list at dynamopackages.com
        /// </summary>
        /// <returns></returns>
        private async Task<AnalyzedPackage> GetAnalyzedPackage()
        {
            AnalyzedPackage analyzedPackage;
            if (packageHeader != null)
            {
                analyzedPackage = ClassConverterHelper.ToAnalyzedPackage(packageHeader);
            }
            else
            {
                PkgJson pkgJson = await GetPkgJson();

                switch (options.LookupDetails)
                {
                    case true:
                        PackageHeaderCustom packageHeader = await DynamoPackagesSource.FindPackage(pkgJson.Name);
                        analyzedPackage = packageHeader != null ? ClassConverterHelper.ToAnalyzedPackage(packageHeader) : ClassConverterHelper.ToAnalyzedPackage(pkgJson);
                        break;
                    default:
                        analyzedPackage = ClassConverterHelper.ToAnalyzedPackage(pkgJson);
                        break;
                }
            }

            analyzedPackage.ArchiveName ??= options.ArchiveName;

            return analyzedPackage;
        }

        /// <summary>
        /// if <see cref="DirectoryOptions.HasZipArchives"/> in <see cref="options"/> is false, then the provided directory is an unzipped package and all the dlls must be analyzed
        /// </summary>
        /// <returns></returns>
        private async Task<List<AnalyzedPackage>> ProcessAsDirectoryAsync()
        {
            List<BinaryAnalyzer> packagesToAnalyze = new List<BinaryAnalyzer>();
            ConcurrentBag<AnalyzedPackage> dllAnalysisResult = new ConcurrentBag<AnalyzedPackage>();

            AnalyzedPackage analyzedPackage = await GetAnalyzedPackage();
            analyzedPackage.DLLs = await GetDlls(workspace);
            analyzedPackage.HasSource = analyzedPackage.DLLs.Any();
            analyzedPackage.ArchiveName = options.ArchiveName ?? workspace.Name;

            switch (analyzedPackage.HasSource)
            {
                case true:
                    foreach (FileInfo dll in analyzedPackage.DLLs)
                    {
                        AnalyzedPackage dllPackage = analyzedPackage.Copy();
                        DirectoryInfo process_workspace = new DirectoryInfo(Path.Combine(dll.Directory.FullName, Path.GetFileNameWithoutExtension(dll.Name)));
                        process_workspace.Create();
                        packagesToAnalyze.Add(new BinaryAnalyzer(dll, analyzedPackage, process_workspace.FullName));
                    }
                    break;
                default:
                    dllAnalysisResult.Add(analyzedPackage);
                    return dllAnalysisResult.ToList();
            }

            switch (options.MultiThread)
            {
                case true:
                    await Parallel.ForEachAsync(packagesToAnalyze,
                        new ParallelOptions { MaxDegreeOfParallelism = int.Parse(configuration["MaxDegreeOfParallelism"]) },
                        async (package, cancellationToken) =>
                        {
                            dllAnalysisResult.Add(await ProcessDLL(package));
                        }
                    );
                    break;
                default:
                    foreach (BinaryAnalyzer package in packagesToAnalyze)
                    {
                        dllAnalysisResult.Add(await ProcessDLL(package));
                    }
                    break;
            }

            return dllAnalysisResult.ToList();
        }

        /// <summary>
        /// Triggers the binary analysis on the previously <see cref="BinaryAnalyzer"/> instance, this method helps to run the analysis in a multithreaded way
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        private async Task<AnalyzedPackage> ProcessDLL(BinaryAnalyzer package)
        {
            AnalyzedPackage analyzedPackage = await package.GetAnalyzedPackage();
            Log(analyzedPackage.Name, $"{package.Name} analysis start");
            analyzedPackage = await package.StartAnalysis();
            Log(analyzedPackage.Name, analyzedPackage.HasAnalysisError ? $"{package.Name} analysis failed" : $"{package.Name} analysis end");
            return analyzedPackage;
        }

        /// <summary>
        /// Reads the pkg.json in the root of the directory
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        internal async Task<PkgJson> GetPkgJson()
        {
            FileInfo file = new FileInfo(Path.Combine(workspace.FullName, "pkg.json"));
            if (!file.Exists)
            {
                throw new FileNotFoundException("pkg.json does not exists");
            }

            PkgJson pkgJson = await JsonHelper.Read<PkgJson>(file);
            pkgJson.Name = HttpUtility.HtmlDecode(pkgJson.Name).Trim();
            return pkgJson;
        }

        /// <summary>
        /// List all the DLL in the provided directory an it's subdirectories
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        internal static async Task<FileInfo[]> GetDlls(DirectoryInfo package)
        {
            DirectoryInfo binDirectory = new DirectoryInfo(Path.Combine(package.FullName, "bin"));
            if (binDirectory.Exists)
            {
                FileInfo[] dlls = Array.Empty<FileInfo>();

                await Task.Run(() =>
                {
                    dlls = Directory.GetFiles(binDirectory.FullName, "*.dll", new EnumerationOptions() { RecurseSubdirectories = true }).Select(f => new FileInfo(f)).ToArray();
                });
                return dlls;
            }
            return Array.Empty<FileInfo>();
        }

        /// <summary>
        /// List all the Zip files in the provided directory an it's subdirectories
        /// </summary>
        /// <param name="base_path"></param>
        /// <returns></returns>
        internal static async Task<FileInfo[]> GetZipFiles(DirectoryInfo base_path)
        {
            if (base_path.Exists)
            {
                FileInfo[] zipFiles = Array.Empty<FileInfo>();

                await Task.Run(() =>
                {
                    zipFiles = Directory.GetFiles(base_path.FullName, "*.zip", new EnumerationOptions() { RecurseSubdirectories = true }).Select(f => new FileInfo(f)).ToArray();
                });
                return zipFiles;
            }
            return Array.Empty<FileInfo>();
        }
    }
}
