using Dynamo.ViewModels;

using DynamoWebServer.Messages;

namespace DynamoWebServer.Interfaces
{
    public interface ISessionManager
    {
        string GetSession(DynamoViewModel viewModel);
        void Add(string id, MessageHandler messageHandler);
        MessageHandler Get(string sessionId);
        void Delete(string id);
    }
}
