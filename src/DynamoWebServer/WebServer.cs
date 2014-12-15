using System;
using System.Configuration;
using System.Net;

using Dynamo.Models;
using DynamoWebServer.Messages;
using DynamoWebServer.Responses;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

using SuperWebSocket;

namespace DynamoWebServer
{
    public class WebServer : IWebServer
    {
        private readonly DynamoModel dynamoModel;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly IWebSocket webSocket;

        private readonly MessageHandler messageHandler;
        private readonly SocketMessageQueue messageQueue;

        public WebServer(DynamoModel dynamoModel, IWebSocket socket)
        {
            webSocket = socket;
            this.dynamoModel = dynamoModel;
            messageHandler = new MessageHandler(dynamoModel);
            messageHandler.ResultReady += SendAnswerToWebSocket;
            messageQueue = new SocketMessageQueue();
            jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        #region IServer

        public void Start()
        {
            string httpBindingAddress = ConfigurationManager.AppSettings["bindingAddress"];
            int httpBindingport = int.Parse(ConfigurationManager.AppSettings["bindingPort"]);

            try
            {
                if (!webSocket.Setup(new RootConfig(), new ServerConfig
                {
                    Port = httpBindingport,
                    Ip = httpBindingAddress,
                    MaxConnectionNumber = 5,
                    ReceiveBufferSize = 256 * 1024
                }))
                {
                    LogInfo("Failed to setup!");
                    LogInfo("DynamoWebServer: failed to setup its socketserver");
                    return;
                }
            }
            catch
            {
                LogInfo("DynamoWebServer: failed to setup its socketserver");
                return;
            }

            BindEvents();

            if (!webSocket.Start())
            {
                LogInfo("DynamoWebServer: failed to start its socketserver");
                UnBindEvents();
                return;
            }

            LogInfo("The server started successfully!");
        }

        public void SendResponse(Response response, string sessionId)
        {
            var session = webSocket.GetAppSessionById(sessionId);
            if (session != null)
            {
                var json = JsonConvert.SerializeObject(response, jsonSettings);
                session.Send(json.Replace("DynamoWebServer.Responses.", "").Replace(", DynamoWebServer", ""));
                LogInfo("Web socket: send [Status: " + response.Status + "]");
            }
            else
            {
                LogInfo("Web socket: can`t send response, socket not initialized! No clients connected? \n  SessionId: [" + sessionId + "]");
            }
        }

        public void ExecuteMessageFromSocket(string message, string sessionId, bool enqueue = true)
        {
            var msg = messageHandler.DeserializeMessage(message);
            ExecuteMessageFromSocket(msg, sessionId, enqueue);
        }

        public void ExecuteFileFromSocket(byte[] file, string sessionId, bool enqueue = true)
        {
            var msg = new UploadFileMessage(file);
            ExecuteMessageFromSocket(msg, sessionId, enqueue);
        }

        public void ProcessExit(object sender, EventArgs e)
        {
            messageQueue.Shutdown();
        }

        #endregion

        #region Private methods

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            // Close connection if not from localhost
            if (!session.RemoteEndPoint.Address.Equals(IPAddress.Loopback))
            {
                session.Close();
                return;
            }

            messageHandler.SessionId = session.SessionID;

            ExecuteMessageFromSocket(new ClearWorkspaceMessage(), session.SessionID, true);
            LogInfo("Web socket: connected");
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string message)
        {
            LogInfo("Web socket: received [" + message + "]");
            SendResponse(new ContentResponse(DateTime.Now.ToShortDateString() + " Message received"), 
                session.SessionID);

            try
            {
                ExecuteMessageFromSocket(message, session.SessionID);
            }
            catch (Exception ex)
            {
                SendResponse(new ContentResponse("Received command was not executed, reason: " + ex.Message),
                    session.SessionID);
            }
        }

        void socketServer_NewDataReceived(WebSocketSession session, byte[] value)
        {
            LogInfo("Web socket: received file");
            try
            {
                ExecuteFileFromSocket(value, session.SessionID);
            }
            catch (Exception ex)
            {
                SendResponse(new ContentResponse("Received file was incorrect: " + ex.Message),
                    session.SessionID);
            }
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            if (reason == CloseReason.ServerShutdown)
            {
                return;
            }

            LogInfo("Web socket: disconnected");
        }

        void SendAnswerToWebSocket(object sender, ResultReadyEventArgs e)
        {
            SendResponse(e.Response, e.SessionId);
        }

        void BindEvents()
        {
            webSocket.NewSessionConnected += socketServer_NewSessionConnected;
            webSocket.NewMessageReceived += socketServer_NewMessageReceived;
            webSocket.SessionClosed += socketServer_SessionClosed;
            webSocket.NewDataReceived += socketServer_NewDataReceived;
        }

        void UnBindEvents()
        {
            webSocket.NewSessionConnected -= socketServer_NewSessionConnected;
            webSocket.NewMessageReceived -= socketServer_NewMessageReceived;
            webSocket.SessionClosed -= socketServer_SessionClosed;
            webSocket.NewDataReceived -= socketServer_NewDataReceived;
        }

        void ExecuteMessageFromSocket(Message message, string sessionId, bool enqueue)
        {
            if (enqueue)
            {
                messageQueue.EnqueueMessage(() => messageHandler.Execute(message, sessionId));
            }
            else
            {
                messageHandler.Execute(message, sessionId);
            }
        }

        void LogInfo(string info)
        {
            if (dynamoModel.Logger != null)
                dynamoModel.Logger.Log(info);
        }

        #endregion
    }
}
