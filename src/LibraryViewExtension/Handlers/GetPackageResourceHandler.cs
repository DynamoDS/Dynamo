using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Dynamo.PackageManager;
using Greg;
using Greg.Requests;
using Greg.Responses;
using Newtonsoft.Json;

namespace Dynamo.LibraryUI.Handlers
{
    class GetPackageResourceHandler : IResourceProvider
    {
        private IGregClient packageClient;
        private bool staticResource = false;

        public GetPackageResourceHandler(bool staticResource)
        {
            this.staticResource = staticResource;
            packageClient = new GregClient(null, "http://dynamopackages.com");
        }

        public string Scheme { get { return "http"; } }

        public bool IsStaticResource { get { return staticResource; } }

        public Stream GetResource(IRequest request, out string extension)
        {
            var uri = new Uri(request.Url);
            var pkgid = uri.AbsolutePath.Split('/').Last(x => !string.IsNullOrEmpty(x));
            var header = new HeaderDownload(pkgid);
            var response = packageClient.ExecuteAndDeserializeWithContent<PackageHeader>(header);
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }
            };
            
            var json = JsonConvert.SerializeObject(response, settings);
            extension = "json";
            return new MemoryStream(Encoding.UTF8.GetBytes(json));
        }
    }
}
