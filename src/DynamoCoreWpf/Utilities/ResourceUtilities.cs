﻿using Dynamo.Logging;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.UI.GuidedTour;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Dynamo.Utilities
{
    public static class ResourceUtilities
    {
        public const string DPISCRIPT = @"<script> function getDPIScale()
        {
            var dpi = 96.0;
            if (window.screen.deviceXDPI != undefined)
            {
                dpi = window.screen.deviceXDPI;
            }
            else
            {
                var tmpNode = document.createElement('DIV');
                tmpNode.style.cssText = 'width:1in;height:1in;position:absolute;left:0px;top:0px;z-index:99;visibility:hidden';
                document.body.appendChild(tmpNode);
                dpi = parseInt(tmpNode.offsetWidth);
                tmpNode.parentNode.removeChild(tmpNode);
            }

            return dpi / 96.0;
        }

        function adaptDPI()
        {
            var dpiScale = getDPIScale();
            document.body.style.zoom = dpiScale;

            var widthPercentage = ((100.0 / dpiScale)-5).toString() + '%';
            document.body.style.width = widthPercentage;
        }
        adaptDPI() 
        </script>";

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ConvertToImageSource(System.Drawing.Bitmap bmp)
        {
            if (bmp == null)
                return null;

            ImageSource imageSource;
            var hbitmap = bmp.GetHbitmap();
            try
            {
                imageSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
            }
            finally
            {
                DeleteObject(hbitmap);
            }

            return imageSource;
        }

        internal static string LoadContentFromResources(string name, Assembly localAssembly = null, bool injectDPI = true, bool removeScriptTags = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            string result;
            // If an assembly was specified in the uri, the resource will be searched there.
            Assembly assembly;
            var assemblyIndex = name.LastIndexOf(";");
            if (assemblyIndex != -1)
            {
                var assemblyName = name.Substring(0, assemblyIndex);
                // Ignore version and public key, in case they were specified.
                var versionIndex = assemblyName.IndexOf(";");
                if (versionIndex != -1)
                {
                    assemblyName = assemblyName.Substring(0, versionIndex);
                }
                assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
                if (assembly == null)
                {
                    // The specified assembly is not loaded
                    return null;
                }
                name = name.Substring(assemblyIndex + 1);
            }
            else
            {
                // Default to documentation browser assembly if no assembly was specified.
                assembly = localAssembly;
            }

            Assembly resourceAssembly = GetResourceAssembly(assembly, name);

            var availableResources = resourceAssembly.GetManifestResourceNames();

            var matchingResource = availableResources
                .FirstOrDefault(str => str.EndsWith(name));

            if (string.IsNullOrEmpty(matchingResource))
            {
                // The resource might exist by a name that includes the culture name in it
                var nameWithCulture = GetResourceNameWithCultureName(name, resourceAssembly.GetName().CultureInfo);
                matchingResource = availableResources.FirstOrDefault(n => n.EndsWith(nameWithCulture));

                if (string.IsNullOrEmpty(matchingResource)) return null;
            }

            Stream stream = null;
            try
            {
                stream = resourceAssembly.GetManifestResourceStream(matchingResource);
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                    stream = null;
                }
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }

            // Clean up possible script tags from document
            if (removeScriptTags &&
                SanitizeHtml(ref result))
            {
                LogMessage.Warning(Resources.ScriptTagsRemovalWarning, WarningLevel.Mild);
            }

            //inject our DPI functions:
            if (injectDPI)
                result += DPISCRIPT;

            return result;
        }

        /// <summary>
        /// Gets a satellite assembly for the specified culture of returns null if not found.
        /// </summary>
        /// <param name="assembly">The main assembly</param>
        /// <param name="cultureInfo">The culture to search a satellite for</param>
        /// <returns>Satellite assembly for requested culture or null</returns>
        private static Assembly GetSatelliteAssembly(Assembly assembly, CultureInfo cultureInfo)
        {
            try
            {
                return assembly.GetSatelliteAssembly(cultureInfo);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Given a file resource name and a culture, it returns an alternate resource name that includes
        /// the culture name before the file extension. Example: NoContent.html => NoContent.de-DE.html
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <returns>Resource name with the culture name appended before the extension</returns>
        internal static string GetResourceNameWithCultureName(string name, CultureInfo culture)
        {
            var extension = Path.GetExtension(name);
            if (string.IsNullOrEmpty(extension) || culture == null)
            {
                return name;
            }

            return $"{name.Substring(0, name.LastIndexOf(extension))}.{culture.Name}{extension}";
        }

        /// <summary>
        /// Checks if the assembly contains a manifest resource that ends with the specified name.
        /// </summary>
        /// <param name="assembly">Assembly to search for resources</param>
        /// <param name="name">Suffix to search</param>
        /// <returns>If such a resource exists or not</returns>
        private static bool ContainsResource(Assembly assembly, string name)
        {
            if (assembly != null)
            {
                var nameWithCulture = GetResourceNameWithCultureName(name, assembly.GetName().CultureInfo);
                return assembly.GetManifestResourceNames().Any(resName => resName.EndsWith(name) || resName.EndsWith(nameWithCulture));
            }
            return false;
        }

        /// <summary>
        /// Resolves the assembly where to look for embedded resources. If no satellite compatible
        /// with the UI culture is found, it returns the provided main/invariant assembly.
        /// </summary>
        /// <param name="assembly">The main assembly</param>
        /// <returns>The resource assembly</returns>
        private static Assembly GetResourceAssembly(Assembly assembly, string name)
        {
            var culture = CultureInfo.CurrentUICulture;
            var satelliteAssembly = GetSatelliteAssembly(assembly, culture);
            // If there is no satellite for the exact culture, try a more specific/neutral one
            // following .NET rules for culture matching.
            if (satelliteAssembly == null || !ContainsResource(satelliteAssembly, name))
            {
                if (culture.IsNeutralCulture)
                {
                    var specificCulture = CultureInfo.CreateSpecificCulture(culture.Name);
                    satelliteAssembly = GetSatelliteAssembly(assembly, specificCulture);
                }
                else if (culture.Parent != CultureInfo.InvariantCulture)
                {
                    satelliteAssembly = GetSatelliteAssembly(assembly, culture.Parent);
                }
            }

            if (satelliteAssembly == null || !ContainsResource(satelliteAssembly, name))
            {
                // Default to main assembly when no compatible satellite assembly was found
                return assembly;
            }
            else
            {
                return satelliteAssembly;
            }
        }

        /// <summary>
        /// Clean up possible dangerous HTML content from the content string.
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Returns true if any content was removed from the content string</returns>
        private static bool SanitizeHtml(ref string content)
        {
            using (var converter = new Md2Html())
            {
                var sanitizedContent = converter.SanitizeHtml(content);

                if (string.IsNullOrEmpty(sanitizedContent))
                {
                    return false;
                }

                content = sanitizedContent;
                return true;
            }
        }

        internal static Stream LoadResourceByUrl(string url)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var textStream = assembly.GetManifestResourceStream(url);
            return textStream;
        }


        internal static string ConvertToBase64(Stream stream)
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }

        /// <summary>
        /// This method will execute a javascript function located in library.hmtl using reflection.
        /// due that we cannot include a reference to LibraryViewExtensionMSWebBrowser then we need to use reflection to get the types
        /// </summary>
        /// <param name="MainWindow">MainWindow in which the LibraryView is located</param>
        /// <param name="popupInfo">Popup Information about the Step </param>
        /// <param name="parametersInvokeScript">Parameters for the WebBrowser.InvokeScript() function</param>
        internal static object ExecuteJSFunction(UIElement MainWindow, HostControlInfo popupInfo, object[] parametersInvokeScript)
        {
            const string webBrowserString = "Browser";
            const string invokeScriptFunction = "InvokeScript";
            object resultJSHTML = null;

            //Try to find the grid that contains the LibraryView
            var sidebarGrid = (MainWindow as Window).FindName(popupInfo.HostUIElementString) as Grid;
            if (sidebarGrid == null) return null;

            //We need to iterate every child in the grid due that we need to apply reflection to get the Type and find the LibraryView (a reference to LibraryViewExtensionMSWebBrowser cannot be added).
            foreach (var child in sidebarGrid.Children)
            {
                Type type = child.GetType();
                if (type.Name.Equals(popupInfo.WindowName))
                {
                    var libraryView = child as UserControl;
                    //get the WebBrowser instance inside the LibraryView
                    var browser = libraryView.FindName(webBrowserString);
                    if (browser == null) return null;

                    Type typeBrowser = browser.GetType();
                    //Due that there are 2 methods with the same name "InvokeScript", then we need to get the one with 2 parameters
                    MethodInfo methodInvokeScriptInfo = typeBrowser.GetMethods().Single(m => m.Name == invokeScriptFunction && m.GetParameters().Length == 2);
                    //Invoke the JS method located in library.html
                    resultJSHTML = methodInvokeScriptInfo.Invoke(browser, parametersInvokeScript);
                    break;
                }
            }
            return resultJSHTML;
        }
    }
}
