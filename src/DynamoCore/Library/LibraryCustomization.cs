using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
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

                fn = fn + Configurations.ResourcesDLL;

                resourceAssemblyPath = Path.Combine(DynamoPathManager.Instance.MainExecPath, fn);

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

    // TODO (Vladimir): Class should not have references on types of PresentationCore.dll
    //                  Should be reworked. Task: MAGN-5656.
    public class LibraryCustomization
    {
        private readonly XDocument xmlDocument;

        private Dictionary<string, BitmapSource> cachedIcons =
            new Dictionary<string, BitmapSource>(StringComparer.OrdinalIgnoreCase);

        private readonly string assemblyName;
        private readonly Assembly resourceAssembly;

        private const string imagesSuffix = "Images";

        internal LibraryCustomization(Assembly resAssembly, XDocument document)
        {
            this.xmlDocument = document;
            if (resAssembly != null)
            {
                resourceAssembly = resAssembly;
                assemblyName = resAssembly.GetName().Name.Split('.').First();
            }
        }

        public string GetNamespaceCategory(string namespaceName)
        {
            var format = "string(/doc/namespaces/namespace[@name='{0}']/category)";
            object obj = String.Empty;
            if (xmlDocument != null)
                obj = xmlDocument.XPathEvaluate(String.Format(format, namespaceName));
            return obj.ToString().Trim();
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        internal BitmapSource LoadIconInternal(string iconKey)
        {
            if (cachedIcons.ContainsKey(iconKey))
                return cachedIcons[iconKey];

            if (resourceAssembly == null)
            {
                cachedIcons.Add(iconKey, null);
                return null;
            }

            ResourceManager rm = new ResourceManager(assemblyName + imagesSuffix, resourceAssembly);

            BitmapSource bitmapSource = null;

            var source = (Bitmap)rm.GetObject(iconKey);
            if (source == null)
            {
                cachedIcons.Add(iconKey, null);
                return null;
            }
            var hBitmap = source.GetHbitmap();

            try
            {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitmapSource = null;
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            cachedIcons.Add(iconKey, bitmapSource);

            return bitmapSource;
        }
    }
}
