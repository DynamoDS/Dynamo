using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Dynamo;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using DynamoWebServer.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Threading;
using Dynamo.Core.Threading;
using ProtoCore.Mirror;

namespace DynamoWebServer.Messages
{
    public class MessageHandler
    {
        const string MESSAGE_TYPE = "DynamoWebServer.Messages.{0}, DynamoWebServer";

        public event ResultReadyEventHandler ResultReady;
        public string SessionId { get; set; }

        private readonly JsonSerializerSettings jsonSettings;
        private readonly DynamoModel dynamoModel;
        private FileUploader uploader;
        private AutoResetEvent nextRunAllowed = new AutoResetEvent(false);
        private bool evaluationTookPlace = false;
        private int maxMsToWait = 20000;

        public MessageHandler(DynamoModel dynamoModel)
        {
            jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            this.dynamoModel = dynamoModel;
            uploader = new FileUploader();
            this.dynamoModel.EvaluationCompleted += RunCommandCompleted;
        }

        /// <summary>
        /// It's called after all computations are done and Dynamo is ready
        /// to comput graphics data and send ComputationResponse
        /// </summary>
        void RunCommandCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            if (evaluationTookPlace = e.EvaluationTookPlace)
            {
                // Get each node in workspace to update their visuals.
                foreach (var node in dynamoModel.CurrentWorkspace.Nodes)
                    node.RequestVisualUpdateAsync(dynamoModel.MaxTesselationDivisions);

                var task = new DelegateBasedAsyncTask(dynamoModel.Scheduler);
                task.Initialize(() => nextRunAllowed.Set());
                dynamoModel.Scheduler.ScheduleForExecution(task);
            }
            else
            {
                nextRunAllowed.Set();
            }
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
                var jObject = JObject.Parse(jsonString);
                var type = GetTypeFromString(jObject["type"].Value<string>());

