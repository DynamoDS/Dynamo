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
        public Dictionary<FunctionDefinition, PackageHeader> LoadedPackageHeaders { get; internal set; }

        #endregion

        /// <summary>
        ///     The class constructor.
        /// </summary>
        /// <param name="controller"> Reference to to the DynamoController object for the app </param>
        public PackageManagerClient(DynamoController controller)
        {
            Controller = controller;

            //IAuthProvider provider = new RevitOxygenProvider(Autodesk.Revit.AdWebServicesBase.GetInstance());
           
            LoadedPackageHeaders = new Dictionary<FunctionDefinition, PackageHeader>();
            Client = new Client(null, "http://54.225.215.247");
            Worker = new BackgroundWorker();
            IsLoggedIn = false;
        }

        /// <summary>
        ///     Asynchronously pull the package headers from the server and update search
        /// </summary>
        public void RefreshAvailable()
        {
            ThreadStart start = () =>
                {
                    HeaderCollectionDownload req = HeaderCollectionDownload.ByEngine("dynamo");

                    try
                    {
                        ResponseWithContentBody<List<PackageHeader>> response =
                            Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(req);
                        if (response.success)
                        {
                            dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                                {
                                    foreach (PackageHeader header in response.content)
                                    {
                                        dynSettings.Controller.SearchViewModel.Add(header);
                                    }
                                }));
                        }
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Controller.DynamoViewModel.Log("Failed to refresh available nodes from server.")));
                    }
                };
            new Thread(start).Start();
        }

        internal List<Search.SearchElements.PackageManagerSearchElement> Search(string search, int MaxNumSearchResults)
        {
            var nv = new Greg.Requests.Search(search);
            var pkgResponse = Client.ExecuteAndDeserializeWithContent<List<PackageHeader>>(nv);
            return pkgResponse.content.GetRange(0, Math.Min(MaxNumSearchResults, pkgResponse.content.Count())).Select((header) => new PackageManagerSearchElement(header)).ToList();
        }

        public PackageUploadHandle Publish( bool isNewVersion, 
                                            string name,
                                            string version,
                                            string description,
                                            IEnumerable<string> keywords,
                                            string license,
                                            string group,
                                            IEnumerable<string> files,
                                            IEnumerable<PackageDependency> deps,
                                            IEnumerable<Tuple<string, string>> nodeNameDescriptionPairs)
        {

            if (!isNewVersion)
            {
                var pkg = GetPackageUpload(name, version, description, keywords, license, group, files, deps,
                                            nodeNameDescriptionPairs);
                return Publish(pkg);
            }
            else
            {
                var pkg = GetPackageVersionUpload(null, version, description, keywords, license, group, files, deps,
                            nodeNameDescriptionPairs);
                return Publish(pkg);
            }
        }

        /// <summary>
        ///     Create a PackageUpload object from the given data
        /// </summary>
        /// <param name="name">The name of the package</param>
        /// <param name="version"> The version, specified in X.Y.Z form</param>
        /// <param name="description"> A description of the user-defined node </param>
        /// <param name="keywords"> Keywords to describe the user-defined node </param>
        /// <param name="license"> A license string (e.g. "MIT") </param>
        /// <param name="group"> The "group" for the package (e.g. DynamoTutorial) </param>
        /// <param name="files"></param>
        /// <param name="deps"></param>
        /// <param name="nodeNameDescriptionPairs"></param>
        /// <returns> Returns null if it fails to get the xmlDoc, otherwise a valid PackageUpload </returns>
        private PackageUpload GetPackageUpload(     string name,
                                                    string version, 
                                                    string description,
                                                    IEnumerable<string> keywords, 
                                                    string license, 
                                                    string group, 
                                                    IEnumerable<string> files, 
                                                    IEnumerable<PackageDependency> deps, 
                                                    IEnumerable<Tuple<string, string>> nodeNameDescriptionPairs)
        {
            // form dynamo dependent data
            string contents = String.Join(", ",
                                          nodeNameDescriptionPairs.Select((pair) => pair.Item1 + " - " + pair.Item2));
            string engineVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string engineMetadata = "";

            // create a directory where the package will be stored
            var dirInfo = Directory.CreateDirectory(Path.Combine(dynSettings.PackageLoader.PackagesDirectory, name));

            // build the directory substructure
            var binDir = dirInfo.CreateSubdirectory("bin");
            var dyfDir = dirInfo.CreateSubdirectory("dyf");
            var extraDir = dirInfo.CreateSubdirectory("extra");

            // build the package header json, which will be stored with the pkg
            var pkgHeader = new Dynamo.PackageManager.PackageHeader
            {
                description = description,
                engine_version = engineVersion,
                group = group,
                keywords = keywords.ToList(),
                name = name,
                version = version
            };
            var jsSer = new JsonSerializer();
            var pkgHeaderStr = jsSer.Serialize(pkgHeader);
            
            // write the pkg header to the root directory of the pkg
            File.WriteAllText(Path.Combine(dirInfo.FullName, "pkg.json"), pkgHeaderStr);

            var dirs = new List<string>();

            // copy the files to their destination
            foreach (var file in files)
            {

                if (file == null) continue;
                if (!File.Exists(file)) continue;

                if (file.EndsWith("dyf"))
                {
                    File.Copy(file, Path.Combine(dyfDir.FullName, Path.GetFileName(file)));
                }
                else if (file.EndsWith("dll") || file.EndsWith("exe"))
                {
                    File.Copy(file, Path.Combine(binDir.FullName, Path.GetFileName(file)));
                }
                else
                {
                    File.Copy(file, Path.Combine(extraDir.FullName, Path.GetFileName(file)));
                }

            }
            
            // zip up the folder and get its path 
            var zipPath = Greg.Utility.FileUtilities.Zip(dirs);
            var content = new List<string> {zipPath};

            var pkg = PackageUpload.MakeDynamoPackage(  name, 
                                                        version, 
                                                        description, 
                                                        keywords, 
                                                        license, 
                                                        contents, 
                                                        engineVersion, 
                                                        engineMetadata, 
                                                        content,
                                                        deps);
            return pkg;
        }

        /// <summary>
        ///     Create a PackageVersionUpload object from the given data
        /// </summary>
        /// <param name="packageHeader"> The PackageHeader object </param>
        /// <param name="version"> The version, specified in X.Y.Z form</param>
        /// <param name="description"> A description of the user-defined node </param>
        /// <param name="keywords"> Keywords to describe the user-defined node </param>
        /// <param name="license"> A license string (e.g. "MIT") </param>
        /// <param name="group"> The "group" for the package (e.g. DynamoTutorial) </param>
        /// <param name="files"></param>
        /// <param name="deps"></param>
        /// <returns>Returns null if it fails to get the xmlDoc, otherwise a valid PackageVersionUpload  </returns>
        private PackageVersionUpload GetPackageVersionUpload(PackageHeader packageHeader,
                                                            string version,
                                                            string description, 
                                                            IEnumerable<string> keywords, 
                                                            string license,
                                                            string group, 
                                                            IEnumerable<string> files, 
                                                            IEnumerable<PackageDependency> deps, 
                                                            IEnumerable<Tuple<string, string>> nodeNameDescriptionPairs)
        {

            string contents = String.Join(", ",
                                          nodeNameDescriptionPairs.Select((pair) => pair.Item1 + " - " + pair.Item2));
            string engineVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string engineMetadata = ""; 

            // compile all of this stuff into the correct form

            var pkg = new PackageVersionUpload( packageHeader.name, 
                                                version, 
                                                description, 
                                                keywords, 
                                                contents,  
                                                "dynamo",  
                                                engineVersion,
                                                engineMetadata, 
                                                files.ToList(), 
                                                deps ); 
            return pkg;
        }

        ObservableCollection<PackageUploadHandle> _uploads = new ObservableCollection<PackageUploadHandle>();
        public ObservableCollection<PackageUploadHandle> Uploads
        {
            get { return _uploads; }
            set { _uploads = value; }
        }

        /// <summary>
        ///     Attempt to upload PackageUpload
        /// </summary>
        /// <param name="packageUpload"> The PackageUpload object - the payload </param>
        /// <param name="funDef">
        ///     The function definition for the user-defined node - necessary to
        ///     update the LoadedPackageHeaders array on load
        /// </param>
        private PackageUploadHandle Publish(PackageUpload packageUpload)
        {

            ThreadStart start = () =>
                {
                    try
                    {
                        ResponseWithContentBody<PackageHeader> ret =
                            Client.ExecuteAndDeserializeWithContent<PackageHeader>(packageUpload);
                        dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                            {
                                dynSettings.Controller.DynamoViewModel.Log("Message from server: " + ret.message);
                                SavePackageHeader(ret.content);
                            }));
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Controller.DynamoViewModel.Log("Failed to publish package.")));
                    }
                };
            new Thread(start).Start();

        }

        /// <summary>
        ///     Attempt to upload PackageVersionUpload
        /// </summary>
        /// <param name="pkgVersUpload"> The PackageUpload object - the payload </param>
        private PackageUploadHandle Publish(PackageVersionUpload pkgVersUpload)
        {

            ThreadStart start = () =>
                {
                    try
                    {
                        ResponseWithContentBody<PackageHeader> ret =
                            Client.ExecuteAndDeserializeWithContent<PackageHeader>(pkgVersUpload);
                        dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                            {
                                dynSettings.Controller.DynamoViewModel.Log(ret.message);
                                SavePackageHeader(ret.content);
                            }));
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Controller.DynamoViewModel.Log("Failed to publish package.")));
                    }
                };
            new Thread(start).Start();

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

        ObservableCollection<DynamoInstalledPackage> _installedPackages = new ObservableCollection<DynamoInstalledPackage>();
        public ObservableCollection<DynamoInstalledPackage> InstalledPackages
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
                    DynamoInstalledPackage dynPkg;

                    if (dynSettings.PackageLoader.InstalledPackageNames.ContainsKey(packageDownloadHandle.Name))
                    {
                        var pkgRemove = dynSettings.PackageLoader.InstalledPackageNames[packageDownloadHandle.Name];

                        pkgRemove.Uninstall();
                    }

                    if (packageDownloadHandle.Extract(out dynPkg))
                    {
                        dynPkg.RegisterWithHost();
                        packageDownloadHandle.DownloadState = PackageDownloadHandle.State.Installed;
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