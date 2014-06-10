using System;
using System.Configuration;
using DynamoWebServer.Responses;
using Newtonsoft.Json;
using SuperSocket.SocketBase;
using SuperWebSocket;

namespace DynamoWebServer
{
    public delegate string MessageEventHandler(string message, string sessionId);
    public delegate void MessageEventHandlerWithoutError(string message);

    public class WebServer
    {
        public event MessageEventHandler ReceivedMessage;
        public event MessageEventHandlerWithoutError Info;
        public event MessageEventHandlerWithoutError Error;
        private WebSocketServer socketServer;

        public void Start()
        {
            string httpBindingAddress = ConfigurationManager.AppSettings["bindingAddress"];
            int httpBindingport = int.Parse(ConfigurationManager.AppSettings["bindingPort"]);

            socketServer = new WebSocketServer();
            try
            {
                if (!socketServer.Setup(httpBindingAddress, httpBindingport))
                {
                    LogError("Failed to setup!");
                    return;
                }
            }
            catch
            {
                return;
            }

            socketServer.NewSessionConnected += socketServer_NewSessionConnected;
            socketServer.NewMessageReceived += socketServer_NewMessageReceived;
            socketServer.SessionClosed += socketServer_SessionClosed;

            if (!socketServer.Start())
            {
                LogError("Failed to start!");
                return;
            }

            LogInfo("The server started successfully!");
        }

        public void SendResponse(Response response, string sessionId)
        {
            var session = socketServer.GetAppSessionByID(sessionId);
            if (session != null)
            {
                session.Send(response.Status + "\n" + response.GetResponse());
                LogInfo("Web socket: send [Status: " + response.Status + "\n" + response.GetResponse() + "]");
            }
            else
            {
                LogError("Web socket: can`t send response, socket not initialized! No clients connected? \n  SessionId: [" + sessionId + "]");
            }
            
        }

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            LogInfo("Web socket: connected");
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            LogInfo("Web socket: recived [" + e + "]");
            string errorMessage = null;
            if (ReceivedMessage != null)
            {
                errorMessage = ReceivedMessage(e, session.SessionID);
            }
            session.Send(DateTime.Now.ToShortDateString() + " Message recived: " + e
                + "\nReceived command was " + (errorMessage == null ? "executed" : "not executed, reason: " + errorMessage)
                + "\n" + session.SessionID);
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            if (reason == CloseReason.ServerShutdown)
                return;

            LogInfo("Web socket: disconnected");
        }

        private void LogInfo(string info)
        {
            if (Info != null)
                Info(info);
        }

        private void LogError(string error)
        {
            if (Error != null)
                Error(error);
        }
    }
}
