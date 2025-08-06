using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dynamo.Graph.Workspaces;
using Greg;
using Greg.Requests;
using Greg.Responses;
using Newtonsoft.Json.Linq;

namespace Dynamo.PackageManager
{
    public class PackageManagerClient
    {
        #region Properties/Fields

        public const string PackageEngineName = "dynamo";
        private IEnumerable<string> cachedHosts;

        // These were used early on in order to identify packages with binaries and python scripts
        // This is now a bona fide field in the DB so they are obsolete. 
        // TODO: Try to update the old packages while migrating them to Forge Conetent API, so that this const check can be avoided.
        [Obsolete] internal static readonly string PackageContainsBinariesConstant =
            "|ContainsBinaries(5C698212-A139-4DDD-8657-1BF892C79821)";

        [Obsolete] internal static readonly string PackageContainsPythonScriptsConstant =
            "|ContainsPythonScripts(58B25C0B-CBBE-4DDC-AC39-ECBEB8B55B10)";

        private readonly IGregClient client;
        private readonly IPackageUploadBuilder uploadBuilder;

        /// <summary>
        ///     The directory where new packages are created during the upload process.
        /// </summary>
        private readonly string packageUploadDirectory;

        /// <summary>
        ///     The dictionay stores the package name corresponding to the boolean result of whether the user is an author of that package or not.
        /// </summary>
        private Dictionary<string, bool> packageMaintainers;
       
        /// <summary>
        ///     The URL of the package manager website
        /// </summary>
        public string BaseUrl
        {
            get { return this.client.BaseUrl; }
        }

        internal readonly bool NoNetworkMode;

        #endregion

        internal PackageManagerClient(IGregClient client, IPackageUploadBuilder builder, string packageUploadDirectory,
            bool noNetworkMode = false)
        {
            this.packageUploadDirectory = packageUploadDirectory;
            this.uploadBuilder = builder;
            this.client = client;
            this.packageMaintainers = new Dictionary<string, bool>();
            this.NoNetworkMode = noNetworkMode;
        }

        internal bool Upvote(string packageId)
        {
            return FailFunc.TryExecute(() =>
            {
                var pkgResponse = this.client.ExecuteAndDeserialize(new Upvote(packageId));
                return pkgResponse.success;
            }, false);
        }

        internal List<string> UserVotes()
        {
            var votes = FailFunc.TryExecute(() =>
            {
                var nv = new GetUserVotes();
                var pkgResponse = this.client.ExecuteAndDeserializeWithContent<UserVotes>(nv);
                return pkgResponse.content;
            }, null);

            return votes?.has_upvoted;
        }

        internal List<JObject> CompatibilityMap()
        {
            var compatibilityMap = FailFunc.TryExecute(() =>
            {
                var cm = new GetCompatibilityMap();
                var pkgResponse = this.client.ExecuteAndDeserializeWithContent<object>(cm);

                // Serialize the response to JSON and parse it
                var content = JsonSerializer.Serialize(pkgResponse.content);
                return JArray.Parse(content).Cast<JObject>().ToList();
            }, null);

            return compatibilityMap;
        }

        internal PackageManagerResult DownloadPackage(string packageId, string version, out string pathToPackage)
        {
            try
            {
                var response = this.client.Execute(new PackageDownload(packageId, version));
                pathToPackage = PackageDownload.GetFileFromResponse(response);
                return PackageManagerResult.Succeeded();
            }
            catch (Exception e)
            {
                pathToPackage = null;
                return PackageManagerResult.Failed(e.Message);
            }
        }

        internal IEnumerable<PackageHeader> ListAll()
        {
            return FailFunc.TryExecute(() => {
                var nv = HeaderCollectionDownload.ByEngine("dynamo");
                var pkgResponse = this.client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);                
                CleanPackagesWithWrongVersions(pkgResponse.content);
                return pkgResponse.content;
            }, new List<PackageHeader>());
        }

        /// <summary>
        /// For every package Checks its versions and exclude the ones with errors
        /// </summary>
        /// <param name="packages"></param>
        void CleanPackagesWithWrongVersions(List<PackageHeader> packages)
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
        /// Gets maintainers for a specific package
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <returns></returns>
        internal PackageHeader GetPackageMaintainers(IPackageInfo packageInfo)
        {
            var header = FailFunc.TryExecute(() =>
            {
                var nv = new GetMaintainers("dynamo", packageInfo.Name);
                var pkgResponse = this.client.ExecuteAndDeserializeWithContent<PackageHeader>(nv);
                return pkgResponse?.content;
            }, null);

            return header;
        }

