using System;
using System.Collections.Generic;
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
        private readonly DynamoViewModel dynamoViewModel;
        private readonly JsonSerializerSettings jsonSettings;

        public event ResultReadyEventHandler ResultReady;

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

        internal void Execute(DynamoViewModel dynamo, Message message)
        {
            if (message is RecordableCommandsMessage)
            {
                ExecuteCommands(dynamo, message);
            }
            else if (message is LibraryItemsListMessage)
            {
                OnResultReady(this, new ResultReadyEventArgs(new LibraryItemsListResponse
                {
                    LibraryItems = dynamo.SearchViewModel.GetAllLibraryItemsByCategory()
                }));
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

        private void ExecuteCommands(DynamoViewModel dynamo, Message message)
        {
            var recordableCommandMsg = (RecordableCommandsMessage)message;

            var manager = dynamo.VisualizationManager;
            SelectTabByGuid(dynamo, recordableCommandMsg.WorkspaceGuid);

            foreach (var command in recordableCommandMsg.Commands)
            {
                if (command is DynamoViewModel.RunCancelCommand)
                {
                    manager.RenderComplete += NodesDataModified;
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

        private void NodesDataModified(object sender, RenderCompletionEventArgs e)
        {
            var nodes = new List<ExecutedNode>();
            foreach (var node in dynamoViewModel.Model.CurrentWorkspace.Nodes)
            {
                string data;
                var codeBlock = node as CodeBlockNodeModel;
                if (codeBlock != null)
                {
                    var inPorts = codeBlock.InPorts.Select(port => "\"" + port.PortName + "\"").ToList();
                    var outPorts = codeBlock.OutPorts.Select(port => "\"" + port.ToolTipContent + "\"").ToList();

                    var stringBuilder = new StringBuilder();

                    stringBuilder.Append("{\"Code\":\"");
                    stringBuilder.Append(codeBlock.Code.Replace("\n", "\\n"));
                    stringBuilder.Append("\", \"InPorts\": [");
                    stringBuilder.Append(inPorts.Any() ? inPorts.Aggregate((i, j) => i + "," + j) : "");
                    stringBuilder.Append("], \"OutPorts\": [");
                    stringBuilder.Append(outPorts.Any() ? outPorts.Aggregate((i, j) => i + "," + j) : "");
                    stringBuilder.Append("]}");

                    data = stringBuilder.ToString();
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
            }

            OnResultReady(this, new ResultReadyEventArgs(new ComputationResponse
            {
                Nodes = nodes
            }));

            dynamoViewModel.VisualizationManager.RenderComplete -= NodesDataModified;
        }

        #endregion

    }
}
