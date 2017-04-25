using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
#pragma warning disable CS3001 // Argument type is not CLS-compliant
#pragma warning disable CS3002 // Return type is not CLS-compliant

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// Interface that provides resource stream for a web given request
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// The request scheme supported by this resource provider, it could be 'http' or 'https'
        /// </summary>
        string Scheme { get; }

        /// <summary>
        /// Checks if it serves only static resources
        /// </summary>
        bool IsStaticResource { get; }

        /// <summary>
        /// Gets a resource stream and an extension to define mime type for the given request.
        /// </summary>
        /// <param name="request">Web request</param>
        /// <param name="extension">output data stream extension</param>
        /// <returns>Resource stream</returns>
        Stream GetResource(IRequest request, out string extension);
    }

    /// <summary>
    /// A simple DllResourceProvider, which provides embeded resources from the dll.
    /// </summary>
    public class DllResourceProvider : IResourceProvider
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
        /// Supports only http scheme
        /// </summary>
        public string Scheme { get { return "http"; } }

        /// <summary>
        /// Supports only static resources
        /// </summary>
        public bool IsStaticResource { get { return true; } }

        /// <summary>
        /// Gets resource stream for the given request.
        /// </summary>
        public Stream GetResource(IRequest request, out string extension)
        {
            var uri = new Uri(request.Url);
            var path = uri.AbsoluteUri.Replace(BaseUrl, "").Replace('/', '.');
            var resourceName = RootNamespace + path;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            var idx = resourceName.LastIndexOf('.');
            if (idx > 0)
            {
                extension = resourceName.Substring(idx);
            }
            else
            {
                extension = "txt";
            }

            return stream;
        }
    }
}
