using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACGClientForCEF.AuthProviders
{
    public class BasicProvider : IAuthProvider
    {
        private readonly string _username;
        private readonly string _password;
        public Dictionary<string, string> SessionData { get; set; }

        public BasicProvider(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public void SignRequest(ref RestRequest m, RestClient client)
        {
            // Use the HttpBasicAuthenticator to write the auth information
            // into the request header. This coincides with with the "basic"
            // authentication strategy on Greg.
            client.Authenticator = new HttpBasicAuthenticator(_username, _password);
        }

        public void Logout()
        {
        }

        public bool Login()
        {
            return true;
        }

        public LoginState LoginState
        {
            get { return LoginState.LoggedIn; }
        }

        public string Username
        {
            get { return _username; }
        }

        public event Func<object, Dictionary<string, string>> RequestLogin;
        public event Action<LoginState> LoginStateChanged;
    }
}
