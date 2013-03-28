using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Dynamo.Commands;
using Dynamo.Nodes;
using Dynamo.Search;
using Dynamo.Utilities;
using Greg;
using Greg.Requests;
using Greg.Responses;

namespace Dynamo.PackageManager
{
    public class PackageManagerClient

    {
        public Greg.Client Client { get; internal set; }
        private static DynamoController Controller = dynSettings.Controller;

        public PackageManagerClient(DynamoController controller)
        {
            Client = new Greg.Client("https://accounts-dev.autodesk.com", "http://54.243.225.192:8080");
        }

        public void RefreshAvailable()
        {
            ThreadStart start = () =>
            {
                var req = Greg.Requests.HeaderCollectionDownload.ByEngine("dynamo");
                var response = Client.ExecuteAndDeserializeWithContent<List<Greg.Responses.PackageHeader>>(req);

                if (response.success)
                {
                    dynSettings.Bench.Dispatcher.Invoke((Action)(() =>
                        {
                            foreach (var header in response.content)
                            {
                                dynSettings.Controller.SearchController.Add(header);
                            }
                        }));                   
                }
            };
            new Thread(start).Start();
        }

        public static XmlDocument GetXmlDocumentFromWorkspace(dynWorkspace workspace)
        {
            return Controller.GetXmlDocumentFromWorkspace( workspace );
        }

        public PackageUpload GetPackageUploadFromFunctionDefinition( FunctionDefinition funDef, Guid funcDefGuid, string version, string description, List<string> keywords, string license)
        {
            try
            {
                var group = ((FuncWorkspace)funDef.Workspace).Category; 
                var name = funDef.Workspace.Name;
                var contents = Controller.GetXmlDocumentFromWorkspace(funDef.Workspace).OuterXml;   
                var engineVersion = "0.1.0";                                                //nope
                var engineMetadata = "FunctionDefinitionGuid:"+funcDefGuid.ToString();      //store the guid here

                var pkg = PackageUpload.MakeDynamoPackage(name, version, description, keywords, license, contents, engineVersion, engineMetadata);
                return pkg;
            } 
            catch 
            {
                return null;
            }
        }

        public void Publish( PackageUpload newPackage )
        {
            ThreadStart start = () =>
            {
                var ret =  this.Client.ExecuteAndDeserialize(newPackage);
                dynSettings.Bench.Dispatcher.Invoke( (Action) ( () => dynSettings.Bench.Log(ret.message) ));
            };
            new Thread(start).Start();
        }

        public void ImportFunctionDefinition(string id, string version, Action<Guid> callback)
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

                        dynSettings.Bench.Dispatcher.Invoke((Action) (() =>
                            {
                                // then open it via controller
                                Controller.OpenDefinition(path);
                                dynSettings.Bench.Log("Successfully imported package " + p.content.name);
                            }));
                    }
                    catch
                    {
                        dynSettings.Bench.Dispatcher.Invoke((Action)(() =>
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
                            Client.IsAuthenticatedAsync((auth) => dynSettings.Bench.Dispatcher.Invoke((Action) (() =>
                                {
                                    if (auth)
                                    {
                                        dynSettings.Bench.PackageManagerLoginState.Text = "Logged in";
                                        dynSettings.Bench.PackageManagerLoginButton.IsEnabled = false;
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
            var pattern = "FunctionDefinitionGuid:{([0-9a-f-]{36})}"; // match a FunctionDefinition
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

        //public dynWorkspace GetCurrentWorkspace()
        //{
        //    return Controller.CurrentSpace.
        //}

        //public PackageUpload GetPackageUploadFromCurrentWorkspace()
        //{
        //    return GetPackageUploadFromWorkspace(GetCurrentWorkspace());
        //}


    }
}
