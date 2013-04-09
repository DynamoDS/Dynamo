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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml;
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

            LoadedPackageHeaders = new Dictionary<FunctionDefinition, PackageHeader>();
            Client = new Client("https://accounts-dev.autodesk.com", "http://54.243.225.192:8080");
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
                            (Action) (() => dynSettings.Bench.Log("Failed to refresh available nodes from server.")));
                    }
                };
            new Thread(start).Start();
        }

        /// <summary>
        ///     Create a PackageUpload object from the given data
        /// </summary>
        /// <param name="funDef"> The function definition for the user-defined node </param>
        /// <param name="version"> The version, specified in X.Y.Z form</param>
        /// <param name="description"> A description of the user-defined node </param>
        /// <param name="keywords"> Keywords to describe the user-defined node </param>
        /// <param name="license"> A license string (e.g. "MIT") </param>
        /// <param name="group"> The "group" for the package (e.g. DynamoTutorial) </param>
        /// <returns> Returns null if it fails to get the xmlDoc, otherwise a valid PackageUpload </returns>
        public PackageUpload GetPackageUpload(FunctionDefinition funDef, string version, string description,
                                              List<string> keywords, string license, string group)
        {
            // var group = ((FuncWorkspace) funDef.Workspace).Category;
            string name = funDef.Workspace.Name;
            var xml = Controller.GetXmlDocFromWorkspace(funDef.Workspace);
            if (xml == null) return null;
            var contents = xml.OuterXml;
            string engineVersion = "0.1.0"; //nope
            string engineMetadata = "FunctionDefinitionGuid:" + funDef.FunctionId.ToString(); //store the guid here

            PackageUpload pkg = PackageUpload.MakeDynamoPackage(name, version, description, keywords, license,
                                                                contents,
                                                                engineVersion, engineMetadata);
            return pkg;
        }

        /// <summary>
        ///     Create a PackageVersionUpload object from the given data
        /// </summary>
        /// <param name="funDef"> The function definition for the user-defined node </param>
        /// <param name="packageHeader"> The PackageHeader object </param>
        /// <param name="version"> The version, specified in X.Y.Z form</param>
        /// <param name="description"> A description of the user-defined node </param>
        /// <param name="keywords"> Keywords to describe the user-defined node </param>
        /// <param name="license"> A license string (e.g. "MIT") </param>
        /// <param name="group"> The "group" for the package (e.g. DynamoTutorial) </param>
        /// <returns>Returns null if it fails to get the xmlDoc, otherwise a valid PackageVersionUpload  </returns>
        public PackageVersionUpload GetPackageVersionUpload(FunctionDefinition funDef, PackageHeader packageHeader,
                                                            string version,
                                                            string description, List<string> keywords, string license,
                                                            string group)
        {
            // var group = ((FuncWorkspace) funDef.Workspace).Category;
            string name = funDef.Workspace.Name;
            var xml = Controller.GetXmlDocFromWorkspace(funDef.Workspace);
            if (xml == null) return null;
            var contents = xml.OuterXml;
            string engineVersion = "0.1.0"; //nope
            string engineMetadata = "FunctionDefinitionGuid:" + funDef.FunctionId.ToString();

            var pkg = new PackageVersionUpload(name, version, description, keywords, contents, "dynamo",
                                                engineVersion,
                                                engineMetadata);
            return pkg;
        }

        /// <summary>
        ///     Attempt to upload PackageUpload
        /// </summary>
        /// <param name="packageUpload"> The PackageUpload object - the payload </param>
        /// <param name="funDef">
        ///     The function definition for the user-defined node - necessary to
        ///     update the LoadedPackageHeaders array on load
        /// </param>
        public void Publish(PackageUpload packageUpload, FunctionDefinition funDef)
        {
            ThreadStart start = () =>
                {
                    try
                    {
                        ResponseWithContentBody<PackageHeader> ret =
                            Client.ExecuteAndDeserializeWithContent<PackageHeader>(packageUpload);
                        dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                            {
                                dynSettings.Bench.Log("Message form server: " + ret.message);
                                LoadedPackageHeaders.Add(funDef, ret.content);
                                SavePackageHeader(ret.content);
                            }));
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Bench.Log("Failed to publish package.")));
                    }
                };
            new Thread(start).Start();
        }

        /// <summary>
        ///     Attempt to upload PackageVersionUpload
        /// </summary>
        /// <param name="pkgVersUpload"> The PackageUpload object - the payload </param>
        public void Publish(PackageVersionUpload pkgVersUpload)
        {
            ThreadStart start = () =>
                {
                    try
                    {
                        ResponseWithContentBody<PackageHeader> ret =
                            Client.ExecuteAndDeserializeWithContent<PackageHeader>(pkgVersUpload);
                        dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                            {
                                dynSettings.Bench.Log(ret.message);
                                SavePackageHeader(ret.content);
                            }));
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Bench.Log("Failed to publish package.")));
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
                    (() => dynSettings.Bench.Log(
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

                    // then save it to a file in packages
                    var d = new XmlDocument();
                    d.LoadXml(p.content.versions[p.content.versions.Count-1].contents);

                    // obtain the funcDefGuid
                    Guid funcDefGuid = ExtractFunctionDefinitionGuid(p.content, 0);
                    if (Guid.Empty == funcDefGuid)
                    {
                        return;
                    }

                    // for which we need to create path
                    string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    if (directory == null)
                    {
                        throw new DirectoryNotFoundException();
                    }
                    string pluginsPath = Path.Combine(directory, "definitions");

                    try
                    {
                        if (!Directory.Exists(pluginsPath))
                            Directory.CreateDirectory(pluginsPath);

                        // now save it
                        string path = Path.Combine(pluginsPath, p.content.name + ".dyf");
                        d.Save(path);

                        SavePackageHeader(p.content);

                        dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                            {
                                Controller.OpenDefinition(path);
                                dynSettings.Bench.Log("Successfully imported package " + p.content.name);
                                callback(funcDefGuid);
                            }));
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke(
                            (Action) (() => dynSettings.Bench.Log("Failed to load package " + p.content.name)));
                    }
                };
            new Thread(start).Start();
        }

        /// <summary>
        ///     Asynchronously obtain an AccessToken from the server, this enables upload.  Assumes
        ///     you've already obtained a RequestToken.
        /// </summary>
        internal void GetAccessToken()
        {
            ThreadStart start = () =>
                {
                    try
                    {
                        Client.GetAccessTokenAsync(
                            (s) =>
                            Client.IsAuthenticatedAsync(
                                (auth) => dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                                    {
                                        if (auth)
                                        {
                                            // TODO: these elements should observe the package manager state
                                            //dynSettings.Bench.PackageManagerLoginState.Text = "Logged in";
                                            //dynSettings.Bench.PackageManagerLoginButton.IsEnabled = false;
                                            IsLoggedIn = true;
                                        }
                                    }))));
                    }
                    catch
                    {
                        dynSettings.Bench.Log("Failed to login. Are you connected to the internet?");
                    }
                };
            new Thread(start).Start();
        }

        /// <summary>
        ///     Extract a guid from the engine-meta data string, usually from a PackageHeader
        /// </summary>
        /// <param name="metadataString"> The string itself </param>
        /// <returns> Returns the guid if the guid was found, otherwise Guid.Empty. </returns>
        internal static Guid ExtractFunctionDefinitionGuid(string metadataString)
        {
            string pattern = "FunctionDefinitionGuid:([0-9a-f-]{36})"; // match a FunctionDefinition
            MatchCollection matches = Regex.Matches(metadataString, pattern, RegexOptions.IgnoreCase);

            if (matches.Count != 1)
            {
                return Guid.Empty;
            }

            return new Guid(matches[0].Groups[1].Value);
        }

        /// <summary>
        ///     Extract a guid from the engine-meta data string from a PackageHeader
        /// </summary>
        /// <param name="header"> The package header </param>
        /// <param name="versionIndex"> The index of the version to obtain </param>
        /// <returns> Returns the guid if the guid was found, otherwise Guid.Empty. </returns>
        public static Guid ExtractFunctionDefinitionGuid(PackageHeader header, int versionIndex)
        {
            if (versionIndex < 0 || versionIndex >= header.versions.Count)
                return Guid.Empty;
            return ExtractFunctionDefinitionGuid(header.versions[versionIndex].engine_metadata);
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
                    // open and deserialize to a PackageHeader object
                    // this is a bit hacky looking, but does the job
                    var proxyResponse = new RestResponse();
                    proxyResponse.Content = File.ReadAllText(files[0]);
                    var jsonDes = new JsonDeserializer();
                    var packageHeader = jsonDes.Deserialize<PackageHeader>(proxyResponse);
                    dynSettings.Bench.Log("Loading package control information for " + name + " from packages");
                    LoadedPackageHeaders.Add(funcDef, packageHeader);
                }
            }
            catch (Exception ex)
            {
                dynSettings.Bench.Log("Failed to open the package header information.");
                dynSettings.Bench.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);
            }
        } 

        /// <summary>
        ///     Shows a string in the UI associated with whether the open workspace is package controlled
        /// </summary>
        public void ShowPackageControlInformation()
        {
            //FunctionDefinition f =
            //    dynSettings.FunctionDict.First(x => x.Value.Workspace == Controller.CurrentSpace).Value;

            //if (f != null)
            //{
            //    if (LoadedPackageHeaders.ContainsKey(f))
            //    {
            //        dynSettings.Bench.packageControlLabel.Content = "Under package control";
            //        dynSettings.Bench.editNameButton.Visibility = Visibility.Collapsed;
            //        dynSettings.Bench.editNameButton.IsHitTestVisible = true;
            //    }
            //    else
            //    {
            //        dynSettings.Bench.packageControlLabel.Content = "Not under package control";
            //    }
            //    dynSettings.Bench.packageControlLabel.Visibility = Visibility.Visible;
            //}
        }

        /// <summary>
        ///     Hides the string in the UI associated with package control for the current workspace
        /// </summary>
        public void HidePackageControlInformation()
        {
            //dynSettings.Bench.packageControlLabel.Visibility = Visibility.Collapsed;
        }
    }
}