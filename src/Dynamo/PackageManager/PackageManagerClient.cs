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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Greg;
using Greg.Requests;
using Greg.Responses;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace Dynamo.PackageManager
{
    /// <summary>
    ///     A thin wrapper on the Greg rest client for performing IO with
    ///     the Package Manager
    /// </summary>
    public class PackageManagerClient
    {

        /// <summary>
        /// Indicates whether we should look for login information
        /// </summary>
        public static bool DEBUG_MODE = true;

        #region Properties

        /// <summary>
        ///     Controller property
        /// </summary>
        /// <value>
        ///     Reference to the main DynamoController
        /// </value>
        private readonly DynamoController Controller;

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

        /// <summary>
        ///     Worker property
        /// </summary>
        /// <value>
        ///     Helps to do asynchronous calls to the server
        /// </value>
        public BackgroundWorker Worker { get; internal set; }

        /// <summary>
        ///     LoadedPackageHeaders property
        /// </summary>
        /// <value>
        ///     Tells which package headers are currently loaded
        /// </value>
        public Dictionary<FunctionDefinition, PackageUploadRequestBody> LoadedPackageHeaders { get; internal set; }

        #endregion

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="controller"> Reference to to the DynamoController object for the app </param>
        public PackageManagerClient(DynamoController controller)
        {
            Controller = controller;

            //IAuthProvider provider = new RevitOxygenProvider(Autodesk.Revit.AdWebServicesBase.GetInstance());

            LoadedPackageHeaders = new Dictionary<FunctionDefinition, PackageUploadRequestBody>();
            Client = new Client(null, "http://54.225.215.247");

            Worker = new BackgroundWorker();
            IsLoggedIn = false;
        }

        ///// <summary>
        /////     Asynchronously pull the package headers from the server and update search
        ///// </summary>
        //public void RefreshAvailable()
        //{
        //    ThreadStart start = () =>
        //        {
        //            HeaderCollectionDownload req = HeaderCollectionDownload.ByEngine("dynamo");

        //            try
        //            {
        //                ResponseWithContentBody<List<PackageUploadRequestBody>> response =
        //                    Client.ExecuteAndDeserializeWithContent<List<PackageUploadRequestBody>>(req);
        //                if (response.success)
        //                {
        //                    dynSettings.Bench.Dispatcher.BeginInvoke((Action)(() =>
        //                        {
        //                            foreach (PackageUploadRequestBody header in response.content)
        //                            {
        //                                dynSettings.Controller.SearchViewModel.Add(header);
        //                            }
        //                        }));
        //                }
        //            }
        //            catch
        //            {
        //                dynSettings.Bench.Dispatcher.BeginInvoke(
        //                    (Action)(() => dynSettings.Controller.DynamoViewModel.Log("Failed to refresh available nodes from server.")));
        //            }
        //        };
        //    new Thread(start).Start();
        //}

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

        private ResponseWithContentBody<PackageHeader> UploadDynamoPackageTest()
        {
            var keywords = new List<string>();
            var nv = new PackageUpload("RootNode6", "0.1.0", "This is the best", keywords, "MIT",
                                                     "SecondLevelNode1 - No description provided, ThirdLevelCustomNodeA1 - No description provided, ThirdLevelCustomNodeA2 - No description provided, SecondLevelNode2 - No description provided, ThirdLevelCustomNodeB1 - No description provided, ThirdLevelCustomNodeB2 - No description provided, RootNode - No description provided", "dynamo", "0.5.2.20207", "", "",
                                                     @"C:\Users\boyerp\Desktop\Home2.zip", new List<PackageDependency>());

            //var keywords = new List<string>() { "neat", "ok" };
            //var nv = new PackageUpload("Third .NET Package4", "1.1.0", "description", keywords, "MIT",
            //                                         "contents", "dynamo", "0.1.0", "", "group", new List<string>(), new List<PackageDependency>());
            var response = Client.ExecuteAndDeserializeWithContent<PackageHeader>(nv);
            return response;
        }

        public PackageUploadHandle Publish( Package l, List<string> files, bool isNewVersion )
        {

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

        /// <summary>
        ///     Serialize and save a PackageHeader to the "Packages" directory
        /// </summary>
        /// <param name="pkgHeader"> The PackageHeader object </param>
        public void SavePackageHeader(PackageHeader pkgHeader)
        {
            try
            {
                var m2 = new JsonSerializer();
                string s = m2.Serialize(pkgHeader);

                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "packages");

                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);

                // now save it
                string path = Path.Combine(pluginsPath, pkgHeader.name + ".json");
                File.WriteAllText(path, s);
            }
            catch
            {
                dynSettings.Bench.Dispatcher.BeginInvoke(
                    (Action)
                    (() => dynSettings.Controller.DynamoViewModel.Log(
                        "Failed to write package header information, won't be under source control.")));
            }
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


        internal void GoToWebsite()
        {
            Process.Start(Client.BaseUrl);
        }
    }
}