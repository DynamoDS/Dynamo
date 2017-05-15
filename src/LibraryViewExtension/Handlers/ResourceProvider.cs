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
        /// Gets a resource stream and an extension to define mime type for the given request.
        /// </summary>
        /// <param name="request">Web request</param>
        /// <param name="extension">output data stream extension</param>
        /// <returns>Resource stream</returns>
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
