using System;

using DynamoWebServer.Responses;
using Dynamo.Interfaces;

namespace DynamoWebServer.Messages
{
    public delegate void ResultReadyEventHandler(object sender, ResultReadyEventArgs e);

    public class ResultReadyEventArgs : EventArgs
    {
        public ResultReadyEventArgs(Response response) 
        {
            this.Response = response;
        }

        public Response Response { get; private set; }
        public string SessionID { get; set; }
    }
}
