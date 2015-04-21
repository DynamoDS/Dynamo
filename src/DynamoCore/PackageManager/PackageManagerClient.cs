using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dynamo.Core;
using Dynamo.Utilities;
using Greg;
using Greg.AuthProviders;
using Greg.Requests;
using Greg.Responses;

namespace Dynamo.PackageManager
{
    public class PackageManagerClient
    {
        #region Properties/Fields

        // These were used early on in order to identify packages with binaries and python scripts
        // This is now a bona fide field in the DB so they are obsolete. 

        [Obsolete] internal static readonly string PackageContainsBinariesConstant =
            "|ContainsBinaries(5C698212-A139-4DDD-8657-1BF892C79821)";

        [Obsolete] internal static readonly string PackageContainsPythonScriptsConstant =
            "|ContainsPythonScripts(58B25C0B-CBBE-4DDC-AC39-ECBEB8B55B10)";

        private readonly IGregClient client;
        private readonly CustomNodeManager customNodeManager;
        private readonly string rootPackageDirectory;
        private readonly IAuthProvider authProvider;

        public event Action<LoginState> LoginStateChanged;

        /// <summary>
        ///     Specifies whether the user is logged in or not.
        /// </summary>
        public LoginState LoginState
        {
            get { return HasAuthProvider ? this.authProvider.LoginState : Greg.AuthProviders.LoginState.LoggedOut; }
        }

        /// <summary>
        ///     The username of the current user, if logged in.  Otherwise null
        /// </summary>
        public string Username
        {
            get { return HasAuthProvider ? this.authProvider.Username : ""; }
        }

        /// <summary>
        ///     The URL of the package manager website
        /// </summary>
        public string BaseUrl
        {
            get { return this.client.BaseUrl; }
        }

        /// <summary>
        ///     Determines if the this.client has login capabilities
        /// </summary>
        public bool HasAuthProvider
        {
            get { return authProvider != null; }
        }

        #endregion

        internal PackageManagerClient(IGregClient client, string rootPackageDirectory, CustomNodeManager customNodeManager)
        {
            this.rootPackageDirectory = rootPackageDirectory;
            this.customNodeManager = customNodeManager;
            this.client = client;
            this.authProvider = this.client.AuthProvider;

            if (this.authProvider != null)
            {
                this.authProvider.LoginStateChanged += OnLoginStateChanged;
            }
        }

        private void OnLoginStateChanged(LoginState status)
        {
            if (LoginStateChanged != null)
            {
                LoginStateChanged(status);
            }
        }

        internal bool Upvote(string packageId)
        {
            return FailFunc.TryExecute(() =>
            {
                var pkgResponse = this.client.ExecuteAndDeserialize(new Upvote(packageId));
                return pkgResponse.success;
            }, false);
        }

        internal bool Downvote(string packageId)
        {
            return FailFunc.TryExecute(() =>
            {
                var pkgResponse = this.client.ExecuteAndDeserialize(new Downvote(packageId));
                return pkgResponse.success;
            }, false);
        }

        internal string DownloadPackage(string packageId, string version)
        {
            var response = this.client.Execute( new PackageDownload( packageId, version) );
            return PackageDownload.GetFileFromResponse(response);
        }

        internal IEnumerable<PackageHeader> ListAll()
        {
            return FailFunc.TryExecute(() => {
                var nv = HeaderCollectionDownload.ByEngine("dynamo");
                var pkgResponse = this.client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
                return pkgResponse.content;
            }, new List<PackageHeader>());
        }

        internal bool GetTermsOfUseAcceptanceStatus()
        {
            return ExecuteTermsOfUseCall(true);
        }

        internal bool SetTermsOfUseAcceptanceStatus()
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

        internal PackageUploadHandle Publish(Package package, List<string> files, bool isNewVersion, bool isTestMode)
        {
            var packageUploadHandle = new PackageUploadHandle(PackageUploadBuilder.NewPackageHeader(package));
            return PublishPackage(isNewVersion, package, files, packageUploadHandle, isTestMode);
        }

        private PackageUploadHandle PublishPackage(bool isNewVersion, Package package, List<string> files,
            PackageUploadHandle packageUploadHandle, bool isTestMode)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ResponseBody ret = null;
                    if (isNewVersion)
                    {
                        var pkg = PackageUploadBuilder.NewPackageVersion(this.rootPackageDirectory, this.customNodeManager, package, files,
                            packageUploadHandle, isTestMode);
                        packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                        ret = this.client.ExecuteAndDeserialize(pkg);
                    }
                    else
                    {
                        var pkg = PackageUploadBuilder.NewPackage(this.rootPackageDirectory, this.customNodeManager, package, files,
                            packageUploadHandle, isTestMode);
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
            });

            return packageUploadHandle;
        }

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
                var pkgResponse = this.client.ExecuteAndDeserialize(new Deprecate(name, "dynamo"));
                return new PackageManagerResult(pkgResponse.message, pkgResponse.success);
            }, new PackageManagerResult("Failed to send.", false));
        }

        internal PackageManagerResult Undeprecate(string name)
        {
            return FailFunc.TryExecute(() =>
            {
                var pkgResponse = this.client.ExecuteAndDeserialize(new Undeprecate(name, "dynamo"));
                return new PackageManagerResult(pkgResponse.message, pkgResponse.success);
            }, new PackageManagerResult("Failed to send.", false));
        }

        internal void Logout()
        {
            if (!HasAuthProvider) return; 
            this.authProvider.Logout();
        }

        internal void Login()
        {
            if (!HasAuthProvider) return;
            this.authProvider.Login();
        }
    }
}
