using ACGClientForCEF.AuthProviders;
using ACGClientForCEF.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACGClientForCEF
{
    public interface IACGClientForCEF
    {
        IAuthProvider AuthProvider { get; }
        string BaseUrl { get; }

        CefResponse Execute(CefRequest m);
        CefResponseBody ExecuteAndDeserialize(CefRequest m);
        CefResponseWithContentBody ExecuteAndDeserializeWithContent<dynamic>(CefRequest m);

        void GetGuestSession();
    }
}
