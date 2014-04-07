using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Greg;
using Greg.Requests;
using Greg.Responses;
using Greg.Utility;

namespace Dynamo.PackageManager
{

    public delegate void AuthenticationRequestHandler(PackageManagerClient sender);

    /// <summary>
    ///     A thin wrapper on the Greg rest client for performing IO with
    ///     the Package Manager
    /// </summary>
    public class PackageManagerClient
    {

        /// <summary>
        /// Indicates whether we should look for login information
        /// </summary>
        public static bool DEBUG_MODE = false;

        #region Properties

        /// <summary>
        /// A cached version of the package list.  Updated by ListAll()
        /// </summary>
        public List<PackageManagerSearchElement> CachedPackageList { get; private set; }

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
                dynSettings.Controller.DynamoViewModel.OnRequestAuthentication(); 

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
                dynSettings.Controller.DynamoViewModel.OnRequestAuthentication();

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

        public PackageManagerClient()
        {
            Client = new Client(null, "http://54.225.121.251"); 
            this.CachedPackageList = new List<PackageManagerSearchElement>();
        }

        #region Under construction

        public bool IsNewestVersion(string packageId, string currentVersion, ref string newerVersion )
        {
            var searchEle = CachedPackageList.FirstOrDefault(x => x.Id == packageId);
            
            PackageHeader header = null;
            if (searchEle != null)
            {
                header = searchEle.Header;
            }

            if (header == null)
            {
                DownloadPackageHeader(packageId, out header);
            }

            if (header == null)
            {
                return false;
            }

            return !PackageUtilities.IsNewerVersion(currentVersion, header._id);
        }

        public bool IsUserPackageOwner(string packageId)
        {
            if (!LoggedIn) return false;
            var un = this.Username;

            if (un == null) return false;

            if (CachedPackageList.Any(x => x.Id == packageId && x.Maintainers.Contains(un)))
            {
                return true;
            }

            var l = ListAll();
            return l.Any(x => x.Id == packageId && x.Maintainers.Contains(un));

        }

        #endregion

