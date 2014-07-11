using DynamoWebServer.Responses;

namespace DynamoWebServer
{
    public interface IServer
    {
        event MessageEventHandler ReceivedMessage;
        event MessageEventHandler Info;
        event MessageEventHandler Error;

        void Start();
        void SendResponse(Response response, string sessionId);
    }
}