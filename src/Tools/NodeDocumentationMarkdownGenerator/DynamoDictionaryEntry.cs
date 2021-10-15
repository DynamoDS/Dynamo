using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NodeDocumentationMarkdownGenerator
{
    /// <summary>
    /// Class used to serialize an entry in the Dynamo Dictionary
    /// </summary>
    internal class DynamoDictionaryEntry
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("imageFile")]
        public List<string> ImageFile { get; set; }

        [JsonProperty("dynFile")]
        public List<string> DynFile { get; set; }

        [JsonProperty("folderPath")]
        public string FolderPath { get; set; }

        [JsonProperty("inDepth")]
        public string InDepth { get; set; }

        [JsonConstructor]
        public DynamoDictionaryEntry(string name, List<string> imageFile, List<string> dynFile, string folderPath, string inDepth)
        {
            Name = name;
            ImageFile = imageFile;
            DynFile = dynFile;
            FolderPath = folderPath;
            InDepth = inDepth;
        }
    }
}
