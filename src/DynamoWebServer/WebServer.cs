using System;
using System.Configuration;
using System.Net;
using System.Windows;
using System.Windows.Threading;

using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynamoWebServer.Interfaces;
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
        private readonly DynamoViewModel dynamoViewModel;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly IWebSocket webSocket;
        private readonly ISessionManager sessionManager;

        private readonly MessageHandler messageHandler;

        public WebServer(DynamoViewModel dynamoViewModel, IWebSocket socket, ISessionManager manager)
        {
            webSocket = socket;
            sessionManager = manager;
            this.dynamoViewModel = dynamoViewModel;
            messageHandler = new MessageHandler(dynamoViewModel);
            messageHandler.ResultReady += SendAnswerToWebSocket;

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
            var session = webSocket.GetAppSessionByID(sessionId);
            if (session != null)
            {
                session.Send(JsonConvert.SerializeObject(response, jsonSettings));
                LogInfo("Web socket: send [Status: " + response.Status + "]");
            }
            else
            {
                LogInfo("Web socket: can`t send response, socket not initialized! No clients connected? \n  SessionId: [" + sessionId + "]");
            }
        }

        public void ExecuteMessageFromSocket(string message, string sessionId)
        {
            var msg = messageHandler.DeserializeMessage(message);
            //Save session in session manager
            sessionManager.SetSession(sessionId);

            ExecuteMessageFromSocket(msg);
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

            LogInfo("Web socket: connected");
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string message)
        {
            LogInfo("Web socket: recived [" + message + "]");
            SendResponse(new ContentResponse
            {
                Message = DateTime.Now.ToShortDateString() + " Message received"
            }, session.SessionID);

            try
            {
                message = message.Replace("Message, DynamoCore", "Message, DynamoWebServer");
                ExecuteMessageFromSocket(message, session.SessionID);
            }
            catch (Exception ex)
            {
                SendResponse(new ContentResponse
                {
                    Message = "Received command was not executed, reason: " + ex.Message
                }, session.SessionID);
            }

        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            if (reason == CloseReason.ServerShutdown)
                return;

            LogInfo("Web socket: disconnected");
        }

        void SendAnswerToWebSocket(object sender, ResultReadyEventArgs e)
        {
            SendResponse(e.Response, sessionManager.GetSession(dynamoViewModel));
        }

        void BindEvents()
        {
            webSocket.NewSessionConnected += socketServer_NewSessionConnected;
            webSocket.NewMessageReceived += socketServer_NewMessageReceived;
            webSocket.SessionClosed += socketServer_SessionClosed;
        }

        void UnBindEvents()
        {
            webSocket.NewSessionConnected -= socketServer_NewSessionConnected;
            webSocket.NewMessageReceived -= socketServer_NewMessageReceived;
            webSocket.SessionClosed -= socketServer_SessionClosed;
        }

        void ExecuteMessageFromSocket(Message message)
        {
            (Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher)
                .Invoke(new Action(() => messageHandler.Execute(dynamoViewModel, message)));
        }

        void LogInfo(string info)
        {
            if (dynamoViewModel.Model.Logger != null)
                dynamoViewModel.Model.Logger.Log(info);
        }

        #endregion
    }
}
