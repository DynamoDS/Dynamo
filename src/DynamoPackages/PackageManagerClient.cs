using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamo.Graph.Workspaces;
using Greg;
using Greg.Requests;
using Greg.Responses;

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

        #endregion

        internal PackageManagerClient(IGregClient client, IPackageUploadBuilder builder, string packageUploadDirectory)
        {
            this.packageUploadDirectory = packageUploadDirectory;
            this.uploadBuilder = builder;
            this.client = client;
            this.packageMaintainers = new Dictionary<string, bool>();
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
                return pkgResponse.content;
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

        internal PackageUploadHandle PublishAsync(Package package, object files, IEnumerable<string> markdownFiles, bool isNewVersion, bool retainFolderStructure)
        {
            var packageUploadHandle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(package));

            Task.Factory.StartNew(() =>
            {
                Publish(package, files, markdownFiles, isNewVersion, packageUploadHandle, retainFolderStructure);
            });

            return packageUploadHandle;
        }

        internal void Publish(Package package, object files, IEnumerable<string> markdownFiles, bool isNewVersion, PackageUploadHandle packageUploadHandle, bool retainFolderStructure = false)
        {
            try
            {
                ResponseBody ret = null;
                if (isNewVersion)
                {
                    var pkg = retainFolderStructure ?
                        uploadBuilder.NewPackageVersionRetainUpload(package, packageUploadDirectory, (IEnumerable<IEnumerable<string>>)files, markdownFiles,
                        packageUploadHandle)
                        : uploadBuilder.NewPackageVersionUpload(package, packageUploadDirectory, (IEnumerable<string>)files, markdownFiles,
                        packageUploadHandle);
                    packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                    ret = this.client.ExecuteAndDeserialize(pkg);
                }
                else
                {
                    var pkg = retainFolderStructure ?
                        uploadBuilder.NewPackageRetainUpload(package, packageUploadDirectory, (IEnumerable<IEnumerable<string>>)files, markdownFiles,
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
            catch (Exception e)
            {
                packageUploadHandle.Error(e.GetType() + ": " + e.Message);
            }
        }

        /// <summary>
        /// This method allows the user to publish a package retaining their predefined folder structure
        /// In this case, Dynamo will not allocate files in specific folders, but instead will replicate the folder structure under the chosen publish path
        /// </summary>
        /// <param name="package">The newly created package</param>
        /// <param name="files">List of folders. Each list of lists represents a root folder. There can be one or many root folders.</param>
        /// <param name="markdownFiles">Any files located in the user specified markdown folder.</param>
        /// <param name="isNewVersion">A boolean showing if this is a new package, or an update to an existing package.</param>
        /// <param name="packageUploadHandle">The PackageUploadHandle used to communicate the status of the upload.</param>
        internal void PublishRetainFolderStructure(Package package, IEnumerable<IEnumerable<string>> files, IEnumerable<string> markdownFiles, bool isNewVersion, PackageUploadHandle packageUploadHandle)
        {
            try
            {
                ResponseBody ret = null;
                if (isNewVersion)
                {
                    var pkg = uploadBuilder.NewPackageVersionRetainUpload(package, packageUploadDirectory, files, markdownFiles,
                        packageUploadHandle);
                    packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                    ret = this.client.ExecuteAndDeserialize(pkg);
                }
                else
                {
                    var pkg = uploadBuilder.NewPackageRetainUpload(package, packageUploadDirectory, files, markdownFiles,
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
            catch (Exception e)
            {
                packageUploadHandle.Error(e.GetType() + ": " + e.Message);
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
            if (this.packageMaintainers.Count > 0 && this.packageMaintainers.TryGetValue(package.Name, out value)) {
                return value;
            }
            var pkg = new PackageInfo(package.Name, new Version(package.VersionName));
            var mnt = GetPackageMaintainers(pkg);
            value = (mnt != null) && (mnt.maintainers.Any(maintainer => maintainer.username.Equals(username)));
            this.packageMaintainers[package.Name] = value;
            return value;
        }
    }
}
