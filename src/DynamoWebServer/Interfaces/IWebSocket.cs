using System;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

using SuperWebSocket;

namespace DynamoWebServer
{
    public interface IWebSocket
    {
        bool Setup(IRootConfig rootConfig, IServerConfig serverConfig);
        bool Start();
        WebSocketSession GetAppSessionById(string sessionId);
        int GetSessionCount();

        event Action<WebSocketSession> NewSessionConnected;
        event Action<WebSocketSession, string> NewMessageReceived;
        event Action<WebSocketSession, byte[]> NewDataReceived;
        event Action<WebSocketSession, CloseReason> SessionClosed;
    }
}
