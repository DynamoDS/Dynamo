using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dynamo.Logging;
using Dynamo.Models;
using Greg;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ContentType = RestSharp.DataFormat;

namespace DynamoMLDataPipeline
{
    internal class DynamoMLDataPipeline
    {
        internal static string CollectionId { get; set; }
        internal static string ExchangeContainerId { get; set; }
        internal static string BinaryAssetGuid { get; set; }

        internal string StagingClientUrl {
            get
            {
                return DynamoUtilities.PathHelper.GetServiceBackendAddress(this, "StagingClientUrl");
            }
        }

        internal string ProductionClientUrl
        {
            get
            {
                return DynamoUtilities.PathHelper.GetServiceBackendAddress(this, "ProductionClientUrl");
            }
        }

        internal string StagingCollectionID {
            get
            {
                return DynamoUtilities.PathHelper.getServiceConfigValues(this, "StagingCollectionID");
            }
        }


        internal string ProductionCollectionID
        {
            get
            {
                return DynamoUtilities.PathHelper.getServiceConfigValues(this, "ProductionCollectionID");
            }
        }

        internal DynamoModel DynamoModel { get; set; }

        internal IOAuth2AccessTokenProvider AuthTokenProvider { get; set; }

        internal IOAuth2UserIDProvider AuthUserInfoProvider { get; set; }

        // Id of the user sending the post request.
        private string GetUserId()
        {
            return AuthUserInfoProvider.UserId;
        }

        // Authorization token needed for the restsharp post request in this pipeline.
        private string GetAuthorizationToken()
        {
            return AuthTokenProvider.GetAccessToken();
        }

        internal static void AddHeadersToPostRequest(RestRequest request, string token)
        {
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Content-Type", "application/json");
        }

        internal string ConstructCreateAssetRequestBody(string schemaNamespaceId, string binaryId, string operation)
        {
            // Define the custom parameter schemas
            var schemas = new List<Schema>();
            var userIdSchema = new StringParameterSchema("UserId", schemaNamespaceId, "DynamoUserIdParam");
            schemas.Add(userIdSchema);
            var hostSchema = new StringParameterSchema("Host", schemaNamespaceId, "DynamoHostParam");
            schemas.Add(hostSchema);
            var dynamoVersionSchema = new StringParameterSchema("DynamoVersion", schemaNamespaceId, "DynamoVersionParam");
            schemas.Add(dynamoVersionSchema);

            // Define the assets
            var assets = new List<InstanceAsset>();

            // Construct parameter component
            var parameterComponent = new ParameterComponent();

            parameterComponent.AddParameterFromSchema(GetUserId(), userIdSchema);
            parameterComponent.AddParameterFromSchema(DynamoModel.HostAnalyticsInfo.HostName, hostSchema);
            parameterComponent.AddParameterFromSchema(DynamoModel.HostAnalyticsInfo.HostVersion?.ToString(), dynamoVersionSchema);

            // Construct the base component
            var baseComponent = new BaseComponent("DynamoGraphLog");
            // Construct the binary reference component
            var binaryReferenceComponent = new BinaryReferenceComponent(binaryId);

            var instanceAsset = new InstanceAsset(parameterComponent, baseComponent, binaryReferenceComponent, operation);
            assets.Add(instanceAsset);

            // Construct request body
            var body = new UploadAssetsRequestBody(schemas, assets, "insert");
            string bodyJSON = JsonConvert.SerializeObject(body);
            return bodyJSON;
        }

        public static string ConstructCreateExchangeRequestBody()
        {
            // Instantiate attributes we want to pass to the data exchange
            // Note: we want to pass attributes to be able to query the data once collected
            // filtering on these attributes (query based on a custom attribute not currently
            // supported by the FDX API, but it may become available in the future)
            var clientIdAttribute = new Attribute("clientId", "Dynamo");
            var clientVersionAttribute = new Attribute("clientVersion", DynamoModel.HostAnalyticsInfo.HostVersion?.ToString());

            var attributes = new List<Attribute>
            {
                clientIdAttribute,
                clientVersionAttribute
            };

            var exchangeComponent = new ExchangeComponent(attributes);
            string bodyJSON = JsonConvert.SerializeObject(exchangeComponent);
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
        }

        static public string ConvertDynToBase64(string filePath)
        {
            // Read .dyn file as a string
            string sourceFileContent = File.ReadAllText(filePath);

            // Convert the string to a byte array (buffer)
            byte[] stringBuffer = Encoding.UTF8.GetBytes(sourceFileContent);

            // Compress to gzip and then convert to base64 to optimize size
            byte[] compressedBuffer = DataUtilities.Compress(stringBuffer);

            string base64CompressedBuffer = Convert.ToBase64String(compressedBuffer);

            return base64CompressedBuffer;
        }

        void SendToMLDataPipeline(string filePath, RestClient client, string token)
        {
            // STEP 1: CREATE A DATA EXCHANGE CONTAINER ---------------------            
            string exchangeBody = ConstructCreateExchangeRequestBody();

            var createExchangeURL = $"/v1/collections/{CollectionId}/exchanges";
            RestRequest createExchangeRequest = new RestRequest(createExchangeURL);
            createExchangeRequest.AddStringBody(exchangeBody, ContentType.Json);
            AddHeadersToPostRequest(createExchangeRequest, token);
            var exchangeRequestResponse = client.ExecutePost(createExchangeRequest);

            dynamic exchangeRequestResponseBody = JObject.Parse(exchangeRequestResponse.Content);
            // We extract the exchange container id, the collection id and the schemaNamespace id 
            // from the response - these will be consumed by the following API calls.
            ExchangeContainerId = exchangeRequestResponseBody["id"].Value;

            Analytics.TrackEvent(Actions.Export, Categories.DynamoMLDataPipelineOperations, ExchangeContainerId);

            var schemaNamespaceId = exchangeRequestResponseBody["components"]["data"]["insert"]["autodesk.data:exchange.source.default-1.0.0"]["source"]["String"]["id"].Value;

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

            uploadBinaryRequest.AddOrUpdateParameter("text/txt", base64CompressedBuffer, ParameterType.RequestBody);

            var uploadBinaryResponse = fileUploadClient.ExecutePut(uploadBinaryRequest);
            if (uploadBinaryResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                LogMessage.Info("Binary upload failed!");
            }
            LogMessage.Info("Binary upload started!");

            Analytics.TrackEvent(Actions.Export, Categories.DynamoMLDataPipelineOperations, "BinarySize", base64CompressedBuffer.Length);

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
            if (finishBinaryUploadResponse.IsSuccessful)
            {
                LogMessage.Info("Binary upload completed!");
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

        public void SendWorkspaceLog(string filePath)
        {
            // Depending on whether we are using Stage or Prod, a token needs to be retrieved 
            // using client and secret id from the app created in the respective environment.
            var token = GetAuthorizationToken();

            // CollectionId created for Dynamo
            CollectionId = ProductionCollectionID;

            Analytics.TrackEvent(Actions.Export, Categories.DynamoMLDataPipelineOperations, CollectionId);

            var client = new RestClient(ProductionClientUrl);

            try
            {
                SendToMLDataPipeline(filePath, client, token);
            }
            catch (Exception ex)
            {
                LogMessage.Error("Failed to share the workspace with ML data pipeline: " + ex.StackTrace);
            }
        }
    }
}
