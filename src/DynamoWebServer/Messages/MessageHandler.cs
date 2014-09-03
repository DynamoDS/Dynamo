﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Dynamo;
using Dynamo.Models;
using Dynamo.Nodes;
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
            else if (message is UploadFileMessage)
            {
                UploadFile(dynamo, message, sessionId);
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
                RenderCompleteHandler = (sender, e) => NodesDataModified(sender, e, sessionId, true);
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

        private void ExecuteCommands(DynamoViewModel dynamo, Message message, string sessionId)
        {
            var recordableCommandMsg = (RunCommandsMessage)message;

            var manager = dynamo.VisualizationManager;
            SelectTabByGuid(dynamo, recordableCommandMsg.WorkspaceGuid);

            foreach (var command in recordableCommandMsg.Commands)
            {
                if (command is DynamoViewModel.RunCancelCommand)
                {
                    RenderCompleteHandler = (sender, e) => NodesDataModified(sender, e, sessionId, false);
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
                    var workspace = dynamo.Workspaces.First(elem => elem.Name == name);
                    var index = dynamo.Workspaces.IndexOf(workspace);

                    dynamo.CurrentWorkspaceIndex = index;
                }
            }
        }

        private void NodesDataModified(object sender, RenderCompletionEventArgs e, string sessionId, bool isNeededCreationData)
        {
            var nodes = new List<ExecutedNode>();
            var currentWorkspace = dynamoViewModel.Model.CurrentWorkspace;

            foreach (var node in currentWorkspace.Nodes)
            {
                string data;
                var codeBlock = node as CodeBlockNodeModel;
                if (codeBlock != null)
                {
                    data = GetExtendedData(node);
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
                        else
                        {
                            if (node.CachedValue.Data != null)
                            {
                                data = node.CachedValue.Data.ToString();
                            }
                        }
                    }
                }

                var execNode = new ExecutedNode(node, data);
                nodes.Add(execNode);

                // if we loaded a custom node workspace node.IsUpdated will be false
                if (isNeededCreationData)
                {
                    if (node is Function)
                        // include data about number of inputs and outputs
                        data = GetExtendedData(node);
                    uploader.AddCreationData(node, data);
                }
            }

            if (isNeededCreationData)
            {
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
            }
            else
            {
                OnResultReady(this, new ResultReadyEventArgs(new ComputationResponse
                {
                    Nodes = nodes
                }, sessionId));
            }

            dynamoViewModel.VisualizationManager.RenderComplete -= RenderCompleteHandler;
        }

        private string GetExtendedData(NodeModel node)
        {
            if (node is CodeBlockNodeModel || node is Function)
            {
                var inPorts = node.InPorts.Select(port => "\"" + port.PortName + "\"").ToList();
                var outPorts = node.OutPorts.Select(port => "\"" + port.ToolTipContent + "\"").ToList();

                var stringBuilder = new StringBuilder();

                stringBuilder.Append("{");
                if (node is CodeBlockNodeModel)
                {
                    stringBuilder.Append("\"Code\":\"");
                    stringBuilder.Append((node as CodeBlockNodeModel).Code.Replace("\n", "\\n") + "\", ");
                }
                stringBuilder.Append("\"InPorts\": [");
                stringBuilder.Append(inPorts.Any() ? inPorts.Aggregate((i, j) => i + "," + j) : "");
                stringBuilder.Append("], \"OutPorts\": [");
                stringBuilder.Append(outPorts.Any() ? outPorts.Aggregate((i, j) => i + "," + j) : "");
                stringBuilder.Append("]}");

                return stringBuilder.ToString();
            }
            return null;
        }

        #endregion

    }
}
