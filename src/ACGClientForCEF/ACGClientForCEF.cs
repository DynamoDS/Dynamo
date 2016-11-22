using ACGClientForCEF.AuthProviders;
using ACGClientForCEF.Requests;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACGClientForCEF
{
    public class ACGClientForCEF : IACGClientForCEF
    {
        private readonly RestClient _client;
        private readonly RestClient _fileClient;

        public string BaseUrl { get { return _client.BaseUrl.ToString(); } }

        public readonly IAuthProvider _authProvider;
        public IAuthProvider AuthProvider
        {
            get { return _authProvider; }
        }

        public ACGClientForCEF(IAuthProvider provider, string packageManagerUrl, string fileStorageUrl)
        {
            _authProvider = provider;
            _client = new RestClient(packageManagerUrl);
            _fileClient = new RestClient(fileStorageUrl);
        }

        private IRestResponse ExecuteInternal(CefRequest m)
        {
            var req = new RestRequest(m.Path, m.HttpMethod);

            m.Build(ref req);

            if (m.RequiresAuthorization)
            {
                /*
                if (m.HttpMethod != Method.GET || m.Path.Contains("members"))
                {
                    
                    if ((AuthProvider.SessionData == null) && !AuthProvider.SessionData.ContainsKey("guest-session"))
                    {
                        DynamoRequest guestSessionReq = new DynamoRequest("session?afc=DY1ON1", Method.GET);
                        var tempReq = new RestRequest(guestSessionReq.Path, guestSessionReq.HttpMethod);
                        var response = _client.Execute(tempReq);
                        dynamic res = JsonConvert.DeserializeObject<dynamic>(response.Content);

                        if (AuthProvider.SessionData == null)
                            AuthProvider.SessionData = new System.Collections.Generic.Dictionary<string, string>();

                        AuthProvider.SessionData.Add("guest-session", res.SESSION.ToString());
                    }
                    AuthProvider.SignRequest(ref req, _client);
                }
                else*/
                if (m.HttpMethod == Method.GET && !m.Path.Contains("members"))
                    SignRequest(req);
                else
                    AuthProvider.SignRequest(ref req, _client);
            }
            if (m.fileRequest)
                return _fileClient.Execute(req);
            else
                return _client.Execute(req);
        }

        public void SignRequest(RestRequest m)
        {
            m.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            m.AddHeader("X-AFC", "DY1ON1");
        }

        public CefResponse Execute(CefRequest m)
        {
            var res = new CefResponse(ExecuteInternal(m));
            return res;
        }

        public CefResponseBody ExecuteAndDeserialize(CefRequest m)
        {
            var res = Execute(m).Deserialize();
            res.success = true;
            return res;
        }

        public CefResponseWithContentBody ExecuteAndDeserializeWithContent<dynamic>(CefRequest m)
        {
            var response = this.ExecuteInternal(m);
            var res = new ResponseWithContent(response).DeserializeWithContent();
            res.success = true;
            return res;
        }

        public void GetGuestSession()
        {
            DynamoRequest guestSessionReq = new DynamoRequest("session?afc=DY1ON1", Method.GET);
            var tempReq = new RestRequest(guestSessionReq.Path, guestSessionReq.HttpMethod);
            var response = _client.Execute(tempReq);
            dynamic res = JsonConvert.DeserializeObject<dynamic>(response.Content);

            if (AuthProvider.SessionData == null)
                AuthProvider.SessionData = new System.Collections.Generic.Dictionary<string, string>();

            AuthProvider.SessionData.Add("guest-session", res.SESSION.ToString());
        }

    }
}
