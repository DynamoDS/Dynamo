using System;
using System.Configuration;
using System.Net;
using DynamoWebServer.Responses;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

using SuperWebSocket;

namespace DynamoWebServer
{
    public delegate void MessageEventHandler(string message, string sessionId);

    public class WebServer : IServer
    {

        #region Init

        static readonly JsonSerializerSettings JsonSettings;
        static WebServer()
        {
            JsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private WebSocketServer socketServer;

        #endregion

        #region IServer

        public event MessageEventHandler ReceivedMessage;
        public event MessageEventHandler Info;
        public event MessageEventHandler Error;

        public void Start()
        {
            string httpBindingAddress = ConfigurationManager.AppSettings["bindingAddress"];
            int httpBindingport = int.Parse(ConfigurationManager.AppSettings["bindingPort"]);

            socketServer = new WebSocketServer();
            try
            {
                if (!socketServer.Setup(new RootConfig(), new ServerConfig
                {
                    Port = httpBindingport,
                    Ip = httpBindingAddress,
                    MaxConnectionNumber = 100,
                    ReceiveBufferSize = 256 * 1024
                }))
                {
                    LogError("Failed to setup!", "");
                    LogError("DynamoWebServer: failed to setup its socketserver", "");
                    return;
                }
            }
            catch
            {
                LogError("DynamoWebServer: failed to setup its socketserver", "");
                return;
            }

            BindEvents();

            if (!socketServer.Start())
            {
                LogError("DynamoWebServer: failed to start its socketserver", "");
                UnBindEvents();
                return;
            }

            LogInfo("The server started successfully!", "");
        }

        public void SendResponse(Response response, string sessionId)
        {
            var session = socketServer.GetAppSessionByID(sessionId);
            if (session != null)
            {
                session.Send(JsonConvert.SerializeObject(response, JsonSettings));
                LogInfo("Web socket: send [Status: " + response.Status + "]", session.SessionID);
            }
            else
            {
                LogError("Web socket: can`t send response, socket not initialized! No clients connected? \n  SessionId: [" + sessionId + "]", sessionId);
            }
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

            if (ReceivedMessage != null)
            {
                try
                {
                    ReceivedMessage(message, session.SessionID);
                }
                catch (Exception ex)
                {
                    SendResponse(new ContentResponse()
                    {
                        Message = "Received command was not executed, reason: " + ex.Message
                    }, session.SessionID);
                }
            }
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            if (reason == CloseReason.ServerShutdown)
                return;

            LogInfo("Web socket: disconnected", session.SessionID);
        }

        private void LogInfo(string info, string sessionId)
        {
            if (Info != null)
                Info(info, sessionId);
        }

        private void LogError(string error, string sessionId)
        {
            if (Error != null)
                Error(error, sessionId);
        }

        private void BindEvents()
        {
            socketServer.NewSessionConnected += socketServer_NewSessionConnected;
            socketServer.NewMessageReceived += socketServer_NewMessageReceived;
            socketServer.SessionClosed += socketServer_SessionClosed;
        }

        private void UnBindEvents()
        {
            socketServer.NewSessionConnected -= socketServer_NewSessionConnected;
            socketServer.NewMessageReceived -= socketServer_NewMessageReceived;
            socketServer.SessionClosed -= socketServer_SessionClosed;
        }
    }
}
