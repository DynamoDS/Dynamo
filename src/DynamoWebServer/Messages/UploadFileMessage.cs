using System.IO;
using System.Runtime.Serialization;
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
                if (IsValidDynamoFilePath(value))
                {
                    path = value;
                    IsCustomNode = value.EndsWith(".dyf");
                }
            }
        }

        private string path;

        public static bool IsValidDynamoFilePath(string toCheck)
        {
            try
            {
                var extension = System.IO.Path.GetExtension(toCheck);
                switch (extension.ToLower())
                {
                    case ".dyn":
                    case ".dyf":
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
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
