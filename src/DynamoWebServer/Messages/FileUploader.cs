using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoWebServer.Responses;

namespace DynamoWebServer.Messages
{
    class FileUploader
    {
        public bool IsCustomNode { get; private set; }

        public List<NodeToCreate> NodesToCreate { get; private set; }
        public List<ConnectorToCreate> ConnectorsToCreate { get; private set; }

        public FileUploader()
        {
            NodesToCreate = new List<NodeToCreate>();
            ConnectorsToCreate = new List<ConnectorToCreate>();
        }

        internal bool ProcessFileData(UploadFileMessage uploadFileMessage, DynamoViewModel dynamoViewModel)
        {
            try
            {
                var content = uploadFileMessage.FileContent;
                var filePath = Path.GetTempPath() + "\\" + GetFileName(content);
                
                File.WriteAllBytes(filePath, content);

                dynamoViewModel.ExecuteCommand(new DynamoViewModel.OpenFileCommand(filePath));
                IsCustomNode = dynamoViewModel.Model.CurrentWorkspace is CustomNodeWorkspaceModel;

                File.Delete(filePath);

                NodesToCreate.Clear();
                ConnectorsToCreate.Clear();

                return true;
            }
            catch
            {
                return false;
            }
        }

        internal void AddCreationData(Dynamo.Models.NodeModel node, string data)
        {
            var nodeToCreate = new NodeToCreate(node, data);
            NodesToCreate.Add(nodeToCreate);
            ConnectorsToCreate.AddRange(ConnectorToCreate.GetComingOutConnectors(node));
        }

        /// <summary>
        /// Gets the file name from the file content
        /// </summary>
        /// <param name="content">The file content</param>
        /// <returns>The file name with correct extension</returns>
        private string GetFileName(byte[] content)
        {
            var xmlDoc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(content))
            {
                xmlDoc.Load(ms);
            }

            string name = null;

            var topNode = xmlDoc.GetElementsByTagName("Workspace");

            // legacy support
            if (topNode.Count == 0)
            {
                topNode = xmlDoc.GetElementsByTagName("dynWorkspace");
            }

            // find workspace name
            foreach (XmlNode node in topNode)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (att.Name.Equals("Name"))
                        name = att.Value;
                    else if (att.Name.Equals("ID"))
                        IsCustomNode = !string.IsNullOrEmpty(att.Value);
                }
            }

            if (!IsCustomNode)
                return "Home.dyn";
            else
                return name + ".dyf";
        }
    }
}
