using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Dynamo.Utilities;
using Greg.Requests;
using Greg.Responses;
using RestSharp.Serializers;

namespace Dynamo.PackageManager
{
    public class PackageManagerClient

    {
        public Greg.Client Client { get; internal set; }
        private DynamoController Controller;
        public bool IsLoggedIn { get; internal set; }

        public PackageManagerClient(DynamoController controller)
        {
            Controller = controller;
            Client = new Greg.Client("https://accounts-dev.autodesk.com", "http://54.243.225.192:8080");
            this.IsLoggedIn = false;
        }
        
        public void RefreshAvailable()
        {
            ThreadStart start = () =>
            {
                var req = Greg.Requests.HeaderCollectionDownload.ByEngine("dynamo");

                try
                {
                    var response = Client.ExecuteAndDeserializeWithContent<List<Greg.Responses.PackageHeader>>(req);
                    if (response.success)
                    {
                        dynSettings.Bench.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                foreach (var header in response.content)
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

        public PackageUpload GetPackageUpload( FunctionDefinition funDef, string version, string description, List<string> keywords, string license, string group)
        {
            try
            {
                // var group = ((FuncWorkspace) funDef.Workspace).Category;
                var name = funDef.Workspace.Name;
                var contents = Controller.GetXmlDocumentFromWorkspace(funDef.Workspace).OuterXml;
                var engineVersion = "0.1.0"; //nope
                var engineMetadata = "FunctionDefinitionGuid:" + funDef.FunctionId.ToString(); //store the guid here

                var pkg = PackageUpload.MakeDynamoPackage(name, version, description, keywords, license, contents,
                                                          engineVersion, engineMetadata);
                return pkg;
            }
            catch
            {
                return null;
            }
        }

        public PackageVersionUpload GetPackageVersionUpload(FunctionDefinition funDef, PackageHeader ph, string version,
                                                            string description, List<string> keywords, string license, string group )
        {
            try
            {
                // var group = ((FuncWorkspace) funDef.Workspace).Category;
                var name = funDef.Workspace.Name;
                var contents = Controller.GetXmlDocumentFromWorkspace(funDef.Workspace).OuterXml;
                var engineVersion = "0.1.0"; //nope
                var engineMetadata = "FunctionDefinitionGuid:" + funDef.FunctionId.ToString();

                var pkg = new PackageVersionUpload(name, version, description, keywords, contents, "dynamo",
                                                   engineVersion,
                                                   engineMetadata);
                return pkg;
            }
            catch
            {
                return null;
            }
        }
        
        public void Publish( PackageUpload newPackage, FunctionDefinition funDef )
        {
            ThreadStart start = () =>
            {
                try
                {
        
                    var ret = this.Client.ExecuteAndDeserializeWithContent<PackageHeader>(newPackage);
                    dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                        {
                            dynSettings.Bench.Log(ret.message);
                            dynSettings.Controller.PackageHeaders.Add(funDef, ret.content);
                            this.SavePackageHeader(ret.content);
                        }));

                }
                catch
                {
                    dynSettings.Bench.Dispatcher.BeginInvoke((Action)(() => dynSettings.Bench.Log("Failed to publish package.")));
                }
                
            };
            new Thread(start).Start();
        }

        public void Publish( PackageVersionUpload newPackage, FunctionDefinition funDef)
        {
            ThreadStart start = () =>
            {
                try
                {
                    var ret = this.Client.ExecuteAndDeserializeWithContent<PackageHeader>(newPackage);
                    dynSettings.Bench.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        dynSettings.Bench.Log(ret.message);
                        this.SavePackageHeader(ret.content);
                    }));

                }
                catch
                {
                    dynSettings.Bench.Dispatcher.BeginInvoke((Action)(() => dynSettings.Bench.Log("Failed to publish package.")));
                }

            };
            new Thread(start).Start();
        }

        // save package information for a downloaded package
        public void SavePackageHeader( PackageHeader ph )
        {
            try
            {
                var m2 = new JsonSerializer();
                var s = m2.Serialize(ph);

                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "packages");

                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);

                // now save it
                string path = Path.Combine(pluginsPath, ph.name + ".json");
                File.WriteAllText(path, s);
                
            }
            catch 
            {
                dynSettings.Bench.Dispatcher.BeginInvoke((Action)(() =>
                {
                    dynSettings.Bench.Log("Failed to write package header information, won't be under source control.");
                }));
            }
            
        }

        public void Download(string id, string version, Action<Guid> callback)
        {
            ThreadStart start = () =>
            {
                // download the package
                var m = new HeaderDownload(id);
                var p = this.Client.ExecuteAndDeserializeWithContent<PackageHeader>(m);
                
                // then save it to a file in packages
                var d = new XmlDocument();
                d.LoadXml(p.content.versions[0].contents);

                // obtain the funcDefGuid
                var funcDefGuid = ExtractFunctionDefinitionGuid(p.content, 0);
                if (Guid.Empty == funcDefGuid)
                {
                    return;
                }

                // for which we need to create path
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string pluginsPath = Path.Combine(directory, "definitions");

                try
                {
                    if (!Directory.Exists(pluginsPath))
                        Directory.CreateDirectory(pluginsPath);

                    // now save it
                    string path = Path.Combine(pluginsPath, p.content.name + ".dyf");
                    d.Save(path);

                    this.SavePackageHeader(p.content);

                    dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                        {
                            Controller.OpenDefinition(path);
                            dynSettings.Bench.Log("Successfully imported package " + p.content.name);
                            callback(funcDefGuid);
                        }));
                }
                catch
                {
                    dynSettings.Bench.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        dynSettings.Bench.Log("Failed to load package " + p.content.name);
                    }));
                }
            };
            new Thread(start).Start();
        }

        internal void GetAccessToken()
        {
            ThreadStart start = () =>
                {
                    try
                    {
                        Client.GetAccessTokenAsync(
                            (s) =>
                            Client.IsAuthenticatedAsync((auth) => dynSettings.Bench.Dispatcher.BeginInvoke((Action) (() =>
                                {
                                    if (auth)
                                    {
                                        dynSettings.Bench.PackageManagerLoginState.Text = "Logged in";
                                        dynSettings.Bench.PackageManagerLoginButton.IsEnabled = false;
                                        this.IsLoggedIn = true;
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
        
        internal static Guid ExtractFunctionDefinitionGuid(string s)
        {
            var pattern = "FunctionDefinitionGuid:([0-9a-f-]{36})"; // match a FunctionDefinition
            var matches = Regex.Matches(s, pattern, RegexOptions.IgnoreCase);

            if (matches.Count != 1)
            {
                return Guid.Empty;
            }

            return new Guid(matches[0].Groups[1].Value);
        } 

        public static Guid ExtractFunctionDefinitionGuid(PackageHeader header, int versionIndex)
        {
            return ExtractFunctionDefinitionGuid( header.versions[versionIndex].engine_metadata );
        }
    }
}
