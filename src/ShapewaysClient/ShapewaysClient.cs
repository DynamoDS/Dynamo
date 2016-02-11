using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions.MonoHttp;

namespace ShapewaysClient
{
    /// <summary>
    ///     Class provides the interaction between Dynamo client and Shapeways API
    /// </summary>
    internal class ShapewaysClient
    {
        /// <summary>
        /// LoginUrl property
        /// </summary>
        /// <value>Contains URL to Shapeways login UI</value>
        public string LoginUrl { get; private set; }

        /// <summary>
        /// Secret property
        /// </summary>
        /// <value>Contains oauth_secret param</value>
        public string Secret { get; private set; }

        /// <summary>
        /// Token property
        /// </summary>
        /// <value>Contains oauth_token param</value>
        public string Token { get; private set; }

        /// <summary>
        /// Verifier property
        /// </summary>
        /// <value>Contains oauth_verifier param</value>
        public string Verifier { get; private set; }

        private readonly RestClient Client;

        public ShapewaysClient(string serverUrl)
        {
            Client = new RestClient(serverUrl);
        }

        public void SetToken(string query)
        {
            var result = HttpUtility.ParseQueryString(query);
            Token = result.Get("oauth_token");
            Verifier = result.Get("oauth_verifier");
        }

        /// <summary>
        /// Get Oauth request token to authorized with Shapeways service
        /// </summary>
        public void RequestToken()
        {
            var request = new RestRequest("/loginPrintDynamo/shapeways")
            {
                Method = Method.GET,
                RequestFormat = RestSharp.DataFormat.Json
            };

            var res = Client.Execute(request).Content;
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(res);
            Secret = obj["secret"];
            LoginUrl = obj["url"];
        }

        /// <summary>
        /// Upload an STL model to Shapeways service using obtained credentials
        /// </summary>
        /// <param name="rawData">Raw STL geometry data</param>
        /// <param name="filename">Gives a name to your uploaded file</param>
        public string UploadModel(string rawData, string filename) 
        {
            var bodyRequest = new
            {
                token = Token,
                secret = Secret,
                verifier = Verifier,
                file = rawData,
                fileName = filename
            };
            var req = new RestRequest("/printUploadDynamo/shapeways")
            {
                Method = Method.POST,
                RequestFormat = RestSharp.DataFormat.Json
            };
            req.AddBody(bodyRequest);

            return Client.Execute(req).Content;
        }
    }
}
