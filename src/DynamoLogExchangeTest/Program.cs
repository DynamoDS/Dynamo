using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ContentType = RestSharp.ContentType;

namespace DynamoLogExchangeTest
{

    class Program
    {
        public static string CollectionId { get; set; }
        public static string ExchangeContainerId { get; set; }
        public static string BinaryAssetGuid { get; set; }


        public static void AddHeadersToPostRequest(RestRequest request, string token)
        {
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
        }

        public static string ConstructCreateAssetRequestBody(string schemaNamespaceId, string binaryId, string operation)
        {
            // Define the custom parameter schemas
            var schemas = new List<Schema>();
            var checkSumSchema = new StringParameterSchema("CheckSum", schemaNamespaceId);
            schemas.Add(checkSumSchema);
            var hostSchema = new StringParameterSchema("Host", schemaNamespaceId);
            schemas.Add(hostSchema);
            var hasWarningsSchema = new BooleanParameterSchema("HasWarnings", schemaNamespaceId);
            schemas.Add(hasWarningsSchema);

            // Define the assets
            var assets = new List<InstanceAsset>();

            // Construct parameter component
            var parameterComponent = new ParameterComponent();
            parameterComponent.AddParameterFromSchema("this is a hash of the Dynamo file", checkSumSchema);
            parameterComponent.AddParameterFromSchema("Dynamo host", hostSchema);
            parameterComponent.AddParameterFromSchema(false, hasWarningsSchema);
            // Construct the base component
            var baseComponent = new BaseComponent("test-instance-asset");
            // Construct the binary reference component
            var binaryReferenceComponent = new BinaryReferenceComponent(binaryId);

            var instanceAsset = new InstanceAsset(parameterComponent, baseComponent, binaryReferenceComponent, operation);
            assets.Add(instanceAsset);

            // Construct request body
            var body = new UploadAssetsRequestBody(schemas, assets, "insert");
            string bodyJSON = JsonConvert.SerializeObject(body);
            return bodyJSON;
        }

        static public string StartFullfillment(RestClient client, string collectionId, string exchangeContainerId, string token)
        {
            var fulfillmentUrl = $"/v1/collections/{collectionId}/exchanges/{exchangeContainerId}/fulfillments:start";
            RestRequest startFulfillmentRequest = new RestRequest(fulfillmentUrl);
            AddHeadersToPostRequest(startFulfillmentRequest, token);
            var startFulfillmentResponse = client.ExecutePost(startFulfillmentRequest);

            dynamic responseBody = JObject.Parse(startFulfillmentResponse.Content);
            // We extract the fullfilment id from the response needed from the follow up API calls
            var fulfillmentId = responseBody["id"].Value;

            return fulfillmentId;
        }

        static public void EndFullFillment(RestClient client, string collectionId, string exchangeContainerId, string fulfillmentId, string token)
        {
            var endFulfillmentUrl = $"/v1/collections/{collectionId}/exchanges/{exchangeContainerId}/fulfillments/{fulfillmentId}:finish";
            RestRequest endFulfillmentRequest = new RestRequest(endFulfillmentUrl);
            AddHeadersToPostRequest(endFulfillmentRequest, token);
            var endFulfillmentResponse = client.ExecutePost(endFulfillmentRequest);
            dynamic endFulfillmentResponseBody = JObject.Parse(endFulfillmentResponse.Content);
            var fulfillmentStatus = endFulfillmentResponseBody.status.Value;
            while (fulfillmentStatus == "IN_PROGRESS")
            {
                var fulfillmentStatusUrl = $"/v1/collections/{collectionId}/exchanges/{exchangeContainerId}/fulfillments/{fulfillmentId}";
                RestRequest fulfillementStatusRequest = new RestRequest(fulfillmentStatusUrl);
                fulfillementStatusRequest.AddHeader("Authorization", $"Bearer {token}");
                var fulfillmentStatusResponse = client.ExecuteGet(fulfillementStatusRequest);
                dynamic fulfillmentStatusResponseBody = JObject.Parse(fulfillmentStatusResponse.Content);
                fulfillmentStatus = fulfillmentStatusResponseBody.status.Value;
            }
            Console.WriteLine($"The data exchange is {fulfillmentStatus}");
        }

