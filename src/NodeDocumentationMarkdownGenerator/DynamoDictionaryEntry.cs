using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NodeDocumentationMarkdownGenerator
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    class DynamoDictionaryEntry
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
