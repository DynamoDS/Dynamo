using System.IO;
using CefSharp;

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
        /// Call this method to get the stream for a given requested resource.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="extension">Output parameter whose value is the extension
        /// of the requested resource. This extension does not contain "." character.</param>
        /// <returns>Returns the stream if the requested resource can be found, or null 
        /// otherwise.</returns>
        Stream GetResource(IRequest request, out string extension);
    }

    /// <summary>
    /// The abstract base class for Resource provider
    /// </summary>
    public abstract class ResourceProviderBase : IResourceProvider
    {
        protected ResourceProviderBase(bool isStatic = true, string scheme = "http")
        {
            IsStaticResource = isStatic;
            Scheme = scheme;
        }

        public bool IsStaticResource { get; private set; }

        public string Scheme { get; private set; }

        public abstract Stream GetResource(IRequest request, out string extension);
    }
}
