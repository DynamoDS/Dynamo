using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DynamoMLDataPipeline
{
    // Schema and the assets for the data request body.
    class UploadAssetsRequestBody
    {
        public UploadAssetsRequestBody(List<Schema> schemas, List<InstanceAsset> assets, string operation)
        {
            Schemas = new Dictionary<string, List<Schema>>();
            Schemas[operation] = schemas;

            Assets = new Dictionary<string, List<InstanceAsset>>();
            Assets[operation] = assets;

            Root = Guid.NewGuid().ToString("N").ToUpper();
        }
        [JsonProperty("schemas")]
        public Dictionary<string, List<Schema>> Schemas { get; set; }

        [JsonProperty("assets")]
        public Dictionary<string, List<InstanceAsset>> Assets { get; set; }

        [JsonProperty("root")]
        public string Root { get; set; }
    }
}