        public bool Upvote(string packageId)
        {
            dynSettings.Controller.DynamoViewModel.OnRequestAuthentication();

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
            dynSettings.Controller.DynamoViewModel.OnRequestAuthentication();

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

        public List<PackageManagerSearchElement> ListAll()
        {
            try
            {
                var nv = Greg.Requests.HeaderCollectionDownload.ByEngine("dynamo");
                var pkgResponse = Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
                this.CachedPackageList = 
                    pkgResponse.content
                               .Select((header) => new PackageManagerSearchElement(header))
                               .ToList();

                return CachedPackageList;
            }
            catch
            {
                return CachedPackageList;
            }
        }

        public List<PackageManagerSearchElement> Search(string search, int maxNumSearchResults)
        {
            try
            {
                var nv = new Greg.Requests.Search(search);
                var pkgResponse = Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
                return
                    pkgResponse.content.GetRange(0, Math.Min(maxNumSearchResults, pkgResponse.content.Count()))
                               .Select((header) => new PackageManagerSearchElement(header))
                               .ToList();
            }
            catch
            {
                return new List<PackageManagerSearchElement>();
            }
            
        }

        public void PublishCurrentWorkspace()
        {
            var currentFunDef =
                dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(dynSettings.Controller.DynamoViewModel.CurrentSpace);

            if (currentFunDef != null)
            {
                ShowNodePublishInfo(new List<CustomNodeDefinition> { currentFunDef });
            }
            else
            {
                MessageBox.Show("The selected symbol was not found in the workspace", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);
            }

        }

        public bool CanPublishCurrentWorkspace()
        {
            return dynSettings.Controller.DynamoViewModel.CurrentSpace is CustomNodeWorkspaceModel;
        }

        public void PublishSelectedNode()
        {
            var nodeList = DynamoSelection.Instance.Selection
                                .Where(x => x is Function)
                                .Cast<Function>()
                                .Select(x => x.Definition.FunctionId)
                                .ToList();

            if (!nodeList.Any())
            {
                MessageBox.Show("You must select at least one custom node.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);
                return;
            }

            var defs = nodeList.Select(dynSettings.CustomNodeManager.GetFunctionDefinition).ToList();

            if (defs.Any(x => x == null))
                MessageBox.Show("There was a problem getting the node from the workspace.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Question);

            ShowNodePublishInfo(defs);
        }

        public bool CanPublishSelectedNode(object m)
        {
            return DynamoSelection.Instance.Selection.Count > 0 &&
                   DynamoSelection.Instance.Selection.All(x => x is Function);
        }

        private void ShowNodePublishInfo(object funcDef)
        {
            if (funcDef is List<CustomNodeDefinition>)
            {
                var fs = funcDef as List<CustomNodeDefinition>;

                foreach (var f in fs)
                {
                    var pkg = dynSettings.PackageLoader.GetOwnerPackage(f);

                    if (dynSettings.PackageLoader.GetOwnerPackage(f) != null)
                    {
                        var m = MessageBox.Show("The node is part of the dynamo package called \"" + pkg.Name +
                            "\" - do you want to submit a new version of this package?  \n\nIf not, this node will be moved to the new package you are creating.",
                            "Package Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (m == MessageBoxResult.Yes)
                        {
                            pkg.PublishNewPackageVersionCommand.Execute();
                            return;
                        }
                    }
                }

                var newPkgVm = new PublishPackageViewModel(dynSettings.PackageManagerClient);
                newPkgVm.FunctionDefinitions = fs;
                dynSettings.Controller.DynamoViewModel.OnRequestPackagePublishDialog(newPkgVm);
            }
            else
            {
                DynamoLogger.Instance.Log("Failed to obtain function definition from node.");
                return;
            }
        }

        public PackageUploadHandle Publish( Package l, List<string> files, bool isNewVersion )
        {
            dynSettings.Controller.DynamoViewModel.OnRequestAuthentication();

            var nv = new ValidateAuth();
            var pkgResponse = Client.ExecuteAndDeserialize(nv);

            if (pkgResponse == null)
            {
                throw new AuthenticationException(
                    "It looks like you're not logged into Autodesk 360.  Log in to submit a package.");
            }

            var packageUploadHandle = new PackageUploadHandle(l.Header);
            return PublishPackage(isNewVersion, l, files, packageUploadHandle);

        }

        ObservableCollection<PackageUploadHandle> _uploads = new ObservableCollection<PackageUploadHandle>();
        public ObservableCollection<PackageUploadHandle> Uploads
        {
            get { return _uploads; }
            set { _uploads = value; }
        }

        private PackageUploadHandle PublishPackage( bool isNewVersion, 
                                                    Package l, 
                                                    List<string> files,
                                                    PackageUploadHandle packageUploadHandle )
        {

            Task.Factory.StartNew(() =>
            {
                try
                {
                    ResponseBody ret = null;
                    if (isNewVersion)
                    {
                        var pkg = PackageUploadBuilder.NewPackageVersion(l, files, packageUploadHandle);
                        ret = Client.ExecuteAndDeserialize(pkg);
                    }
                    else
                    {
                        var pkg = PackageUploadBuilder.NewPackage(l, files, packageUploadHandle);
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

        ObservableCollection<PackageDownloadHandle> _downloads = new ObservableCollection<PackageDownloadHandle>();
        public ObservableCollection<PackageDownloadHandle> Downloads
        {
            get { return _downloads; }
            set { _downloads = value; }
        }

        public void ClearCompletedDownloads()
        {
            Downloads.Where((x) => x.DownloadState == PackageDownloadHandle.State.Installed ||
                x.DownloadState == PackageDownloadHandle.State.Error).ToList().ForEach(x=>Downloads.Remove(x));
        }

        internal void DownloadAndInstall(PackageDownloadHandle packageDownloadHandle)
        {

            var pkgDownload = new PackageDownload(packageDownloadHandle.Header._id, packageDownloadHandle.VersionName);
            Downloads.Add( packageDownloadHandle );

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var response = Client.Execute(pkgDownload);
                    var pathDl = PackageDownload.GetFileFromResponse(response);

                    dynSettings.Controller.UIDispatcher.BeginInvoke((Action) (() =>
                        {
                            try
                            {
                                packageDownloadHandle.Done(pathDl);

                                Package dynPkg;

                                var firstOrDefault = dynSettings.PackageLoader.LocalPackages.FirstOrDefault(pkg => pkg.Name == packageDownloadHandle.Name);
                                if (firstOrDefault != null)
                                    firstOrDefault.UninstallCommand.Execute();

                                if (packageDownloadHandle.Extract(out dynPkg))
                                {

                                    var downloadPkg = Package.FromDirectory(dynPkg.RootDirectory);
                                    downloadPkg.Load();
                                    dynSettings.PackageLoader.LocalPackages.Add(downloadPkg);
                                    packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;

                                }
                            }
                            catch (Exception e)
                            {
                                packageDownloadHandle.Error(e.Message);
                            }
                        }));
                    
                }
                catch (Exception e)
                {
                    packageDownloadHandle.Error(e.Message);
                }
            });

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

        internal void GoToWebsite()
        {
            Process.Start(Client.BaseUrl);
        }

        internal PackageManagerResult Deprecate(string name)
        {
            dynSettings.Controller.DynamoViewModel.OnRequestAuthentication();

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
            dynSettings.Controller.DynamoViewModel.OnRequestAuthentication();

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
}
