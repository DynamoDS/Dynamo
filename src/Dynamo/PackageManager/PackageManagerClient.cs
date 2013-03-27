using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
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
            Client = new Greg.Client("https://accounts-dev.autodesk.com", "http://10.142.107.26:8080");
            this.Controller = controller;
        }

        // TODO: make async
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

        public dynWorkspace GetCurrentWorkspace()
        {
            return Controller.CurrentSpace;
        }

        public XmlDocument GetXmlDocumentFromWorkspace()
        {
            return Controller.GetXmlFromWorkspace( GetCurrentWorkspace() );
        }

        public PackageUpload GetPackageUploadFromCurrentWorkspace()
        {
            return GetPackageUploadFromWorkspace(GetCurrentWorkspace());
        }

        public PackageUpload GetPackageUploadFromWorkspace(dynWorkspace workspace)
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

                return PackageUpload.MakeDynamoPackage(name, version, description, keywords, license, contents, engineVersion);
            } 
            catch 
            {
                return null;
            }
        }

        public bool Publish( PackageUpload newPackage )
        {
            return this.Client.ExecuteAndDeserialize(newPackage).success;
        }

        public bool ImportPackage( out string name, string id, string version = "")
        {

            // download the package
            HeaderDownload m = new HeaderDownload(id);
            var p = this.Client.ExecuteAndDeserializeWithContent<PackageHeader>(m);

            // then save it to a file in packages
            var d = new XmlDocument();
            d.LoadXml(p.content.versions[0].contents);

            // for which we need to create path
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pluginsPath = Path.Combine(directory, "definitions");

            try
            {
                if (!Directory.Exists(pluginsPath))
                    Directory.CreateDirectory(pluginsPath);

                // now save it
                string path = Path.Combine(pluginsPath, p.content.name + p.content.versions[0].version + ".dyf");
                d.Save(path);

                // then open it via controller
                Controller.OpenDefinition(path);

                // now return the name of the successfully imported node
                name = p.content.name;

                // this all needs to get out of here
                    var node = dynSettings.Controller.CreateDragNode(name);
                
                    var el = node.NodeUI;

                    dynSettings.Workbench.Children.Add(el);
                    dynSettings.Controller.Nodes.Add(el.NodeLogic);

                    // by default place node at center
                    var x = dynSettings.Bench.outerCanvas.ActualWidth / 2.0;
                    var y = dynSettings.Bench.outerCanvas.ActualHeight / 2.0;

                    Point dropPt = new Point(x, y);

                    var a = dynSettings.Bench.outerCanvas.TransformToDescendant(dynSettings.Bench.WorkBench);
                    dropPt = a.Transform(dropPt);

                    // center the node at the drop point
                    dropPt.X -= (el.Width / 2.0);
                    dropPt.Y -= (el.Height / 2.0);

                    Canvas.SetLeft(el, dropPt.X);
                    Canvas.SetTop(el, dropPt.Y);

                    el.EnableInteraction();

                    if (dynSettings.Controller.ViewingHomespace)
                    {
                        el.NodeLogic.SaveResult = true;
                    }

                return true;
            }
            catch
            {
                name = "";
                return false;
            } 

        }


    }
}
