using System.Collections.Generic;
using Newtonsoft.Json;

namespace NodeDocumentationMarkdownGenerator
{
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
    }
}
