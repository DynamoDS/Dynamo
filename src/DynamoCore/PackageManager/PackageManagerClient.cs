//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Greg;
using Greg.Requests;
using Greg.Responses;

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

        public event AuthenticationRequestHandler AuthenticationRequested;

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
        public bool IsLoggedIn { get; internal set; }

        #endregion

        /// <summary>
        ///     The class constructor.
        /// </summary>
        public PackageManagerClient()
        {
            Client = new Client(null, "http://54.225.121.251"); // initialize authenticator later
            IsLoggedIn = false;
        }

        public void OnAuthenticationRequested()
        {
            if (AuthenticationRequested != null)
                AuthenticationRequested(this);
        }

        internal List<PackageManagerSearchElement> Search(string search, int maxNumSearchResults)
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

        public PackageUploadHandle Publish( Package l, List<string> files, bool isNewVersion )
        {
            OnAuthenticationRequested();

            int maxRetries = 5;
            int count = 0;
            var nv = new ValidateAuth();
            ResponseBody pkgResponse = null;

            while (pkgResponse == null && count < maxRetries)
            {
                count++;
                pkgResponse = Client.ExecuteAndDeserialize(nv);
            }

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

            ThreadStart start = () =>
                {
                    try
                    {
                        int maxRetries = 5;
                        int count = 0;
                        ResponseBody ret = null;
                        if (isNewVersion)
                        {
                            var pkg = PackageUploadBuilder.NewPackageVersion(l, files, packageUploadHandle);
                            while (ret == null && count < maxRetries)
                            {
                                count++;
                                ret = Client.ExecuteAndDeserialize(pkg);
                            }
                        }
                        else
                        {
                            var pkg = PackageUploadBuilder.NewPackage(l, files, packageUploadHandle);
                            while (ret == null && count < maxRetries)
                            {
                                count++;
                                ret = Client.ExecuteAndDeserialize(pkg);
                            }
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
                };
            new Thread(start).Start();

            return packageUploadHandle;

        }

        ObservableCollection<PackageDownloadHandle> _downloads = new ObservableCollection<PackageDownloadHandle>();
        public ObservableCollection<PackageDownloadHandle> Downloads
        {
            get { return _downloads; }
            set { _downloads = value; }
        }

        internal void ClearInstalled()
        {
            foreach (
                var ele in Downloads.Where((x) => x.DownloadState == PackageDownloadHandle.State.Installed).ToList())
            {
                Downloads.Remove(ele);
            }
        }

        internal void DownloadAndInstall(PackageDownloadHandle packageDownloadHandle)
        {

            var pkgDownload = new PackageDownload(packageDownloadHandle.Header._id, packageDownloadHandle.VersionName);
            Downloads.Add( packageDownloadHandle );

            ThreadStart start = () =>
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
            };
            new Thread(start).Start();
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
