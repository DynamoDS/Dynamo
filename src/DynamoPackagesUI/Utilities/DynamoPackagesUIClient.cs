using ACGClientForCEF;
using ACGClientForCEF.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoPackagesUI.Utilities
{
    internal class DynamoPackagesUIClient
    {
        private readonly IACGClientForCEF cefClient;
        private readonly IACGClientForCEF cefFileClient;

        private const string ACG_API_URL = "https://api.acg.autodesk.com/api/v2";
        private const string ACG_STORAGE_URL = "https://storage.123dapp.com/api/v2";

        public DynamoPackagesUIClient()
        {
            cefClient = new ACGClientForCEF.ACGClientForCEF(null, ACG_API_URL, ACG_STORAGE_URL);
        }

        internal CefResponseWithContentBody ExecuteAndDeserializeDynamoCefRequest(DynamoRequest req)
        {
            return cefClient.ExecuteAndDeserializeWithContent<ACGClientForCEF.CefResponseWithContentBody>(req);
        }

        internal CefResponse ExecuteDynamoCefRequest(DynamoRequest req)
        {
            return cefClient.Execute(req);
        }

        internal Dictionary<string, string> GetSession()
        {
            if (cefClient.AuthProvider != null)
                return cefClient.AuthProvider.SessionData;
            else
                return new Dictionary<string, string>();
        }

        internal void GetGuestSession()
        {
            cefClient.GetGuestSession();
        }
    }
}
