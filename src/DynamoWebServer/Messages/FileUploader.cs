using System.Collections.Generic;
using System.IO;

using Dynamo.Models;

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

        internal ProcessResult ProcessFileData(UploadFileMessage uploadFileMessage, DynamoModel dynamoModel)
        {
            try
            {
                var result = ProcessResult.Succeeded;

                // if path was specified it means NWK is used
                if (!string.IsNullOrEmpty(uploadFileMessage.Path))
                {
                    dynamoModel.ExecuteCommand(new DynamoModel.OpenFileCommand(uploadFileMessage.Path));
                    result = ProcessResult.RespondWithPath;
                    IsCustomNode = dynamoModel.CurrentWorkspace is CustomNodeWorkspaceModel;
                }
                else
                {
                    IsCustomNode = uploadFileMessage.IsCustomNode;
                    var content = uploadFileMessage.FileContent;
                    var filePath = Path.GetTempPath() + "\\" + uploadFileMessage.FileName;
                    
                    File.WriteAllBytes(filePath, content);

                    dynamoModel.ExecuteCommand(new DynamoModel.OpenFileCommand(filePath));

                    File.Delete(filePath);
                }

                nodesToCreate.Clear();
                connectorsToCreate.Clear();

                return result;
            }
            catch
            {
                return ProcessResult.Failed;
            }
        }

        internal void AddCreationData(NodeModel node, string data)
        {
            var nodeToCreate = new NodeToCreate(node, data);
            nodesToCreate.Add(nodeToCreate);
            connectorsToCreate.AddRange(ConnectorToCreate.GetOutgoingConnectors(node));
        }
    }

    enum ProcessResult
    {
        Failed,
        Succeeded,
        RespondWithPath
    }
}
