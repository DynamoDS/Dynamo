using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using CefSharp;
using Dynamo.Engine;
using Dynamo.Interfaces;

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// Implements resource provider for icons
    /// </summary>
    class IconResourceProvider : ResourceProviderBase
    {
        private const string imagesSuffix = "Images";
        private IPathManager pathManager;
        private Stream defaultIconStream;
        private string defaultIconName;

        /// <summary>
        /// Default constructor for the IconResourceProvider
        /// </summary>
        /// <param name="pathManager">Path manager instance to resolve the 
        /// customization resource path</param>
        /// <param name="defaultIcon">Name of the default icon (including 
        /// extension) to be used when it can't find the requested icon</param>
        public IconResourceProvider(IPathManager pathManager, string defaultIcon = "default-icon.svg") : base(true)
        {
            this.pathManager = pathManager;
            defaultIconName = defaultIcon;
        }

        /// <summary>
        /// Gets the stream for the given icon resource request.
        /// </summary>
        /// <param name="request">Request object for the icon resource.</param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns>A valid Stream if the icon resource found successfully else null.</returns>
        public override Stream GetResource(IRequest request, out string extension)
        {
            //Create IconUrl to parse the request.Url to icon name and path.
            var icon = new IconUrl(new Uri(request.Url));
            
            var stream = GetIconStream(icon, out extension);
            if (stream == null)
                stream = GetDefaultIconStream(out extension);

            return stream;
        }

        /// <summary>
        /// Gets the stream for a default icon, to be used when no icon found 
        /// for a given request. This keeps a cache of the stream to reuse next
        /// time.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns>A valid Stream if the icon resource found successfully else null.</returns>
        private Stream GetDefaultIconStream(out string extension)
        {
            extension = Path.GetExtension(defaultIconName);
            if (defaultIconStream == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resource = assembly.GetManifestResourceNames().FirstOrDefault(s => s.Contains(defaultIconName));

                if (string.IsNullOrEmpty(resource)) return null;

                defaultIconName = resource;
                defaultIconStream = assembly.GetManifestResourceStream(defaultIconName);
            }

            return defaultIconStream;
        }

        /// <summary>
        /// Gets the stream for the given icon resource
        /// </summary>
        /// <param name="icon">Icon Url</param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns>A valid Stream if the icon resource found successfully else null.</returns>
        private Stream GetIconStream(IconUrl icon, out string extension)
        {
            extension = "png";

            var path = Path.GetFullPath(icon.Path); //Get full path if it's a relative path.
            var libraryCustomization = LibraryCustomizationServices.GetForAssembly(path, pathManager, true);
            if (libraryCustomization == null)
                return null;

            var assembly = libraryCustomization.ResourceAssembly;
            if (assembly == null)
                return null;

            // "Name" can be "Some.Assembly.Name.customization" with multiple dots, 
            // we are interested in removal of the "customization" part and the middle dots.
            var temp = assembly.GetName().Name.Split('.');
            var assemblyName = String.Join("", temp.Take(temp.Length - 1));
            var rm = new ResourceManager(assemblyName + imagesSuffix, assembly);

            using (var image = (Bitmap)rm.GetObject(icon.Name))
            {
                if (image == null) return null;

                var stream = new MemoryStream();
                image.Save(stream, ImageFormat.Png);

                return stream;
            }
        }
    }
}