                return JsonConvert.DeserializeObject(jsonString, type, jsonSettings) as Message;
            }
            catch
            {
                throw new ArgumentException("Invalid jsonString for creating Message");
            }
        }

        /// <summary>
        /// Execute Message on current dynamoModel and session
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="sessionId">The identifier string that represents the current session</param>
        internal void Execute(Message message, string sessionId)
        {
            if (message is RunCommandsMessage)
            {
                ExecuteCommands(message, sessionId);
            }
            else if (message is GetLibraryItemsMessage)
            {
                OnResultReady(this, new ResultReadyEventArgs(
                    new LibraryItemsListResponse(dynamoModel.SearchModel.GetAllLibraryItemsByCategory()),
                    sessionId));
            }
            else if (message is SaveFileMessage)
            {
                SaveFile(message as SaveFileMessage, sessionId);
            }
            else if (message is UploadFileMessage)
            {
                UploadFile(message as UploadFileMessage, sessionId);
            }
            else if (message is GetNodeGeometryMessage)
            {
                RetrieveGeometry(((GetNodeGeometryMessage)message).NodeId, sessionId);
            }
            else if (message is ClearWorkspaceMessage)
            {
                ClearWorkspace((message as ClearWorkspaceMessage).ClearOnlyHome);
            }
            else if (message is SetModelPositionMessage)
            {
                UpdateCoordinates(message as SetModelPositionMessage);
            }
            else if (message is HasUnsavedChangesMessage)
            {
                var guid = (message as HasUnsavedChangesMessage).WorkspaceGuid;
                var workspace = GetWorkspaceByGuid(guid);

                OnResultReady(this, new ResultReadyEventArgs(
                    new HasUnsavedChangesResponse(guid, workspace.HasUnsavedChanges), sessionId));
            }
        }

        private void UpdateCoordinates(SetModelPositionMessage message)
        {
            WorkspaceModel workspaceToUpdate = GetWorkspaceByGuid(message.WorkspaceGuid);
            if (workspaceToUpdate == null)
                return;

            if (!string.IsNullOrWhiteSpace(message.WorkspaceName) 
                && !(workspaceToUpdate is HomeWorkspaceModel))
                workspaceToUpdate.Name = message.WorkspaceName;

            NodeModel node;
            Guid nodeId;
            foreach (var nodePos in message.ModelPositions)
            {
                if (!Guid.TryParse(nodePos.ModelId, out nodeId))
                    continue;

                node = workspaceToUpdate.Nodes.FirstOrDefault(n => n.GUID == nodeId);
                if (node != null)
                {
                    node.X = nodePos.X;
                    node.Y = nodePos.Y;
                    node.ReportPosition();
                }
            }
        }

        private WorkspaceModel GetWorkspaceByGuid(string guidStr)
        {
            Guid guidValue;
            if (!Guid.TryParse(guidStr, out guidValue) || guidValue.Equals(Guid.Empty))
                return dynamoModel.HomeSpace;

            var defs = dynamoModel.CustomNodeManager.GetLoadedDefinitions();
            var definition = defs.FirstOrDefault(d => d.FunctionId == guidValue);

            return definition != null ? definition.WorkspaceModel : null;
        }

        private string GetCurrentWorkspaceGuid()
        {
            var cnModel = dynamoModel.CurrentWorkspace as CustomNodeWorkspaceModel;

            if (cnModel != null)
                return cnModel.CustomNodeDefinition.FunctionId.ToString();
            
            return "";
        }

        /// <summary>
        /// This method sends ComputationResponse when running is completed
        /// </summary>
        /// <param name="sessionId">The identifier string that represents the current session</param>
        internal void NodesDataModified(string sessionId)
        {
            if (!evaluationTookPlace)
                return;

            var nodes = GetExecutedNodes();
            if (nodes == null || !nodes.Any())
                return;

            OnResultReady(this, new ResultReadyEventArgs(new ComputationResponse(nodes), sessionId));
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

        private void UploadFile(UploadFileMessage message, string sessionId)
        {
            var result = uploader.ProcessFileData(message, dynamoModel);
            if (result != ProcessResult.Failed)
            {
                dynamoModel.ExecuteCommand(new DynamoModel.RunCancelCommand(false, false));
                WaitForRunCompletion();
                bool respondWithPath = (result == ProcessResult.RespondWithPath);
                NodesDataCreated(sessionId, respondWithPath);
            }
            else
            {
                OnResultReady(this, new ResultReadyEventArgs(
                    new UploadFileResponse(ResponceStatuses.Error, "Bad file request"), sessionId));
            }
        }

        private void SaveFile(SaveFileMessage message, string sessionId)
        {
            WorkspaceModel workspaceToSave = GetWorkspaceByGuid(message.Guid);
            if (workspaceToSave == null)
                return;

            byte[] fileContent;
            try
            {
                string fileName, filePath = message.FilePath;

                // if path was specified it means NWK is used and we need just to save file
                if (!string.IsNullOrEmpty(filePath))
                {
                    if (!workspaceToSave.SaveAs(filePath))
                        throw new Exception(string.Format("Failed to save file: {0}", filePath));
                }
                else
                {
                    // Add to file name a correct extension 
                    // dependently on its type (custom node or home)
                    if (workspaceToSave is CustomNodeWorkspaceModel)
                    {
                        fileName = (workspaceToSave.Name != null ? workspaceToSave.Name : "MyCustomNode") + ".dyf";
                    }
                    else
                    {
                        fileName = (workspaceToSave.Name != null ? workspaceToSave.Name : "Home") + ".dyn";
                    }

                    filePath = Path.GetTempPath() + "\\" + fileName;

                    // Temporarily save workspace into a drive 
                    // using existing functionality for saving
                    if (!workspaceToSave.SaveAs(filePath))
                        throw new Exception(string.Format("Failed to save file: {0}", filePath));

                    // Get the file as byte array and after that delete it
                    fileContent = File.ReadAllBytes(filePath);
                    File.Delete(filePath);

                    // Send to the Flood the file as byte array and its name
                    OnResultReady(this, new ResultReadyEventArgs(
                        new SavedFileResponse(fileName, fileContent), sessionId));
                }

                // reset file path
                workspaceToSave.FileName = null;
            }
            catch
            {
                // If there was something wrong
                OnResultReady(this, new ResultReadyEventArgs(
                    new SavedFileResponse(ResponceStatuses.Error), sessionId));
            }
        }

        private void ExecuteCommands(Message message, string sessionId)
        {
            var recordableCommandMsg = (RunCommandsMessage)message;

            SelectTabByGuid(recordableCommandMsg.WorkspaceGuid);

            foreach (var command in recordableCommandMsg.Commands)
            {
                dynamoModel.ExecuteCommand(command);
                if (command is DynamoModel.RunCancelCommand)
                {
                    WaitForRunCompletion();
                    NodesDataModified(sessionId);
                }
                else
                {
                    var updateCommand = command as DynamoModel.UpdateModelValueCommand;
                    // if a cbn is updated send back the data to redraw it in Flood
                    if (updateCommand != null)
                    {
                        var cbn = dynamoModel.CurrentWorkspace.GetModelInternal(updateCommand.ModelGuid) as CodeBlockNodeModel;
                        if (cbn != null)
                        {
                            var wsGuid = GetCurrentWorkspaceGuid();
                            var nodeGuid = updateCommand.ModelGuidAsString;
                            var data = GetInOutPortsData(cbn);
                            
                            var response = new CodeBlockDataResponse(wsGuid, nodeGuid, data);
                            OnResultReady(this, new ResultReadyEventArgs(response, sessionId));
                        }
                    }
                }
            }
        }

        private void WaitForRunCompletion()
        {
            nextRunAllowed.WaitOne(maxMsToWait);
        }

        private void SelectTabByGuid(Guid guid)
        {
            // If guid is Empty - switch to HomeWorkspace
            if (guid.Equals(Guid.Empty) && dynamoModel.CurrentWorkspace != dynamoModel.HomeSpace)
            {
                dynamoModel.Home(null);
            }

            if (!guid.Equals(Guid.Empty))
            {
                if (dynamoModel.CustomNodeManager.LoadedCustomNodes.Contains(guid))
                {
                    var node = (CustomNodeDefinition)dynamoModel.CustomNodeManager.LoadedCustomNodes[guid];
                    var name = node.WorkspaceModel.Name;
                    var workspace = dynamoModel.Workspaces.FirstOrDefault(elem => elem.Name == name);
                    if (workspace != null)
                    {
                        var index = dynamoModel.Workspaces.IndexOf(workspace);

                        dynamoModel.ExecuteCommand(new DynamoModel.SwitchTabCommand(index));
                    }
                }
            }
        }

        private void NodesDataCreated(string sessionId, bool respondWithPath)
        {
            var nodes = GetExecutedNodes();

            var currentWorkspace = dynamoModel.CurrentWorkspace;

            foreach (var node in currentWorkspace.Nodes)
            {
                string data;
                ExecutedNode exnode = nodes.FirstOrDefault(n => n.NodeId == node.GUID.ToString());
                if (node is Function || node is CodeBlockNodeModel)
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
                    data = GetValue(node);
                }

                uploader.AddCreationData(node, data);
            }


            var response = new NodeCreationDataResponse(currentWorkspace.Name,
                GetCurrentWorkspaceGuid(), uploader.NodesToCreate, uploader.ConnectorsToCreate, nodes);

            var proxyNodesResponses = new List<UpdateProxyNodesResponse>();
            if (uploader.IsCustomNode)
            {
                var model = currentWorkspace as CustomNodeWorkspaceModel;

                // after uploading custom node definition there may be proxy nodes
                // that were updated 
                var allWorkspaces = dynamoModel.Workspaces;
                foreach (var ws in allWorkspaces)
                {
                    // current workspace id
                    string wsId = ws is CustomNodeWorkspaceModel ?
                        (ws as CustomNodeWorkspaceModel).CustomNodeDefinition.FunctionId.ToString() : "";
                    var nodeIds = new List<string>();

                    // foreach custom node within current workspace
                    foreach (var node in ws.Nodes.Where(n => n is Function))
                    {
                        Function func = node as Function;
                        // if this node was updated by uploading current custom node definition
                        if (func.Definition.FunctionId == model.CustomNodeDefinition.FunctionId)
                            nodeIds.Add(node.GUID.ToString());
                    }

                    // if there are updated nodes add the response data
                    if (nodeIds.Any())
                    {
                        proxyNodesResponses.Add(new UpdateProxyNodesResponse(wsId, response.WorkspaceId, nodeIds));
                    }
                }
            }

            OnResultReady(this, new ResultReadyEventArgs(response, sessionId));

            foreach (var pnResponse in proxyNodesResponses)
            {
                OnResultReady(this, new ResultReadyEventArgs(pnResponse, sessionId));
            }

            if (respondWithPath)
            {
                var wsResponse = new WorkspacePathResponse(response.WorkspaceId, currentWorkspace.FileName);
                OnResultReady(this, new ResultReadyEventArgs(wsResponse, sessionId));
            }
        }

        private string GetValueFromMirrorData(MirrorData cachedValue)
        {
            if (cachedValue.IsCollection)
            {
                Func<MirrorData, string> wrappedValue = (el) => "\"" + GetValueFromMirrorData(el) + "\"";
                
                var elements = cachedValue.GetElements().ConvertAll(e => wrappedValue(e));
                
                return "[" + string.Join(", ", elements) + "]";
            }
            else if (cachedValue.Data != null)
            {
                return cachedValue.Data.ToString();
            }

            return "null";
        }

        private string GetValue(NodeModel node)
        {
            string data = "null";
            if (node.CachedValue != null)
            {
                data = GetValueFromMirrorData(node.CachedValue);
            }
            else if (node is DoubleInput)
            {
                data = (node as DoubleInput).Value;
            }
            else if (node is StringInput)
            {
                data = (node as StringInput).Value;
            }
            else if (node is Symbol)
            {
                data = (node as Symbol).InputSymbol;
            }
            else if (node is Output)
            {
                data = (node as Output).Symbol;
            }

            return data;
        }

        private IEnumerable<ExecutedNode> GetExecutedNodes()
        {
            var result = new List<ExecutedNode>();
            var currentWorkspace = dynamoModel.CurrentWorkspace;

            foreach (var node in currentWorkspace.Nodes)
            {
                // send only updated nodes back
                if (node.IsUpdated)
                {
                    string data = GetValue(node);
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
                var allDefs = CodeBlockUtils.GetDefinitionLineIndexMap(codeBlock.CodeStatements);
                var lineIndices = new List<int>();

                foreach (var def in allDefs)
                {
                    var logicalIndex = def.Value - 1;
                    var visualIndex = map.ElementAt(logicalIndex);
                    lineIndices.Add(visualIndex);
                }

                stringBuilder.Append("\"Code\":\"");
                stringBuilder.Append(codeBlock.Code.
                                     Replace("\n", "\\n").
                                     Replace("\"", "\\\""));
                stringBuilder.Append("\", ");
                stringBuilder.Append("\"LineIndices\": [");
                stringBuilder.Append(string.Join(", ", lineIndices.Select(x => x.ToString()).ToArray()));
                stringBuilder.Append("],");
            }

            stringBuilder.Append("\"InPorts\": [");
            stringBuilder.Append(inPorts.Any() ? inPorts.Aggregate((i, j) => i + "," + j) : "");
            stringBuilder.Append("], \"OutPorts\": [");
            stringBuilder.Append(outPorts.Any() ? outPorts.Aggregate((i, j) => i + "," + j) : "");
            stringBuilder.Append("], \"Data\": \"" + GetValue(node) + "\"}");

            return stringBuilder.ToString();
        }

        private void RetrieveGeometry(string nodeId, string sessionId)
        {
            Guid guid;
            var nodeMap = dynamoModel.NodeMap;
            if (Guid.TryParse(nodeId, out guid) && nodeMap.ContainsKey(guid))
            {
                NodeModel model = nodeMap[guid];

                OnResultReady(this, new ResultReadyEventArgs(
                    new GeometryDataResponse(new GeometryData(nodeId, model.RenderPackages)), sessionId));
            }
        }
        
        /// <summary>
        /// Cleanup Home workspace and remove all custom nodes
        /// </summary>
        /// <param name="clearOnlyHome">if clearOnlyHome is true
        /// custom nodes won't be removed</param>
        private void ClearWorkspace(bool clearOnlyHome)
        {
            dynamoModel.Home(null);

            if (!clearOnlyHome)
            {
                var customNodeManager = dynamoModel.CustomNodeManager;
                var searchModel = dynamoModel.SearchModel;
                var nodeInfos = customNodeManager.NodeInfos;

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
            }

            dynamoModel.Clear(null);
        }

        private Type GetTypeFromString(string type)
        {
            return Type.GetType(String.Format(MESSAGE_TYPE, type));
        }

        #endregion

    }
}
