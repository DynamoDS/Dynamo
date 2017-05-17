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
        /// Root namespace for this resource provider. For an example, if this provider
        /// serves resources at "Dynamo.LibraryUI.Web.Xxxx" (where "Xxxx" is the actual
        /// name of the resource), then root namespace would be "Dynamo.LibraryUI.Web".
        /// </summary>
        public string RootNamespace { get; set; }

        /// <summary>
        /// The base url from where the request is served. For example, if this provider 
        /// serves requests at http://localhost/dist/v0.0.1/Xxxx (where "Xxxx" is the name
        /// of the request), then the base url is "http://localhost/dist/v0.0.1".
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Call this method to get the stream for a given requested resource.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="extension">Output parameter whose value is the extension
        /// of the requested resource. This extension does not contain "." character.</param>
        /// <returns>Returns the stream if the requested resource can be found, or null 
        /// otherwise.</returns>
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
