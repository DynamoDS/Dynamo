using Dynamo.ViewModels;
using DynamoWebServer.Interfaces;

namespace DynamoWebServer
{
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
