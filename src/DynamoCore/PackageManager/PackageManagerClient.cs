using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;

using Dynamo.Models;
using Dynamo.Utilities;

using Greg;
using Greg.Requests;
using Greg.Responses;

namespace Dynamo.PackageManager
{
    public class LoginStateEventArgs : EventArgs
    {
        public string Text { get; set; }
        public bool Enabled { get; set; }

        public LoginStateEventArgs(string text, bool enabled)
        {
            Text = text;
            Enabled = enabled;
        }
    }

    /// <summary>
    ///     A thin wrapper on the Greg rest client for performing IO with
    ///     the Package Manager
    /// </summary>
    public class PackageManagerClient
    {

        #region Events

        internal delegate void RequestAuthenticationHandler(PackageManagerClient sender);
        internal event RequestAuthenticationHandler RequestAuthentication;
        private void OnRequestAuthentication()
        {
            if (RequestAuthentication != null)
            {
                RequestAuthentication(this);
            }
        }

        #endregion

        #region Properties/Fields

        private readonly string rootPkgDir;
        private readonly CustomNodeManager customNodeManager;

        [Obsolete]
        internal readonly static string PackageContainsBinariesConstant = "|ContainsBinaries(5C698212-A139-4DDD-8657-1BF892C79821)";

        [Obsolete]
        internal readonly static string PackageContainsPythonScriptsConstant = "|ContainsPythonScripts(58B25C0B-CBBE-4DDC-AC39-ECBEB8B55B10)";


        public bool HasAuthenticator
        {
            get { return this.RequestAuthentication != null; }
        }

        /// <summary>
        ///     Client property
        /// </summary>
        /// <value>
        ///     The client for the Package Manager
        /// </value>
        public Client Client { get; internal set; }

        /// <summary>
        ///     IsLoggedIn property
        /// </summary>
        /// <value>
        ///     Specifies whether the user is logged in or not.
        /// </value>
        public bool LoggedIn {
            get
            {
                this.OnRequestAuthentication(); 

                try
                {
                    return (Client.Provider as dynamic).LoggedIn;
                } 
                catch
                {
                    return false;
                }
            } 
        }

        /// <summary>
        /// The username of the current user, if logged in.  Otherwise null
        /// </summary>
        public string Username
        {
            get
            {
                this.OnRequestAuthentication();

                try
                {
                    return (Client.Provider as dynamic).Username;
                }
                catch
                {
                    return null;
                }
            }
        }

        #endregion

        private static readonly string serverUrl = "https://www.dynamopackages.com/";
        
        public PackageManagerClient(string rootPkgDir,  CustomNodeManager customNodeManager)
        {
            this.rootPkgDir = rootPkgDir;
            this.customNodeManager = customNodeManager;

            Client = new Client(null, "http://www.dynamopackages.com");
        }

        //public bool IsNewestVersion(string packageId, string currentVersion, ref string newerVersion )
        //{
        //    var searchEle = CachedPackageList.FirstOrDefault(x => x.Id == packageId);
            
        //    PackageHeader header = null;
        //    if (searchEle != null)
        //    {
        //        header = searchEle.Header;
        //    }

        //    if (header == null)
        //    {
        //        DownloadPackageHeader(packageId, out header);
        //    }

        //    if (header == null)
        //    {
        //        return false;
        //    }

        //    return !Greg.Utility.PackageUtilities.IsNewerVersion(currentVersion, header._id);
        //}

        //public bool IsUserPackageOwner(string packageId)
        //{
        //    if (!LoggedIn) return false;
        //    var un = this.Username;

        //    if (un == null) return false;

        //    if (CachedPackageList.Any(x => x.Id == packageId && x.Maintainers.Contains(un)))
        //    {
        //        return true;
        //    }

        //    var l = ListAll();
        //    return l.Any(x => x.Id == packageId && x.Maintainers.Contains(un));

        //}

        public bool Upvote(string packageId)
        {
            this.OnRequestAuthentication();

            try
            {
                var nv = new Greg.Requests.Upvote(packageId);
                var pkgResponse = Client.ExecuteAndDeserialize(nv);
                return pkgResponse.success;
            }
            catch
            {
                return false;
            }
        }