        /// <summary>
        /// Gets last published version of all the packages published by the current user.
        /// </summary>
        /// <returns></returns>
        internal UserPackages GetUsersLatestPackages()
        {
            var packages = FailFunc.TryExecute(() =>
            {
                var nv = new GetMyPackages();
                var pkgResponse = this.client.ExecuteAndDeserializeWithContent<UserPackages>(nv);
                return pkgResponse.content;
            }, null);

            return packages;
        }

        /// <summary>
        /// Gets the metadata for a specific version of a package.
        /// </summary>
        /// <param name="packageInfo">Name and version of a package</param>
        /// <returns>Package version metadata</returns>
        internal PackageVersion GetPackageVersionHeader(IPackageInfo packageInfo)
        {
            var req = new HeaderVersionDownload("dynamo", packageInfo.Name, packageInfo.Version.ToString());
            var pkgResponse = this.client.ExecuteAndDeserializeWithContent<PackageVersion>(req);
            if (!pkgResponse.success)
            {
                throw new ApplicationException(pkgResponse.message);
            }
            return pkgResponse.content;
        }

        /// <summary>
        /// Gets the metadata for a specific version of a package.
        /// </summary>
        /// <param name="id">Name and version of a package</param>
        /// <param name="version"></param>
        /// <returns>Package version metadata</returns>
        internal virtual PackageVersion GetPackageVersionHeader(string id, string version)
        {
            var req = new HeaderVersionDownload(id, version);
            var pkgResponse = this.client.ExecuteAndDeserializeWithContent<PackageVersion>(req);
            if (!pkgResponse.success)
            {
                throw new ApplicationException(pkgResponse.message);
            }
            return pkgResponse.content;
        }

        /// <summary>
        /// Make a call to Package Manager to get the known
        /// supported hosts for package publishing and filtering
        /// </summary>
        internal virtual IEnumerable<string> GetKnownHosts()
        {
            if (cachedHosts == null)
            {
                cachedHosts = FailFunc.TryExecute(() =>
                {
                    var hosts = new Hosts();
                    var hostsResponse = this.client.ExecuteAndDeserializeWithContent<List<String>>(hosts);
                    return hostsResponse.content;
                }, new List<string>());
            }
            return cachedHosts;
        }

        internal bool GetTermsOfUseAcceptanceStatus()
        {
            return ExecuteTermsOfUseCall(true);
        }

        public bool SetTermsOfUseAcceptanceStatus()
        {
            return ExecuteTermsOfUseCall(false);
        }

        private bool ExecuteTermsOfUseCall(bool queryAcceptanceStatus)
        {
            return FailFunc.TryExecute(() =>
            {
                var request = new TermsOfUse(queryAcceptanceStatus);
                var response = client.ExecuteAndDeserializeWithContent<TermsOfUseStatus>(request);
                return response.content.accepted;
            }, false);
        }

        /// <summary>
        ///     Asynchronously publishes a package along with associated files and markdown files.
        /// We use the 'roots' collection as a guide to the folders we expect to find in the root directory of the package.
        /// Based on that, we either want to nest inside a new package folder (if more than 2 root folders are found)
        /// or if just 1 root folder is found, then use that as the new package folder
        /// </summary>
        /// <param name="package">The package to be published.</param>
        /// <param name="files">The files to be included in the package.</param>
        /// <param name="markdownFiles">The markdown files to be included in the package.</param>
        /// <param name="isNewVersion">Indicates if this is a new version of an existing package.</param>
        /// <param name="roots">A collection of root directories to normalize file paths against.</param>
        /// <param name="retainFolderStructure">Indicates whether to retain the original folder structure.</param>
        /// <returns>A <see cref="PackageUploadHandle"/> Used to track the upload status.</returns>
        internal PackageUploadHandle PublishAsync(Package package, object files, IEnumerable<string> markdownFiles, bool isNewVersion, IEnumerable<string> roots, bool retainFolderStructure)
        {
            var packageUploadHandle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(package));

            Task.Factory.StartNew(() =>
            {
                Publish(package, files, markdownFiles, isNewVersion, packageUploadHandle, roots, retainFolderStructure);
            });

