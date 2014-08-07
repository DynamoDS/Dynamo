using Dynamo.ViewModels;
using DynamoWebServer.Interfaces;

namespace DynamoWebServer
{
    /// For each of the clients connected to DynamoWebServer, there is a corresponding 
    /// session. Whenever a new client connects to DynamoWebServer, a new session is 
    /// created; likewise, whenever a client disconnects, its corresponding session is 
    /// removed. Each session is represented by a unique identifier.
    /// 
    /// DynamoWebServer owns an instance of SessionManager. SessionManager internally
    /// manages a map between a given session identifier and the corresponding 
    /// DynamoViewModel instance. DynamoWebServer makes use the session identifier to 
    /// correctly identify the target DynamoViewModel instance to work with for any 
    /// incoming message sent from a connected client.
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
