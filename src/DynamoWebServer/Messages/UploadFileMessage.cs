using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace DynamoWebServer.Messages
{
    [DataContract]
    public class UploadFileMessage : Message
    {
        public string FileName { get; private set; }
        public bool IsCustomNode { get; private set; }
        public byte[] FileContent { get; private set; }
        
        /// <summary>
        /// Path to the specified .dyn or .dyf file
        /// </summary>
        [DataMember]
        public string Path
        {
            get { return path; }
            set
            {
                if (CheckPath(value))
                {
                    path = value;
                    IsCustomNode = value.EndsWith(".dyf");
                }
            }
        }

        private string path;

        private bool CheckPath(string toCheck)
        {
            return (toCheck != null && (toCheck.EndsWith(".dyn") || toCheck.EndsWith(".dyf")));
        }
        
        public UploadFileMessage(byte[] content)
        {
            FileContent = content;
            FileName = GetFileName();
        }

        public UploadFileMessage() { }
        
        /// <summary>
        /// Gets the file name from the file content
        /// </summary>
        /// <returns>The file name with correct extension</returns>
        private string GetFileName()
        {
            var xmlDoc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(FileContent))
            {
                xmlDoc.Load(ms);
            }

            string name = null;
            
            var topNode = xmlDoc.GetElementsByTagName("Workspace");

            // legacy support
            if (topNode.Count == 0)
            {
                topNode = xmlDoc.GetElementsByTagName("dynWorkspace");
            }

            // find workspace name
            foreach (XmlNode node in topNode)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (att.Name.Equals("Name"))
                        name = att.Value;
                    else if (att.Name.Equals("ID"))
                        IsCustomNode = !string.IsNullOrEmpty(att.Value);
                }
            }

            if (!IsCustomNode)
                return "Home.dyn";
            else
                return name + ".dyf";
        }
    }
}
