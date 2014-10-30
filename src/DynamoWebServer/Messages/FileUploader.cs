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
        List<NodeToCreate> nodesToCreate;
        List<ConnectorToCreate> connectorsToCreate;

        public bool IsCustomNode { get; private set; }
        public IEnumerable<NodeToCreate> NodesToCreate { get { return nodesToCreate; } }
        public IEnumerable<ConnectorToCreate> ConnectorsToCreate { get { return connectorsToCreate; } }

        public FileUploader()
        {
            nodesToCreate = new List<NodeToCreate>();
            connectorsToCreate = new List<ConnectorToCreate>();
        }

        internal bool ProcessFileData(UploadFileMessage uploadFileMessage, DynamoModel dynamoViewModel)
        {
            try
            {
                IsCustomNode = uploadFileMessage.IsCustomNode;

                // if path was specified it means NWK is used
                if (!string.IsNullOrEmpty(uploadFileMessage.Path))
                {
                    dynamoViewModel.ExecuteCommand(new DynamoModel.OpenFileCommand(uploadFileMessage.Path));
                }
                else
                {
                    var content = uploadFileMessage.FileContent;
                    var filePath = Path.GetTempPath() + "\\" + uploadFileMessage.FileName;
                    
                    File.WriteAllBytes(filePath, content);

                    dynamoViewModel.ExecuteCommand(new DynamoModel.OpenFileCommand(filePath));

                    File.Delete(filePath);
                }

                nodesToCreate.Clear();
                connectorsToCreate.Clear();

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
            nodesToCreate.Add(nodeToCreate);
            connectorsToCreate.AddRange(ConnectorToCreate.GetOutgoingConnectors(node));
        }
    }
}
