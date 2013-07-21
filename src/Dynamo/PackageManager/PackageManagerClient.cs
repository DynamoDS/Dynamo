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
        //                    dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
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
        //                    (Action) (() => dynSettings.Controller.DynamoViewModel.Log("Failed to refresh available nodes from server.")));
        //            }
        //        };
        //    new Thread(start).Start();
        //}

        internal List<PackageManagerSearchElement> Search(string search, int maxNumSearchResults)
        {
            var nv = new Greg.Requests.Search(search);
            var pkgResponse = Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
            return pkgResponse.content.GetRange(0, Math.Min(maxNumSearchResults, pkgResponse.content.Count())).Select((header) => new PackageManagerSearchElement(header)).ToList();
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

        public PackageUploadHandle Publish( LocalPackage l, List<string> files, bool isNewVersion )
        {

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
                                                    LocalPackage l, 
                                                    List<string> files,
                                                    PackageUploadHandle packageUploadHandle )
        {

            ThreadStart start = () =>
                {
                    try
                    {

                        ResponseBody ret = isNewVersion ?
                                               Client.ExecuteAndDeserialize(PackageUploadBuilder.NewPackageVersion(l,
                                                                                                                     files,
                                                                                                                     packageUploadHandle))
                                               : Client.ExecuteAndDeserialize(PackageUploadBuilder.NewPackage(l, files,
                                                                                                              packageUploadHandle));

                        if (!ret.success)
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

        /// <summary>
        ///     Asynchronously download a specific user-defined node from the server
        /// </summary>
        /// <param name="id"> The id that uniquely defines the package, usually obtained from a PackageHeader </param>
        /// <param name="version"> A version name for the download </param>
        /// <param name="callback"> Delegate to execute upon receiving the package </param>
        public void Download(string id, string version, Action<Guid> callback)
        {
            ThreadStart start = () =>
                {   
                    // download the package
                    var m = new HeaderDownload(id);
                    ResponseWithContentBody<PackageHeader> p = Client.ExecuteAndDeserializeWithContent<PackageHeader>(m);
                };
            new Thread(start).Start();
        }


        /// <summary>
        ///     Attempts to load a PackageHeader from the Packages directory, if successful, stores the PackageHeader
        /// </summary>
        /// <param name="funcDef"> The FunctionDefinition to which the loaded user-defined node is to be assigned </param>
        /// <param name="name">
        ///     The name of the package, necessary for looking it up in Packages. Note that
        ///     two package version cannot exist side by side.
        /// </param>
        public void LoadPackageHeader(FunctionDefinition funcDef, string name)
        {
            try
            {
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "packages");

                // find the file matching the expected name
                string[] files = Directory.GetFiles(pluginsPath, name + ".json");

                if (files.Length == 1) // There can only be one!
                {
                    
                }
            }
            catch (Exception ex)
            {
                dynSettings.Controller.DynamoViewModel.Log("Failed to open the package header information.");
                dynSettings.Controller.DynamoViewModel.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
            }
        } 

        ObservableCollection<PackageDownloadHandle> _downloads = new ObservableCollection<PackageDownloadHandle>();
        public ObservableCollection<PackageDownloadHandle> Downloads
        {
            get { return _downloads; }
            set { _downloads = value; }
        }

        ObservableCollection<LocalPackage> _installedPackages = new ObservableCollection<LocalPackage>();
        public ObservableCollection<LocalPackage> InstalledPackages
        {
            get { return _installedPackages; }
            set { _installedPackages = value; }
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
                    packageDownloadHandle.Done(pathDl);
                    LocalPackage dynPkg;

                    var firstOrDefault = dynSettings.PackageLoader.LocalPackages.FirstOrDefault(pkg => pkg.Name == packageDownloadHandle.Name);
                    if ( firstOrDefault != null)
                        firstOrDefault.UninstallCommand.Execute();

                    if (packageDownloadHandle.Extract(out dynPkg))
                    {
                        dynSettings.Controller.UIDispatcher.BeginInvoke((Action) (() =>
                            {
                                dynPkg.Load();
                                dynSettings.PackageLoader.LocalPackages.Add(dynPkg);
                                packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;
                            }));
                    }
                    else
                    {
                        throw new Exception("Failed to extract the package");
                    }
                }
                catch (Exception e)
                {
                    packageDownloadHandle.Error(e.Message);
                }
            };
            new Thread(start).Start();
        }

    }
}