using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACGClientForCEF.Requests
{
    public abstract class CefRequest
    {
        public readonly bool fileRequest;
        internal string fileToUpload;
        public CefRequest()
        {
            this.ForceAuthentication = false;
        }

        public CefRequest(bool fileRequest, string fileToUpload)
        {
            this.ForceAuthentication = false;
            this.fileRequest = fileRequest;
            this.fileToUpload = fileToUpload;
        }

        public bool RequiresAuthorization
        {
            get
            {
                return true;
                //return HttpMethod == Method.POST || HttpMethod == Method.PUT || ForceAuthentication;
            }
        }

        public abstract string Path { get; }

        public abstract Method HttpMethod { get; }

        public CefRequestBody RequestBody { get; set; }

        public bool ForceAuthentication { get; set; }

        internal abstract void Build(ref RestRequest request);
    }
}
