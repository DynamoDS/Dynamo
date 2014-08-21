﻿using System;

namespace DynamoWebServer.Messages
{
    public delegate void ResultReadyEventHandler(object sender, ResultReadyEventArgs e);

    public class ResultReadyEventArgs : EventArgs
    {
        public Response Response { get; private set; }
        public string SessionId { get; private set; }

        public ResultReadyEventArgs(Response response, string sessionId)
        {
            SessionId = sessionId;
            Response = response;
        }
    }
}
