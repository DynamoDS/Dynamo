using Greg;
using Greg.Requests;
using Greg.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dynamo.DynamoPackagesUI.Utilities
{
    /// <summary>
    /// ACG API Client, initialize rest and file client using PackageManagerClient type from Greg
    /// </summary>
    internal class DynamoPackagesUIClient
    {
        private readonly PackageManagerClient restClient;

        private const string ACG_API_URL = "https://api.acg.autodesk.com/api/v2";
        private const string ACG_STORAGE_URL = "https://storage.123dapp.com/api/v2";

        public DynamoPackagesUIClient()
        {
            restClient = new PackageManagerClient(null, ACG_API_URL, ACG_STORAGE_URL);
        }

        public ResponseWithContentBody<dynamic> ExecuteAndDeserializeDynamoRequest(Request req)
        {
            return restClient.ExecuteAndDeserializeWithContent<dynamic>(req);
        }

        internal Response ExecuteDynamoRequest(Request req)
        {
            return restClient.Execute(req);
        }

        public string GetFileFromResponse(Response gregResponse)
        {
            return restClient.GetFileFromResponse(gregResponse);
        }
     
        //internal Dictionary<string, string> GetSession()
        //{
        //    if (cefClient.AuthProvider != null)
        //        return cefClient.AuthProvider.SessionData;
        //    else
        //        return new Dictionary<string, string>();
        //}
    }
}
