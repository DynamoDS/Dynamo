using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
        private DynamoController Controller;

        public PackageManagerClient(DynamoController controller)
        {
            Client = new Greg.Client("https://accounts-dev.autodesk.com", "http://54.243.225.192:8080");
            this.Controller = controller;
        }

        public void RefreshAvailable()
        {

            var req = Greg.Requests.HeaderCollectionDownload.ByEngine("dynamo");
            var response = Client.ExecuteAndDeserializeWithContent< List<Greg.Responses.PackageHeader> >(req);

            if (response.success)
            {
                foreach (var header in response.content)
                {
                    dynSettings.Controller.SearchController.Add(header);
                }
            }
        }

        //public dynWorkspace GetCurrentWorkspace()
        //{
        //    return Controller.CurrentSpace.
        //}

        //public PackageUpload GetPackageUploadFromCurrentWorkspace()
        //{
        //    return GetPackageUploadFromWorkspace(GetCurrentWorkspace());
        //}

        public XmlDocument GetXmlDocumentFromWorkspace(dynWorkspace workspace)
        {
            return Controller.GetXmlFromWorkspace( workspace );
        }

        public PackageUpload GetPackageUploadFromWorkspace(dynWorkspace workspace, Guid FuncDefGuid )
        {
            try
            {
                var group = ((FuncWorkspace)workspace).Category; 
                var name = workspace.Name;
                var version = "0.0.1";                          //nope
                var description = "User-defined Dynamo node";   //nope
                var keywords = new List<string>() { group };    //nope
                var license = "MIT";                            //nope
                var contents = Controller.GetXmlFromWorkspace(workspace).OuterXml;
                var engineVersion = "0.1.0";                    //nope
                var engineMetadata = "";                        //store the guid here

                var pkg = PackageUpload.MakeDynamoPackage(name, version, description, keywords, license, contents, engineVersion, engineMetadata);

                return pkg;
            } 
            catch 
            {
                return null;
            }
        }

       public bool Publish( PackageUpload newPackage )
        {
            var ret =  this.Client.ExecuteAndDeserialize(newPackage);
            dynSettings.Bench.Log(ret.message);
            return ret.success;
        }

        public bool ImportFunctionDefinition( out Guid funcDefGuid, string id, string version = "")
        {

            // download the package
            HeaderDownload m = new HeaderDownload(id);
            var p = this.Client.ExecuteAndDeserializeWithContent<PackageHeader>(m);

            // then save it to a file in packages
            var d = new XmlDocument();
            d.LoadXml(p.content.versions[0].contents);

            // obtain the funcDefGuid
            funcDefGuid = PackageManagerClient.ExtractFunctionDefinitionGuid(p.content, 0);
            if (Guid.Empty == funcDefGuid)
            {
                return false;
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

                // then open it via controller
                Controller.OpenDefinition(path);

                dynSettings.Bench.Log("Successfully imported package " + p.content.name);
                return true;
            }
            catch
            {
                funcDefGuid = new Guid();
                return false;
            } 

        }
        
        internal static Guid ExtractFunctionDefinitionGuid(string s)
        {
            var pattern = "FunctionDefinitionGuid:{([0-9a-f-]{36})}"; // match a FunctionDefinition
            var matches = Regex.Matches(s, pattern, RegexOptions.IgnoreCase);

            if (matches.Count != 1)
            {
                return new Guid();
            }

            return new Guid(matches[0].Groups[1].Value);
        } 

        public static Guid ExtractFunctionDefinitionGuid(PackageHeader header, int versionIndex)
        {
            return PackageManagerClient.ExtractFunctionDefinitionGuid(header.versions[versionIndex].engine_metadata);
        }


    }
}
