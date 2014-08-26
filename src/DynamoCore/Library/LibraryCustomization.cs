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
            if (ResolveForAssembly(assemblyPath, ref customizationPath) &&
                ResolveResourceAssembly(assemblyPath, ref resourceAssemblyPath))
            {
                var c = new LibraryCustomization(Assembly.LoadFrom(resourceAssemblyPath),
                    XDocument.Load(customizationPath));
                triedPaths.Add(assemblyPath, true);
                cache.Add(assemblyPath, c);
                return c;
            }

            // For those, which don't have LibraryCustomization e.g. CoreNodesUI.dll
            if (ResolveResourceAssembly(assemblyPath, ref resourceAssemblyPath))
            {
                var c = new LibraryCustomization(Assembly.LoadFrom(resourceAssemblyPath));
                triedPaths.Add(assemblyPath, true);
                cache.Add(assemblyPath, c);
                return c;
            }

            triedPaths.Add(assemblyPath, false);
            return null;
        }

        public static bool ResolveForAssembly(string assemblyLocation, ref string customizationPath)
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

        public static bool ResolveResourceAssembly(
            string assemblyLocation,
            ref string resourceAssemblyPath)
        {
            var qualifiedPath = Path.GetFullPath(assemblyLocation);
            var fn = Path.GetFileNameWithoutExtension(qualifiedPath);
            var dir = Path.GetDirectoryName(qualifiedPath);

            fn = fn + ".resources.dll";

            resourceAssemblyPath = Path.Combine(dir, fn);

            return File.Exists(resourceAssemblyPath);
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

            cachedIcons = new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);

            Stream stream =
                assembly.GetManifestResourceStream(assembly.GetManifestResourceNames()[0]);

            if (stream == null)
                return;
            
            resourcesReader = new ResourceReader(stream);
        }

        internal LibraryCustomization(Assembly assembly)
        {
            this.XmlDocument = null;

            cachedIcons = new Dictionary<string, BitmapImage>(StringComparer.OrdinalIgnoreCase);

            Stream stream =
                assembly.GetManifestResourceStream(assembly.GetManifestResourceNames()[0]);

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

        internal BitmapImage GetSmallIcon(string qualifiedName)
        {
            string iconKey = qualifiedName + Configurations.SmallIconPostfix;
            return LoadIconInternal(iconKey);
        }

        internal BitmapImage GetLargeIcon(FunctionDescriptor descriptor)
        {
            string iconKey = descriptor.QualifiedName + Configurations.LargeIconPostfix;
            return LoadIconInternal(iconKey);
        }

        private BitmapImage LoadIconInternal(string iconKey)
        {
            if (cachedIcons.ContainsKey(iconKey))
                return cachedIcons[iconKey];

            if (resourcesReader == null)
                return null;

            // Gets all images from resReader where they are saved as DictionaryEntries and
            // choose one of them with correct key.
            DictionaryEntry iconData = resourcesReader
                .OfType<DictionaryEntry>()
                .Where(i => i.Key.ToString() == iconKey).FirstOrDefault();

            if (iconData.Key == null || iconData.Value == null)
                return null;

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
