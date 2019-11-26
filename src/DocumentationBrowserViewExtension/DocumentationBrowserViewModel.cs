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
        private const string HTML_TEMPLATE_IDENTIFIER = "%TEMPLATE%";
        private const string DOCUMENTATION_FOLDER_NAME = "Docs";
        private const string BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME = nameof(Resources.InternalError) + ".html";
        private const string BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME = nameof(Resources.FileNotFound) + ".html";

        private Uri link;
        public Uri Link
        {
            get { return this.link; }
            private set
            {
                this.link = value;
                OnLinkChanged(value);
            }
        }

        public string Content { get; private set; }

        public bool isRemoteResource;
        public bool IsRemoteResource
        {
            get { return this.isRemoteResource; }
            set { this.isRemoteResource = value; RaisePropertyChanged(nameof(this.IsRemoteResource)); }
        }

        public Action<Uri> LinkChanged;
        private void OnLinkChanged(Uri link) => this.LinkChanged?.Invoke(link);


        public DocumentationBrowserViewModel()
        {
            this.isRemoteResource = false;
        }

        public void HandleOpenDocumentationLinkEvent(OpenDocumentationLinkEventArgs e)
        {
            // if the link is not pointing to a local file, but to a network or internet address
            // we treat it as a remote resource that can be loaded in the browser directly
            this.IsRemoteResource = e.Link.IsAbsoluteUri && !e.Link.IsFile;

            // otherwise, we load & cache the content of the file
            // this is to avoid doing IO in the View layer
            try
            {
                if (!this.IsRemoteResource) LoadFileContent(e.Link);
            }
            catch (FileNotFoundException)
            {
                this.Content = LoadFileContentFromResources(BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME);
            }
            catch (Exception ex)
            {
                this.Content = LoadFileContentFromResources(BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME);
                this.Content = ReplaceTemplateInContentWithString(ex.Message + "<br>" + ex.StackTrace);
            }

            this.Link = e.Link;
        }

        #region Content handling & loading

        private void LoadFileContent(Uri link)
        {
            var path = ResolveFilePath(link);
            this.Content = File.ReadAllText(path);
        }

        private string LoadFileContentFromResources(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            string result;

            var assembly = GetType().Assembly;
            var a = assembly.GetManifestResourceStream(name);
            var names = assembly
                .GetManifestResourceNames();

            string resourceName = assembly
                .GetManifestResourceNames()
                .Where(str => str.EndsWith(name))
                .FirstOrDefault();
            if (string.IsNullOrEmpty(resourceName)) return null;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        private string ReplaceTemplateInContentWithString(string content)
        {
            return this.Content.Replace(HTML_TEMPLATE_IDENTIFIER, content);
        }

        /// <summary>
        /// Resolves the path to local documentation file.
        /// It attempts to use the given path and if it fails, it searches for the file in the built-in docs folder.
        /// </summary>
        /// <param name="link">The link to the file to resolve.</param>
        /// <returns>An absolute path to a local file, as a string.</returns>
        private static string ResolveFilePath(Uri link)
        {
            // always check if the uri is valid first
            if (!Uri.IsWellFormedUriString(link.ToString(), UriKind.RelativeOrAbsolute))
                throw new ArgumentException("Documentation link is not a valid URI.");

            // return the path to the file directly if it exists
            if (File.Exists(link.ToString())) return link.ToString();

            // search for file in the docs folder and return its path if found
            string docsFolderPath = GetBuiltInDocumentationFolderPath();
            if (Directory.Exists(docsFolderPath))
            {
                var files = Directory.EnumerateFiles(docsFolderPath, link.ToString());
                if (files != null && files.Count() > 0) return files.First();
            }

            // if we reached this point it means the path could not be resolved
            throw new FileNotFoundException(link.ToString());
        }

        #endregion

        private static string GetBuiltInDocumentationFolderPath()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var docsFolderPath = Path.Combine(assemblyPath, DOCUMENTATION_FOLDER_NAME);

            return docsFolderPath;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
