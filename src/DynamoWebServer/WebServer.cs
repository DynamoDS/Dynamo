using System;
using System.Configuration;
using System.Net;
using DynamoWebServer.Responses;
using SuperSocket.SocketBase;
using SuperWebSocket;

namespace DynamoWebServer
{
    public delegate void MessageEventHandler(string message, string sessionId);
    
    public class WebServer
    {
        public event MessageEventHandler ReceivedMessage;
        public event MessageEventHandler Info;
        public event MessageEventHandler Error;
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
                session.Send(response.Status + "\n" + response.GetResponse());
                LogInfo("Web socket: send [Status: " + response.Status + "\n" + response.GetResponse() + "]", session.SessionID);
            }
            else
            {
                LogError("Web socket: can`t send response, socket not initialized! No clients connected? \n  SessionId: [" + sessionId + "]", session.SessionID);
            }
            
        }

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
            session.Send(DateTime.Now.ToShortDateString() + " Message recived: " + message);
            session.Send("Session ID: " + session.SessionID);
            if (ReceivedMessage != null)
            {
                try
                {
                    ReceivedMessage(message, session.SessionID);
                }
                catch (Exception ex)
                {
                    session.Send("Received command was not executed, reason: " + ex.Message);
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
