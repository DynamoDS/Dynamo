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

namespace Dynamo.LibraryViewExtensionMSWebBrowser.Handlers
{
    /// <summary>
    /// Implements resource provider for icons
    /// </summary>
    class IconResourceProvider : ResourceProviderBase
    {
        private const string imagesSuffix = "Images";
        private IPathManager pathManager;
        private string defaultIconDataString;
        private string defaultIconName;
        private DllResourceProvider embeddedDllResourceProvider;
        //TODO remove these at some point in future after Dynamo 2.6 release.
        private MethodInfo getForAssemblyMethodInfo;
        private PropertyInfo LibraryCustomizationResourceAssemblyProperty;
        private Type LibraryCustomizationType;

        private LibraryViewCustomization customization;
        //internal cache of url to base64 encoded image data. (url,tuple<data,extension>)
        private Dictionary<string, Tuple<string,string>> urlToBase64DataCache = new Dictionary<string, Tuple<string, string>>();

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
            //TODO replace with direct calls after Dynamo 2.6 is released.
            this.getForAssemblyMethodInfo = dynCore.GetType("Dynamo.Engine.LibraryCustomizationServices").GetMethod("GetForAssembly", BindingFlags.Static | BindingFlags.Public);
            this.LibraryCustomizationType = dynCore.GetType("Dynamo.Engine.LibraryCustomization");
            this.LibraryCustomizationResourceAssemblyProperty = LibraryCustomizationType.GetProperty("ResourceAssembly", BindingFlags.Instance|BindingFlags.Public);
        }

        /// <summary>
        /// Additional constructor used to access customization resources and dll resource provider directly during icon lookup.
        /// </summary>
        /// <param name="pathManager"></param>
        /// <param name="dllResourceProvider"></param>
        /// <param name="customization"></param>
        /// <param name="defaultIcon"></param>
        public IconResourceProvider(IPathManager pathManager, DllResourceProvider dllResourceProvider,LibraryViewCustomization customization, string defaultIcon = "default-icon.svg") :
            this(pathManager, defaultIcon)
        {
            this.customization = customization;
            this.embeddedDllResourceProvider = dllResourceProvider;
        }


        /// <summary>
        /// Retrieves the resource for a given url as a base64 encoded string.
        /// ie data:image/png;base64, stringxxxyyyzzz
        /// </summary>
        /// <param name="url">url for the requested icon</param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns></returns>
        public string GetResourceAsString(string url, out string extension)
        {
            //sometimes the urls have "about:" added to them - remove this
            //and do it before checking cache.
            //https://en.wikipedia.org/wiki/About_URI_scheme
            if (url.StartsWith("about:"))
            {
                url = url?.Replace("about:", string.Empty);
            }

            if (!String.IsNullOrEmpty(url) && urlToBase64DataCache.ContainsKey(url))
            {
                var cachedData = urlToBase64DataCache[url];
                extension = cachedData.Item2;
                return cachedData.Item1;
            }
           
            //because we modify the spec the icon urls may have been replaced by base64 encoded images
            //if thats the case, no need to look them up again from disk, just return the string.
            if (!String.IsNullOrEmpty(url) && url.Contains("data:image/"))
            {
                var match = Regex.Match(url, @"data:(?<type>.+?);base64,(?<data>.+)");
                var base64Data = match.Groups["data"].Value;
                var contentType = match.Groups["type"].Value;
                //image/png -> png
                extension = contentType.Split('/').Skip(1).FirstOrDefault();
                urlToBase64DataCache.Add(url, Tuple.Create(base64Data, extension));
                return base64Data;
            }
          

            var base64String = string.Empty;
            extension = "png";
            //Create IconUrl to parse the request.Url to icon name and path.
            if (String.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            //before trying to create a uri we have to handle resources that might 
            //be embedded into the resources.dlls
            //these paths will start with ./dist
            if (url.StartsWith(@"./dist"))
            {
                //make relative url a full uri
                var urlAbs = url.Replace(@"./dist", @"http://localhost/dist");
                var ext = string.Empty;
                var stream = embeddedDllResourceProvider.GetResource(urlAbs, out ext);
                if (stream != null)
                {
                    extension = ext;
                    base64String = GetIconAsBase64(stream, ext);
                }
            }
            else
            {
                //TODO check if absolute instead of using try/catch
                try
                {
                    var icon = new IconUrl(new Uri(url));
                    base64String = GetIconAsBase64(icon, out extension);

                }
                catch (Exception e)
                {
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
                base64String = GetDefaultIconBase64(out extension);
                
            }
            urlToBase64DataCache.Add(url, Tuple.Create(base64String, extension));
            return base64String;
        }

        /// <summary>
        /// Do not use this, in most cases you want to call GetResourceAsString() directly.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public override Stream GetResource(string url, out string extension)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(GetResourceAsString(url, out extension));
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Gets the string for a default icon, to be used when no icon found 
        /// for a given request. This keeps a cache of the string to reuse next
        /// time.
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns>A valid Stream if the icon resource found successfully else null.</returns>
        private string GetDefaultIconBase64(out string extension)
        {
            extension = Path.GetExtension(defaultIconName).Replace(".", "");
            if (defaultIconDataString == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resource = assembly.GetManifestResourceNames().FirstOrDefault(s => s.Contains(defaultIconName));

                if (string.IsNullOrEmpty(resource)) return null;

                defaultIconName = resource;
                var reader = new StreamReader(assembly.GetManifestResourceStream(defaultIconName));
                defaultIconDataString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(reader.ReadToEnd()));
                reader.Dispose();
            }

            return defaultIconDataString;
        }

        /// <summary>
        /// Gets the base64 encoded string for the given icon resource url. This method has the potential to be very slow
        /// as it will create a new resourceManager for a given assembly and search that assembly, which may require disk access.
        /// </summary>
        /// <param name="icon">Icon Url</param>
        /// <param name="extension">Returns the extension to describe the type of resource.</param>
        /// <returns>a base64 encoded string version of an image if found else null.</returns>
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

        /// <summary>
        /// Gets icon as base64 string from a given stream which points to some image (or font) data.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        private string GetIconAsBase64(Stream stream, string extension)
        {
            string base64String = string.Empty;
            if (extension.ToLower().Contains("svg"))
            {
                var reader = new BinaryReader(stream);
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
