using Dynamo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoWebServer.Responses
{
    public class SavedFileResponse: Response
    {
        public string FileName { get; set; }
        public IEnumerable<byte> FileContent { get; set; }
    }
}