        public bool Downvote(string packageId)
        {
            this.OnRequestAuthentication();

            try
            {
                var nv = new Greg.Requests.Downvote(packageId);
                var pkgResponse = Client.ExecuteAndDeserialize(nv);
                return pkgResponse.success;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<PackageHeader> ListAll()
        {
            try
            {
                var nv = Greg.Requests.HeaderCollectionDownload.ByEngine("dynamo");
                var pkgResponse = Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
                return pkgResponse.content;
            }
            catch
            {
                return new List<PackageHeader>();
            }
        }

        public IEnumerable<PackageHeader> Search(string search, int maxNumSearchResults)
        {
            try
            {
                var nv = new Greg.Requests.Search(search);
                var pkgResponse = Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
                return
                    pkgResponse.content.GetRange(0, Math.Min(maxNumSearchResults, pkgResponse.content.Count()));
            }
            catch
            {
                return new List<PackageHeader>();
            }
        }

        public PackageUploadHandle Publish(Package l, List<string> files, bool isNewVersion, bool isTestMode)
        {
            this.OnRequestAuthentication();

            var nv = new ValidateAuth();
            var pkgResponse = Client.ExecuteAndDeserialize(nv);

            if (pkgResponse == null)
            {
                throw new AuthenticationException(
                    "It looks like you're not logged into Autodesk 360.  Log in to submit a package.");
            }

            var packageUploadHandle = new PackageUploadHandle(PackageUploadBuilder.NewPackageHeader(l));
            return PublishPackage(isNewVersion, l, files, packageUploadHandle, isTestMode);

        }

        private PackageUploadHandle PublishPackage(bool isNewVersion, Package l, List<string> files, PackageUploadHandle packageUploadHandle, bool isTestMode)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ResponseBody ret = null;
                    if (isNewVersion)
                    {
                        var pkg = PackageUploadBuilder.NewPackageVersion(rootPkgDir, customNodeManager, l, files, packageUploadHandle, isTestMode);
                        packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                        ret = Client.ExecuteAndDeserialize(pkg);
                    }
                    else
                    {
                        var pkg = PackageUploadBuilder.NewPackage(rootPkgDir, customNodeManager, l, files, packageUploadHandle, isTestMode);
                        packageUploadHandle.UploadState = PackageUploadHandle.State.Uploading;
                        ret = Client.ExecuteAndDeserialize(pkg);
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

        public class PackageManagerResult
        {
            public PackageManagerResult(string error, bool success)
            {
                Error = error;
                Success = success;
            }

            public static PackageManagerResult Succeeded()
            {
                return new PackageManagerResult("", true);
            }

            public static PackageManagerResult Failed(string error)
            {
                return new PackageManagerResult(error, false);
            }

            public string Error { get; set; }
            public bool Success { get; set; }
        }

        /// <summary>
        ///     Synchronously download a package header
        /// </summary>
        /// <param name="id"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public PackageManagerResult DownloadPackageHeader(string id, out PackageHeader header)
        {
            var pkgDownload = new HeaderDownload(id);
            
            try
            {
                var response = Client.ExecuteAndDeserializeWithContent<PackageHeader>(pkgDownload);
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
            this.OnRequestAuthentication();

            try
            {
                var nv = new Greg.Requests.Deprecate(name, "dynamo");
                var pkgResponse = Client.ExecuteAndDeserialize(nv);
                return new PackageManagerResult(pkgResponse.message, pkgResponse.success);
            }
            catch
            {
                return new PackageManagerResult("Failed to send.", false);
            }
        }

        internal PackageManagerResult Undeprecate(string name)
        {
            this.OnRequestAuthentication();

            try
            {
                var nv = new Greg.Requests.Undeprecate(name, "dynamo");
                var pkgResponse = Client.ExecuteAndDeserialize(nv);
                return new PackageManagerResult(pkgResponse.message, pkgResponse.success);
            }
            catch
            {
                return new PackageManagerResult("Failed to send.", false);
            }
        }

    }


}
