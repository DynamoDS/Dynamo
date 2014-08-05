using Dynamo.ViewModels;

namespace DynamoWebServer.Interfaces
{
    public interface ISessionManager
    {
        string GetSession(DynamoViewModel dynamo);
        void SetSession(string sessionId);
    }
}
