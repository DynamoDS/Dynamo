using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.XPath;
using Dynamo.UI;
using DynamoUtilities;

namespace Dynamo.DSEngine
{
    public class LibraryCustomizationServices
    {
        private static Dictionary<string, bool> triedPaths = new Dictionary<string, bool>();
        private static Dictionary<string, LibraryCustomization> cache = new Dictionary<string, LibraryCustomization>();

        public static LibraryCustomization GetForAssembly(string assemblyPath)
        {
            if (triedPaths.ContainsKey(assemblyPath))
            {
                return triedPaths[assemblyPath] ? cache[assemblyPath] : null;
            }

            var customizationPath = "";
            var resourceAssemblyPath = "";

            XDocument xDocument = null;
            Assembly resAssembly = null;

            if (ResolveForAssembly(assemblyPath, ref customizationPath))
            {
                xDocument = XDocument.Load(customizationPath);
            }

            if (ResolveResourceAssembly(assemblyPath, out resourceAssemblyPath))
            {
                resAssembly = Assembly.LoadFrom(resourceAssemblyPath);
            }

            // We need 'LibraryCustomization' if either one is not 'null'
            if (xDocument != null || (resAssembly != null))
            {
                var c = new LibraryCustomization(resAssembly, xDocument);
                triedPaths.Add(assemblyPath, true);
                cache.Add(assemblyPath, c);
                return c;
            }

            triedPaths.Add(assemblyPath, false);
            return null;

        }

        public static bool ResolveForAssembly(string assemblyLocation, ref string customizationPath)
        {
            try
            {
                if (!DynamoPathManager.Instance.ResolveLibraryPath(ref assemblyLocation))
                {
                    return false;
                }

                var qualifiedPath = Path.GetFullPath(assemblyLocation);
                var fn = Path.GetFileNameWithoutExtension(qualifiedPath);
                var dir = Path.GetDirectoryName(qualifiedPath);

                fn = fn + "_DynamoCustomization.xml";

                customizationPath = Path.Combine(dir, fn);

                return File.Exists(customizationPath);
            }
            catch
            {
                // Just to be sure, that nothing will be crashed.
                customizationPath = "";
                return false;
            }
        }

        public static bool ResolveResourceAssembly(
            string assemblyLocation,
            out string resourceAssemblyPath)
        {
            try
            {
                var qualifiedPath = Path.GetFullPath(assemblyLocation);
                var fn = Path.GetFileNameWithoutExtension(qualifiedPath);
                var dir = Path.GetDirectoryName(qualifiedPath);

                fn = fn + Configurations.ResourcesDLL;

                resourceAssemblyPath = Path.Combine(dir, fn);

                return File.Exists(resourceAssemblyPath);
            }
            catch
            {
                // Just to be sure, that nothing will be crashed.
                resourceAssemblyPath = "";
                return false;
            }
        }
    }

    public class LibraryCustomization
    {
        private XDocument XmlDocument;

        // resourcesReader is a storage of our icons.
        private ResourceReader resourcesReader;
        private Dictionary<string, BitmapImage> cachedIcons;

        internal LibraryCustomization(Assembly assembly, XDocument document)
        {
            this.XmlDocument = document;
            if (assembly != null)
                this.LoadResourceStream(assembly);
        }

        private void LoadResourceStream(Assembly assembly)
        {
            cachedIcons = new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);

            var rsrcNames = assembly.GetManifestResourceNames();
            if (rsrcNames == null || (rsrcNames.Length <= 0))
                return; // Ensure we won't crash.

            Stream stream =
                assembly.GetManifestResourceStream(rsrcNames[0]);

            if (stream == null)
                return;

            resourcesReader = new ResourceReader(stream);
        }

        public string GetNamespaceCategory(string namespaceName)
        {
            var format = "string(/doc/namespaces/namespace[@name='{0}']/category)";
            var obj = XmlDocument.XPathEvaluate(String.Format(format, namespaceName));
            return obj.ToString().Trim();
        }

        internal BitmapImage LoadIconInternal(string iconKey)
        {
            if (cachedIcons.ContainsKey(iconKey))
                return cachedIcons[iconKey];

            if (resourcesReader == null)
            {
                cachedIcons.Add(iconKey, null);
                return null;
            }

            // Gets all images from resReader where they are saved as DictionaryEntries and
            // choose one of them with correct key.
            DictionaryEntry iconData = resourcesReader
                .OfType<DictionaryEntry>()
                .Where(i => i.Key.ToString() == iconKey).FirstOrDefault();

            if (iconData.Key == null || iconData.Value == null)
            {
                cachedIcons.Add(iconKey, null);
                return null;
            }

            MemoryStream memory = new MemoryStream();
            Bitmap bitmap;
            BitmapImage bitmapImage = new BitmapImage();
            bitmap = iconData.Value as Bitmap;
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            cachedIcons.Add(iconKey, bitmapImage);

            return bitmapImage;
        }
    }
}
