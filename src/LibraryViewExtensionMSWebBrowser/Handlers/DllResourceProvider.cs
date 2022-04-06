using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.LibraryViewExtensionMSWebBrowser.Handlers
{
    /// <summary>
    /// A simple DllResourceProvider, which provides embedded resources from the dll.
    /// </summary>
    public class DllResourceProvider : ResourceProviderBase
    {
        /// <summary>
        /// Creates a resource provider to get embedded resources from a given assembly.
        /// </summary>
        /// <param name="baseUrl">The base url from where the request is served.</param>
        /// <param name="rootNamespace">Root namespace for this resource provider.</param>
        /// <param name="assembly">Assembly from where the resources are to be obtained.</param>
        public DllResourceProvider(string baseUrl, string rootNamespace, Assembly assembly = null)
        {
            RootNamespace = rootNamespace;
            BaseUrl = baseUrl;
            Assembly = assembly;
            if (Assembly == null)
                Assembly = Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// Root namespace for this resource provider. For an example, if this provider
        /// serves resources at "Dynamo.LibraryUI.Web.Xxxx" (where "Xxxx" is the actual
        /// name of the resource), then root namespace would be "Dynamo.LibraryUI.Web".
        /// </summary>
        public string RootNamespace { get; private set; }

        /// <summary>
        /// The base url from where the request is served. For example, if this provider 
        /// serves requests at http://localhost/dist/v0.0.1/Xxxx (where "Xxxx" is the name
        /// of the request), then the base url is "http://localhost/dist/v0.0.1".
        /// </summary>
        public string BaseUrl { get; private set; }

        /// <summary>
        /// Assembly from where the resources are to be obtained.
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// Call this method to get the stream for a given requested resource.
        /// </summary>
        /// <param name="url">The requested url.</param>
        /// <param name="extension">Output parameter whose value is the extension
        /// of the requested resource. This extension does not contain "." character.</param>
        /// <returns>Returns the stream if the requested resource can be found, or null 
        /// otherwise.</returns>
        public override Stream GetResource(string url, out string extension)
        {
            extension = "txt";
            var uri = new Uri(url);
            string resourceName;
            var assembly = GetResourceAssembly(uri, out resourceName);
            if (null == assembly)
            {
                return null;
            }

            var stream = assembly.GetManifestResourceStream(resourceName);
            var idx = resourceName.LastIndexOf('.');
            if (idx > 0)
            {
                extension = resourceName.Substring(idx + 1);
            }

            return stream;
        }

        protected virtual Assembly GetResourceAssembly(Uri url, out string resourceName)
        {
            var path = url.AbsoluteUri.Replace(BaseUrl, "").Replace('/', '.');
            resourceName = RootNamespace + path;
            return this.Assembly;
        }
    }
}
