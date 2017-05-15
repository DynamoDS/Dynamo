using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Dynamo.LibraryUI.Handlers
{
    class IconUrl
    {
        public static readonly string ServiceEndpoint = "/icons";
        private const string query = @"?path=";
        /// <summary>
        /// Creates IconUrl object by extracting parameters from the given Uri
        /// </summary>
        /// <param name="url">Uri representing the url string</param>
        public IconUrl(Uri url)
        {
            Url = url.AbsoluteUri;

            var assemblyPath = url.Query.Substring(query.Length);
            Path = WebUtility.UrlDecode(assemblyPath); //Decode the path to normal path

            Name = url.Segments.Last();
        }

        /// <summary>
        /// Creates IconUrl object from given icon name and resource path
        /// </summary>
        public IconUrl(string iconName, string resourcePath, bool customNode = false)
        {
            Path = resourcePath;
            Name = iconName + ".Small";
            if (customNode)
            {
                string customizationPath = System.IO.Path.GetDirectoryName(Path);
                customizationPath = Directory.GetParent(customizationPath).FullName;
                customizationPath = System.IO.Path.Combine(customizationPath, "bin", "Package.dll");
                Path = customizationPath;
                if (!File.Exists(customizationPath))
                {
                    Path = "DynamoCore.dll";
                    Name = "DefaultCustomNode.Small";
                }
            }

            Url = string.Format(@"http://localhost{0}/{1}{2}{3}", ServiceEndpoint, Name, query, Path);
        }

        /// <summary>
        /// Path for the image resource assembly
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Image resource name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A simple url string representation
        /// </summary>
        public string Url { get; private set; }
    }
}
