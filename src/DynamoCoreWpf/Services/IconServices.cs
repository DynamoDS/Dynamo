using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Media;
using Dynamo.Engine;
using Dynamo.Interfaces;
using Dynamo.Utilities;

namespace Dynamo.Wpf.Services
{
    public class IconServices
    {
        private readonly IPathManager pathManager;
        private Dictionary<Assembly, IconWarehouse> warehouses =
            new Dictionary<Assembly, IconWarehouse>();

        internal IconServices(IPathManager pathManager)
        {
            this.pathManager = pathManager;
        }

        internal IconWarehouse GetForAssembly(string assemblyPath, bool useAdditionalPaths = true)
        {
            var libraryCustomization = LibraryCustomizationServices.GetForAssembly(assemblyPath, pathManager, useAdditionalPaths);
            if (libraryCustomization == null)
                return null;

            var assembly = libraryCustomization.ResourceAssembly;
            if (assembly == null)
                return null;

            if (!warehouses.ContainsKey(assembly))
                warehouses[assembly] = new IconWarehouse(assembly);

            return warehouses[assembly];
        }
    }

    public class IconWarehouse
    {
        private Dictionary<string, ImageSource> cachedIcons =
            new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

        private readonly string assemblyName;
        private readonly Assembly resourceAssembly;

        private const string imagesSuffix = "Images";

        internal IconWarehouse(Assembly resAssembly)
        {
            if (resAssembly != null)
            {
                resourceAssembly = resAssembly;

                // "Name" can be "Some.Assembly.Name.customization" with multiple dots, 
                // we are interested in removal of the "customization" part and the middle dots.
                var temp = resAssembly.GetName().Name.Split('.');
                assemblyName = String.Join("", temp.Take(temp.Length - 1));
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        internal ImageSource LoadIconInternal(string iconKey)
        {
            ImageSource icon;
            if (cachedIcons.TryGetValue(iconKey, out icon))
                return icon;

            if (resourceAssembly == null)
            {
                cachedIcons.Add(iconKey, null);
                return null;
            }

            ResourceManager rm = new ResourceManager(assemblyName + imagesSuffix, resourceAssembly);

            ImageSource bitmapSource = null;

            var source = (Bitmap)rm.GetObject(iconKey);
            if (source == null)
            {
                cachedIcons.Add(iconKey, null);
                return null;
            }

            bitmapSource = ResourceUtilities.ConvertToImageSource(source);

            cachedIcons.Add(iconKey, bitmapSource);
            return bitmapSource;
        }
    }
}
