using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dynamo.Graph.Workspaces;
using Greg;
using Greg.Requests;
using Greg.Responses;
using System.Linq;

namespace Dynamo.PackageManager
{
    public class PackageManagerClient
    {
        #region Properties/Fields

        public const string PackageEngineName = "dynamo";

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
        ///     The directory where all packages are to be stored for this session.
        /// </summary>
        private readonly string packagesDirectory;
       
        /// <summary>
        ///     The URL of the package manager website
        /// </summary>
        public string BaseUrl
        {
            get { return this.client.BaseUrl; }
        }

        #endregion

        internal PackageManagerClient(IGregClient client, IPackageUploadBuilder builder, string packagesDirectory)
        {
            this.packagesDirectory = packagesDirectory;
            this.uploadBuilder = builder;
            this.client = client;
        }

        internal bool Upvote(string packageId)
        {
            return FailFunc.TryExecute(() =>
            {
                var pkgResponse = this.client.ExecuteAndDeserialize(new Upvote(packageId));
                return pkgResponse.success;
            }, false);
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
                return pkgResponse.content;
            }, new List<PackageHeader>());
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
        /// <param name="packageInfo">Name and version of a package</param>
        /// <returns>Package version metadata</returns>
        internal PackageVersion GetPackageVersionHeader(string id, string version)
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
        internal IEnumerable<string> GetKnownHosts()
        {
            return FailFunc.TryExecute(() =>
            {
                var hosts = new Hosts();
                var hostsResponse = this.client.ExecuteAndDeserializeWithContent<List<String>>(hosts);
                return hostsResponse.content;
            }, new List<string>());
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

        internal PackageUploadHandle PublishAsync(Package package, IEnumerable<string> files, bool isNewVersion)
        {
            var packageUploadHandle = new PackageUploadHandle(PackageUploadBuilder.NewRequestBody(package));

            Task.Factory.StartNew(() =>
            {
                Publish(package, files, isNewVersion, packageUploadHandle);
            });

            return packageUploadHandle;
        }

        internal void Publish(Package package, IEnumerable<string> files, bool isNewVersion, PackageUploadHandle packageUploadHandle)
        {
            try
            {
                ResponseBody ret = null;
                if (isNewVersion)
                {
                    var pkg = uploadBuilder.NewPackageVersionUpload(package, packagesDirectory, files,
                        packageUploadHandle);
                    packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                    ret = this.client.ExecuteAndDeserialize(pkg);
                }
                else
                {
                    var pkg = uploadBuilder.NewPackageUpload(package, packagesDirectory, files,
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

        [Obsolete("No longer used. Delete in 3.0")]
        internal PackageManagerResult DownloadPackageHeader(string id, out PackageHeader header)
        {
            var pkgDownload = new HeaderDownload(id);

            try
            {
                var response = this.client.ExecuteAndDeserializeWithContent<PackageHeader>(pkgDownload);
                if (!response.success) throw new Exception(response.message);
                header = response.content;
            }
            catch (Exception e)
            {
                var a = PackageManagerResult.Failed(e.Message);
                header = null;
                return a;
            }

            return new PackageManagerResult("", true);
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
            var pkg = new PackageInfo(package.Name, new Version(package.VersionName));
            var mnt = GetPackageMaintainers(pkg);
            return (mnt != null) && (mnt.maintainers.Any(maintainer => maintainer.username.Equals(username)));
        }
    }
}
