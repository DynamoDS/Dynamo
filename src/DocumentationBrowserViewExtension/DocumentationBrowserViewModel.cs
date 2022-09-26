using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Core;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Logging;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.DocumentationBrowser
{
    public class DocumentationBrowserViewModel : NotificationObject, IDisposable
    {
        #region Constants

        private const string HTML_TEMPLATE_IDENTIFIER = "%TEMPLATE%";
        private const string BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME = "InternalError.html";
        private const string BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME = "FileNotFound.html";
        private const string BUILT_IN_CONTENT_NO_CONTENT_FILENAME = "NoContent.html";
        private const string STYLE_RESOURCE = "Dynamo.DocumentationBrowser.Docs.MarkdownStyling.html";

        #endregion

        #region Properties

        private bool shouldLoadDefaultContent;
        private readonly PackageDocumentationManager packageManagerDoc;
        private MarkdownHandler markdownHandler;
        private FileSystemWatcher markdownFileWatcher;

        /// <summary>
        /// The link to the documentation website or file to display.
        /// Updating this property will trigger the embedded browser to attempt navigation to the link.
        /// </summary>
        public Uri Link
        {
            get { return this.link; }
            private set
            {
                var oldLink = link;
                // if the link is changed we unsubscribe the current watcher
                // and create a new watcher for the new link, if its a markdown file.
                if (value != oldLink)
                {
                    UnsubscribeMdWatcher();
                    WatchMdFile(value.OriginalString);
                }

                this.link = value;
                OnLinkChanged(value);

            }
        }

        private Uri link;

        private string content;

        private MarkdownHandler MarkdownHandlerInstance => markdownHandler ?? (markdownHandler = new MarkdownHandler());
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
        private OpenDocumentationLinkEventArgs openDocumentationLinkEventArgs;

        #endregion

        #region Constructor & Dispose

        public DocumentationBrowserViewModel()
        {
            this.isRemoteResource = false;

            // default to no content page on first start or no error selected
            this.shouldLoadDefaultContent = true;
            packageManagerDoc = PackageDocumentationManager.Instance;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Models.DynamoModel.IsTestMode)
            {
                this.content = "";
            }
            markdownHandler?.Dispose();
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
            openDocumentationLinkEventArgs = e;
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
                string targetContent;
                Uri link;
                switch (e)
                {
                    case OpenNodeAnnotationEventArgs openNodeAnnotationEventArgs:
                        var mdLink = packageManagerDoc.GetAnnotationDoc(
                            openNodeAnnotationEventArgs.MinimumQualifiedName, 
                            openNodeAnnotationEventArgs.PackageName);

                        link = string.IsNullOrEmpty(mdLink) ? new Uri(String.Empty, UriKind.Relative) : new Uri(mdLink);
                        targetContent = CreateNodeAnnotationContent(openNodeAnnotationEventArgs);
                        break;

                    case OpenDocumentationLinkEventArgs openDocumentationLink:
                        link = openDocumentationLink.Link;
                        targetContent = ResourceUtilities.LoadContentFromResources(openDocumentationLink.Link.ToString(), GetType().Assembly);
                        break;

                    default:
                        // Navigate to unsupported 
                        targetContent = null;
                        link = null;
                        break;
                }

                if (targetContent == null)
                {
                    NavigateToContentMissingPage();
                }
                else
                {
                    this.content = targetContent;
                    this.Link = link;
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

        private void WatchMdFile(string mdLink)
        {
            if (string.IsNullOrWhiteSpace(mdLink))
                return;

            var fileName = Path.GetFileNameWithoutExtension(mdLink);
            if (!packageManagerDoc.ContainsAnnotationDoc(fileName))
                return;

            markdownFileWatcher = new FileSystemWatcher(Path.GetDirectoryName(mdLink))
            {
                Filter = Path.GetFileName(mdLink),
                EnableRaisingEvents = true
            };
            markdownFileWatcher.Changed += OnCurrentMdFileChanged;
        }

        private void UnsubscribeMdWatcher()
        {
            if (markdownFileWatcher is null)
                return;

            markdownFileWatcher.Changed -= OnCurrentMdFileChanged;
        }

        // Its a known issue that FileWatchers raises the same event twice when a file is modified from some programs.
        // https://stackoverflow.com/questions/1764809/filesystemwatcher-changed-event-is-raised-twice
        private void OnCurrentMdFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != link.OriginalString)
                return;
            if (!(openDocumentationLinkEventArgs is OpenNodeAnnotationEventArgs))
                return;

            var nodeAnnotationArgs = openDocumentationLinkEventArgs as OpenNodeAnnotationEventArgs;
            this.content = CreateNodeAnnotationContent(nodeAnnotationArgs);
            this.Link = new Uri(e.FullPath);
        }

        private string CreateNodeAnnotationContent(OpenNodeAnnotationEventArgs e)
        {
            //var lookup = packageManagerDoc.SpeckManager;

            var writer = new StringWriter();
            try
            {
                writer.WriteLine(DocumentationBrowserUtils.GetContentFromEmbeddedResource(STYLE_RESOURCE));

                // Convert the markdown file to html
                var mkDown = MarkdownHandlerInstance.ParseToHtml(e.MinimumQualifiedName, e.PackageName);

                // Get the Node info section
                var nodeDocumentation = NodeDocumentationHtmlGenerator.FromAnnotationEventArgs(e, mkDown);
                writer.WriteLine(nodeDocumentation);

                writer.Flush();
                var output = writer.ToString();

                // Sanitize html and warn if any changes where made
                if (MarkdownHandlerInstance.SanitizeHtml(ref output))
                {
                    LogWarning(Resources.ScriptTagsRemovalWarning, WarningLevel.Mild);
                }

                // inject the syntax highlighting script at the bottom at the document.
                output += DocumentationBrowserUtils.GetDPIScript();
                output += DocumentationBrowserUtils.GetSyntaxHighlighting();

                return output;
            }
            finally
            {
                writer?.Dispose();
            }
        }

        #endregion

        #region Navigation to built-in pages

        public void NavigateToInternalErrorPage(string errorDetails)
        {
            this.content = ResourceUtilities.LoadContentFromResources(BUILT_IN_CONTENT_INTERNAL_ERROR_FILENAME, GetType().Assembly);

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
            this.content = ResourceUtilities.LoadContentFromResources(BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME, GetType().Assembly);
            UpdateLinkSafely(BUILT_IN_CONTENT_FILE_NOT_FOUND_FILENAME);
        }

        public void NavigateToNoContentPage()
        {
            this.content = ResourceUtilities.LoadContentFromResources(BUILT_IN_CONTENT_NO_CONTENT_FILENAME, GetType().Assembly);
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

        private void LogWarning(string msg, WarningLevel level) => this.MessageLogged?.Invoke(LogMessage.Warning(msg, level));

        private string ReplaceTemplateInContentWithString(string content)
        {
            return this.content.Replace(HTML_TEMPLATE_IDENTIFIER, content);
        }

        #endregion
    }
}
