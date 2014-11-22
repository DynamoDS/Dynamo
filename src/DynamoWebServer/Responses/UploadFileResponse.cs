using Dynamo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoWebServer.Responses
{
    public class UploadFileResponse:Response
    {
        public string StatusMessage { get; private set; }

        public UploadFileResponse(ResponceStatuses status, string message)
            : base(status)
        {
            StatusMessage = message;
        }
    }
}
