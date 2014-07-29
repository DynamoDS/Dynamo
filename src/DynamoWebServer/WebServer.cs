using System;
using System.Configuration;
using System.Net;
using System.Windows;
using System.Windows.Threading;

using Dynamo.Utilities;

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

        #region Init

        readonly JsonSerializerSettings JsonSettings;

        private IWebSocket webSocket;

        public WebServer(IWebSocket socket)
        {
            webSocket = socket;

            JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        #endregion

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
                    MaxConnectionNumber = 100,
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
                session.Send(JsonConvert.SerializeObject(response, JsonSettings));
                LogInfo("Web socket: send [Status: " + response.Status + "]", session.SessionID);
            }
            else
            {
                LogInfo("Web socket: can`t send response, socket not initialized! No clients connected? \n  SessionId: [" + sessionId + "]", sessionId);
            }
        }

        public void ExecuteMessageFromSocket(string message, string sessionId)
        {
            Message msg = MessageHandler.DeserializeMessage(message);
            ExecuteMessageFromSocket(msg, sessionId);
        }

        void ExecuteMessageFromSocket(Message message, string sessionId)
        {
            MessageHandler handler = new MessageHandler(message, sessionId);
            handler.ResultReady += SendAnswerToWebSocket;

            (Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher)
                .Invoke(new Action(() => handler.Execute(dynSettings.Controller.DynamoViewModel)));
        }

        #endregion

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            // Close connection if not from localhost
            if (!session.RemoteEndPoint.Address.Equals(IPAddress.Loopback))
            {
                session.Close();
                return;
            }

            LogInfo("Web socket: connected", session.SessionID);
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string message)
        {
            LogInfo("Web socket: recived [" + message + "]", session.SessionID);
            SendResponse(new ContentResponse()
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
                SendResponse(new ContentResponse()
                {
                    Message = "Received command was not executed, reason: " + ex.Message
                }, session.SessionID);
            }

        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            if (reason == CloseReason.ServerShutdown)
                return;

            LogInfo("Web socket: disconnected", session.SessionID);
        }

        void SendAnswerToWebSocket(object sender, ResultReadyEventArgs e)
        {
            SendResponse(e.Response, e.SessionID);
        }

        private void BindEvents()
        {
            webSocket.NewSessionConnected += socketServer_NewSessionConnected;
            webSocket.NewMessageReceived += socketServer_NewMessageReceived;
            webSocket.SessionClosed += socketServer_SessionClosed;
        }

        private void UnBindEvents()
        {
            webSocket.NewSessionConnected -= socketServer_NewSessionConnected;
            webSocket.NewMessageReceived -= socketServer_NewMessageReceived;
            webSocket.SessionClosed -= socketServer_SessionClosed;
        }

        private void LogInfo(string info, string sessionId)
        {
            if (dynSettings.DynamoLogger != null)
                dynSettings.DynamoLogger.Log(info);
        }

    }
}
