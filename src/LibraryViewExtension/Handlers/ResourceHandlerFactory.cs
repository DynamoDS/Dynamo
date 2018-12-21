using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Dynamo.Logging;

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// Implements IResourceHandlerFactory to provide resource handler for a given request
    /// </summary>
    class ResourceHandlerFactory : DefaultResourceHandlerFactory
    {
        private Dictionary<string, IResourceProvider> resourceProviders = new Dictionary<string, IResourceProvider>();
        private HashSet<string> supportedSchemes = new HashSet<string>();
        private DynamoLogger logger;

        //TODO: Remove this after testing.
        //For testing purpose.
        public ResourceHandlerFactory()
        {

        }

        //TODO: Remove this after testing.
        //For testing purpose.
        public ResourceHandlerFactory(DynamoLogger log)
        {
            this.logger = log;
        }

        /// <summary>
        /// This method is called before a resource is loaded, if a valid resource handler
        /// is returned then this resource handler would serve the response of web request.
        /// </summary>
        public override IResourceHandler GetResourceHandler(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request)
        {
            try
            {
                DefaultResourceHandlerFactoryItem handlerItem;

                // Create a handlerItem for the new resource,
                // if the resource has already been loaded don't load it again
                if(!Handlers.TryGetValue(request.Url, out handlerItem))
                {
                    IResourceHandler handler = this.GetResourceHandler(request);

                    // Make sure the handler is unregistered after use
                    // See: https://cefsharp.github.io/api/63.0.0/html/T_CefSharp_DefaultResourceHandlerFactoryItem.htm
                    handlerItem = new DefaultResourceHandlerFactoryItem(handler, true);
                }

                return handlerItem.Handler;
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
