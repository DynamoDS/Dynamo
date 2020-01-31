using Dynamo.Core;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dynamo.DocumentationBrowser
{
    public class DocumentationBrowserViewModel : NotificationObject, IDisposable
    {
        #region Constants

        private const string HTML_TEMPLATE_IDENTIFIER = "%TEMPLATE%";
        private const string DOCUMENTATION_FOLDER_NAME = "Docs";
        private const string BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME = nameof(Resources.InternalError) + ".html";
        private const string BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME = nameof(Resources.FileNotFound) + ".html";
        private const string BUILT_IN_CONTENT_NO_CONTENT_FILENAME = "NoContent.html";

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
        public bool isRemoteResource;


        internal Action<Uri> LinkChanged;
        private void OnLinkChanged(Uri link) => this.LinkChanged?.Invoke(link);

        #endregion

        #region Constructor & Dispose
        public DocumentationBrowserViewModel()
        {
            this.isRemoteResource = false;

            // default to no content page
            this.shouldLoadDefaultContent = true;
        }

        public void Dispose()
        {
            this.content = null;
        }
        #endregion

        internal void HandleOpenDocumentationLinkEvent(OpenDocumentationLinkEventArgs e)
        {
            if (e is null)
                NavigateToNoContentPage();

            // if the link is not pointing to a local file, but to a network or internet address
            // we treat it as a remote resource that can be loaded in the browser directly
            this.IsRemoteResource = e.Link.IsAbsoluteUri && !e.Link.IsFile;

            if (this.IsRemoteResource)
            {
                this.Link = e.Link;
                return;
            }

            // if target is local file, we load & cache the content of the file
            // avoiding doing IO in the View layer
            try
            {
                if (!this.IsRemoteResource) LoadContentFromFile(e.Link);
                this.Link = e.Link;
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

            this.Link = new Uri(BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME);
        }

        public void NavigateToContentMissingPage()
        {
            this.content = LoadContentFromResources(BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME);
            this.Link = new Uri(BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME, UriKind.Relative);
        }

        public void NavigateToNoContentPage()
        {
            this.content = LoadContentFromResources(BUILT_IN_CONTENT_NO_CONTENT_FILENAME);
            this.Link = new Uri(BUILT_IN_CONTENT_NO_CONTENT_FILENAME, UriKind.Relative);
        }

        internal void EnsurePageHasContent()
        {
            if (this.shouldLoadDefaultContent || !this.HasContent)
                NavigateToNoContentPage();
        }

        #endregion

        #region Content handling & loading

        internal string GetContent()
        {
            return this.content;
        }

        /// <summary>
        /// Updates the content to be displayed in the browser and triggers the LinkChanged action.
        /// </summary>
        /// <param name="newContent">The content to display.</param>
        internal void UpdateContent(string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                return;

            this.content = newContent;
            this.isRemoteResource = false;
            OnLinkChanged(null);
        }

        private void LoadContentFromFile(Uri link)
        {
            var path = ResolveFilePath(link);
            this.content = File.ReadAllText(path);

            // when we have no content fall back to the no content page
            if (string.IsNullOrWhiteSpace(this.content))
                throw new TargetException();
        }

        private string LoadContentFromResources(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            string result;

            var assembly = GetType().Assembly;

            var availableResources = assembly
                .GetManifestResourceNames();

            var matchingResource = availableResources
                .Where(str => str.EndsWith(name))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(matchingResource)) return null;

            using (Stream stream = assembly.GetManifestResourceStream(matchingResource))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        private string ReplaceTemplateInContentWithString(string content)
        {
            return this.content.Replace(HTML_TEMPLATE_IDENTIFIER, content);
        }

        #endregion

        #region Path handling

        /// <summary>
        /// Resolves the path to local documentation file.
        /// It attempts to use the given path and if it fails, it searches for the file in the built-in docs folder.
        /// </summary>
        /// <param name="link">The link to the file to resolve.</param>
        /// <returns>An absolute path to a local file, as a string.</returns>
        private static string ResolveFilePath(Uri link)
        {
            if (link is null)
                throw new FileNotFoundException();

            var address = link.ToString();

            // always check if the uri is valid first
            if (!Uri.IsWellFormedUriString(address, UriKind.RelativeOrAbsolute))
                throw new ArgumentException(Resources.InvalidDocumentationLink);

            // return the path to the file directly if it exists
            if (File.Exists(address)) return address;

            // search for file in the default docs folder and return its path if found
            string docsFolderPath = GetBuiltInDocumentationFolderPath();
            if (Directory.Exists(docsFolderPath))
            {
                var files = Directory.EnumerateFiles(docsFolderPath, address);
                if (files != null && files.Any()) return files.First();
            }

            // if we reached this point it means the path could not be resolved
            throw new FileNotFoundException(address);
        }

        /// <summary>
        /// Returns the path to the default documentation folder.
        /// This folder contains all the HTML documentation files that ship with Dynamo.
        /// </summary>
        /// <returns>The path to the default documentation folder.</returns>
        public static string GetBuiltInDocumentationFolderPath()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var docsFolderPath = Path.Combine(assemblyPath, DOCUMENTATION_FOLDER_NAME);

            return docsFolderPath;
        }

        #endregion
    }
}