using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACGClientForCEF.AuthProviders
{
    public interface IAuthProvider
    {
        event Func<object, Dictionary<string, string>> RequestLogin;
        event Action<LoginState> LoginStateChanged;
        Dictionary<string, string> SessionData { get; set; }

        LoginState LoginState { get; }
        string Username { get; }

        void SignRequest(ref RestRequest m, RestClient client);

        void Logout();
        bool Login();
    }
}
