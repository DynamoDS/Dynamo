using System;
using System.IO;
using System.Reflection;
using CefSharp;
#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3002 // Return type is not CLS-compliant

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// A simple DllResourceProvider, which provides embeded resources from the dll.
    /// </summary>
    public class DllResourceProvider : ResourceProviderBase
    {
        /// <summary>
        /// Root namespace from where resource can be searched. 
        /// For example if this provider serves resources at Dynamo.LibraryUI.web.XXXX
        /// where XXXX is name of the actual resource then the Root namespace is 
        /// 'Dynamo.LibraryUI.web'
        /// </summary>
        public string RootNamespace { get; set; }

        /// <summary>
        /// The base Url from where the request is served.
        /// For example, if this provider serves requests at http://localhost/dist/v0.0.1/XXXX
        /// where XXXX is the name of the request then the base Url is http://localhost/dist/v0.0.1
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets resource stream for the given request.
        /// </summary>
        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "txt";
            var uri = new Uri(request.Url);
            string resourceName;
            var assembly = GetResourceAssembly(uri, out resourceName);
            if (null == assembly) return null;

            var stream = assembly.GetManifestResourceStream(resourceName);
            var idx = resourceName.LastIndexOf('.');
            if (idx > 0)
            {
                extension = resourceName.Substring(idx);
            }
            
            return stream;
        }

        protected virtual Assembly GetResourceAssembly(Uri url, out string resourceName)
        {
            var path = url.AbsoluteUri.Replace(BaseUrl, "").Replace('/', '.');
            resourceName = RootNamespace + path;
            return Assembly.GetExecutingAssembly();
        }
    }
}
