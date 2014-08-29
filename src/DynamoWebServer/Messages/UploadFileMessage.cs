using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoWebServer.Messages
{
    class UploadFileMessage : Message
    {
        public byte[] FileContent { get; private set; }
        public UploadFileMessage(byte[] content)
        {
            FileContent = content;
        }
    }
}
