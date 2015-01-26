using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media.Imaging;
using Dynamo.DSEngine;

namespace Dynamo.Wpf.Services
{
    public class IconServices
    {
        private Dictionary<Assembly, IconWarehouse> warehouses =
            new Dictionary<Assembly, IconWarehouse>();

        internal IconWarehouse GetForAssembly(string assemblyPath)
        {
            var libraryCustomization = LibraryCustomizationServices.GetForAssembly(assemblyPath);
            if (libraryCustomization == null)
                return null;

            var assembly = libraryCustomization.Assembly;

            if (!warehouses.ContainsKey(assembly))
                warehouses[assembly] = new IconWarehouse(assembly);

            return warehouses[assembly];
        }
    }

    public class IconWarehouse
    {
        private Dictionary<string, BitmapSource> cachedIcons =
            new Dictionary<string, BitmapSource>(StringComparer.OrdinalIgnoreCase);

        private readonly string assemblyName;
        private readonly Assembly resourceAssembly;

        private const string imagesSuffix = "Images";

        internal IconWarehouse(Assembly resAssembly)
        {
            if (resAssembly != null)
            {
                resourceAssembly = resAssembly;
                assemblyName = resAssembly.GetName().Name.Split('.').First();
            }
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
