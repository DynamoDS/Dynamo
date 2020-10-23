using System;
using System.IO;
using System.Linq;
using Dynamo.PackageManager;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Handles markdown files by converting them to Html, so they can display in the doc browser.
    /// </summary>
    internal class MarkdownHandler
    {
        private const string NODE_ANNOTATION_NOT_FOUND = "Dynamo.DocumentationBrowser.Docs.NodeAnnotationNotFound.md";
        private const string SYNTAX_HIGHLIGHTING = "Dynamo.DocumentationBrowser.Docs.syntaxHighlight.html";
        private readonly MarkdownPipeline pipeline;


        private static MarkdownHandler instance;
        internal static MarkdownHandler Instance
        {
            get
            {
                if (instance is null) { instance = new MarkdownHandler(); }
                return instance;
            }
        }

        private MarkdownHandler()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder();
            pipeline = pipelineBuilder
                .UseAdvancedExtensions()
                .Build();
        }

        /// <summary>
        /// Converts a markdown string into Html.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="mdFilePath"></param>
        /// <returns>Returns true if any script tags was removed from the string</returns>
        internal bool ParseToHtml(ref StringWriter writer, string nodeNamespace)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            var mdFilePath = PackageDocumentationManager.Instance.GetAnnotationDoc(nodeNamespace);

            string mdString;
            bool scriptTagsRemoved;

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
                if (DocumentationBrowserUtils.RemoveScriptTagsFromString(ref mdString))
                    scriptTagsRemoved = true;
            }
            scriptTagsRemoved = false;

            var renderer = new HtmlRenderer(writer);
            pipeline.Setup(renderer);

            var document = MarkdownParser.Parse(mdString, pipeline);
            ConvertRelativeLocalImagePathsToAbsolute(mdFilePath, document);

            renderer.Render(document);
            // inject the syntax highlighting script at the bottom at the document.
            writer.WriteLine(DocumentationBrowserUtils.GetDPIScript());
            writer.WriteLine(GetSyntaxHighlighting());

            return scriptTagsRemoved;
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

        /// <summary>
        /// For markdown local images needs to be in the same folder as the md file
        /// referencing it with a relative path "./image.png", when we convert to html
        /// we need the full path. This method finds relative image paths and converts them to absolute paths.
        /// </summary>
        private static void ConvertRelativeLocalImagePathsToAbsolute(string mdFilePath, MarkdownDocument document)
        {
            var imageLinks = document.Descendants<ParagraphBlock>()
                .SelectMany(x => x.Inline.Descendants<LinkInline>())
                .Where(x => x.IsImage)
                .Select(x => x).ToList();

            foreach (var image in imageLinks)
            {
                if (!image.Url.StartsWith("./"))
                    continue;

                var imageName = image.Url.Split(new string[] { "./" }, StringSplitOptions.None);
                var dir = Path.GetDirectoryName(mdFilePath);

                var htmlImagePathPrefix = @"file:///";
                var absoluteImagePath = Path.Combine(dir, imageName.Last());

                image.Url = $"{htmlImagePathPrefix}{absoluteImagePath}";
            }
        }
    }
}
