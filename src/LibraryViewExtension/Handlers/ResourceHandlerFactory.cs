using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// Implements IResourceHandlerFactory to provide resource handler for a given request
    /// </summary>
    class ResourceHandlerFactory : DefaultResourceHandlerFactory
    {
        private Dictionary<string, IResourceProvider> resourceProviders = new Dictionary<string, IResourceProvider>();
        private HashSet<string> supportedSchemes = new HashSet<string>();

        /// <summary>
        /// This method is called before a resource is loaded, if a valid resource handler
        /// is returned then this resource handler would serve the response of web request.
        /// </summary>
        public override IResourceHandler GetResourceHandler(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request)
        {
            try
            {
                IResourceHandler handler;
                if(!Handlers.TryGetValue(request.Url, out handler))
                {
                    handler = this.GetResourceHandler(request);
                }

                return handler;
            }
            finally
            {
                request.Dispose();
            }
        }

        /// <summary>
        /// Clients can register a resource provider for a given base url. The 
        /// registered resource provider will be used to serve the request containing
        /// the given base url
        /// </summary>
        /// <param name="baseurl">Base url string in a format of 'http://domain/base/request'</param>
        /// <param name="provider">The resource provider instance that can serve all the
        /// requests starting with the given base url.</param>
        public void RegisterProvider(string baseurl, IResourceProvider provider)
        {
            supportedSchemes.Add(provider.Scheme);
            resourceProviders.Add(baseurl, provider);
        }

        /// <summary>
        /// Creates resource handler for the given request
        /// </summary>
        private IResourceHandler GetResourceHandler(IRequest request)
        {
            Uri uri = new Uri(request.Url);
            
            //Check if the given scheme is supported
            if(!supportedSchemes.Any(s=>s.Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }
            
            var folders = GetAllSubPath(uri);
            foreach (var item in folders)
            {
                IResourceProvider provider;
                if (resourceProviders.TryGetValue(item, out provider))
                {
                    string extension;
                    var stream = provider.GetResource(request, out extension);
                    if (stream == null) return null;
                    
                    var handler = ResourceHandler.FromStream(stream, ResourceHandler.GetMimeType(extension));
                    if (provider.IsStaticResource)
                    {
                        this.RegisterHandler(request.Url, handler);
                    }
                    return handler;
                }
            }

            return null;
        }

        private string[] GetAllSubPath(Uri uri)
        {
            var path = uri.AbsolutePath;
            var folders = path.Split('/');
            var subfolders = new List<string>();

            foreach (var item in folders)
            {
                if (string.IsNullOrEmpty(item)) continue;

                var lastItem = subfolders.LastOrDefault();
                subfolders.Add(string.Format("{0}/{1}", lastItem, item));
            }

            return subfolders.ToArray();
        }
    }
}
