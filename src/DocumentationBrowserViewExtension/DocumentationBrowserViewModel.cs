using Dynamo.Core;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Logging;
using Dynamo.ViewModels;
using System;
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
        private const string BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME = nameof(Resources.InternalError) + ".html";
        private const string BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME = nameof(Resources.FileNotFound) + ".html";
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

            var availableResources = assembly.GetManifestResourceNames();

            var matchingResource = availableResources
                .FirstOrDefault(str => str.EndsWith(name));

            if (string.IsNullOrEmpty(matchingResource)) return null;

            Stream stream = null;
            try
            {
                stream = assembly.GetManifestResourceStream(matchingResource);
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

        private void LogWarning(string msg, WarningLevel level) => this.MessageLogged?.Invoke(LogMessage.Warning(msg, level));

        private string ReplaceTemplateInContentWithString(string content)
        {
            return this.content.Replace(HTML_TEMPLATE_IDENTIFIER, content);
        }

        #endregion
    }
}