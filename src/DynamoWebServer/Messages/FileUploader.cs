using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                var path = Directory.GetCurrentDirectory() + "\\Uploaded";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var fileName = path + "\\MyWorkspace.dyn";


                File.WriteAllBytes(fileName, uploadFileMessage.FileContent);

                dynamoViewModel.ExecuteCommand(new DynamoViewModel.OpenFileCommand(fileName));
                IsCustomNode = dynamoViewModel.Model.CurrentWorkspace is CustomNodeWorkspaceModel;

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
    }
}
