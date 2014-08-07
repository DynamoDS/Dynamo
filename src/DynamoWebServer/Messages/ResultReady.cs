using System;

using DynamoWebServer.Responses;
using Dynamo.Interfaces;

namespace DynamoWebServer.Messages
{
    public delegate void ResultReadyEventHandler(object sender, ResultReadyEventArgs e);

    public class ResultReadyEventArgs : EventArgs
    {
        public Response Response { get; private set; }
        public string SessionId { get; private set; }

        public ResultReadyEventArgs(Response response)
        {
            Response = response;
        }

        public ResultReadyEventArgs(Response response, string sessionId)
        {
            SessionId = sessionId;
            Response = response;
        }
    }
}