        static public string ConvertDynToBase64(string filePath) 
        {
            string parentDir = Directory.GetParent(filePath).FullName;
            string savePath = Path.Combine(parentDir, "data.txt");
            
            // Read .dyn file as a string
            string sourceFileContent = File.ReadAllText(filePath);
            Console.WriteLine($"Read file '{filePath}' with {sourceFileContent.Length} bytes");  // Should be 337,787 bytes

            // Convert the string to a byte array (buffer)
            byte[] stringBuffer = Encoding.UTF8.GetBytes(sourceFileContent);
            Console.WriteLine($"String buffer has {stringBuffer.Length} bytes");

            // Compress to gzip and then convert to base64 to optimize size
            byte[] compressedBuffer = DataUtilities.Compress(stringBuffer);
            Console.WriteLine($"COMPRESSED string buffer has {compressedBuffer.Length} bytes");
            string base64CompressedBuffer = Convert.ToBase64String(compressedBuffer);
            Console.WriteLine($"BASE64 string buffer has {base64CompressedBuffer.Length} bytes");
            // Write to file for testing purposes
            File.WriteAllText(savePath, base64CompressedBuffer);
            return base64CompressedBuffer;
        }

        static void SendNewLog(string filePath, RestClient client, string token)
        {
            // STEP 1: CREATE A DATA EXCHANGE CONTAINER ---------------------
            // We currently use a Revit contract with no host and no source for development purposes. 
            // The FDX team is working on creating a contract for our use case - we will need to swap 
            // this with the new contract before we release the data collection feature to production.
            string exchangeBody = "{\"type\":\"autodesk.fdx:space.exchangecontainer-1.0.0\",\"components\":{\"insert\":{\"autodesk.fdx:contract.revitViewGeometry-1.0.0\":{\"contract\":{\"String\":{\"viewableId\":\"{{viewableId}}\",\"viewName\":\"{{viewableName}}\",\"viewGuid\":\"{{viewGuid}}\"}}}}}}";

            var createExchangeURL = $"/v1/collections/{CollectionId}/exchanges";
            RestRequest createExchangeRequest = new RestRequest(createExchangeURL);
            createExchangeRequest.AddStringBody(exchangeBody, ContentType.Json);
            AddHeadersToPostRequest(createExchangeRequest, token);
            var exchangeRequestResponse = client.ExecutePost(createExchangeRequest);

            dynamic exchangeRequestResponseBody = JObject.Parse(exchangeRequestResponse.Content);
            // We extract the exchange container id, the collection id and the schemaNamespace id 
            // from the response - these will be consumed by the following API calls.
            ExchangeContainerId = exchangeRequestResponseBody["id"].Value;
            var schemaNamespaceId = exchangeRequestResponseBody["components"]["data"]["insert"]["autodesk.fdx:source.base-1.0.0"]["source"]["String"]["schemaNamespaceId"].Value;

            // STEP 2: START A FULLFILLMENT ---------------------
            var fulfillmentId = StartFullfillment(client, CollectionId, ExchangeContainerId, token);

            // STEP 3: CREATE A BINARY ASSET ---------------------
            var createBinaryAssetUrl = $"/v1/collections/{CollectionId}/exchanges/{ExchangeContainerId}/fulfillments/{fulfillmentId}/binaries:upload";

            var binaryAsset = new BinaryAsset();
            BinaryAssetGuid = binaryAsset.Id;
            var binaries = new BinaryAssets();
            binaries.AddBinary(binaryAsset);
            string createBinaryBody = JsonConvert.SerializeObject(binaries);

            RestRequest createBinaryRequest = new RestRequest(createBinaryAssetUrl);
            createBinaryRequest.AddStringBody(createBinaryBody, ContentType.Json);
            AddHeadersToPostRequest(createBinaryRequest, token);
            var createBinaryResponse = client.ExecutePost(createBinaryRequest);

            dynamic createBinaryResponseBody = JObject.Parse(createBinaryResponse.Content);
            var binaryUploadUrl = createBinaryResponseBody["binaries"][0]["uploadUrls"][0].Value;

            // STEP 4: UPLOAD BINARY ---------------------
            // We upload the binary to the AWS url returned by the previous step (createBinaryRequest)
            var fileUploadClient = new RestClient();
            RestRequest uploadBinaryRequest = new RestRequest(binaryUploadUrl);

            // ADD THE FILE TO THE REQUEST
            // Encode .dyn file to comperessed Base64
            var base64CompressedBuffer = ConvertDynToBase64(filePath);

            uploadBinaryRequest.AddHeader("Content-Type", ContentType.Binary);
            uploadBinaryRequest.AddParameter(ContentType.Binary, base64CompressedBuffer, ParameterType.RequestBody);
            var uploadBinaryResponse = fileUploadClient.ExecutePut(uploadBinaryRequest);
            if (uploadBinaryResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Binary upload failed!");
            }
            Console.WriteLine("Binary upload started!");

            // STEP 4b: FINISH BINARY UPLOAD -------------------
            // Finish uploading binary assets: Let the system know that the binary assets have been uploaded and are ready for processing. 
            // This call can be made for a single binary or a batch of 25 binaries.
            var finishBinaryUploadUrl = $"/v1/collections/{CollectionId}/exchanges/{ExchangeContainerId}/fulfillments/{fulfillmentId}/binaries:finish";
            // Construct request body
            var uploadedBinaryAsset = new UploadedBinaryAsset(BinaryAssetGuid);
            var uploadedBinaries = new BinaryAssets();
            uploadedBinaries.AddBinary(uploadedBinaryAsset);
            string finishBinaryUploadBody = JsonConvert.SerializeObject(uploadedBinaries);

            RestRequest finishBinaryUploadRequest = new RestRequest(finishBinaryUploadUrl);
            finishBinaryUploadRequest.AddStringBody(finishBinaryUploadBody, ContentType.Json);
            AddHeadersToPostRequest(finishBinaryUploadRequest, token);
            var finishBinaryUploadResponse = client.ExecutePost(finishBinaryUploadRequest);
            if (finishBinaryUploadResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Binary upload completed!");
            }

            // STEP 5: UPLOAD INSTANCE ASSET AND RELATIONSHIPS ---------------------
            // We create an instance asset as part of the same data exchange that carries 
            // all the metadata and a binary reference to the uploaded binary asset
            // (in our case, the binary asset is the converted Dynamo JSON)
            // This call inserts, modifies, or removes assets and relationships from a source into a given exchange fulfillment.
            // Use this API when you want to fulfill an initial exchange or update an existing exchange. 

            var syncAssetUrl = $"/v1/collections/{CollectionId}/exchanges/{ExchangeContainerId}/fulfillments/{fulfillmentId}:sync";
            // Construct request body for an insert operation, i.e. fulfilling an initial excahnge
            string syncAssetRequestBody = ConstructCreateAssetRequestBody(schemaNamespaceId, BinaryAssetGuid, "insert");

            RestRequest syncAssetRequest = new RestRequest(syncAssetUrl);
            syncAssetRequest.AddStringBody(syncAssetRequestBody, ContentType.Json);
            AddHeadersToPostRequest(syncAssetRequest, token);
            var syncAssetResponse = client.ExecutePost(syncAssetRequest);

            // STEP 6: END FULFILLMENT ---------------------
            EndFullFillment(client, CollectionId, ExchangeContainerId, fulfillmentId, token);
        }

