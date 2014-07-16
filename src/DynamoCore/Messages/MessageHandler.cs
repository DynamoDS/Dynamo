using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dynamo.Messages
{
    class MessageHandler
    {
        static readonly JsonSerializerSettings JsonSettings;
        static MessageHandler()
        {
            JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private Message message;

        public MessageHandler(Message msg, string sessionId)
        {
            this.message = msg;
            this.SessionId = sessionId;
        }

        #region Class Data Members

        public string SessionId { get; private set; }

        /// <summary>
        /// Send the results of the execution
        /// </summary>
        public event ResultReadyEventHandler ResultReady;
        
        protected void OnResultReady(object sender, ResultReadyEventArgs e)
        {
            if (ResultReady != null)
            {
                e.SessionID = SessionId;
                ResultReady(sender, e);
            }
        }

        #endregion

        #region Public Class Operational Methods

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
            if (message != null && message.Commands != null)
            {
                var manager = dynSettings.Controller.VisualizationManager;
                foreach (var c in message.Commands)
                {
                    if (c is DynamoViewModel.RunCancelCommand)
                    {
                        manager.RenderComplete += ModifiedNodesData;
                    }
                    c.Execute(dynamoViewModel);
                }
            }
        }

        private void ModifiedNodesData(object sender, RenderCompletionEventArgs e)
        {
            var nodes = new List<ExecutedNode>();
            foreach (var item in dynSettings.Controller.DynamoModel.NodeMap)
            {
                string data;
                var codeBlock = item.Value as CodeBlockNodeModel;
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

                var execNode = new ExecutedNode(item.Key.ToString(), item.Value.State.ToString(),
                    item.Value.ToolTipText, data, item.Value.RenderPackages);
                nodes.Add(execNode);
            }

            string nodesInfoMessage = JsonConvert.SerializeObject(nodes, JsonSettings);
            OnResultReady(this, new ResultReadyEventArgs(nodesInfoMessage));
            dynSettings.Controller.VisualizationManager.RenderComplete -= ModifiedNodesData;
        }

        #endregion

    }
}
