using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dynamo.Core;

using Greg;
using Greg.AuthProviders;
using Greg.Requests;
using Greg.Responses;

namespace Dynamo.PackageManager
{
    public class PackageManagerClient
    {
        #region Properties/Fields

        [Obsolete] internal static readonly string PackageContainsBinariesConstant =
            "|ContainsBinaries(5C698212-A139-4DDD-8657-1BF892C79821)";

        [Obsolete] internal static readonly string PackageContainsPythonScriptsConstant =
            "|ContainsPythonScripts(58B25C0B-CBBE-4DDC-AC39-ECBEB8B55B10)";

        private readonly IGregClient _client;
        private readonly CustomNodeManager _customNodeManager;
        private readonly string _rootPkgDir;
        private readonly IAuthProvider _authProvider;

        public event Action<LoginState> LoginStateChanged;

        /// <summary>
        ///     Specifies whether the user is logged in or not.
        /// </summary>
        public LoginState LoginState
        {
            get { return HasAuthProvider ? _authProvider.LoginState : Greg.AuthProviders.LoginState.LoggedOut; }
        }

        /// <summary>
        ///     The username of the current user, if logged in.  Otherwise null
        /// </summary>
        public string Username
        {
            get { return HasAuthProvider ? _authProvider.Username : ""; }
        }

        /// <summary>
        ///     The URL of the package manager website
        /// </summary>
        public string BaseUrl
        {
            get { return _client.BaseUrl; }
        }

        /// <summary>
        ///     Determines if the client has login capabilities
        /// </summary>
        public bool HasAuthProvider
        {
            get { return _authProvider != null; }
        }

        #endregion

        internal PackageManagerClient(IGregClient client, string rootPkgDir, CustomNodeManager customNodeManager)
        {
            _rootPkgDir = rootPkgDir;
            _customNodeManager = customNodeManager;
            _client = client;
            _authProvider = client.AuthProvider;

            if (_authProvider != null)
            {
                _client.AuthProvider.LoginStateChanged += OnLoginStateChanged;
            }
        }

        private void OnLoginStateChanged(LoginState status)
        {
            if (LoginStateChanged != null)
            {
                LoginStateChanged(status);
            }
        }

        private T TryExecute<T>(Func<T> func, T failureResult)
        {
            try
            {
                return func();
            }
            catch
            {
                return failureResult;
            }
        }

        internal bool Upvote(string packageId)
        {
            return TryExecute(() =>
            {
                var pkgResponse = _client.ExecuteAndDeserialize(new Upvote(packageId));
                return pkgResponse.success;
            }, false);
        }

        internal bool Downvote(string packageId)
        {
            return TryExecute(() =>
            {
                var pkgResponse = _client.ExecuteAndDeserialize(new Downvote(packageId));
                return pkgResponse.success;
            }, false);
        }

        internal string DownloadPackage(string packageId, string version)
        {
            var response = _client.Execute( new PackageDownload( packageId, version) );
            return PackageDownload.GetFileFromResponse(response);
        }

        internal IEnumerable<PackageHeader> ListAll()
        {
            return TryExecute(() =>
            {
                var nv = HeaderCollectionDownload.ByEngine("dynamo");
                var pkgResponse = _client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
                return pkgResponse.content;
            }, new List<PackageHeader>());
        }

        internal PackageUploadHandle Publish(Package l, List<string> files, bool isNewVersion, bool isTestMode)
        {
            var packageUploadHandle = new PackageUploadHandle(PackageUploadBuilder.NewPackageHeader(l));
            return PublishPackage(isNewVersion, l, files, packageUploadHandle, isTestMode);
        }

        private PackageUploadHandle PublishPackage(bool isNewVersion, Package l, List<string> files,
            PackageUploadHandle packageUploadHandle, bool isTestMode)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ResponseBody ret = null;
                    if (isNewVersion)
                    {
                        var pkg = PackageUploadBuilder.NewPackageVersion(_rootPkgDir, _customNodeManager, l, files,
                            packageUploadHandle, isTestMode);
                        packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                        ret = _client.ExecuteAndDeserialize(pkg);
                    }
                    else
                    {
                        var pkg = PackageUploadBuilder.NewPackage(_rootPkgDir, _customNodeManager, l, files,
                            packageUploadHandle, isTestMode);
                        packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                        ret = _client.ExecuteAndDeserialize(pkg);
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
                var response = _client.ExecuteAndDeserializeWithContent<PackageHeader>(pkgDownload);
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
            return TryExecute(() =>
            {
                var pkgResponse = _client.ExecuteAndDeserialize(new Deprecate(name, "dynamo"));
                return new PackageManagerResult(pkgResponse.message, pkgResponse.success);
            }, new PackageManagerResult("Failed to send.", false));
        }

        internal PackageManagerResult Undeprecate(string name)
        {
            return TryExecute(() =>
            {
                var pkgResponse = _client.ExecuteAndDeserialize(new Undeprecate(name, "dynamo"));
                return new PackageManagerResult(pkgResponse.message, pkgResponse.success);
            }, new PackageManagerResult("Failed to send.", false));
        }

        internal void Logout()
        {
            if (!HasAuthProvider) return; 
            _authProvider.Logout();
        }

        internal void Login()
        {
            if (!HasAuthProvider) return;
            _authProvider.Login();
        }
    }
}