            return packageUploadHandle;
        }

        internal void Publish(Package package, object files, IEnumerable<string> markdownFiles, bool isNewVersion, PackageUploadHandle packageUploadHandle, IEnumerable<string> roots, bool retainFolderStructure = false)
        {
            try
            {
                ResponseBody ret = null;
                if (isNewVersion)
                {
                    var pkg = retainFolderStructure ?
                        uploadBuilder.NewPackageVersionRetainUpload(package, packageUploadDirectory, roots, (IEnumerable<IEnumerable<string>>)files, markdownFiles,
                        packageUploadHandle)
                        : uploadBuilder.NewPackageVersionUpload(package, packageUploadDirectory, (IEnumerable<string>)files, markdownFiles,
                        packageUploadHandle);
                    packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                    ret = this.client.ExecuteAndDeserialize(pkg);
                }
                else
                {
                    var pkg = retainFolderStructure ?
                        uploadBuilder.NewPackageRetainUpload(package, packageUploadDirectory, roots, (IEnumerable<IEnumerable<string>>)files, markdownFiles,
                        packageUploadHandle)
                        : uploadBuilder.NewPackageUpload(package, packageUploadDirectory, (IEnumerable<string>)files, markdownFiles,
                        packageUploadHandle);
                    packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                    ret = this.client.ExecuteAndDeserialize(pkg);
                }
                if (ret == null)
                {
                    packageUploadHandle.Error("Failed to submit.  Try again later.");
                    return;
                }

                if (ret != null && !ret.success)
                {
                    packageUploadHandle.Error(ret.message);
                    return;
                }
                packageUploadHandle.Done(null);
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is UnauthorizedAccessException)
                {
                    packageUploadHandle.Error(DynamoPackages.Properties.Resources.CannotRemovePackageAssemblyTitle + ": " + DynamoPackages.Properties.Resources.CannotRemovePackageAssemblyMessage + "(" + ex.Message + ")");
                }
                else
                {
                    packageUploadHandle.Error(ex.GetType() + ": " + ex.Message);
                }
            }
        }

        internal PackageManagerResult Deprecate(string name)
        {
            return FailFunc.TryExecute(() =>
            {
                var pkgResponse = this.client.ExecuteAndDeserialize(new Deprecate(name, PackageEngineName));
                return new PackageManagerResult(pkgResponse.message, pkgResponse.success);
            }, new PackageManagerResult("Failed to send.", false));
        }

        internal PackageManagerResult Undeprecate(string name)
        {
            return FailFunc.TryExecute(() =>
            {
                var pkgResponse = this.client.ExecuteAndDeserialize(new Undeprecate(name, PackageEngineName));
                return new PackageManagerResult(pkgResponse.message, pkgResponse.success);
            }, new PackageManagerResult("Failed to send.", false));
        }

        internal bool DoesCurrentUserOwnPackage(Package package,string username) 
        {
            bool value;
            if (packageMaintainers.Count > 0 && packageMaintainers.TryGetValue(package.Name, out value)) {
                return value;
            }
            var pkg = new PackageInfo(package.Name, new Version(package.VersionName));
            var mnt = GetPackageMaintainers(pkg);
            value = (mnt != null) && (mnt.maintainers.Any(maintainer => maintainer.username.Equals(username)));
            packageMaintainers[package.Name] = value;
            return value;
        }


        #region Compatibility Map

        // Store the compatibility map as a static property
        private static Dictionary<string, Dictionary<string, string>> compatibilityMap;

        /// <summary>
        /// A static access to the CompatibilityMap
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, Dictionary<string, string>> GetCompatibilityMap()
        {
            return compatibilityMap;
        }

        // Store the full compatibility map
        private static List<JObject> compatibilityMapList;
        /// <summary>
        /// A static access to the full Compatibility Matrix list (including Dynamo)
        /// Used to extract hosts information
        /// </summary>
        /// <returns></returns>
        internal static List<JObject> CompatibilityMapList()
        {
            return compatibilityMapList;
        }

        /// <summary>
        /// Method to load the map once, making it accessible to all elements
        /// </summary>
        internal void LoadCompatibilityMap()
        {
            if (compatibilityMap == null)  // Load only if not already loaded
            {
                compatibilityMap = new Dictionary<string, Dictionary<string, string>>();

                var compatibilityMapList = CompatibilityMap();
                PackageManagerClient.compatibilityMapList = compatibilityMapList;    // Loads the full CompatibilityMap as a side-effect

                foreach (var host in compatibilityMapList)
                {
                    foreach (var property in host.Properties())
                    {
                        string hostName = property.Name;
                        if (hostName.ToLower().Equals("dynamo")) continue;
                        var versionMapping = property.Value.ToObject<Dictionary<string, string>>();

                        if (!compatibilityMap.ContainsKey(hostName))
                        {
                            compatibilityMap[hostName] = new Dictionary<string, string>();
                        }

                        foreach (var version in versionMapping)
                        {
                            compatibilityMap[hostName][version.Key] = version.Value;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
