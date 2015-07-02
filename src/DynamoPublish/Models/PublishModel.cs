using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Wpf.Authentication;
using Greg.AuthProviders;
using Reach;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Dynamo.Publish.Models
{
    public class PublishModel
    {
        private AuthenticationManager manager;
        public LoginService LoginService;

        private XmlDocument xml_configurations = new XmlDocument();

        private string serverUrl;
        private string port;
        private string page;
        private string provider;

        public bool IsLoggedIn
        {
            get
            {
                return manager.LoginState == LoginState.LoggedIn;
            }
        }

        #region Initialization

        public PublishModel()
        {
            if (LoadConfigurationDocument(ref xml_configurations))
            {
                ProcessConfigurations(xml_configurations);
                manager = new AuthenticationManager(new OxygenProvider(provider));
            }            

        }

        #endregion

        internal void Authenticate()
        {
            if (manager == null)
                throw new Exception("Authentication manager is not found. Please, provide correct authentication manager.");

            if (manager.HasAuthProvider && LoginService != null)
            {
                manager.AuthProvider.RequestLogin += LoginService.ShowLogin;
                manager.AuthProvider.Login();
            }
        }

        internal void SendWorkspaces(IEnumerable<WorkspaceModel> workspaces)
        {
            string address = "http://10.39.165.198:3000/ws";
            var reachClient = new WorkspaceStorageClient(address, manager.Username);

            var result = reachClient.Send(workspaces.OfType<HomeWorkspaceModel>().First(), workspaces.OfType<CustomNodeWorkspaceModel>());
        }

        #region XML Configurations

        private string error_message = "Malformed configuration file.";


        /// <summary>
        /// Loads configuration document. 
        /// In most cases it can be found in folder Configurations, near result assembly.
        /// </summary>
        private bool LoadConfigurationDocument(ref XmlDocument doc)
        {
            var assembly_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configirations_path = @"\Configurations\Configurations.xml";
            var full_path = assembly_path + configirations_path;

            if (File.Exists(full_path))
            {
                doc.Load(full_path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads configuration file.
        /// </summary>        
        private void ProcessConfigurations(object doc)
        {
            XmlNode topNode = xml_configurations.GetElementsByTagName("PublishConfigurations")[0];

            if (topNode == null)
            {
                throw new Exception(error_message);
            }

            foreach (XmlNode node in topNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "ServerUrl":
                        serverUrl = node.InnerText;
                        break;
                    case "Port":
                        port = node.InnerText;
                        break;
                    case "Page":
                        page = node.InnerText;
                        break;
                    case "Provider":
                        provider = node.InnerText;
                        break;

                    default:
                        throw new Exception(error_message);
                }
            }
        }

        #endregion
    }
}
