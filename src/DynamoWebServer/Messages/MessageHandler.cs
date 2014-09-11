using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Dynamo;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using DynamoWebServer.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DynamoWebServer.Messages
{
    public class MessageHandler
    {
        public event ResultReadyEventHandler ResultReady;

        private readonly JsonSerializerSettings jsonSettings;
        private readonly DynamoViewModel dynamoViewModel;
        private FileUploader uploader;
        private RenderCompleteEventHandler RenderCompleteHandler;

        public MessageHandler(DynamoViewModel dynamoViewModel)
        {
            jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            this.dynamoViewModel = dynamoViewModel;
        }

        /// <summary>
        /// This method serializes the Message object in the json form. 
        /// </summary>
        /// <returns>The string can be used for reconstructing Message using Deserialize method</returns>
        internal string Serialize(Message message)
        {
            return message == null ? null : JsonConvert.SerializeObject(message, jsonSettings);
        }

        /// <summary>
        /// Call this static method to reconstruct a Message from json string
        /// </summary>
        /// <param name="jsonString">Json string that contains all its arguments.</param>
        /// <returns>Reconstructed Message</returns>
        internal Message DeserializeMessage(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject(jsonString, jsonSettings) as Message;
            }
            catch
            {
                throw new ArgumentException("Invalid jsonString for creating Message");
            }
        }

        /// <summary>
        /// Execute Message on selected ViewModel and session
        /// </summary>
        /// <param name="dynamo">DynamoViewModel</param>
        /// <param name="message">Message</param>
        /// <param name="sessionId">The identifier string that represents the current session</param>
        internal void Execute(DynamoViewModel dynamo, Message message, string sessionId)
        {
            if (message is RunCommandsMessage)
            {
                ExecuteCommands(dynamo, message, sessionId);
            }
            else if (message is GetLibraryItemsMessage)
            {
                OnResultReady(this, new ResultReadyEventArgs(new LibraryItemsListResponse
                {
                    LibraryItems = dynamo.SearchViewModel.GetAllLibraryItemsByCategory()
                }, sessionId));
            }
            else if (message is SaveFileMessage)
            {
                SaveFile(dynamo, message, sessionId);
            }
            else if (message is UploadFileMessage)
            {
                UploadFile(dynamo, message, sessionId);
            }
            else if (message is GetNodeGeometryMessage)
            {
                RetrieveGeometry(((GetNodeGeometryMessage)message).NodeID, sessionId);
            }
            else if (message is ClearWorkspaceMessage)
            {
                ClearWorkspace();
            }
        }

        /// <summary>
        /// Send the results of the execution
        /// </summary>
        private void OnResultReady(object sender, ResultReadyEventArgs e)
        {
            if (ResultReady != null)
            {
                ResultReady(sender, e);
            }
        }

        #region Private Class Helper Methods

        private void UploadFile(DynamoViewModel dynamo, Message message, string sessionId)
        {
            if (uploader == null)
                uploader = new FileUploader();

            if (uploader.ProcessFileData(message as UploadFileMessage, dynamo))
            {
                var manager = dynamo.VisualizationManager;
                RenderCompleteHandler = (sender, e) => NodesDataCreated(sender, e, sessionId);
                manager.RenderComplete += RenderCompleteHandler;
                dynamo.ExecuteCommand(new DynamoViewModel.RunCancelCommand(false, false));
            }
            else
            {
                OnResultReady(this, new ResultReadyEventArgs(new UploadFileResponse
                {
                    Status = ResponceStatuses.Error,
                    StatusMessage = "Bad file request"
                }, sessionId));
            }
        }

        private void SaveFile(DynamoViewModel dynamo, Message message, string sessionId)
        {
            // Put into this list all workspaces that should be saved as files
            var homeWorkspace = dynamo.Model.HomeSpace;

            // Add home workspace into it
            var allWorkspacesToSave = new List<WorkspaceModel> { homeWorkspace };

            byte[] fileContent;
            try
            {
                string fileName, filePath;

                var customNodes = dynamo.Model.CustomNodeManager.GetLoadedDefinitions()
                    .Select(cnd => cnd.WorkspaceModel);

                // Add workspaces of all loaded custom nodes into saving list
                allWorkspacesToSave.AddRange(customNodes);

                foreach (var ws in allWorkspacesToSave)
                {
                    // If workspace has its own filename use it during saving
                    if (!string.IsNullOrEmpty(ws.FileName))
                    {
                        fileName = Path.GetFileName(ws.FileName);
                        filePath = ws.FileName;
                    }
                    else
                    {
                        // Add to file name a correct extension 
                        // dependently on its type (custom node or home)
                        if (ws is CustomNodeWorkspaceModel)
                        {
                            fileName = (ws.Name != null ? ws.Name : "MyCustomNode") + ".dyf";
                        }
                        else
                        {
                            fileName = "Home.dyn";
                        }

                        filePath = Path.GetTempPath() + "\\" + fileName;
                    }

                    // Temporarily save workspace into a drive 
                    // using existing functionality for saving
                    if (!ws.SaveAs(filePath))
                        throw new Exception();

                    // Get the file as byte array and after that delete it
                    fileContent = File.ReadAllBytes(filePath);
                    File.Delete(filePath);

                    // Send to the Flood the file as byte array and its name
                    OnResultReady(this, new ResultReadyEventArgs(new SavedFileResponse
                    {
                        Status = ResponceStatuses.Success,
                        FileContent = fileContent,
                        FileName = fileName
                    }, sessionId));
                }
            }
            catch
            {
                // If there was something wrong
                OnResultReady(this, new ResultReadyEventArgs(new SavedFileResponse
                {
                    Status = ResponceStatuses.Error
                }, sessionId));
            }
        }

        private void ExecuteCommands(DynamoViewModel dynamo, Message message, string sessionId)
        {
            var recordableCommandMsg = (RunCommandsMessage)message;

            var manager = dynamo.VisualizationManager;
            SelectTabByGuid(dynamo, recordableCommandMsg.WorkspaceGuid);

            foreach (var command in recordableCommandMsg.Commands)
            {
                if (command is DynamoViewModel.RunCancelCommand)
                {
                    RenderCompleteHandler = (sender, e) => NodesDataModified(sender, e, sessionId);
                    manager.RenderComplete += RenderCompleteHandler;
                }

                dynamo.ExecuteCommand(command);
            }
        }

        private void SelectTabByGuid(DynamoViewModel dynamo, Guid guid)
        {
            // If guid is Empty - switch to HomeWorkspace
            if (guid.Equals(Guid.Empty) && !dynamo.ViewingHomespace)
            {
                dynamo.CurrentWorkspaceIndex = 0;
            }

            if (!guid.Equals(Guid.Empty))
            {
                if (dynamo.Model.CustomNodeManager.LoadedCustomNodes.ContainsKey(guid))
                {
                    var name = dynamo.Model.CustomNodeManager.LoadedCustomNodes[guid]
                        .WorkspaceModel.Name;
                    var workspace = dynamo.Workspaces.FirstOrDefault(elem => elem.Name == name);
                    if (workspace != null)
                    {
                        var index = dynamo.Workspaces.IndexOf(workspace);

                        dynamo.CurrentWorkspaceIndex = index;
                    }
                }
            }
        }

        private void NodesDataModified(object sender, RenderCompletionEventArgs e, string sessionId)
        {
            var nodes = GetExecutedNodes();

            OnResultReady(this, new ResultReadyEventArgs(new ComputationResponse
            {
                Nodes = nodes
            }, sessionId));

            dynamoViewModel.VisualizationManager.RenderComplete -= RenderCompleteHandler;
        }

        private void NodesDataCreated(object sender, RenderCompletionEventArgs e, string sessionId)
        {
            var nodes = GetExecutedNodes();

            var currentWorkspace = dynamoViewModel.Model.CurrentWorkspace;

            foreach (var node in currentWorkspace.Nodes)
            {
                string data;
                ExecutedNode exnode = nodes.FirstOrDefault(n => n.NodeID == node.GUID.ToString());
                if (node is Function)
                {
                    // include data about number of inputs and outputs
                    data = GetInOutPortsData(node);
                }
                else if (exnode != null)
                {
                    // needed data is already computed and contains in executed nodes
                    data = exnode.Data;
                }
                else
                {
                    // all nodes in custom node workspace are always considered as not updated
                    // so node.IsUpdated=false and there are no executed nodes for them
                    data = GetData(node);
                }

                uploader.AddCreationData(node, data);
            }


            var response = new NodeCreationDataResponse
            {
                Nodes = uploader.NodesToCreate,
                Connections = uploader.ConnectorsToCreate,
                NodesResult = nodes
            };

            var proxyNodesResponses = new List<UpdateProxyNodesResponse>();
            if (uploader.IsCustomNode)
            {
                var model = currentWorkspace as CustomNodeWorkspaceModel;
                response.WorkspaceID = model.CustomNodeDefinition.FunctionId.ToString();

                // after uploading custom node definition there may be proxy nodes
                // that were updated 
                var allWorkspaces = dynamoViewModel.Model.Workspaces;
                foreach (var ws in allWorkspaces)
                {
                    // current workspace id
                    string wsID = ws is CustomNodeWorkspaceModel ?
                        (ws as CustomNodeWorkspaceModel).CustomNodeDefinition.FunctionId.ToString() : "";
                    var nodeIDs = new List<string>();

                    // foreach custom node within current workspace
                    foreach (var node in ws.Nodes.Where(n => n is Function))
                    {
                        Function func = node as Function;
                        // if this node was updated by uploading current custom node definition
                        if (func.Definition.FunctionId == model.CustomNodeDefinition.FunctionId)
                            nodeIDs.Add(node.GUID.ToString());
                    }

                    // if there are updated nodes add the response data
                    if (nodeIDs.Any())
                    {
                        proxyNodesResponses.Add(new UpdateProxyNodesResponse()
                        {
                            WorkspaceID = wsID,
                            NodesIDs = nodeIDs,
                            CustomNodeID = response.WorkspaceID
                        });
                    }
                }
            }

            OnResultReady(this, new ResultReadyEventArgs(response, sessionId));

            foreach (var pnResponse in proxyNodesResponses)
            {
                OnResultReady(this, new ResultReadyEventArgs(pnResponse, sessionId));
            }

            dynamoViewModel.VisualizationManager.RenderComplete -= RenderCompleteHandler;
        }

        private string GetData(NodeModel node)
        {
            string data;
            if (node is CodeBlockNodeModel)
            {
                data = GetInOutPortsData(node);
            }
            else
            {
                data = "null";
                if (node.CachedValue != null)
                {
                    if (node.CachedValue.IsCollection)
                    {
                        data = "Array";
                    }
                    else if (node.CachedValue.Data != null)
                    {
                        data = node.CachedValue.Data.ToString();
                    }
                }
            }

            return data;
        }

        private IEnumerable<ExecutedNode> GetExecutedNodes()
        {
            var result = new List<ExecutedNode>();
            var currentWorkspace = dynamoViewModel.Model.CurrentWorkspace;

            foreach (var node in currentWorkspace.Nodes)
            {
                // send only updated nodes back
                if (node.IsUpdated)
                {
                    string data;
                    if (node is CodeBlockNodeModel)
                    {
                        data = GetInOutPortsData(node);
                    }
                    else
                    {
                        data = GetData(node);
                    }

                    var execNode = new ExecutedNode(node, data);
                    result.Add(execNode);
                }
            }

            return result;
        }

        private string GetInOutPortsData(NodeModel node)
        {
            var inPorts = node.InPorts.Select(port => "\"" + port.PortName + "\"").ToList();
            var outPorts = node.OutPorts.Select(port => "\"" + port.ToolTipContent + "\"").ToList();

            var stringBuilder = new StringBuilder();

            stringBuilder.Append("{");
            if (node is CodeBlockNodeModel)
            {
                var codeBlock = node as CodeBlockNodeModel;
                var map = CodeBlockUtils.MapLogicalToVisualLineIndices(codeBlock.Code);
                var allDefs = codeBlock.GetAllDeffs();
                var indexes = new List<int>();

                foreach (var def in allDefs)
                {
                    var logicalIndex = def.Value - 1;
                    var visualIndex = map.ElementAt(logicalIndex);
                    indexes.Add(visualIndex);
                }

                stringBuilder.Append("\"Code\":\"");
                stringBuilder.Append(codeBlock.Code.Replace("\n", "\\n"));
                stringBuilder.Append("\", ");
                stringBuilder.Append("\"PortIndexes\": [");
                stringBuilder.Append(string.Join(", ", indexes.Select(x => x.ToString()).ToArray()));
                stringBuilder.Append("],");
            }

            stringBuilder.Append("\"InPorts\": [");
            stringBuilder.Append(inPorts.Any() ? inPorts.Aggregate((i, j) => i + "," + j) : "");
            stringBuilder.Append("], \"OutPorts\": [");
            stringBuilder.Append(outPorts.Any() ? outPorts.Aggregate((i, j) => i + "," + j) : "");
            stringBuilder.Append("]}");

            return stringBuilder.ToString();
        }

        private void RetrieveGeometry(string nodeId, string sessionId)
        {
            Guid guid;
            var nodeMap = dynamoViewModel.Model.NodeMap;
            if (Guid.TryParse(nodeId, out guid) && nodeMap.ContainsKey(guid))
            {
                NodeModel model = nodeMap[guid];

                OnResultReady(this, new ResultReadyEventArgs(new GeometryDataResponse
                {
                    GeometryData = new GeometryData(nodeId, model.RenderPackages)
                }, sessionId));
            }
        }

        /// <summary>
        /// Cleanup workspace
        /// </summary>
        private void ClearWorkspace()
        {
            var dynamoModel = dynamoViewModel.Model;
            var customNodeManager = dynamoModel.CustomNodeManager;
            var searchModel = dynamoViewModel.SearchViewModel.Model;
            var nodeInfos = customNodeManager.NodeInfos;

            dynamoModel.Home(null);

            foreach (var guid in nodeInfos.Keys)
            {

                searchModel.RemoveNodeAndEmptyParentCategory(guid);

                var name = nodeInfos[guid].Name;
                dynamoModel.Workspaces.RemoveAll(elem =>
                {
                    // To avoid deleting home workspace 
                    // because of coincidence in the names
                    return elem != dynamoModel.HomeSpace && elem.Name == name;
                });

                customNodeManager.LoadedCustomNodes.Remove(guid);
            }

            nodeInfos.Clear();
            dynamoModel.Clear(null);
        }

        #endregion

    }
}
