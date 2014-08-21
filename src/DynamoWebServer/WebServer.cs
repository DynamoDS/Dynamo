using System;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

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
        private readonly DynamoViewModelPool pool;
        private readonly JsonSerializerSettings jsonSettings;
        private readonly IWebSocket webSocket;
        private readonly ISessionManager sessionManager;

        public WebServer(IWebSocket socket, ISessionManager manager)
        {
            pool = new DynamoViewModelPool(5);
            webSocket = socket;
            sessionManager = manager;

            jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public static void ExecuteWithDispatcher(Action action)
        {
            (Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher)
                .Invoke(action);
        }

        #region IServer

        public void Start()
        {
            string httpBindingAddress = ConfigurationManager.AppSettings["bindingAddress"];
            int httpBindingport = Int32.Parse(ConfigurationManager.AppSettings["bindingPort"]);

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
                    LogInfo("Failed to setup!", "");
                    LogInfo("DynamoWebServer: failed to setup its socketserver", "");
                    return;
                }
            }
            catch
            {
                LogInfo("DynamoWebServer: failed to setup its socketserver", "");
                return;
            }

            BindEvents();

            if (!webSocket.Start())
            {
                LogInfo("DynamoWebServer: failed to start its socketserver", "");
                UnBindEvents();
                return;
            }

            LogInfo("The server started successfully!", "");
        }

        public void SendResponse(Response response, string sessionId)
        {
            var session = webSocket.GetAppSessionByID(sessionId);
            if (session != null)
            {
                session.Send(JsonConvert.SerializeObject(response, jsonSettings));
                LogInfo("Web socket: send [Status: " + response.Status + "]", sessionId);
            }
            else
            {
                LogInfo("Web socket: can`t send response, socket not initialized! No clients connected? \n  SessionId: [" + sessionId + "]", sessionId);
            }
        }

        public void ExecuteMessageFromSocket(string message, string sessionId)
        {
            var msg = sessionManager.Get(sessionId).DeserializeMessage(message);
            ExecuteMessageFromSocket(msg, sessionId);
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
            var messageHandler = new MessageHandler(pool.Get(), session.SessionID);
            messageHandler.ResultReady += SendAnswerToWebSocket;

            sessionManager.Add(session.SessionID, messageHandler);

            LogInfo("Web socket: connected", session.SessionID);
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string message)
        {
            LogInfo("Web socket: recived [" + message + "]", session.SessionID);
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

            pool.Put(sessionManager.Get(session.SessionID).DynamoViewModel);
            sessionManager.Delete(session.SessionID);

            LogInfo("Web socket: disconnected", session.SessionID);
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
        }

        void UnBindEvents()
        {
            webSocket.NewSessionConnected -= socketServer_NewSessionConnected;
            webSocket.NewMessageReceived -= socketServer_NewMessageReceived;
            webSocket.SessionClosed -= socketServer_SessionClosed;
        }

        void ExecuteMessageFromSocket(Message message, string sessionId)
        {
            ExecuteWithDispatcher(() => sessionManager.Get(sessionId).Execute(sessionManager.Get(sessionId).DynamoViewModel, message));
        }

        void LogInfo(string info, string sessionId)
        {
            if (sessionManager.Get(sessionId) == null)
                return;

            var dynamoViewModel = sessionManager.Get(sessionId).DynamoViewModel;

            if (dynamoViewModel.Model.Logger != null)
                dynamoViewModel.Model.Logger.Log(info);
        }

        #endregion
    }
}
