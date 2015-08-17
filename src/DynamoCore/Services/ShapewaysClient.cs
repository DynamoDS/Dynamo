using Newtonsoft.Json;
using RestSharp;
using RestSharp.Contrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Services
{
    public class ShapewaysClient
    {
        public string LoginUrl { get; private set; }
        public string Secret { get; private set; }
        public string Token { get; private set; }
        public string Verifier { get; private set; }
        RestClient Client;

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

        public void RequestToken()
        {
            var request = new RestRequest("/loginShapewaysDynamo")
            {
                Method = Method.GET,
                RequestFormat = RestSharp.DataFormat.Json
            };

            var res = Client.Execute(request).Content;
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(res);
            Secret = obj["secret"];
            LoginUrl = obj["url"];
        }

        public string UploadModel(string rawData) 
        {
            var bodyRequest = new
            {
                token = Token,
                secret = Secret,
                verifier = Verifier,
                file = rawData,
                fileName = "TestDynamo.stl"
            };
            var req = new RestRequest("/postShapewaysDynamo")
            {
                Method = Method.POST,
                RequestFormat = RestSharp.DataFormat.Json
            };
            req.AddBody(bodyRequest);

            return Client.Execute(req).Content;
        }
    }
}
