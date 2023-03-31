using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Windows;
using Dynamo.Core;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Logging;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace Dynamo.DocumentationBrowser
{
    public class InsertDocumentationLinkEventArgs : EventArgs
    {
        private string data;
        private string name;
        public InsertDocumentationLinkEventArgs(string _Data, string _Name)
        {
            data = _Data;
            name = _Name;
        } 

        public string Data
        {
            get { return data; }
        }
        public string Name
        {
            get { return name; }
        }
    }

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
        /// Contains the resolvable node to library location breadcrumbs information
        /// Key: Node Name
        /// Value: Breadcrumbs " / " concatenated information
        /// </summary>
        internal Dictionary<string, string> BreadCrumbsDictionary { get; set; }

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
        private string graphPath;
        private string content;
        private string currentPackageName;


        /// <summary>
        /// Package Name
        /// </summary>
        internal string CurrentPackageName
        {
            get
            {
                return currentPackageName;
            }
            set
            {
                currentPackageName = value;
            }
        }

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

        internal UIElement DynamoView { get; set; }

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
                string graph;
                string graphName;
                Uri link;
                switch (e)
                {
                    case OpenNodeAnnotationEventArgs openNodeAnnotationEventArgs:
                        var mdLink = packageManagerDoc.GetAnnotationDoc(
                            openNodeAnnotationEventArgs.MinimumQualifiedName, 
                            openNodeAnnotationEventArgs.PackageName);

                        link = string.IsNullOrEmpty(mdLink) ? new Uri(String.Empty, UriKind.Relative) : new Uri(mdLink);
                        graph = GetGraphLinkFromMDLocation(link);
                        targetContent = CreateNodeAnnotationContent(openNodeAnnotationEventArgs);
                        graphName = openNodeAnnotationEventArgs.MinimumQualifiedName;
                        break;

                    case OpenDocumentationLinkEventArgs openDocumentationLink:
                        link = openDocumentationLink.Link;
                        graph = GetGraphLinkFromMDLocation(link);
                        targetContent = ResourceUtilities.LoadContentFromResources(openDocumentationLink.Link.ToString(), GetType().Assembly);
                        graphName = null;
                        break;

                    default:
                        // Navigate to unsupported 
                        targetContent = null;
                        graph = null;
                        link = null;
                        graphName = null;
                        break;
                }

                if (targetContent == null)
                {
                    NavigateToContentMissingPage();
                }
                else
                {
                    this.content = targetContent;
                    this.graphPath = graph;
                    this.currentPackageName = graphName;
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

        private string GetGraphLinkFromMDLocation(Uri link)
        {
            if (link == null || link.Equals(new Uri(String.Empty, UriKind.Relative))) return string.Empty;
            try
            {
                string graphPath = DynamoGraphFromMDFilePath(link.AbsolutePath);
                return File.Exists(graphPath) ? graphPath : null;
            }
            catch (Exception)
            {
                return string.Empty;
            }
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
            this.graphPath = GetGraphLinkFromMDLocation(this.Link);
        }

        private string CreateNodeAnnotationContent(OpenNodeAnnotationEventArgs e)
        {
            var writer = new StringWriter();
            try
            {
                writer.WriteLine(DocumentationBrowserUtils.GetContentFromEmbeddedResource(STYLE_RESOURCE));

                // Convert the markdown file to html
                var mkDown = MarkdownHandlerInstance.ParseToHtml(e.MinimumQualifiedName, e.PackageName);
                BreadCrumbs = GetBreadCrumbsValue(e);

                writer.WriteLine(NodeDocumentationHtmlGenerator.OpenDocument());
                // Get the Node info section
                var nodeDocumentation = NodeDocumentationHtmlGenerator.FromAnnotationEventArgs(e, BreadCrumbs, mkDown);
                writer.WriteLine(nodeDocumentation);
                writer.WriteLine(NodeDocumentationHtmlGenerator.CloseDocument());

                writer.Flush();
                var output = writer.ToString();

                // Sanitize html and warn if any changes where made
                if (MarkdownHandlerInstance.SanitizeHtml(ref output))
                {
                    LogWarning(Resources.ScriptTagsRemovalWarning, WarningLevel.Mild);
                }
                
                // inject the syntax highlighting script at the bottom at the document.
                output += DocumentationBrowserUtils.GetImageNavigationScript();
                output += DocumentationBrowserUtils.GetDPIScript();
                output += DocumentationBrowserUtils.GetSyntaxHighlighting();

                return output;
            }
            finally
            {
                writer?.Dispose();
            }
        }

        public string BreadCrumbs { get; set; }

        private const string GEOMETRY_NAMESPACE = "Autodesk.DesignScript.Geometry";
        private const string GEOMETRY_TESSELLATION_NAMESPACE = "Geometry.Tessellation";

        private string GetBreadCrumbsValue(OpenNodeAnnotationEventArgs e)
        {
            string breadCrumbs = null;

            if (BreadCrumbsDictionary == null) return String.Empty;
            if (e.MinimumQualifiedName.Contains(GEOMETRY_NAMESPACE))
            {
                var category = e.Category.Split('.');
                if (category.Length < 3) return null;

                var cat = category[0];
                var group = category[1];
                var type = category[2];

                var lookupName = cat + "." + group;

                // For some reason, Geometry / Modifiers / Geometry already contains the correct amount of information, so we can skip it
                if (BreadCrumbsDictionary.TryGetValue(lookupName, out breadCrumbs) && !breadCrumbs.Contains("Modifiers"))
                {
                    breadCrumbs += " / " + group;
                }

                return breadCrumbs;
            }
            else if (e.Category.Contains(GEOMETRY_TESSELLATION_NAMESPACE))
            {
                var category = e.Category.Split('.');
                if (category.Length < 4) return null;

                var cat = category[0];
                var group = category[1];
                var none = category[2];
                var type = category[3];

                breadCrumbs = cat + " / " + group + " / " + none;
                return breadCrumbs;
            }
            else
            {
                if (!BreadCrumbsDictionary.TryGetValue(e.OriginalName, out breadCrumbs))
                {
                    foreach (var pair in BreadCrumbsDictionary)
                    {
                        if (pair.Key.Contains(e.OriginalName))
                        {
                            breadCrumbs = pair.Value;
                            break;
                        }
                    }
                }
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Load the help graph associated with the node inside the current Dynamo workspace 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal void InsertGraph()
        {
            var raiseInsertGraph = HandleInsertFile;

            if (raiseInsertGraph != null)
            {
                if (graphPath != null)
                {
                    var graphName = this.currentPackageName ?? Path.GetFileNameWithoutExtension(graphPath);
                    raiseInsertGraph(this, new InsertDocumentationLinkEventArgs(graphPath, graphName));
                }
                else
                {
                    raiseInsertGraph(this, new InsertDocumentationLinkEventArgs(Resources.FileNotFoundFailureMessage, DynamoGraphFromMDFilePath(this.Link.AbsolutePath)));
                    return;
                }
            }
        }

        internal delegate void InsertDocumentationLinkEventHandler(object sender, InsertDocumentationLinkEventArgs e);
        internal event InsertDocumentationLinkEventHandler HandleInsertFile;

        private string DynamoGraphFromMDFilePath(string path)
        {
            path = HttpUtility.UrlDecode(path);
            return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + ".dyn";
        }


        internal void CollapseExpandPackage(string section)
        {
            string sectionType;
            var breadBrumbsArray = BreadCrumbs.Replace(" / ", "/").Split('/');

            // We need to expand sequentially all root levels before reaching the desired section
            for (var i = 0; i < breadBrumbsArray.Length; i++)
            {
                sectionType = i == 0 ? "LibraryItemText" : "LibraryItemGroupText";

                object[] jsParameters = new object[] { breadBrumbsArray[i], sectionType, "true" };
                //Create the array for the paramateres that will be sent to the WebBrowser.InvokeScript Method
                object[] parametersInvokeScript = new object[] { "collapseExpandPackage", jsParameters };

                ResourceUtilities.ExecuteJSFunction(DynamoView, parametersInvokeScript);
                
                // After we have reached the desired section, we can exit the method
                // Also check if we have more than one occurrence, i.e Geometry / Modifiers / Geometry 
                if (section.Equals(breadBrumbsArray[i]) && breadBrumbsArray.Count(x => x.Equals(section)) == 1)
                    return;
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
