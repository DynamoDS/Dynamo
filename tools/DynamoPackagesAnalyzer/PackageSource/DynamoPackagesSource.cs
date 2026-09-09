using System.Collections.Concurrent;
using System.Net.Http;
using System.Web;
using DynamoPackagesAnalyzer.Helper;
using DynamoPackagesAnalyzer.Models;
using DynamoPackagesAnalyzer.Models.CommandLine;
using DynamoPackagesAnalyzer.Models.Greg;
using Greg.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DynamoPackagesAnalyzer.PackageSource
{
    /// <summary>
    /// Provides methods to analyze packages at dynamopackages.com
    /// </summary>
    internal class DynamoPackagesSource
    {
        private readonly DynamoPackagesOptions options;
        private static readonly HttpClient httpClient = new HttpClient() 
        { 
            BaseAddress = new Uri(ConfigHelper.GetConfiguration()["DynamoPackagesURL"]) 
        };
        private readonly IConfigurationRoot configuration;
        private static PackageHeaderCustom[] packages;
        private static readonly Mutex getPackagesMutex = new Mutex(false, "");

        internal DynamoPackagesSource(DynamoPackagesOptions option)
        {
            options = option;
            configuration = ConfigHelper.GetConfiguration();
        }

        /// <summary>
        /// Returns all the packages from dynamopackages.com except banned and deprecated
        /// </summary>
        /// <returns></returns>
        internal static async Task<PackageHeaderCustom[]> GetPackages()
        {
            getPackagesMutex.WaitOne();
            if (packages == null)
            {
                LogHelper.Log("Download DynamoPackages", "start");
                var response = await httpClient.GetAsync("packages/dynamo");
                ValidateResponse(response);
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Response<PackageHeaderCustom[]>>(responseContent);
                PackageHeaderCustom[] packages = result.Content.Where(f => !f.deprecated && !f.banned).ToArray();

                //method used in Dynamo\src\DynamoPackages\PackageManagerClient.cs:L97 to filter packages with wrong version numbers
                for (int i = 0; i < packages.Length; i++)
                {
                    PackageHeaderCustom package = packages[i];
                    bool packageHasWrongVersion = package.versions.Any(v => v.url == null);

                    if (packageHasWrongVersion)
                    {
                        List<PackageVersion> cleanVersions = new List<PackageVersion>();
                        cleanVersions.AddRange(package.versions.Where(v => v.url != null));
                        package.versions = cleanVersions;
                    }
                    package.Index = i;
                    //Some packages have names html encoded
                    package.name = HttpUtility.HtmlDecode(package.name).Trim();
                }
                DynamoPackagesSource.packages = packages;
                LogHelper.Log("Download DynamoPackages", "end");
            }
            getPackagesMutex.ReleaseMutex();

            return packages;
        }

        /// <summary>
        /// Finds the package information at dynamopackages.com
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static async Task<PackageHeaderCustom> FindPackage(string name)
        {
            PackageHeaderCustom[] packages = await GetPackages();
            return packages.FirstOrDefault(f => f.name.Equals(name));
        }

        /// <summary>
        /// Downloads a package from dynamopackages.com
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        private async Task<FileInfo> DownloadPackage(PackageHeaderCustom package)
        {
            PackageVersion version = GetLastVersion(package);
            var response = await httpClient.GetAsync($"download/{package._id}/{version.name}");
            ValidateResponse(response);

            string output = Path.Combine(WorkspaceHelper.GetWorkspace().FullName, 
                response.RequestMessage?.RequestUri != null ? Path.GetFileName(response.RequestMessage.RequestUri.AbsolutePath) : $"package_{package._id}_{version.name}.zip");
            using (FileStream fileStream = new FileStream(output, FileMode.Create))
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                await fileStream.WriteAsync(bytes, 0, bytes.Length);
            }

            return new FileInfo(output);
        }

        /// <summary>
        /// Starts the analysis process downloading and processing all of the dynamo packages at dynamopackages.com
        /// </summary>
        /// <returns></returns>
        internal async Task<List<AnalyzedPackage>> RunAnalysis()
        {
            PackageHeaderCustom[] list = await GetPackages();

            ConcurrentBag<AnalyzedPackage> dllAnalysisResult = new ConcurrentBag<AnalyzedPackage>();

            await Parallel.ForEachAsync(list.Take(50),
                new ParallelOptions { MaxDegreeOfParallelism = int.Parse(configuration["MaxDegreeOfParallelism"]) },
                async (packageHeader, cancellationToken) =>
                {
                    FileInfo file = null;
                    try
                    {
                        file = await DownloadPackage(packageHeader);
                        ZipArchiveSource zipArchiveSource = new ZipArchiveSource(new ZipArchiveOptions()
                        {
                            Files = new string[] { file.FullName },
                        }, packageHeader);
                        dllAnalysisResult.AddRange(await zipArchiveSource.RunAnalysis());
                    }
                    catch (Exception ex)
                    {
                        AnalyzedPackage analyzed = ClassConverterHelper.ToAnalyzedPackage(packageHeader);
                        analyzed.HasAnalysisError = true;
                        analyzed.Result = new string[] { (ex.InnerException ?? ex).Message };
                        dllAnalysisResult.Add(analyzed);
                    }
                    finally
                    {
                        file?.Delete();
                    }
                }
            );

            return dllAnalysisResult.ToList();
        }

        /// <summary>
        /// Gets the latest version of the package
        /// </summary>
        /// <param name="packageHeader"></param>
        /// <returns></returns>
        private PackageVersion GetLastVersion(PackageHeaderCustom packageHeader)
        {
            return packageHeader.versions.Last();
        }

        /// <summary>
        /// Validates the response to dynamopackages.com
        /// </summary>
        /// <param name="response"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void ValidateResponse(HttpResponseMessage response)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"HTTP status: {response.ReasonPhrase}");
            }
        }
    }
}
