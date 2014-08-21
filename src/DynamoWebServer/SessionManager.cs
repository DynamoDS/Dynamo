using System.Collections.Generic;
using System.Linq;

using Dynamo.ViewModels;
using DynamoWebServer.Interfaces;
using DynamoWebServer.Messages;

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
        readonly Dictionary<string, MessageHandler> messageHandlersDictionary = new Dictionary<string, MessageHandler>();

        /// <summary>
        /// This will be implemented later when we add multiuser support
        /// Should get session from the sessions list using DynamoViewModel identity
        /// Temporary we save just latest session id
        /// </summary>
        /// <param name="viewModel">This param will help to define the current session for the provided viewmodel</param>
        /// <returns>String session identifier</returns>
        public string GetSession(DynamoViewModel viewModel)
        {
            return messageHandlersDictionary.First(el => el.Value.DynamoViewModel == viewModel).Key;
        }

        /// <summary>
        /// Insert MessageHandler and session Id
        /// </summary>
        /// <param name="id">Session Id</param>
        /// <param name="messageHandler"></param>
        public void Add(string id, MessageHandler messageHandler)
        {
            messageHandlersDictionary.Add(id, messageHandler);
        }

        /// <summary>
        /// Returns corresponding MessageHandler
        /// </summary>
        /// <param name="id">Session Id</param>
        /// <returns></returns>
        public MessageHandler Get(string id)
        {
            return messageHandlersDictionary.ContainsKey(id) ? messageHandlersDictionary[id] : null;
        }

        /// <summary>
        /// Removes MessageHandler from dictionary
        /// </summary>
        /// <param name="id">Session Id</param>
        public void Delete(string id)
        {
            messageHandlersDictionary.Remove(id);
        }
    }
}
