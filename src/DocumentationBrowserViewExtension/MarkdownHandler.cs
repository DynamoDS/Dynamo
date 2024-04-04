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
        internal void ParseToHtml(ref StringWriter writer, string nodeNamespace, string packageName)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            var mdFilePath = PackageDocumentationManager.Instance.GetAnnotationDoc(nodeNamespace, packageName);

            string mdString;

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
                    return;
            }

            var html = converter.ParseMd2Html(mdString, mdFilePath);
            writer.WriteLine(html);
        }

        /// <summary>
        /// Converts a markdown string into Html string.
        /// </summary>
        /// <param name="nodeNamespace"></param>
        internal string ParseToHtml(string nodeNamespace, string packageName)
        {
            var mdFilePath = PackageDocumentationManager.Instance.GetAnnotationDoc(nodeNamespace, packageName);

            string mdString;

            if (string.IsNullOrWhiteSpace(mdFilePath) ||
                !File.Exists(mdFilePath))
                mdString = ResourceUtilities.LoadContentFromResources(NODE_ANNOTATION_NOT_FOUND, GetType().Assembly);

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
                    return string.Empty;
            }

            var html = converter.ParseMd2Html(mdString, mdFilePath);
            return html;
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
    }
}
