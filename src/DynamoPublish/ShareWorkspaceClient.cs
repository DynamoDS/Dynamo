using Greg;
using Greg.Requests;
using Greg.Responses;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Publish
{
    public class ShareWorkspaceClient
    {
        private IAuthProvider _authenticationProvider;
        private string _baseUrl;
        private IGregClient _client;

        public IAuthProvider AuthenticationProvider
        {
            get { return _authenticationProvider; }
        }

        public string BaseUrl
        {
            get { return _baseUrl; }
        }

        public ShareWorkspaceClient(string baseUrl, IAuthProvider authProvider)
        {
            _baseUrl = baseUrl;
            _authenticationProvider = authProvider;
            _client = new GregClient(_authenticationProvider, _baseUrl);
        }

        public bool GetTermsOfUseAcceptanceStatus()
        {
            return ExecuteTermsOfUseCall(true);
        }

        public bool SetTermsOfUseAcceptanceStatus()
        {
            return ExecuteTermsOfUseCall(false);
        }

        private bool ExecuteTermsOfUseCall(bool queryAcceptanceStatus)
        {
            try
            {
                var request = new TermsOfUse(queryAcceptanceStatus);
                var response = _client.ExecuteAndDeserializeWithContent<TermsOfUseStatus>(request);
                return response.content.accepted;
            }
            catch
            {
                return false;
            }
        }
    }
}
