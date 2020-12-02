using System;
using System.IO;
using Dynamo.Utilities;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Handles markdown files by converting them to Html, so they can display in the doc browser.
    /// This class is a singleton that is instantiated at first use by using the Instance property.
    /// </summary>
    internal class MarkdownHandler : IDisposable
    {
        private const string NODE_ANNOTATION_NOT_FOUND = "Dynamo.DocumentationBrowser.Docs.NodeAnnotationNotFound.md";
        private const string SYNTAX_HIGHLIGHTING = "Dynamo.DocumentationBrowser.Docs.syntaxHighlight.html";
        private readonly Md2Html converter = new Md2Html();

        /// <summary>
        /// Constructor
        /// </summary>
        internal MarkdownHandler()
        {
        }

        /// <summary>
        /// Kill the CLI tool, if still running
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            converter.Dispose();
        }

        /// <summary>
        /// Kill the CLI tool, if still running
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Converts a markdown string into Html.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="nodeNamespace"></param>
        /// <returns>Returns true if any script tags was removed from the string</returns>
        internal bool ParseToHtml(ref StringWriter writer, string nodeNamespace)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            var mdFilePath = PackageDocumentationManager.Instance.GetAnnotationDoc(nodeNamespace);

            string mdString;
            bool scriptTagsRemoved = false;

            if (string.IsNullOrWhiteSpace(mdFilePath) ||
                !File.Exists(mdFilePath))
                mdString = DocumentationBrowserUtils.GetContentFromEmbeddedResource(NODE_ANNOTATION_NOT_FOUND);

            else
            {
                // Doing this to avoid 'System.ObjectDisposedException'
                // https://docs.microsoft.com/en-us/visualstudio/code-quality/ca2202?view=vs-2019
                Stream stream = null;
                try
                {
                    stream = File.Open(mdFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        stream = null;
                        mdString = reader.ReadToEnd();
                    }
                }
                finally
                {
                    stream?.Dispose();
                }

                if (string.IsNullOrWhiteSpace(mdString))
                    return false;

                // Remove scripts from user content for security reasons.
                if (SanitizeHtml(ref mdString))
                    scriptTagsRemoved = true;
            }

            var html = converter.ParseMd2Html(mdString, mdFilePath);
            writer.WriteLine(html);

            // inject the syntax highlighting script at the bottom at the document.
            writer.WriteLine(DocumentationBrowserUtils.GetDPIScript());
            writer.WriteLine(GetSyntaxHighlighting());

            return scriptTagsRemoved;
        }

        /// <summary>
        /// Clean up possible dangerous HTML content from the content string.
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Returns true if any content was removed from the content string</returns>
        internal bool SanitizeHtml(ref string content)
        {
            var sanitizedContent = converter.SanitizeHtml(content);

            if (string.IsNullOrEmpty(sanitizedContent))
            {
                return false;
            }

            content = sanitizedContent;
            return true;
        }

        /// <summary>
        /// Inject syntax highlighting into a html string.
        /// </summary>
        /// <param name="content"></param>
        private static string GetSyntaxHighlighting()
        {
            var syntaxHighlightingContent = DocumentationBrowserUtils.GetContentFromEmbeddedResource(SYNTAX_HIGHLIGHTING);
            if (string.IsNullOrWhiteSpace(syntaxHighlightingContent))
                return string.Empty;

            return syntaxHighlightingContent;
        }
    }
}
