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
        static readonly JsonSerializerSettings JsonSettings;

        public string SessionId { get; private set; }
        public event ResultReadyEventHandler ResultReady;

        private Message message;
        private DynamoViewModel dynamoViewModel;

        static MessageHandler()
        {
            JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public MessageHandler(Message msg, string sessionId)
        {
            this.message = msg;
            this.SessionId = sessionId;
        }

        /// <summary>
        /// This method serializes the Message object in the json form. 
        /// </summary>
        /// <returns>The string can be used for reconstructing Message using Deserialize method</returns>
        internal string Serialize()
        {
            return message == null ? null : JsonConvert.SerializeObject(message, JsonSettings);
        }

        /// <summary>
        /// Call this static method to reconstruct a Message from json string
        /// </summary>
        /// <param name="jsonString">Json string that contains all its arguments.</param>
        /// <returns>Reconstructed Message</returns>
        internal static Message DeserializeMessage(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject(jsonString, JsonSettings) as Message;
            }
            catch
            {
                throw new ArgumentException("Invalid jsonString for creating Message");
            }
        }

        internal void Execute(DynamoViewModel dynamoViewModel)
        {
            this.dynamoViewModel = dynamoViewModel;
            if (message is RecordableCommandsMessage)
            {
                ExecuteCommands();
            }
            else if (message is LibraryItemsListMessage)
            {
                OnResultReady(this, new ResultReadyEventArgs(new LibraryItemsListResponse
                {
                    LibraryItems = dynSettings.Controller.SearchViewModel.GetAllLibraryItemsByCategory()
                }));
            }
        }

        /// <summary>
        /// Send the results of the execution
        /// </summary>
        protected void OnResultReady(object sender, ResultReadyEventArgs e)
        {
            if (ResultReady != null)
            {
                e.SessionID = SessionId;
                ResultReady(sender, e);
            }
        }

        #region Private Class Helper Methods

        private void ExecuteCommands()
        {
            var recordableCommandMsg = (RecordableCommandsMessage)message;

            var manager = dynSettings.Controller.VisualizationManager;
            SelectTabByGuid(dynamoViewModel, recordableCommandMsg.WorkspaceGuid);
            foreach (var command in recordableCommandMsg.Commands)
            {
                if (command is DynamoViewModel.RunCancelCommand)
                {
                    manager.RenderComplete += ModifiedNodesData;
                }
                dynamoViewModel.ExecuteCommand(command);
            }
        }

        private void SelectTabByGuid(DynamoViewModel dynamoViewModel, Guid guid)
        {
            // If guid is Empty - switch to HomeWorkspace
            if (guid.Equals(Guid.Empty) && !dynamoViewModel.ViewingHomespace)
            {
                dynamoViewModel.CurrentWorkspaceIndex = 0;
            }

            if (!guid.Equals(Guid.Empty))
            {
                if (dynSettings.Controller.CustomNodeManager.LoadedCustomNodes.ContainsKey(guid))
                {
                    var name = dynSettings.Controller.CustomNodeManager.LoadedCustomNodes[guid]
                        .WorkspaceModel.Name;
                    var workspace = dynamoViewModel.Workspaces.First(elem => elem.Name == name);
                    var index = dynamoViewModel.Workspaces.IndexOf(workspace);

                    dynamoViewModel.CurrentWorkspaceIndex = index;
                }
            }
        }

        private void ModifiedNodesData(object sender, RenderCompletionEventArgs e)
        {
            var nodes = new List<ExecutedNode>();
            foreach (var item in dynSettings.Controller.DynamoModel.NodeMap)
            {
                string data;
                NodeModel node = item.Value;
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
                    if (item.Value.CachedValue != null)
                    {
                        if (item.Value.CachedValue.IsCollection)
                        {
                            data = "Array";
                        }
                        else
                        {
                            if (item.Value.CachedValue.Data != null)
                            {
                                data = item.Value.CachedValue.Data.ToString();
                            }
                        }
                    }                
			    }

                var execNode = new ExecutedNode(item.Key.ToString(), node.State.ToString(), node.ToolTipText, data, node.RenderPackages);
                nodes.Add(execNode);
            }

            OnResultReady(this, new ResultReadyEventArgs(new ComputationResponse
            {
                Nodes = nodes
            }));
            dynSettings.Controller.VisualizationManager.RenderComplete -= ModifiedNodesData;
        }

        #endregion

    }
}
