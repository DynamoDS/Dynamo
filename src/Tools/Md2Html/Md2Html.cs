using System;
using System.IO;
using System.Linq;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Md2Html
{
    /// <summary>
    /// Utilities for converting Markdown to html and for sanitizing html
    /// </summary>
    internal class Md2Html
    {
        private readonly MarkdownPipeline pipeline;
        private static Md2Html instance;

        internal static Md2Html Instance
        {
            get
            {
                if (instance is null) { instance = new Md2Html(); }
                return instance;
            }
        }

        private Md2Html()
        {
            var pipelineBuilder = new MarkdownPipelineBuilder();
            pipeline = pipelineBuilder
                .UseAdvancedExtensions()
                .Build();
        }

        /// <summary>
        /// Converts a markdown string into Html.
        /// </summary>
        /// <param name="mdString"></param>
        /// <param name="mdPath"></param>
        /// <returns>Returns converted markdown as html</returns>
        internal string ParseToHtml(string mdString, string mdPath)
        {
            if (string.IsNullOrWhiteSpace(mdString))
                return string.Empty;

            using (var writer = new StringWriter())
            {
                // Remove scripts from user content for security reasons.
                var renderer = new HtmlRenderer(writer);
                pipeline.Setup(renderer);

                var document = MarkdownParser.Parse(mdString, pipeline);
                ConvertRelativeLocalImagePathsToAbsolute(mdPath, document);

                renderer.Render(document);

                return writer.ToString();
            }

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
                if (!Uri.IsWellFormedUriString(image.Url, UriKind.Relative))
                    continue;

                var imageName = Path.GetFileName(image.Url);
                var dir = Path.GetDirectoryName(mdFilePath);

                var htmlImagePathPrefix = @"file:///";
                var absoluteImagePath = Path.Combine(dir, imageName);

                image.Url = $"{htmlImagePathPrefix}{absoluteImagePath}";
            }
        }

        private static readonly Md2HtmlSanitizer HtmlSanitizer = new Md2HtmlSanitizer();

        /// <summary>
        /// Clean up possible dangerous HTML content from the content string.
        /// </summary>
        /// <param name="content"></param>
        /// <returns>return sanitized content string or an empty string if no sanitizing happened</returns>
        internal static string Sanitize(string content)
        {
            HtmlSanitizer.ContentWasUpdated = false;
            var output = HtmlSanitizer.Sanitize(content);
            if (HtmlSanitizer.ContentWasUpdated)
            {
                return output;
            }

            return string.Empty;
        }
    }
}
