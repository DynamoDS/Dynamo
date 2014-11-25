using Dynamo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoWebServer.Responses
{
    public class SavedFileResponse: Response
    {
        public string FileName { get; private set; }
        public IEnumerable<byte> FileContent { get; private set; }

        public SavedFileResponse(string fileName, IEnumerable<byte> fileContent)
        {
            this.FileName = fileName;
            this.FileContent = fileContent;
        }

        public SavedFileResponse(ResponceStatuses status) : base(status) { }
    }
}
