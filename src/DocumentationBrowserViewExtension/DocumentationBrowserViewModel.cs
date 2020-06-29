using Dynamo.Core;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Logging;
using Dynamo.ViewModels;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dynamo.DocumentationBrowser
{
    public class DocumentationBrowserViewModel : NotificationObject, IDisposable
    {
        #region Constants

        private const string HTML_TEMPLATE_IDENTIFIER = "%TEMPLATE%";
        private const string BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME = "InternalError.html";
        private const string BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME = "FileNotFound.html";
        private const string BUILT_IN_CONTENT_NO_CONTENT_FILENAME = "NoContent.html";
        private const string SCRIPT_TAG_REGEX = @"<script[^>]*>[\s\S]*?</script>";
        private const string DPISCRIPT = @"<script> function getDPIScale()
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
        adaptDPI() </script>";
        #endregion

        #region Properties

        private bool shouldLoadDefaultContent;

        /// <summary>
        /// The link to the documentation website or file to display.
        /// Updating this property will trigger the embedded browser to attempt navigation to the link.
        /// </summary>
        public Uri Link
        {
            get { return this.link; }
            private set
            {
                this.link = value;
                OnLinkChanged(value);
            }
        }
        private Uri link;

        private string content;
        public bool HasContent => !string.IsNullOrWhiteSpace(this.content);

        /// <summary>
        /// Indicates if the document to be displayed is hosted by a remote source (the internet or network location).
        /// If it not, it means it is local (on the PC).
        /// </summary>
        public bool IsRemoteResource
        {
            get { return this.isRemoteResource; }
            set { this.isRemoteResource = value; RaisePropertyChanged(nameof(this.IsRemoteResource)); }
        }
        private bool isRemoteResource;


        internal Action<Uri> LinkChanged;
        private void OnLinkChanged(Uri link) => this.LinkChanged?.Invoke(link);

        private bool showBrowser;
        /// <summary>
        /// Determines the whether the browser control should be displayed.
        /// </summary>
        public bool ShowBrowser
        {
            get
            {
                return showBrowser;
            }
            set
            {
                if (showBrowser != value)
                {
                    showBrowser = value;
                    RaisePropertyChanged(nameof(ShowBrowser));
                }
            }
        }

        internal Action<ILogMessage> MessageLogged;

        #endregion

        #region Constructor & Dispose

        public DocumentationBrowserViewModel()
        {
            this.isRemoteResource = false;

            // default to no content page on first start or no error selected
            this.shouldLoadDefaultContent = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            this.content = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Handle navigation event

        public void HandleOpenDocumentationLinkEvent(OpenDocumentationLinkEventArgs e)
        {
            if (e == null)
                NavigateToNoContentPage();
            else
                this.IsRemoteResource = e.IsRemoteResource;

            // Ignore requests to remote resources
            if (!this.IsRemoteResource)
                HandleLocalResource(e);
        }

        /// <summary>
        /// If target is local file, we load & cache the content of the file
        /// avoiding doing IO in the View layer
        /// </summary>
        /// <param name="e">The event to handle.</param>
        private void HandleLocalResource(OpenDocumentationLinkEventArgs e)
        {
            try
            {
                var targetContent = LoadContentFromResources(e.Link.ToString());
                if (targetContent == null)
                {
                    NavigateToContentMissingPage();
                }
                else
                {
                    this.content = targetContent;
                    this.Link = e.Link;
                }
            }
            catch (FileNotFoundException)
            {
                NavigateToContentMissingPage();
                return;
            }
            catch (TargetException)
            {
                NavigateToNoContentPage();
                return;
            }
            catch (Exception ex)
            {
                NavigateToInternalErrorPage(ex.Message + "<br>" + ex.StackTrace);
                return;
            }
            this.shouldLoadDefaultContent = false;
        }

        #endregion

        #region Navigation to built-in pages

        public void NavigateToInternalErrorPage(string errorDetails)
        {
            this.content = LoadContentFromResources(BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME);

            // if additional details about the error were passed in,
            // update the error page HTML with that content
            if (!string.IsNullOrWhiteSpace(errorDetails))
                this.content = ReplaceTemplateInContentWithString(errorDetails);
            else
                this.content = ReplaceTemplateInContentWithString(Resources.InternalErrorNoDetailsAvailable);

            UpdateLinkSafely(BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME);
        }

        public void NavigateToContentMissingPage()
        {
            this.content = LoadContentFromResources(BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME);
            UpdateLinkSafely(BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME);
        }

        public void NavigateToNoContentPage()
        {
            this.content = LoadContentFromResources(BUILT_IN_CONTENT_NO_CONTENT_FILENAME);
            UpdateLinkSafely(BUILT_IN_CONTENT_NO_CONTENT_FILENAME);
        }

        internal void EnsurePageHasContent()
        {
            if (this.shouldLoadDefaultContent || !this.HasContent)
            {
                NavigateToNoContentPage();
            }
        }

        private void UpdateLinkSafely(string link)
        {
            try
            {
                this.Link = new Uri(link, UriKind.Relative);
            }
            catch (Exception)
            {
                // silently ignore any exceptions as otherwise it brings down all of Dynamo
            }
        }

        #endregion

        #region Content handling & loading

        internal string GetContent()
        {
            return this.content;
        }

        private string LoadContentFromResources(string name)
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
                assembly = GetType().Assembly;
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
            if (Regex.IsMatch(result, SCRIPT_TAG_REGEX, RegexOptions.IgnoreCase))
            {
                LogWarning(Resources.ScriptTagsRemovalWarning, WarningLevel.Mild);
                result = Regex.Replace(result, SCRIPT_TAG_REGEX, "", RegexOptions.IgnoreCase);
            }
            //inject our DPI functions:
            result = result + DPISCRIPT;

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

        private void LogWarning(string msg, WarningLevel level) => this.MessageLogged?.Invoke(LogMessage.Warning(msg, level));

        private string ReplaceTemplateInContentWithString(string content)
        {
            return this.content.Replace(HTML_TEMPLATE_IDENTIFIER, content);
        }

        #endregion
    }
}