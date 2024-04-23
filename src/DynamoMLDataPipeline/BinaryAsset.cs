using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace DynamoMLDataPipeline
{
    internal class BaseBinaryAsset
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    internal class UploadedBinaryAsset : BaseBinaryAsset
    {
        public UploadedBinaryAsset(string guid)
        {
            Id = guid;
        }
    }

    internal class BinaryAsset : BaseBinaryAsset
    {
        public BinaryAsset()
        {
            Id = Guid.NewGuid().ToString("N").ToUpper();
            Parts = 1;
            IncludeUploadUrl = true;
            Type = "single";
        }

        [JsonProperty("parts")]
        public int Parts { get; set; }
        [JsonProperty("includeUploadUrl")]
        public bool IncludeUploadUrl { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    internal class BinaryAssets
    {
        [JsonProperty("binaries")]
        public List<BaseBinaryAsset> Binaries { get; set; }

        public BinaryAssets()
        {
            Binaries = new List<BaseBinaryAsset>();
        }

        public void AddBinary(BaseBinaryAsset binary)
        {
            Binaries.Add(binary);
        }
    }
}
