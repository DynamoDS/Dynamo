using System.IO;

namespace Dynamo.LibraryViewExtensionWebView2.Handlers
{
    /// <summary>
    /// The abstract base class for Resource provider
    /// </summary>
    public abstract class ResourceProviderBase
    {
        protected ResourceProviderBase(bool isStatic = true, string scheme = "http")
        {
            IsStaticResource = isStatic;
            Scheme = scheme;
        }
        public bool IsStaticResource { get; private set; }

        public string Scheme { get; private set; }

        public abstract Stream GetResource(string url, out string extension);
    }
}
