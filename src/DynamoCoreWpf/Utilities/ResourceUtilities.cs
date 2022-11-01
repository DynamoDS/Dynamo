using Dynamo.Logging;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.UI.GuidedTour;
using Dynamo.Wpf.Utilities;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

            var widthPercentage = ((100.0 / dpiScale)-2).toString() + '%';
            document.body.style.width = widthPercentage;
        }
        adaptDPI() 
        </script>";

        public const string IMGNAVIGATIONSCRIPT = @"
            <!-- Set image div container width and height -->
            <script type='text/javascript'>
                maintainContainerRatio();

                function maintainContainerRatio()
                {
                    var container_ele = document.getElementById('img--container');

                    var img_width = document.getElementById('drag--img').naturalWidth;
                    var img_height = document.getElementById('drag--img').naturalHeight;

                    var ratio = img_width / img_height;
                    var calculated_width = document.getElementById('drag--img').width;

                    var calculated_height = calculated_width / ratio;
                    container_ele.style.height = calculated_height + 'px';
                }

                window.onresize = maintainContainerRatio;
        </script>

        <!-- Adds zoom to image container -->
        <script type = 'text/javascript' >

            var img_ele = null;
            var scale = 1.0;
            var increment = 0.1;

            const initial = { x: 0, y: 0 };
            const offset = { x: 0, y: 0 };

            function reset_pan()
            {
                initial.x = initial.y = 0;
                offset.x = offset.y = 0;
            }

            function insert(ev){
                ev.preventDefault();
                document.getElementById('insert').removeEventListener('click', insert);
                let message = 'insert';
                window.chrome.webview.postMessage(message);
                document.getElementById('insert').addEventListener('click', insert);
            }

            function zoom(zoomtype)
            {
                img_ele = document.getElementById('drag--img');

                if (zoomtype === 'out')
                {
                    if (scale <= 1.0) return;
                    scale -= increment;
                }
                else
                {
                    scale += increment;
                }

                img_ele.style.transform = `scale(${ scale}, ${ scale})`;

                reset_pan();
            }

            function fit(){
                if(scale === 1.0 || !img_ele) return;
                scale = 1.0;
                img_ele.style.transform = `scale(${scale}, ${scale})`;
            }

            function scroll(ev){
                ev.preventDefault();
                if(ev.deltaY < 0) zoom('in');
                else zoom('out');
            }

            // check against the future frame so you don't get stuck
            function check_edges(el, x, y)
            {
                const container_rect = el.getBoundingClientRect();
                const image_rect = el.firstElementChild.getBoundingClientRect();

                if (image_rect.top > container_rect.top - y)
                {
                    y = 0;
                }
                if (image_rect.left > container_rect.left - x)
                {
                    x = 0;
                }
                if (image_rect.right < container_rect.right - x)
                {
                    x = 0;
                }
                if (image_rect.bottom < container_rect.bottom - y)
                {
                    y = 0;
                }

                return { delta_x: x, delta_y: y};
            }

            const pannable = (el) => {
                const img_canvas = el.firstElementChild; // the image

                let is_pan = false;

                // get the click location relative to the div and current image scale
                const getXY = ({ clientX, clientY}) => {
                    const { left, top} = el.getBoundingClientRect(); // the left/top of the container div
                    return { x: (clientX - left), y: (clientY - top) }
                }

                const pan_start = (ev) => {
                    if (scale === 1) return; // do not start panning if not zoomed in
                    ev.preventDefault();
                    is_pan = true;
                    const { x, y} = getXY(ev);
                    initial.x = x - offset.x;
                    initial.y = y - offset.y;
                }
            
                const pan_move = (ev) => {
                    if (!is_pan) return;

                    const { x, y} = getXY(ev);

                    var d_x = (x - initial.x);
                    var d_y = (y - initial.y);

                    const { delta_x, delta_y} = check_edges(el, d_x - offset.x, d_y - offset.y);

                    img_canvas.style.transform = `translate(${ offset.x + delta_x}px, ${ offset.y + delta_y}px) scale(${ scale}, ${ scale})`;

                    offset.x += (delta_x);
                    offset.y += (delta_y);
                }

                const pan_end = (ev) => {
                    is_pan = false;
                };

                el.addEventListener('mousedown', pan_start);
                document.addEventListener('mousemove', pan_move);
                document.addEventListener('mouseup', pan_end);
            }

            document.getElementById('zoomout').addEventListener('click', function() {
                zoom('out');
            });
            document.getElementById('zoomin').addEventListener('click', function() {
                zoom('in');
            });
            document.getElementById('zoomfit').addEventListener('click', function() {
              fit();
            });
            document.getElementById('insert').addEventListener('click', insert);

            document.getElementById('img--container').addEventListener('wheel', scroll);
            document.querySelectorAll('.container').forEach(pannable);
        </script>
        ";

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

        /// <summary>
        /// This method converts a stream to base64 string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ConvertToBase64(Stream stream)
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
        internal static async Task<object> ExecuteJSFunction(UIElement MainWindow, HostControlInfo popupInfo, object[] parametersInvokeScript)
        {
            const string invokeScriptFunction = "ExecuteScriptAsync";
            object resultJSHTML = null;
            Task<string> resutlJS = null;

            //Try to find the grid that contains the LibraryView
            var sidebarGrid = (MainWindow as Window).FindName(popupInfo.HostUIElementString) as Grid;
            if (sidebarGrid == null) return null;

            var functionName = parametersInvokeScript[0] as string;
            object[] functionParameters = parametersInvokeScript[1] as object[];
            List<string> functionParametersList = new List<string>();
            foreach(var parameter in functionParameters)
            {
                string strParameter = string.Empty;
                //If we try to use boolVariable.ToString() returns "True" or "False" but the expected parameter in the js function is "true" or "false"
                if (parameter is bool)
                    strParameter = parameter.ToString().Equals("True") ? "true" : "false";
                //The parameter expected in the js method is a string so we need to add the quotes at the beginning and at the end
                else
                    strParameter = "\"" + parameter + "\"";

                functionParametersList.Add(strParameter);
            }
            //Due that the method WebView2.ExecuteScriptAsync() is expecting just one string parameter, we will need to contatecate the function name and the parameters in parentesis
            var parametersExecuteScriptString = string.Join(",", functionParametersList);

            //We need to iterate every child in the grid due that we need to apply reflection to get the Type and find the LibraryView (a reference to LibraryViewExtensionMSWebBrowser cannot be added).
            foreach (var child in sidebarGrid.Children)
            {
                Type type = child.GetType();
                if (type.Name.Equals(popupInfo.WindowName))
                {
                    var libraryView = child as UserControl;
                    //get the WebBrowser instance inside the LibraryView
                    var browser = libraryView.ChildOfType<WebView2>();
                    if (browser == null) return null;
                    Type typeBrowser = browser.GetType();
                    MethodInfo methodInvokeScriptInfo = typeBrowser.GetMethods().Single(m => m.Name == invokeScriptFunction && m.GetParameters().Length == 1);
                    //Due that WebView2.ExecuteScriptAsync() method is async we need to wait until we get a response
                    resutlJS = (Task<string>)methodInvokeScriptInfo.Invoke(browser, new object[] { functionName + "(" + parametersExecuteScriptString + ")" });
                    await resutlJS;
                    var resultProperty = resutlJS.GetType().GetProperty("Result");
                    resultJSHTML = resultProperty.GetValue(resutlJS);
                    if (resultJSHTML.ToString().Equals("null"))
                        resultJSHTML = null;
                    break;
                }
            }
            return resultJSHTML;
        }

        /// <summary>
        /// Loads HTML file from resource assembly and replace it's key values by base64 files
        /// </summary>
        /// <param name="htmlPage">Contains filename and resources to be loaded in page</param>
        /// <param name="webBrowserComponent">WebView2 instance that will be initialized</param>
        /// <param name="resourcesPath">Path of the resources that will be loaded into the HTML page</param>
        /// <param name="fontStylePath">Path to the Font Style that will be used in some part of the HTML page</param>
        /// <param name="localAssembly">Local Assembly in which the resource will be loaded</param>
        /// <param name="userDataFolder">the folder that WebView2 will use for storing cache info</param>
        internal static async void LoadWebBrowser(HtmlPage htmlPage, WebView2 webBrowserComponent, string resourcesPath, string fontStylePath, Assembly localAssembly, string userDataFolder = default(string))
        {
            var bodyHtmlPage = ResourceUtilities.LoadContentFromResources(htmlPage.FileName, localAssembly, false, false);

            bodyHtmlPage = LoadResouces(bodyHtmlPage, htmlPage.Resources, resourcesPath);
            bodyHtmlPage = LoadResourceAndReplaceByKey(bodyHtmlPage, "#fontStyle", fontStylePath);

            if (!string.IsNullOrEmpty(userDataFolder))
            {
                //This indicates in which location will be created the WebView2 cache folder
                webBrowserComponent.CreationProperties = new CoreWebView2CreationProperties()
                {
                    UserDataFolder = userDataFolder
                };
            }

            await webBrowserComponent.EnsureCoreWebView2Async();
            // Context menu disabled
            webBrowserComponent.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webBrowserComponent.NavigateToString(bodyHtmlPage);
        }

        /// <summary>
        /// Loads resource from a dictionary and replaces its key by an embedded file
        /// </summary>
        /// <param name="bodyHtmlPage">Html page string</param>
        /// <param name="resources">Resources to be loaded</param>
        /// <param name="resourcesPath">Path to resources</param>
        /// <returns></returns>
        internal static string LoadResouces(string bodyHtmlPage, Dictionary<string, string> resources, string resourcesPath)
        {
            if (resources != null && resources.Any())
            {
                foreach (var resource in resources)
                {
                    bodyHtmlPage = LoadResourceAndReplaceByKey(bodyHtmlPage, resource.Key, $"{resourcesPath}.{resource.Value}");
                }
            }
            return bodyHtmlPage;
        }

        /// <summary>
        /// Finds a key word inside the html page and replace by a resource file
        /// </summary>
        /// <param name="bodyHtmlPage">Current html page</param>
        /// <param name="key">Key that is going to be replaced</param>
        /// <param name="resourceFile">Resource file to be included in the page</param>
        /// <returns></returns>
        internal static string LoadResourceAndReplaceByKey(string bodyHtmlPage, string key, string resourceFile)
        {
            Stream resourceStream = ResourceUtilities.LoadResourceByUrl(resourceFile);

            if (resourceStream != null)
            {
                var resourceBase64 = ResourceUtilities.ConvertToBase64(resourceStream);
                bodyHtmlPage = bodyHtmlPage.Replace(key, resourceBase64);
            }

            return bodyHtmlPage;
        }
    }
}