        static void TestPipeline(string filePath, string token, RestClient client)
        {
            // TEST THE PIPELINE -----------------------------------------------------------
            // RETRIEVE BACK THE UPLOADED BINARY ASSET AND CONVERT TO DYNAMO GRAPH
            // STEP 1: Download the binary asset from a specified exchange - this call will return an AWS download URL
            var downloadBinaryUrl = $"/v1/collections/{CollectionId}/exchanges/{ExchangeContainerId}/assets/binaries:download";
            RestRequest downloadBinaryRequest = new RestRequest(downloadBinaryUrl);

            var downloadBinaryAsset = new UploadedBinaryAsset(BinaryAssetGuid);
            var downloadBinaries = new BinaryAssets();
            downloadBinaries.AddBinary(downloadBinaryAsset);
            string binaryDownloadBody = JsonConvert.SerializeObject(downloadBinaries);

            downloadBinaryRequest.AddStringBody(binaryDownloadBody, ContentType.Json);
            AddHeadersToPostRequest(downloadBinaryRequest, token);
            var downloadBinaryResponse = client.ExecutePost(downloadBinaryRequest);
            dynamic downloadBinaryResponseBody = JObject.Parse(downloadBinaryResponse.Content);
            var downloadURL = downloadBinaryResponseBody["binaries"][0]["downloadUrl"].Value;

            // STEP 2: Retrieve the binary data using the returned AWS download URL
            var fileDownloadClient = new RestClient();
            RestRequest downloadDataRequest = new RestRequest(downloadURL);
            var downloadDataResponse = fileDownloadClient.ExecuteGet(downloadDataRequest); // Data returned in base 64 compressed
            var base64Data = downloadDataResponse.Content;

            // STEP 3: Convert base64 compressed data back to .dyn file
            byte[] compressedData = Convert.FromBase64String(base64Data);
            byte[] uncompressedData = DataUtilities.Decompress(compressedData);
            string uncompressedString = Encoding.UTF8.GetString(uncompressedData);
            string parentDir = Directory.GetParent(filePath).FullName;
            var dynamoParsedPath = Path.Combine(parentDir, "MaximizeWindowViews-deserialized.dyn");
            File.WriteAllText(dynamoParsedPath, uncompressedString);
        }

        
        static void Main(string[] args)
        {
            // Pass a sample dynamo file to test with
            var filePath = args[0];
            // The Forge team has recommended to use the Stage environment for testing.
            // Depending on whether we are using Stage or Prod, a Forge token needs to be retrieved 
            // using client and secret id from a Forge app created in the respective environment.
            // Assuming there is a way to retrieve the 3-leg Forge token in Dynamo, I will use a hardcoded one here.
            var token = "eyJhbGciOiJSUzI1NiIsImtpZCI6InY1Y2tfS0F1UXBud2ladkFVTElQbEFQaTdhbyIsInBpLmF0bSI6Ijd6M2gifQ.eyJzY29wZSI6WyJkYXRhOnJlYWQiLCJkYXRhOndyaXRlIiwiZGF0YTpjcmVhdGUiXSwiY2xpZW50X2lkIjoic3N5MXlLNkRXaEVBSkxkc0RhZEFFQmN4c29sNGQyVmYiLCJhdWQiOiJodHRwczovL2F1dG9kZXNrLmNvbS9hdWQvYWp3dGV4cDYwIiwianRpIjoiYjlyVmRZVEpVOTR6Z0djWVlUNEEwOVhHSWp2eGZLTmZVUmxNUWhsNms2NndUNXRvQWM1bExDT2ZOZ3ExYjMyUyIsInVzZXJpZCI6IlBRVE1SOEpBSkxZWiIsImV4cCI6MTY5OTQ5NDc0Mn0.IBzde_sBCCShpTbShI8BmGEe6NbDqqECK5TeL1csxWJ5o_iE7UG75HFF7OXiKf9aQ7Hzpp7RHXMGcSrQM4FaZATw4Sefkr0tN9eI81XMZizQvUqGw1hB2zogTLsQeVrj6UCyxPGnQF8i6PmrqZUuQhAUFLHcChbTtHpRxOWNuIhgHeyitVo9n0cUKzQhd7rM1SsGZQBnQ1xeKjpmBimIAHzFL0Z61t6m4ByKjOmhNWIOdBthq7XUOSXnn-shl4YgtppmDE4LfJdVUI8GKf5xxAyFKakvh_m6XdAre_h5l7-lEN3MazOYCkRXExkaH3dcZyoxxLj7aaViIZm1ZZWIAg";
            var stageClientUrl = "https://developer-stg.api.autodesk.com/exchange";
            //var prodClientUrl = "https://developer.api.autodesk.com/exchange";

            // Stage collectionId created for Dynamo
            CollectionId = "9R09ArUBUEDVRIGQ5OE373_L2C";
            var forgeClient = new RestClient(stageClientUrl);
            
            SendNewLog(filePath, forgeClient, token);

            // This is needed here to allow for the testing workflow to run after the upload has been completed.
            // It looks like even though the fullfillment ran by teh SendNewLog function is being reported 
            // as COMPLETED the upload is not done.
            Thread.Sleep(5000);
            TestPipeline(filePath, token, forgeClient);

            Console.ReadKey();
        }
    }
}
