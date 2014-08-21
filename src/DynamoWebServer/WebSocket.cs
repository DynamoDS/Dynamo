using System;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

using SuperWebSocket;

namespace DynamoWebServer
{
    public class WebSocket : IWebSocket
    {
        private readonly WebSocketServer webSocketServer = new WebSocketServer();

        public event Action<WebSocketSession> NewSessionConnected;
        public event Action<WebSocketSession, string> NewMessageReceived;
        public event Action<WebSocketSession, byte[]> NewDataReceived;
        public event Action<WebSocketSession, CloseReason> SessionClosed;

        public WebSocket()
        {
            webSocketServer.NewSessionConnected += socketServer_NewSessionConnected;
            webSocketServer.NewMessageReceived += socketServer_NewMessageReceived;
            webSocketServer.SessionClosed += socketServer_SessionClosed;
            webSocketServer.NewDataReceived += socketServer_NewDataReceived;
        }

        public bool Setup(IRootConfig rootConfig, IServerConfig serverConfig)
        {
            return webSocketServer.Setup(rootConfig, serverConfig);
        }

        public bool Start()
        {
            return webSocketServer.Start();
        }

        public WebSocketSession GetAppSessionByID(string sessionID)
        {
            return webSocketServer.GetAppSessionByID(sessionID);
        }

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            WebServer.ExecuteWithDispatcher(() =>
            {
                if (NewSessionConnected != null)
                    NewSessionConnected(session);
            });
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string message)
        {
            WebServer.ExecuteWithDispatcher(() =>
            {
                if (NewMessageReceived != null)
                    NewMessageReceived(session, message);
            });
        }

        void socketServer_NewDataReceived(WebSocketSession session, byte[] value)
        {
            WebServer.ExecuteWithDispatcher(() =>
            {
                if (NewDataReceived != null)
                    NewDataReceived(session, value);
            });
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            WebServer.ExecuteWithDispatcher(() =>
            {
                if (SessionClosed != null)
                    SessionClosed(session, reason);
            });
        }
    }
}
