using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dynamo.Engine;
using Dynamo.Interfaces;
using Dynamo.Models;

namespace Dynamo.LibraryViewExtensionMSWebView.Handlers
{
    /// <summary>
    /// Implements resource provider for icons
    /// </summary>
    class IconResourceProvider : ResourceProviderBase
    {
        private const string imagesSuffix = "Images";
        private IPathManager pathManager;
        private string defaultIconString;
        private string defaultIconName;
        private DllResourceProvider embeddedDllResourceProvider;
        private MethodInfo getForAssemblyMethodInfo;
        private PropertyInfo LibraryCustomizationResourceAssemblyProperty;
        private Type LibraryCustomizationType;
        private LibraryViewCustomization customization;


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
            var dynCore = typeof(DynamoModel).Assembly;
            this.getForAssemblyMethodInfo = dynCore.GetType("Dynamo.Engine.LibraryCustomizationServices").GetMethod("GetForAssembly", BindingFlags.Static | BindingFlags.Public);
            this.LibraryCustomizationType = dynCore.GetType("Dynamo.Engine.LibraryCustomization");
            this.LibraryCustomizationResourceAssemblyProperty = LibraryCustomizationType.GetProperty("ResourceAssembly", BindingFlags.Instance|BindingFlags.Public);
        }

        public IconResourceProvider(IPathManager pathManager, DllResourceProvider dllResourceProvider,LibraryViewCustomization customization, string defaultIcon = "default-icon.svg") :
            this(pathManager, defaultIcon)
        {
            this.customization = customization;
            this.embeddedDllResourceProvider = dllResourceProvider;
        }


        /// <summary>
        /// Gets the stream for the given icon resource request.
        /// </summary>
        /// <param name="request">Request object for the icon resource.</param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns>A valid Stream if the icon resource found successfully else null.</returns>
        public string GetResourceAsString(string url, out string extension)
        {

#if DEBUG
            Console.WriteLine(url);
#endif
            //because we modify the spec the icon urls may have been replaed by base64 enoded images
            //if thats the case, no need to look them up again from disk, just return the string.
            if (!String.IsNullOrEmpty(url) && url.Contains("data:image/"))
            {
                var match = Regex.Match(url, @"data:(?<type>.+?);base64,(?<data>.+)");
                var base64Data = match.Groups["data"].Value;
                var contentType = match.Groups["type"].Value;
                //image/png -> png
                extension = contentType.Split('/').Skip(1).FirstOrDefault();
                return base64Data;
            }
            var base64String = string.Empty;
            extension = "png";
            //Create IconUrl to parse the request.Url to icon name and path.
            if (String.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            //before trying to create a uri - we have to handle resources that might 
            //be embedded into the resources.dlls
            //these paths will start with ./dist
            if (url.StartsWith(@"./dist"))
            {
                //make relative url a full uri
                url = url.Replace(@"./dist", @"http://localhost/dist");
                var ext = string.Empty;
                var stream = embeddedDllResourceProvider.GetResource(url, out ext);
                if (stream != null)
                {
                    extension = ext;
                    base64String = GetIconAsBase64(stream, ext);
                }
            }
            else
            {
                try
                {
                    var icon = new IconUrl(new Uri(url));
                    base64String = GetIconAsBase64(icon, out extension);

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"{e.Message} {url}");
                    //look in resources for registered path and just use the stream directly
                    var resourceDict = this.customization.Resources.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    if (resourceDict.ContainsKey(url))
                    {
                        var fileExtension = System.IO.Path.GetExtension(url);
                        extension = fileExtension;
                        base64String = GetIconAsBase64(resourceDict[url], fileExtension);
                    }
                }
            }
            if (base64String == null)
            {
                //TODO this might need to use the dllresprovider as well.
                base64String = GetDefaultIconBase64(out extension);
            }

            return base64String;
        }

        public override Stream GetResource(string url, out string extension)
        {
            //TODO unify.
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the stream for a default icon, to be used when no icon found 
        /// for a given request. This keeps a cache of the stream to reuse next
        /// time.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns>A valid Stream if the icon resource found successfully else null.</returns>
        private string GetDefaultIconBase64(out string extension)
        {
            extension = Path.GetExtension(defaultIconName).Replace(".", "");
            if (defaultIconString == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resource = assembly.GetManifestResourceNames().FirstOrDefault(s => s.Contains(defaultIconName));

                if (string.IsNullOrEmpty(resource)) return null;

                defaultIconName = resource;
                var reader = new StreamReader(assembly.GetManifestResourceStream(defaultIconName));
                defaultIconString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(reader.ReadToEnd()));
                reader.Dispose();
            }

            return defaultIconString;
        }

        /// <summary>
        /// Gets the stream for the given icon resource
        /// </summary>
        /// <param name="icon">Icon Url</param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns>A valid Stream if the icon resource found successfully else null.</returns>
        private string GetIconAsBase64(IconUrl icon, out string extension)
        {
            extension = "png";

            var path = Path.GetFullPath(icon.Path); //Get full path if it's a relative path.
            var libraryCustomization = getForAssemblyMethodInfo.Invoke(null,new object[] { path, pathManager, true });
            if (libraryCustomization == null)
                return null;

            var assembly = LibraryCustomizationResourceAssemblyProperty.GetValue(libraryCustomization) as Assembly;
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

                var tempstream = new MemoryStream();

                image.Save(tempstream, image.RawFormat);
                byte[] imageBytes = tempstream.ToArray();
                tempstream.Dispose();
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        private string GetIconAsBase64(Stream stream, string extension)
        {
            string base64String = string.Empty;
            if (extension.ToLower().Contains("svg"))
            {
                var reader = new BinaryReader(stream);
                //TODO will fail for very large svgs....
                var imageBytes = reader.ReadBytes((int)stream.Length);
                base64String = Convert.ToBase64String(imageBytes);
                reader.Dispose();
            }

            else if (extension.ToLower().Contains("png"))
            {
                var reader = new BinaryReader(stream);
                var imageBytes = reader.ReadBytes((int)stream.Length);
                base64String = Convert.ToBase64String(imageBytes);
                reader.Dispose();
            }

            else if (extension.ToLower().Contains("ttf") || extension.ToLower().Contains("woff"))
            {
                var reader = new BinaryReader(stream);
                var fontBytes = reader.ReadBytes((int)stream.Length);
                base64String = Convert.ToBase64String(fontBytes);
                reader.Dispose();
            }

            return base64String;

        }

    }
}
