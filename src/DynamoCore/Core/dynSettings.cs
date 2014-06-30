using System;
using System.Collections.Generic;
using Dynamo.Interfaces;
using System.Linq;
using DynamoWebServer;
using DynamoWebServer.Responses;

using Dynamo.PackageManager;
using Dynamo.Messages;
using System.Windows;

namespace Dynamo.Utilities
{
    public static class dynSettings
    {
        public static ObservableDictionary<string, Guid> CustomNodes
        {
            get { return Controller.CustomNodeManager.GetAllNodeNames(); }
        }

        public static PackageLoader PackageLoader { get; internal set; }
        public static CustomNodeManager CustomNodeManager { get { return Controller.CustomNodeManager; } }
        public static DynamoController Controller { get; set; }
        public static WebServer WebSocketServer { get; private set; }

        private static PackageManagerClient _packageManagerClient;
        public static PackageManagerClient PackageManagerClient
        {
            get { return _packageManagerClient ?? (_packageManagerClient = new PackageManagerClient()); }
        }

        public static ILogger DynamoLogger { get; set; }

        /// <summary>
        /// Setting this flag enables creation of an XML in following format that records 
        /// node mapping information - which old node has been converted to which to new node(s) 
        /// </summary>
        public static bool EnableMigrationLogging { get; set; }

        public static string FormatFileName(string filename)
        {
            return RemoveChars(
                filename,
                new[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" });
        }

        public static void ReturnFocusToSearch()
        {
            Controller.SearchViewModel.OnRequestReturnFocusToSearch(null, EventArgs.Empty);
        }

        public static string RemoveChars(string s, IEnumerable<string> chars)
        {
            return chars.Aggregate(s, (current, c) => current.Replace(c, ""));
        }

        public static void EnableServer()
        {
            WebSocketServer = new WebServer();

            WebSocketServer.ReceivedMessage += ExecuteMessageFromSocket;
            WebSocketServer.Info += LogInfo;
            WebSocketServer.Error += LogError;

            WebSocketServer.Start();
        }

        /// <summary>
        /// WebSocketServer calls this method on message recive.
        /// </summary>
        /// <param name="message">Serialized message from client</param>
        /// <param name="sessionId">Current session Id</param>
        public static void ExecuteMessageFromSocket(string message, string sessionId)
        {
            Message msg = Message.Deserialize(message);
            msg.SessionId = sessionId;
            msg.SendAnswer += SendAnswerToWebSocket;

            Application.Current.Dispatcher.Invoke(new Action(() => msg.Execute(Controller.DynamoViewModel)));
        }

        private static void SendAnswerToWebSocket(string answer, string sessionId)
        {
            WebSocketServer.SendResponse(new ComputationResponse()
            {
                Status = ResponceStatuses.Success,
                NodesInJson = answer
            }, sessionId);
        }

        private static void LogInfo(string message, string sessionId)
        {
            if (DynamoLogger != null)
                DynamoLogger.Log(message);
        }

        private static void LogError(string message, string sessionId)
        {
            if (DynamoLogger != null)
                DynamoLogger.Log(message);
        }
    }
}