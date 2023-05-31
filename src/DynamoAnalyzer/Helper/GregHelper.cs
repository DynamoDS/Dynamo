using DynamoAnalyzer.Models;
using Greg;
using Greg.Requests;
using Greg.Responses;

namespace DynamoAnalyzer.Helper
{
    /// <summary>
    /// Provides methos to manage packages
    /// </summary>
    public static class GregHelper
    {
        private static GregClient _client;

        public static GregClient GetClient()
        {
            return _client ??= new GregClient(new Dynamo.Core.IDSDKManager(), ConfigHelper.GetConfiguration()["DynamoPackagesURL"]);
        }

        /// <summary>
        /// Downloads the full package list
        /// </summary>
        /// <returns></returns>
        public static async Task<List<PackageHeader>> GetPackages()
        {
            ResponseWithContentBody<List<PackageHeader>> pkgResponse = null;

            LogHelper.Log("PackageList", "Download start");
            await Task.Run(() =>
            {
                pkgResponse = GetClient().ExecuteAndDeserializeWithContent<List<PackageHeader>>(HeaderCollectionDownload.ByEngine(ConfigHelper.GetConfiguration()["Engine"]));
            });

            CleanPackagesWithWrongVersions(pkgResponse.content);

            List<PackageHeader> response = pkgResponse.content.Where(f => !f.banned).ToList();

            LogHelper.Log("PackageList", "Download end");

            return response;
        }

        /// <summary>
        /// For every package Checks its versions and exclude the ones with errors
        /// </summary>
        /// <param name="packages"></param>
        static void CleanPackagesWithWrongVersions(List<PackageHeader> packages)
        {
            foreach (var package in packages)
            {
                bool packageHasWrongVersion = package.versions.Count(v => v.url == null) > 0;

                if (packageHasWrongVersion)
                {
                    List<PackageVersion> cleanVersions = new List<PackageVersion>();
                    cleanVersions.AddRange(package.versions.Where(v => v.url != null));
                    package.versions = cleanVersions;
                }
            }
        }

        /// <summary>
        /// Combines the workspace path and the archive name without the extension
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetPackageWorkspace(FileInfo file)
        {
            return Path.Combine(AnalyzeEnvironment.GetWorkspace().FullName, Path.GetFileNameWithoutExtension(file.Name));
        }

        /// <summary>
        /// Download and extract a package to the workspace
        /// </summary>
        /// <param name="packageHeader"></param>
        /// <returns></returns>
        public static async Task<AnalyzedPackage> DownloadAndExtract(PackageHeader packageHeader)
        {
            PackageVersion version = GetLastVersion(packageHeader);

            User user = packageHeader.maintainers.FirstOrDefault();
            FileInfo file = null;
            DirectoryInfo package = null;
            AnalyzedPackage result = new AnalyzedPackage()
            {
                HasSource = !string.IsNullOrEmpty(packageHeader.repository_url),
                Id = packageHeader._id,
                Name = packageHeader.name,
                Version = version.version,
                ArchiveName = version.url,
                UserId = user?._id,
                UserName = user?.username
            };

            try
            {

                LogHelper.Log(packageHeader.name, $"{nameof(Download)}: start");
                file = await Download(packageHeader._id, version.version);
                LogHelper.Log(packageHeader.name, $"{nameof(Download)}: end");
                LogHelper.Log(packageHeader.name, $"{nameof(Extract)}: start");
                package = Extract(file);
                LogHelper.Log(packageHeader.name, $"{nameof(Extract)}: end");
                result.DLLs = await HasBin(package);
            }
            catch (Exception ex)
            {
                result.HasAnalysisError = true;
                result.Result = new string[] { (ex.InnerException ?? ex).Message };
                CleanWorkspaceOnError(file, package);
            }

            return result;
        }

        private static void CleanWorkspaceOnError(FileInfo file,
        DirectoryInfo package)
        {
            try
            {
                if (file != null && file.Exists)
                {
                    file.Delete();
                }

                if (package != null && package.Exists)
                {
                    package.Delete(true);
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Download a package from dynamopackages.com
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static async Task<FileInfo> Download(string id, string version)
        {
            AnalyzeEnvironment.GetWorkspace().Create();

            Response response = null;
            string defaultDownloadPath = null;

            await Task.Run(() =>
            {
                response = GetClient().Execute(new PackageDownload(id, version));
                defaultDownloadPath = PackageDownload.GetFileFromResponse(response);
            });

            FileInfo fileInfo = new FileInfo(defaultDownloadPath);

            string newFileLocation = Path.Combine(AnalyzeEnvironment.GetWorkspace().FullName, fileInfo.Name);

            fileInfo.MoveTo(newFileLocation, true);
            fileInfo = new FileInfo(newFileLocation);

            return fileInfo;
        }

        /// <summary>
        /// Extract a package archive to the workspace
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static DirectoryInfo Extract(FileInfo file)
        {
            DirectoryInfo outputPath = new DirectoryInfo(GetPackageWorkspace(file));
            if (outputPath.Exists)
            {
                outputPath.Delete(true);
                outputPath.Refresh();
            }

            DirectoryInfo unzipPath = new DirectoryInfo(Greg.Utility.FileUtilities.UnZip(file.FullName));
            unzipPath.MoveTo(outputPath.FullName);

            outputPath.Refresh();

            return outputPath;
        }

        /// <summary>
        /// List all the DLL in the provided directory an it's subdirectories
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public static async Task<FileInfo[]> HasBin(DirectoryInfo package)
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
        /// Returns the latest version of a package
        /// </summary>
        /// <param name="packageHeader"></param>
        /// <returns></returns>
        public static PackageVersion GetLastVersion(PackageHeader packageHeader)
        {
            return packageHeader.versions.Last();
        }

        /// <summary>
        /// Deletes a package from the workspace
        /// </summary>
        /// <param name="packageHeader"></param>
        /// <returns></returns>
        public static async Task DeletePackage(PackageHeader packageHeader)
        {
            PackageVersion version = GetLastVersion(packageHeader);

            DirectoryInfo directory = new DirectoryInfo(Path.Combine(AnalyzeEnvironment.GetWorkspace().FullName, Path.GetFileNameWithoutExtension(version.url)));
            FileInfo file = new FileInfo(Path.Combine(AnalyzeEnvironment.GetWorkspace().FullName, version.url));

            if (directory.Exists)
            {
                await Task.Run(() =>
                {
                    directory.Delete(true);
                    file.Delete();
                });
            }
        }
    }
}
