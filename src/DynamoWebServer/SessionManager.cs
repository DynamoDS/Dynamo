using Dynamo.ViewModels;
using DynamoWebServer.Interfaces;

namespace DynamoWebServer
{
    /// <summary>
    /// Intended for managing multiple users/dynamo instances
    /// When user logs in to the Flood, new session is created in DynamoWebServer, this session has a unique Id,
    /// so we can push it to the SessionManager dictionary { sessionId: dynamoInstanceId }. So the dynamo web server
    /// would know the correct Dynamo instance which should be called. 
    /// DynamoInstanceId could be a property of dynamoViewModel, something like random GUID, created on first property call,
    /// So each instance of dynamo would have a unique dynamoInstanceId.
    /// 
    /// We have to cover in more details how the new dynamo instances are instantiated. We need to keep one extra instance always
    /// up and running, so the new user won't wait new instance setup (Idle state - 1 instance running, user connected - 2nd, 
    /// reserve instance up, etc.) Not very efficient way, but don't have other ideas yet.
    /// 
    /// On client disconnected event session manager clears
    /// corresponding record in sessions list dictionary and stops correct Dynamo instance.
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private string sessionId;

        /// <summary>
        /// This will be implemented later when we add multiuser support
        /// Should get session from the sessions list using DynamoViewModel identity
        /// Temporary we save just latest session id
        /// </summary>
        /// <param name="viewModel">This param will help to define the current session for the provided viewmodel</param>
        /// <returns>String session identifier</returns>
        public string GetSession(DynamoViewModel viewModel)
        {
            return sessionId;
        }

        /// <summary>
        /// Adds session to the sessions list if it's not yet in there
        /// </summary>
        /// <param name="id">String session identifier</param>
        public void SetSession(string id)
        {
            sessionId = id;
        }
    }
}
